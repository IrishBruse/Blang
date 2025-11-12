main() {
    extrn printf;
    auto x;
    auto i;
    x = 5;
    i = 0;

    while (i < x) {
        if (i == 2) {
            printf("two\n");
        }
        i = i + 1;
    }
}
