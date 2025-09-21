main()
{
    extrn printf;
    auto a, b, c;

    a = 12;
    printf("a %d\n", a);

    b = &a;

    c = *b;
    printf("c %d\n", c);
}
