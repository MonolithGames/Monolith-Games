#include <stdio.h>
#include <stdlib.h>
#include <math.h>
#include <stdint.h>

typedef uint8_t BYTE;
typedef uint16_t WORD;
typedef uint32_t DWORD;
typedef int32_t LONG;

void hsv_to_rgb(float h, float s, float v, float *r, float *g, float *b) {
    float c = v * s;
    float x = c * (1 - fabsf(fmodf(h / 60.0f, 2) - 1));
    float m = v - c;
    float r1, g1, b1;

    if (h >= 0 && h < 60) { r1 = c; g1 = x; b1 = 0; }
    else if (h >= 60 && h < 120) { r1 = x; g1 = c; b1 = 0; }
    else if (h >= 120 && h < 180) { r1 = 0; g1 = c; b1 = x; }
    else if (h >= 180 && h < 240) { r1 = 0; g1 = x; b1 = c; }
    else if (h >= 240 && h < 300) { r1 = x; g1 = 0; b1 = c; }
    else { r1 = c; g1 = 0; b1 = x; }

    *r = r1 + m;
    *g = g1 + m;
    *b = b1 + m;
}

typedef struct {
    int size;
    DWORD imageSize;
    DWORD offset;
    BYTE *data;
} IconImage;

int main() {
    int sizes[] = { 16, 32, 48, 64, 128, 256, 512 };
    int numImages = sizeof(sizes) / sizeof(sizes[0]);

    IconImage *images = malloc(numImages * sizeof(IconImage));

    DWORD currentOffset = 6 + (numImages * 16); // ICONDIR (6) + N * ICONDIRENTRY (16)

    for (int i = 0; i < numImages; i++) {
        int sz = sizes[i];
        images[i].size = sz;

        int headerSize = 40;
        int pixelDataSize = sz * sz * 4;
        int andMaskSize = ((sz + 31) / 32) * 4 * sz; // 4-byte padded row per scanline
        images[i].imageSize = headerSize + pixelDataSize + andMaskSize;
        images[i].offset = currentOffset;

        images[i].data = malloc(images[i].imageSize);
        BYTE *ptr = images[i].data;

        // BITMAPINFOHEADER (40 bytes)
        DWORD biSize = 40;
        LONG biWidth = sz;
        LONG biHeight = sz * 2; // XOR + AND mask
        WORD biPlanes = 1;
        WORD biBitCount = 32;
        DWORD biCompression = 0;
        DWORD biSizeImage = pixelDataSize;
        LONG biXPelsPerMeter = 0;
        LONG biYPelsPerMeter = 0;
        DWORD biClrUsed = 0;
        DWORD biClrImportant = 0;

        *(DWORD*)ptr = biSize; ptr += 4;
        *(LONG*)ptr = biWidth; ptr += 4;
        *(LONG*)ptr = biHeight; ptr += 4;
        *(WORD*)ptr = biPlanes; ptr += 2;
        *(WORD*)ptr = biBitCount; ptr += 2;
        *(DWORD*)ptr = biCompression; ptr += 4;
        *(DWORD*)ptr = biSizeImage; ptr += 4;
        *(LONG*)ptr = biXPelsPerMeter; ptr += 4;
        *(LONG*)ptr = biYPelsPerMeter; ptr += 4;
        *(DWORD*)ptr = biClrUsed; ptr += 4;
        *(DWORD*)ptr = biClrImportant; ptr += 4;

        // Pixel data (XOR mask), bottom-up, pre-multiplied BGRA
        float cx = (sz - 1.0f) / 2.0f;
        float cy = (sz - 1.0f) / 2.0f;
        float radius = sz / 2.0f - (sz > 32 ? 4.0f : 1.5f);
        float borderWidth = sz > 32 ? (sz * 0.12f) : 2.5f;

        for (int y = sz - 1; y >= 0; y--) {
            for (int x = 0; x < sz; x++) {
                float dx = x - cx;
                float dy = y - cy;
                float dist = sqrtf(dx * dx + dy * dy);

                float alpha_f = 0.0f;
                if (dist <= radius) {
                    // Anti-aliased outer edge
                    if (dist > radius - 1.5f) {
                        alpha_f = (radius - dist) / 1.5f;
                        if (alpha_f < 0.0f) alpha_f = 0.0f;
                        if (alpha_f > 1.0f) alpha_f = 1.0f;
                    } else {
                        alpha_f = 1.0f;
                    }
                }

                float rf = 0, gf = 0, bf = 0;
                if (alpha_f > 0.0f) {
                    float angle = atan2f(dy, dx) * (180.0f / 3.1415926535f);
                    if (angle < 0) angle += 360.0f;
                    hsv_to_rgb(angle, 1.0f, 1.0f, &rf, &gf, &bf);

                    // Add an inner dark/white core or ring effect for a sleek HD icon look
                    float innerRadius = radius - borderWidth;
                    if (dist < innerRadius && dist > innerRadius - 1.5f) {
                        // Smooth inner ring edge
                        float innerAlpha = (dist - (innerRadius - 1.5f)) / 1.5f;
                        rf = rf * innerAlpha + 1.0f * (1.0f - innerAlpha);
                        gf = gf * innerAlpha + 1.0f * (1.0f - innerAlpha);
                        bf = bf * innerAlpha + 1.0f * (1.0f - innerAlpha);
                    } else if (dist <= innerRadius) {
                        rf = 1.0f; gf = 1.0f; bf = 1.0f; // White center core
                    }
                }

                // Pre-multiplied alpha for perfect transparency without black fringe
                BYTE a = (BYTE)(255 * alpha_f);
                BYTE b = (BYTE)(bf * alpha_f * 255.0f + 0.5f);
                BYTE g = (BYTE)(gf * alpha_f * 255.0f + 0.5f);
                BYTE r = (BYTE)(rf * alpha_f * 255.0f + 0.5f);

                *ptr++ = b;
                *ptr++ = g;
                *ptr++ = r;
                *ptr++ = a;
            }
        }

        // AND mask (all zeros since 32-bit alpha handles transparency)
        for (int y = 0; y < sz; y++) {
            for (int i = 0; i < ((sz + 31) / 32) * 4; i++) {
                *ptr++ = 0x00;
            }
        }

        currentOffset += images[i].imageSize;
    }

    FILE *f = fopen("icon.ico", "wb");
    if (!f) {
        perror("Failed to open icon.ico");
        return 1;
    }

    // ICONDIR
    WORD reserved = 0;
    WORD type = 1;
    WORD count = (WORD)numImages;
    fwrite(&reserved, 2, 1, f);
    fwrite(&type, 2, 1, f);
    fwrite(&count, 2, 1, f);

    // ICONDIRENTRY list
    for (int i = 0; i < numImages; i++) {
        BYTE bWidth = (images[i].size >= 256) ? 0 : (BYTE)images[i].size;
        BYTE bHeight = (images[i].size >= 256) ? 0 : (BYTE)images[i].size;
        BYTE colorCount = 0;
        BYTE reserved2 = 0;
        WORD planes = 1;
        WORD bitCount = 32;
        DWORD bytesInRes = images[i].imageSize;
        DWORD imageOffset = images[i].offset;

        fwrite(&bWidth, 1, 1, f);
        fwrite(&bHeight, 1, 1, f);
        fwrite(&colorCount, 1, 1, f);
        fwrite(&reserved2, 1, 1, f);
        fwrite(&planes, 2, 1, f);
        fwrite(&bitCount, 2, 1, f);
        fwrite(&bytesInRes, 4, 1, f);
        fwrite(&imageOffset, 4, 1, f);
    }

    // Image data payloads
    for (int i = 0; i < numImages; i++) {
        fwrite(images[i].data, images[i].imageSize, 1, f);
        free(images[i].data);
    }

    fclose(f);
    free(images);
    printf("HD 8K Multi-Resolution transparent icon.ico generated successfully.\n");
    return 0;
}
