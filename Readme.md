# B Compiler

Implementing B in C# with qbe backend

## Quickstart

Run an example with some debug info

`❯ ./bc run Examples/HelloWorld.b`

```
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
  <file>  Path to b file to build

Options:
  --tokens        Print tokenizers tokens
  --symbols       Print symbol table
  --ast           Dump compiler ast information
  -v, --verbose   Verbose output
  -vv             Very Verbose output
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
✓ Tests/ok/comments.b (33ms)
✓ Tests/ok/empty.b (15ms)
✓ Tests/ok/helloworld.b (20ms)
✓ Tests/ok/integer.b (18ms)
✓ Tests/ok/pointers.b (18ms)
✓ Tests/ok/Statement/auto.b (15ms)
✓ Tests/ok/Statement/extrn.b (15ms)
✓ Tests/ok/Statement/function.b (16ms)
✓ Tests/ok/Statement/functionCall.b (16ms)
✓ Tests/ok/Statement/if.b (20ms)
✓ Tests/ok/Statement/ifelse.b (17ms)
✓ Tests/ok/Statement/while.b (16ms)
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
