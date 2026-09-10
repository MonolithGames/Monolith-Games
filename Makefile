CC = gcc
CFLAGS = -Wall -Wextra -std=c11
LDFLAGS = -lgdi32

SRC = src/main.c
OUT = Alphabet_Media.exe

all:
	$(CC) $(CFLAGS) $(SRC) -o $(OUT) $(LDFLAGS)

clean:
	del $(OUT)
