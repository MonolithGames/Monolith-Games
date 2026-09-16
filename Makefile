CC = gcc
CFLAGS = -Wall -Wextra -std=c11 -Iinclude
LDFLAGS = -lgdi32 -lmsimg32 -mwindows

VERSION ?= 1
PRODUCT = Monolith

SRC = src/main.c src/platform.c src/config.c src/pipeline.c src/settings.c src/chat.c
RES = res/AlphabetMedia.rc
RESOBJ = build/Monolith.res

OUT = build/$(PRODUCT)_v$(VERSION).exe

all: $(RESOBJ)
	@if not exist build mkdir build
	$(CC) $(CFLAGS) $(SRC) $(RESOBJ) -o $(OUT) $(LDFLAGS)

$(RESOBJ): $(RES)
	@if not exist build mkdir build
	windres -DMONOLITH_VERSION=$(VERSION) $(RES) -O coff -o $(RESOBJ)

clean:
	-del /Q build\$(PRODUCT)_v*.exe
	-del /Q $(RESOBJ)

release:
	$(MAKE) VERSION=$(VERSION) all



