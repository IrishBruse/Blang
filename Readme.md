# B Compiler

Implementing B in C# with qbe backend

## Quickstart

Run an example with some debug info

`❯ ./bc run Examples/HelloWorld.b --debug`

```
[CMD] qbe Examples/obj/qbe/HelloWorld.ssa -o Examples/obj/qbe/HelloWorld.s
[CMD] gcc Examples/obj/qbe/HelloWorld.s -o Examples/bin/qbe/HelloWorld
Hello, World
```

## Help

`❯ ./bc --help`

```
Description:
  Compiler for the b programming lanaugage

Usage:
  bc <file> [command] [options]

Arguments:
  <file>  Path to b file

Options:
  --debug         Print compiler debug information
  --ast           Dump compiler ast information
  --tokens        Print tokenizers tokens
  --symbols       Print symbol table
  -?, -h, --help  Show help and usage information
  --version       Show version information

Commands:
  run <file>   Run .b file
  test <file>  Test compiler output
```

## Testing

Run all the compiler tests

`❯ ./bc test`

```shell
✓ Tests/empty.b (66ms)
✓ Tests/integer.b (24ms)
✓ Tests/pointers.b (21ms)
✓ Tests/ok/comments.b (17ms)
✓ Tests/ok/helloworld.b (14ms)
✓ Tests/ok/Statement/auto.b (15ms)
✓ Tests/ok/Statement/extrn.b (15ms)
✓ Tests/ok/Statement/function.b (15ms)
✓ Tests/ok/Statement/functionCall.b (15ms)
✓ Tests/ok/Statement/if.b (22ms)
✓ Tests/ok/Statement/ifelse.b (15ms)
✓ Tests/ok/Statement/while.b (17ms)
```


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
-   [x] pointers \* &
-   [ ] Array index [i]
-   [ ] switch

## Targets

-   qbe

## Dev Setup

-   Dotnet 8
-   QBE executable either build from source or use the linux-x86_64 one in `misc/qbe`
-   cd into the BLang folder in the repo and run
    -   `dotnet run -- run '../Examples/HelloWorld.b'`

## References

-   [QBE Docs](https://c9x.me/compile/doc/il.html)
-   [tsoding/b](https://github.com/tsoding/b)
