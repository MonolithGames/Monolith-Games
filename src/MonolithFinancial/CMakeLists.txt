cmake_minimum_required(VERSION 3.20)
project(MonolithFinancial C)

set(CMAKE_C_STANDARD 11)

add_executable(icon_generator icon_generator.c)

add_custom_command(
    OUTPUT ${CMAKE_CURRENT_SOURCE_DIR}/icon.ico
    DEPENDS icon_generator
    COMMAND icon_generator
    WORKING_DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}
)

add_executable(MonolithFinancial WIN32 main.c app.rc ${CMAKE_CURRENT_SOURCE_DIR}/icon.ico)
add_dependencies(MonolithFinancial icon_generator)

if(WIN32)
    target_link_libraries(MonolithFinancial PRIVATE user32 gdi32 kernel32)
endif()
