This is a syntax reference for common QBE Intermediate Language (IL) use cases, with examples to illustrate.

The IL is a higher-level language than machine assembly, designed to abstract hardware irregularities and allow for an infinite number of temporaries, enabling frontend programmers to focus on language design. IL files are provided to QBE as text, typically one file per compilation unit, and consist of definitions for data, functions, and types.

### Basic Concepts

-   **Comments**: Start with a `#` character and extend to the end of the line.

    ```qbe
    # This is a comment
    data $my_data = { w 10 } # This comment is after an instruction
    ```

-   **Sigils**: All user-defined names are prefixed with a sigil to avoid keyword conflicts and quickly identify the scope and nature of identifiers.
    -   `:` for user-defined aggregate types.
    -   `$` for globals (represented by a pointer).
    -   `%` for function-scope temporaries.
    -   `@` for block labels.

### Types

QBE IL makes minimal use of types, restricting them to what is necessary for unambiguous compilation to machine code and C interfacing.

-   **Base Types**:

    -   `w`: 32-bit integer (word).
    -   `l`: 64-bit integer (long).
    -   `s`: 32-bit floating-point (single).
    -   `d`: 64-bit floating-point (double).
    -   _Note_: Pointers are typed by an integer type wide enough to represent memory addresses (e.g., `l` on 64-bit systems). Temporaries can only have a base type.

-   **Extended Types**: Include base types plus `b` (8-bit byte) and `h` (16-bit half word), used in aggregate types and data definitions.
-   **Sub-word Types for C Interfacing**:
    -   `sb`: Signed 8-bit value.
    -   `ub`: Unsigned 8-bit value.
    -   `sh`: Signed 16-bit value.
    -   `uh`: Unsigned 16-bit value.
    -   _Note_: Parameters with sub-word types have their N least significant bits set and have base type `w`.

### Constants and Vals

-   **Constants**:
    -   Decimal integers (e.g., `123`, `-42`).
    -   Single-precision floats (e.g., `s_3.14`, `s_0.0`).
    -   Double-precision floats (e.g., `d_3.14159`, `d_1.0e-5`).
    -   Global symbols (e.g., `$my_global`).
    -   Thread-local symbols (e.g., `thread $my_thread_local`).
-   **Vals**: Can be constants or function-scope temporaries (e.g., `%temp_var`).

    ```qbe
    %x =w add 10, %y # 10 is a constant, %y is a val (temporary)
    %ptr =l copy $my_global # $my_global is a constant (global symbol)
    ```

### Linkage

Linkage information is passed to the assembler and linker.

-   `export`: Marks the defined item as visible outside the current file. Functions called from C need to be exported.
-   `thread`: Qualifies data definitions, mandating the object is stored in thread-local storage.
-   `section SECNAME [SECFLAGS]`: Puts the defined item in a specific section.

    ```qbe
    export function w $my_c_function(w %arg) { ... }
    thread data $my_thread_data = { w 0 }
    section ".init_array" data $.init.func_ptr = { l $init_function }
    ```

### Definitions

IL files define aggregate types, data, and functions.

#### Aggregate Types

Defined with the `type` keyword. They have file scope and must be defined before being referenced.

-   **Regular Type**:
    ```qbe
    type :my_struct = { w, l, s }
    type :array_of_words = { w 10 } # An array of 10 words
    type :aligned_struct = align 16 { w, w }
    ```
-   **Union Type**:
    ```qbe
    type :my_union = { { w }, { d } }
    ```
-   **Opaque Type**: Used when the inner structure cannot be specified; alignment is mandatory.
    ```qbe
    type :my_opaque_type = align 8 { 64 } # An opaque type of 64 bytes, aligned to 8 bytes
    ```

#### Data

Defines global objects, emitted in the compiled file, accessible via a pointer.

-   **Syntax**:
    ```
    DATADEF := LINKAGE* 'data' $IDENT '=' ['align' NUMBER] '{' ( EXTTY DATAITEM+ | 'z' NUMBER ), '}'
    DATAITEM := $IDENT ['+' NUMBER] | '"' ... '"' | CONST
    ```
-   **Examples**:
    ```qbe
    data $my_string = { b "hello", b 0 } # String constant with null terminator
    data $my_array = { w 1, w 2, w 3 } # Array of three 32-bit words
    data $zero_initialized_data = { z 128 } # 128 bytes zero-initialized
    data $pointer_to_self = { l $pointer_to_self } # A pointer to itself
    ```

#### Functions

Contain the actual code to emit, defining a global symbol that points to the function code.

-   **Syntax**:
    ```
    FUNCDEF := LINKAGE* 'function' [ABITY] $IDENT '(' (PARAM), ')' [NL] '{' NL BLOCK+ '}'
    PARAM := ABITY %IDENT | 'env' %IDENT | '...'
    ABITY := BASETY | SUBWTY | :IDENT
    ```
-   **Return Type**: Specified before the function name; all return values must match this type. If missing, the function must not return a value.
-   **Parameters**: Comma-separated list of temporary names prefixed by types. Types are used for C compatibility.
-   **Environment Parameter**: Optional `env %e` as the first parameter, a 64-bit integer temporary. Invisible to C callers.
-   **Variadic Functions**: If the parameter list ends with `...`, the function accepts a variable number of arguments.
-   **Examples**:

    ```qbe
    export function w $add_integers(w %a, w %b) {
    @start
        %result =w add %a, %b
        ret %result
    }

    function s $process_struct(:my_struct %p) {
    @start
        %val =w loadw %p # Load the first word from the struct pointer
        ret %val
    }

    export function w $printf_wrapper(env %env_ptr, l %format_str_ptr, ...) {
    @start
        # ...
        ret 0
    }
    ```

### Control Flow

Programs are represented as textual transcriptions of control flow graphs, serialized as blocks of straight-line code connected by jump instructions.

#### Blocks

-   **Syntax**:
    ```
    BLOCK := @IDENT NL ( PHI NL )* ( INST NL )* JUMP NL
    ```
-   All blocks have a name specified by a label (e.g., `@start`).
-   Followed by an optional sequence of phi instructions, then regular instructions, and finally terminated by a jump instruction.
-   The first block in a function must not be the target of any jump.
-   Fall-through: If one block jumps to the next block in the IL file, the jump instruction can be omitted and will be automatically added by the parser.

    ```qbe
    function w $example_flow(w %x) {
    @start
        cmp %x, 0
        jnz %x, @loop, @end # Conditional jump
    @loop
        %x_phi =w phi @start %x, @loop %x_next
        %x_next =w sub %x_phi, 1
        jnz %x_next, @loop, @end # Fall-through to @end if %x_next is zero
    @end
        ret %x_next
    }
    ```

#### Jumps

End every block and transfer control to another program location. The target of a jump must never be the first block in a function.

-   `jmp @IDENT`: Unconditional jump to another block.
-   `jnz VAL, @IDENT, @IDENT`: Conditional jump. If `VAL` is non-zero, jumps to the first label; otherwise, jumps to the second. `VAL` must be of word type (32-bit), though a long argument can be passed (only its least significant 32 bits are compared).
-   `ret [VAL]`: Terminates the function, optionally returning a value matching the function prototype.
-   `hlt`: Terminates program execution with a target-dependent error.

    ```qbe
    @block1
        jmp @block2
    @block2
        %cond =w copy 1
        jnz %cond, @true_block, @false_block
    @true_block
        ret 1
    @false_block
        hlt # Program should not reach here if logic is correct
    ```

### Instructions

Smallest pieces of code, forming the body of blocks. QBE IL uses a three-address code: one instruction computes an operation between two operands and assigns the result to a third one.

-   An instruction has a name and a base return type, defining the size of its result. Argument types are inferred from the instruction name and return type.
-   Type strings describe valid return types, arity, and argument types. For example, `T(T,T)` means the instruction can have any base type (`w`, `l`, `s`, `d`) as return type, and its two arguments must also be of that same type.

#### Arithmetic and Bits

-   `add`, `sub`, `div`, `mul`: `T(T,T)` (available for all base types).
-   `neg`: `T(T)`.
-   `udiv`, `rem`, `urem`: `I(I,I)` (unsigned division, signed and unsigned remainder for integers).
-   `or`, `xor`, `and`: `I(I,I)` (bitwise operations for integers).
-   `sar`, `shr`, `shl`: `I(I,ww)` (arithmetic right shift, logical right shift, logical left shift).

    ```qbe
    %a =w add %x, %y
    %b =d div %f1, %f2
    %c =l and %mask, %value
    %d =w sar %val, 2 # Signed arithmetic right shift by 2 bits
    ```

#### Memory

-   **Store Instructions**: Store a value of any base or extended type.
    -   `stored` -- `(d,m)`
    -   `stores` -- `(s,m)`
    -   `storel` -- `(l,m)`
    -   `storew` -- `(w,m)`
    -   `storeh` -- `(w,m)` (takes a word, stores 16 bits)
    -   `storeb` -- `(w,m)` (takes a word, stores 8 bits)
    -   _Note_: `m` stands for the pointer type (`l` on 64-bit).
-   **Load Instructions**:
    -   `loadd` -- `d(m)`
    -   `loads` -- `s(m)`
    -   `loadl` -- `l(m)`
    -   `loadsw`, `loaduw` -- `I(m)` (signed/unsigned extend word load)
    -   `loadsh`, `loaduh` -- `I(m)` (signed/unsigned extend half-word load)
    -   `loadsb`, `loadub` -- `I(m)` (signed/unsigned extend byte load)
    -   _Note_: `loadw` is syntactic sugar for `loadsw`.
-   **Blits**: `blit` -- `(m,m,w)` (copies in-memory data, third argument is byte count and must be a nonnegative numeric constant).
-   **Stack Allocation**:

    -   `alloc4` -- `m(l)` (allocates memory on stack, 4-byte aligned)
    -   `alloc8` -- `m(l)` (8-byte aligned)
    -   `alloc16` -- `m(l)` (16-byte aligned)

    ```qbe
    %ptr =l alloc8 100 # Allocate 100 bytes on stack, 8-byte aligned
    storew 42, %ptr # Store word 42 at %ptr
    %value =w loaduw %ptr # Load unsigned word from %ptr
    blit %dest_ptr, %src_ptr, 64 # Copy 64 bytes
    ```

#### Comparisons

Return an integer value (word or long): 1 if the comparison is true, 0 otherwise. Names follow `c` + comparison type + operand type suffix.

-   **Integer Comparison Types**: `eq`, `ne`, `sle`, `slt`, `sge`, `sgt`, `ule`, `ult`, `uge`, `ugt`.
-   **Floating Point Comparison Types**: `eq`, `ne`, `le`, `lt`, `ge`, `gt`, `o` (ordered), `uo` (unordered).

    ```qbe
    %is_equal =w ceqw %x, %y # Compare two words for equality
    %is_less =w csltl %a, %b # Compare two longs (signed) for less than
    %is_unordered =w cuod %f1, %f2 # Check if double-precision floats are unordered (NaN)
    ```

#### Conversions

Change value representation; can modify if target type cannot hold source value.

-   `extsw`, `extuw`: `l(w)` (sign/zero extend word to long).
-   `extsh`, `extuh`: `I(ww)` (sign/zero extend half-word).
-   `extsb`, `extub`: `I(ww)` (sign/zero extend byte).
-   `exts`: `d(s)` (extend single to double).
-   `truncd`: `s(d)` (truncate double to single).
-   `stosi`, `stoui`: `I(ss)` (single to signed/unsigned integer).
-   `dtosi`, `dtoui`: `I(dd)` (double to signed/unsigned integer).
-   `swtof`, `uwtof`: `F(ww)` (signed/unsigned word to float).
-   `sltof`, `ultof`: `F(ll)` (signed/unsigned long to float).

    ```qbe
    %extended_byte =l extsb %byte_val # Sign-extend an 8-bit value to a long
    %double_from_single =d exts %single_val
    %int_from_double =w dtosi %double_val
    ```

#### Cast and Copy

Return bits of argument verbatim. `cast` changes integer to float of same width and vice versa. `copy` returns a verbatim copy.

-   `cast`: `wlsd(sdwl)`
-   `copy`: `T(T)`

    ```qbe
    %float_bits =w cast %my_float # Get the raw bits of a float as a word
    %float_from_bits =s cast %my_word_bits # Interpret word bits as a single-precision float
    %duplicate =w copy %original_val
    ```

#### Call

Special instruction for function calls, not a three-address instruction, requires argument types. Return type can be base or aggregate.

-   **Syntax**: `[%IDENT '=' ABITY] 'call' VAL '(' (ARG), ')'`
-   **Arguments**:
    -   `ABITY VAL`: Regular argument.
    -   `env VAL`: Environment argument (first).
    -   `...`: Variadic marker.
-   Aggregate types as arguments or return types pass pointers to memory locations holding the value.
-   Sub-word types for arguments/returns are handled for C compatibility.
-   If the called function doesn't return a value, no return temporary is specified.

    ```qbe
    %ret_val =w call $my_function(w %arg1, l %arg2)
    call $void_function(s %farg)
    %struct_ret_ptr =l call $get_struct_by_value() # Returns a pointer to the returned struct
    call $printf_wrapper(env %my_env, l $format_str_ptr, ... l $arg_int, d $arg_double)
    ```

#### Variadic

Access extra parameters of a variadic function.

-   `vastart` -- `(m)`: Initializes a variable argument list.
-   `vaarg` -- `T(m)`: Fetches the next argument from a variable argument list. Limited to base types.

    ```qbe
    function w $sum_variadic(w %count, ...) {
    @start
        %ap =l alloc8 32 # Allocate space for va_list (e.g., for amd64_sysv)
        vastart %ap
        %sum =w copy 0
        %i =w copy 0
    @loop
        %cond =w csltw %i, %count
        jnz %cond, @next_arg, @end
    @next_arg
        %arg =w vaarg %ap # Fetch next argument
        %sum =w add %sum, %arg
        %i =w add %i, 1
        jmp @loop
    @end
        ret %sum
    }
    ```

#### Phi

Specific to SSA form. Return one of their arguments based on control flow origin. QBE can fix non-SSA programs without requiring phi instructions if preferred, by using stack-allocated variables and memory operations.

-   `PHI := %IDENT '=' BASETY 'phi' ( @IDENT VAL ),`

    ```qbe
    @entry
        %cond =w copy 1
        jnz %cond, @then_block, @else_block
    @then_block
        %val_then =w copy 10
        jmp @merge_block
    @else_block
        %val_else =w copy 20
        jmp @merge_block
    @merge_block
        %result =w phi @then_block %val_then, @else_block %val_else
        ret %result
    ```
