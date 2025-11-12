main() {
    extrn printf;
    auto i;
    auto j;
    i = 0;
    while (i < 3) {
        j = 0;
        while (j < 2) {
            printf("%d,%d ", i, j);
            j = j + 1;
        }
        i = i + 1;
    }
}
