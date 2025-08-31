main() {
    extrn printf, malloc;

    auto p;
    p = malloc(16);

    printf("%p", p);
}
