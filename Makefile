CC = gcc
CFLAGS = -Wall -Wextra -std=c11

SRC = src/main.c
OUT = Alphabet_Media.exe

all:
	$(CC) $(CFLAGS) $(SRC) -o $(OUT)

clean:
	del $(OUT)
