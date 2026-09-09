CC = gcc
CFLAGS = -Wall -Wextra -Iinclude

SRC = $(wildcard src/**/*.c)
OBJ = $(SRC:.c=.o)

BIN = monolith_engine

all: $(BIN)

$(BIN): $(OBJ)
    $(CC) $(OBJ) -o $(BIN)

clean:
    rm -f $(OBJ) $(BIN)
