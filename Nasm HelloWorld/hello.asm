extern GetStdHandle                             ; Import external symbols
extern WriteConsoleA                                ; Windows API functions, not decorated
extern ExitProcess

global main                                    ; Export symbols. The entry point

section .text                                   ; Code segment
main:
    call stdOutHandler


    lea   rdx, [hello]                      ; 2nd parameter
    mov   r8, helloLength                   ; 3rd parameter
    call print


    lea   rdx, [goodbye]                      ; 2nd parameter
    mov   r8, goodbyeLength                   ; 3rd parameter
    call print

    mov ecx, 0
    call  ExitProcess

; pass rdx the string and r8 the string len
print:
    mov   ecx, dword [stdout_handler]       ; 1st parameter
    xor   r9, r9                            ; 4th parameter
    call  WriteConsoleA
    ret

stdOutHandler:
    push  qword 0
    sub   RSP, 32                                   ; 32 bytes of shadow space
    mov   ecx, -11                                  ; STD_OUTPUT_HANDLE -  write to std out
    call  GetStdHandle
    mov   dword [stdout_handler], eax
    add   RSP, 40
    ret

section .data                                   ; Initialized data segment
    hello        db "Hello, World!", 0Dh, 0Ah,0
    helloLength  EQU $-hello
    goodbye        db "Goodbye, cruel World!", 0Dh, 0Ah,0
    goodbyeLength  EQU $-goodbye
    stdout_handler dd 0