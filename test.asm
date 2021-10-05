extern GetStdHandle
extern WriteConsoleA
extern ExitProcess

global main   ; Export the entry point
section .text
main:
    mov ecx, 0
    call  ExitProcess
