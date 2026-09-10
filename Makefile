CC = gcc
CFLAGS = -Wall -Wextra -std=c11

SRC = src/main.c src/commands.c
OUT = console_app.exe

all:
	$(CC) $(CFLAGS) $(SRC) -o $(OUT)

clean:
	del $(OUT)
