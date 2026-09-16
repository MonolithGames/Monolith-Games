cmake_minimum_required(VERSION 3.20)
project(MonolithNative LANGUAGES C CXX)

set(CMAKE_C_STANDARD 11)
set(CMAKE_CXX_STANDARD 17)

add_library(monolith_c OBJECT engine/c/src/main.c)
target_include_directories(monolith_c PRIVATE engine/c/include engine/shared)

add_library(monolith_cpp OBJECT engine/cpp/src/main.cpp)
target_include_directories(monolith_cpp PRIVATE engine/cpp/include engine/shared)
