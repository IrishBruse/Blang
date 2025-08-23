# B Langauge implementation

Implementing the B lang in C# with qbe backend

## Quickstart

Run an example

```shell
./bc run Examples/HelloWorld.b --debug
```

![bc](misc/screenshots/bc_run.png)

Run the compiler tests

```shell
./bt
```

![bt](misc/screenshots/bt.png)

## Currently Implemented

-   [x] function definitions
-   [x] extrn
-   [x] function calls
-   [x] auto
-   [x] variable assignment
-   [x] math `foo = (1 + 2 * 3)` foo = 7
-   [x] Global variables
-   [x] if
-   [x] else
-   [x] while
-   [ ] switch

## Targets

-   qbe

## Dev Setup

-   Dotnet 8
-   QBE executable either build from source or use the linux-x86_64 one in `misc/qbe`
-   cd into the BLang folder in the repo and run
    -   `dotnet run -- --run -t qbe '../Examples/HelloWorld.b'`

## References

-   [QBE Docs](https://c9x.me/compile/doc/il.html)
-   [tsoding/b](https://github.com/tsoding/b)
