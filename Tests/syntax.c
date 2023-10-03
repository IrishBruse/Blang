#include <stdio.h>

void PrintNumber(int n)
{
    printf("%d\n", n);
}

void PrintString(const char* s)
{
    puts(s);
}

void Example__Print__int() {}

typedef char* string;

int main();
int foo();
void test(string s);

int main()
{
    if (foo() == 4)
    {
        return 0;
    }
    else
    {
        test("Hello World \n");
    }
}

int foo()
{
    return 4;
}

void test(string s)
{
    PrintString(s);
}
