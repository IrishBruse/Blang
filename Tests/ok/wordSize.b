main() {
    extrn printf;

    auto word;
    word = &0[1];

    printf("word size = %d", word);
}
