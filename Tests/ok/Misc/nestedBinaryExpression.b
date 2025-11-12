main() {
    extrn printf;
    auto result;
    auto a;
    auto b;
    auto c;
    auto d;
    a = 5;
    b = 3;
    c = 2;
    d = 1;
    result = (a + b) * (c - d);
    printf("%d\n", result);
}
