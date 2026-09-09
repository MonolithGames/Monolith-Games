CC = gcc
CFLAGS = -Wall -Wextra

BIN = hello_world
SRC = main.c
OBJ = main.o

all: $(BIN)

$(BIN): $(OBJ)
	$(CC) $(OBJ) -o $(BIN)

clean:
	rm -f $(OBJ) $(BIN)
