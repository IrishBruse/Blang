main()
{
    extrn printf;
    auto a, b, c;

    a = 12;
    printf("a %d\n", a);

    b = &a;
    printf("b %p\n", b);

    c = *b;
    printf("c %d\n", c);
}
