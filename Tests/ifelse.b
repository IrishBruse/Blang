main()
{
    extrn printf;
    printf("Test: ifelse\n");
    if (1<0)
    {
        printf("Fail\n");
    }
    else
    {
        printf("Pass\n");
    }
}
