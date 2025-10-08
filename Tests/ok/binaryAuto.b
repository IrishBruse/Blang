main() {
    extrn printf;
    auto result;
    result = 10 < 20; /* Should evaluate to 1 (true) */
    printf("%d\n", result);
}
