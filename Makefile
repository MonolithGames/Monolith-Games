CC = gcc
CFLAGS = -Wall -Wextra -std=c11
LDFLAGS = -lgdi32 -mwindows

SRC = src/main.c src/platform.c
RES = res/AlphabetMedia.rc
RESOBJ = res/AlphabetMedia.res

OUT = Alphabet_Media.exe

all: $(RESOBJ)
	$(CC) $(CFLAGS) $(SRC) $(RESOBJ) -o $(OUT) $(LDFLAGS)

$(RESOBJ): $(RES)
	windres $(RES) -O coff -o $(RESOBJ)

clean:
	del $(OUT)
	del $(RESOBJ)
