main() {
    extrn printf;
    auto x;
    x = 2;
    if (x == 1) {
        printf("one\n");
    } else {
        if (x == 2) {
            printf("two\n");
        } else {
            printf("other\n");
        }
    }
}
