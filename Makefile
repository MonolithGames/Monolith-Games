CC = gcc
CFLAGS = -Wall -Wextra

BIN = hello_world
SRC = src/main.c
OBJ = src/main.o

all: $(BIN)

$(BIN): $(OBJ)
	$(CC) $(OBJ) -o $(BIN)

clean:
	rm -f $(OBJ) $(BIN)
