#include <stdio.h>

void PrintString(char *val) { printf("%s", val); }
void PrintNumber(int val) { printf("%d", val); }

int Fibonacci(int n){

if(n<=1){

return n;
}
else{

return Fibonacci(n- 1)+ Fibonacci(n- 2);
}
;
}
int main(){

int result=Fibonacci(10);PrintNumber(result);return 1;
}
