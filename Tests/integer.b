main() {
    extrn printf;
    printf("%d\n", -2147483648);
    /* qbe is set to use 32 bit word size */
    printf("%d\n", 2147483647);
}
