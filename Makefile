CC = gcc
CFLAGS = -Wall -Wextra -Iinclude

SRC = $(wildcard src/*.c)
OBJ = $(SRC:.c=.o)

BIN = monolith_game

all: $(BIN)

$(BIN): $(OBJ)
    $(CC) $(OBJ) -o $(BIN)

clean:
    rm -f src/*.o $(BIN)
