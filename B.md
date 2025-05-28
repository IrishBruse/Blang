## Language Overview

B is a small, recursive, and non-numeric programming language, primarily designed for system programming. It's a direct descendant of BCPL and was an ancestor to C. Key characteristics include:

-   **Compactness:** Due to its rich set of operators and expressive freedom, B programs can be quite concise.
-   **Machine Independence:** Intended for applications that are not tied to specific hardware.
-   **PDP-11 and UNIX-11 Focus:** This manual specifically describes the PDP-11 implementation under the UNIX-11 time-sharing system.
-   **Syntactic Richness in Expressions, Poor in Statements:** This implies a focus on complex expressions rather than numerous distinct statement types.
-   **No Type Checking or Automatic Conversions:** A significant point, meaning the programmer is responsible for type consistency.
-   **Values as Bit Patterns:** All values are fixed-length bit patterns (16 bits on PDP-11).
-   **Lvalues and Rvalues:** Distinct concepts for storage locations (lvalues) and their contents (rvalues).
    -   `*X`: Interprets an rvalue `X` as an lvalue (indirection).
    -   `&x`: Interprets an lvalue `x` as an rvalue (address-of).

## Compiler Implementation Features

### Lexical Analysis

-   **Comments:** Delimited by `/*` and `*/` (PL/I style).
-   **Separators:** Tokens are generally separated by blanks, comments, or newlines. Implicit separators around `(){}[],;?:` and maximal sequences of `+-*/<>&|!.`
-   **Character Set:** ANSCII.
-   **Escape Sequences:**
    -   `*0`: null
    -   `*e`: end-of-file
    -   `*(`: `{`
    -   `*)`: `}`
    -   `*t`: tab
    -   `**`: `*`
    -   `*'`: `'`
    -   `*"`: `"`
    -   `*n`: new line
-   **Keywords:** Recognized only in lowercase and are reserved.
-   **Identifiers (Names):** `alpha {alpha-digit}07`.
    -   `alpha`: `A-Z`, `a-z`, `_`, backspace.
    -   `digit`: `0-9`.

### Syntax Analysis (BNF with specific extensions)

-   **No `<` and `>`:** Literals are underlined (e.g., `_return_`).
-   **No `|`:** Each syntactic alternative is on a new line.
-   **`{}` for Grouping:** Standard grouping.
-   **Super/Subscripts for Repetition:**
    -   `{..}m`: `m, m+1, ...`
    -   `{..}mn`: `m, m+1, ..., n`

#### Canonical Syntax Elements:

-   **`program`**: Zero or more definitions.
-   **`definition`**:
    -   `name [ {constant}* ] {ival {, ival}0}* ;` (External variable/vector definition)
    -   `name ( {name {, name}0}* ) statement` (Function definition)
-   **`ival`**: `constant` or `name`.
-   **`statement`**:
    -   `auto name {constant}* {, name {constant}*}0 ; statement`
    -   `extrn name {, name}0 ; statement`
    -   `name : statement` (Label)
    -   `case constant : statement`
    -   `{ {statement}0 }` (Compound statement)
    -   `if ( rvalue ) statement {else statement}*`
    -   `while ( rvalue ) statement`
    -   `switch rvalue statement`
    -   `goto rvalue ;`
    -   `return {( rvalue )}* ;`
    -   `{rvalue}* ;` (Expression statement, often assignment or function call)
-   **`rvalue`**:
    -   `( rvalue )`
    -   `lvalue`
    -   `constant`
    -   `lvalue assign rvalue`
    -   `inc-dec lvalue`
    -   `lvalue inc-dec`
    -   `unary rvalue`
    -   `& lvalue`
    -   `rvalue binary rvalue`
    -   `rvalue ? rvalue : rvalue` (Conditional expression)
    -   `rvalue ( {rvalue {, rvalue}0 }* )` (Function call)
-   **`assign`**: `= {binary}*` (Handles `+=`, `*=`, etc.)
-   **`inc-dec`**: `++`, `--`
-   **`unary`**: `-`, `!`
-   **`binary`**: `|`, `&`, `==`, `!=`, `<`, `<=`, `>`, `>=`, `<<`, `>>`, `-`, `+`, `%`, `*`, `/`
-   **`lvalue`**:
    -   `name`
    -   `* rvalue` (Indirection)
    -   `rvalue [ rvalue ]` (Array indexing, equivalent to `*(E1+E2)`)
-   **`constant`**:
    -   `{digit}1` (Decimal or octal if starting with `0`)
    -   `' {char}12 '` (Character constant, 1 or 2 characters)
    -   `" {char}0 "` (String literal)

### Semantic Analysis / Intermediate Representation

-   **Expression Binding Order:**
    1.  Primary Expressions (left to right)
    2.  Unary Operators (right to left)
    3.  Multiplicative Operators (`*`, `/`, `%`) (left to right)
    4.  Additive Operators (`+`, `-`) (left to right)
    5.  Shift Operators (`<<`, `>>`) (left to right)
    6.  Relational Operators (`<`, `<=`, `>`, `>=`)
    7.  Equality Operators (`==`, `!=`)
    8.  AND Operator (`&`) (left to right)
    9.  OR Operator (`|`) (left to right)
    10. Conditional Expression (`? :`) (right to left)
    11. Assignment Operators (all 16 forms, right to left)
-   **No Type Checking:** The compiler should _not_ enforce type compatibility.
-   **No Type Conversions:** No implicit conversions are performed.
-   **Integer Arithmetic:** `*`, `/`, `%` expect integer operands and produce integer results. Division truncation toward zero for positive operands; otherwise, undefined. Modulo undefined for negative operands.
-   **Shift Operators:** Vacated bits filled with zeros. Undefined for negative or overly large shift counts.

### Code Generation / Runtime

-   **Storage Classes:**
    -   **Automatic:** Allocated on function invocation (stack-based). Initialized to base of automatic vector if a constant is provided.
    -   **External:** Allocated before execution, global to all functions.
    -   **Internal:** Local to a function, but available to all invocations of that function. (First reference not `extrn` or `auto`).
-   **External Definitions:**
    -   **Simple:** `name {ival , ival ...}0 ;` (initializes with zero if no `ival`, constant value, or address of `name`).
    -   **Vector:** `name [ {constant}* ] {ival , ival ...}0 ;` (base of vector, initial values determine size if missing).
    -   **Function:** `name ( arguments ) statement` (arguments are automatic lvalues).
-   **Function Calls:** Recursive by design, with minimal cost. Parameters are assigned to function's automatic lvalues.
-   **Switch Statement:** Evaluates `rvalue`, compares to `case constant`s (order undefined). Jumps to matching `case`. If no match, `statement1` (the switch body) is skipped. No fall-through behavior explicitly mentioned, but the examples suggest it (i.e., you need `goto loop;` in `printf` to prevent fall-through).
-   **Labels:** Local to the function, `goto` cannot cross function boundaries.
-   **Return Statement:** Can optionally return an rvalue.
-   **Main Function:** Execution starts with `main(); exit();`.
-   **Standard Library:** Essential functions like `printf`, `putchar`, `getchar`, file I/O, process control (`fork`, `exec`), etc. (`/etc/libb.a`).
-   **Command-line Arguments:** Available via the predefined external vector `argv`. `argv[0]` is the count.
-   **PDP-11 Implementation Details (Threaded Code Interpreter):**
    -   Reverse Polish threaded code.
    -   Machine Registers: `R3` (interpreter program counter), `R4` (interpreter display pointer/stack frame base), `R5` (interpreter stack pointer).
    -   Stack Frame Structure: `prev_display_pointer`, `saved_interpreter_pc`, `automatic_variables`.
    -   Lvalues as word addresses (need to shift to byte addresses for memory access).
    -   External variables are global with names prefixed by `.` (e.g., `.external`).
