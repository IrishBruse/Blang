export default {
    "Arithmetic and Bits": {
        add: {
            description:
                "Performs addition for both integer and floating-point types.",
            args: {
                a: "int",
                b: "int",
            },
        },
        sub: {
            description:
                "Performs subtraction for both integer and floating-point types.",
        },
        div: {
            description:
                "Performs signed division for integers or standard division for floating-point numbers.",
        },
        mul: {
            description:
                "Performs multiplication for both integer and floating-point types.",
        },
        neg: {
            description:
                "Computes the arithmetic negative of an integer or floating-point value.",
        },
        udiv: {
            description: "Performs unsigned division for integer types.",
        },
        rem: {
            description: "Computes the signed remainder for integer division.",
        },
        urem: {
            description:
                "Computes the unsigned remainder for integer division.",
        },
        or: {
            description: "Performs a bitwise OR operation on integer types.",
        },
        xor: {
            description: "Performs a bitwise XOR operation on integer types.",
        },
        and: {
            description: "Performs a bitwise AND operation on integer types.",
        },
        sar: {
            description:
                "Performs an arithmetic (sign-preserving) right shift on an integer.",
        },
        shr: {
            description:
                "Performs a logical (zero-fill) right shift on an integer.",
        },
        shl: {
            description:
                "Performs a logical left shift on an integer, filling with zeroes.",
        },
    },
    Memory: {
        stored: {
            description: "Stores a double-precision float value into memory.",
        },
        stores: {
            description: "Stores a single-precision float value into memory.",
        },
        storel: {
            description: "Stores a long (64-bit) integer value into memory.",
        },
        storew: {
            description: "Stores a word (32-bit) integer value into memory.",
            args: {
                val: "int",
                address: "string",
            },
        },
        storeh: {
            description:
                "Stores the lower 16 bits of a word value into memory.",
        },
        storeb: {
            description: "Stores the lower 8 bits of a word value into memory.",
        },
        loadd: {
            description: "Loads a double-precision float value from memory.",
        },
        loads: {
            description: "Loads a single-precision float value from memory.",
        },
        loadl: {
            description: "Loads a long (64-bit) integer value from memory.",
        },
        loadsw: {
            description: "Loads a word from memory and sign-extends it.",
        },
        loaduw: {
            description: "Loads a word from memory and zero-extends it.",
        },
        loadw: {
            description:
                "Syntactic sugar to load a word from memory when the extension type is irrelevant.",
        },
        loadsh: {
            description:
                "Loads a half-word (16-bit) from memory and sign-extends it.",
        },
        loaduh: {
            description:
                "Loads a half-word (16-bit) from memory and zero-extends it.",
        },
        loadsb: {
            description:
                "Loads a byte (8-bit) from memory and sign-extends it.",
        },
        loadub: {
            description:
                "Loads a byte (8-bit) from memory and zero-extends it.",
        },
        blit: {
            description:
                "Copies a constant number of bytes from a source memory address to a destination address.",
        },
        alloc4: {
            description: "Allocates memory on the stack with 4-byte alignment.",
        },
        alloc8: {
            description: "Allocates memory on the stack with 8-byte alignment.",
        },
        alloc16: {
            description:
                "Allocates memory on the stack with 16-byte alignment.",
        },
    },
    Comparisons: {
        ceqd: {
            description: "Compares two double-precision floats for equality.",
        },
        ceql: {
            description: "Compares two long integers for equality.",
        },
        ceqs: {
            description: "Compares two single-precision floats for equality.",
        },
        ceqw: {
            description: "Compares two word integers for equality.",
        },
        cged: {
            description:
                "Compares if the first double is greater than or equal to the second.",
        },
        cges: {
            description:
                "Compares if the first single is greater than or equal to the second.",
        },
        cgtd: {
            description:
                "Compares if the first double is greater than the second.",
        },
        cgts: {
            description:
                "Compares if the first single is greater than the second.",
        },
        cled: {
            description:
                "Compares if the first double is lower than or equal to the second.",
        },
        cles: {
            description:
                "Compares if the first single is lower than or equal to the second.",
        },
        cltd: {
            description:
                "Compares if the first double is lower than the second.",
        },
        clts: {
            description:
                "Compares if the first single is lower than the second.",
        },
        cned: {
            description: "Compares two double-precision floats for inequality.",
        },
        cnel: {
            description: "Compares two long integers for inequality.",
        },
        cnes: {
            description: "Compares two single-precision floats for inequality.",
        },
        cnew: {
            description: "Compares two word integers for inequality.",
        },
        cod: {
            description:
                "Checks if two double-precision floats are ordered (neither is NaN).",
        },
        cos: {
            description:
                "Checks if two single-precision floats are ordered (neither is NaN).",
        },
        csgel: {
            description:
                "Compares if the first signed long is greater than or equal to the second.",
        },
        csgew: {
            description:
                "Compares if the first signed word is greater than or equal to the second.",
        },
        csgtl: {
            description:
                "Compares if the first signed long is greater than the second.",
        },
        csgtw: {
            description:
                "Compares if the first signed word is greater than the second.",
        },
        cslel: {
            description:
                "Compares if the first signed long is lower than or equal to the second.",
        },
        cslew: {
            description:
                "Compares if the first signed word is lower than or equal to the second.",
        },
        csltl: {
            description:
                "Compares if the first signed long is lower than the second.",
        },
        csltw: {
            description:
                "Compares if the first signed word is lower than the second.",
        },
        cugel: {
            description:
                "Compares if the first unsigned long is greater than or equal to the second.",
        },
        cugew: {
            description:
                "Compares if the first unsigned word is greater than or equal to the second.",
        },
        cugtl: {
            description:
                "Compares if the first unsigned long is greater than the second.",
        },
        cugtw: {
            description:
                "Compares if the first unsigned word is greater than the second.",
        },
        culel: {
            description:
                "Compares if the first unsigned long is lower than or equal to the second.",
        },
        culew: {
            description:
                "Compares if the first unsigned word is lower than or equal to the second.",
        },
        cultl: {
            description:
                "Compares if the first unsigned long is lower than the second.",
        },
        cultw: {
            description:
                "Compares if the first unsigned word is lower than the second.",
        },
        cuod: {
            description:
                "Checks if two double-precision floats are unordered (at least one is NaN).",
        },
        cuos: {
            description:
                "Checks if two single-precision floats are unordered (at least one is NaN).",
        },
    },
    "Conversions, Cast, and Copy": {
        dtosi: {
            description:
                "Converts a double-precision float to a signed integer.",
        },
        dtoui: {
            description:
                "Converts a double-precision float to an unsigned integer.",
        },
        exts: {
            description:
                "Extends a single-precision float to a double-precision float.",
        },
        extsb: {
            description:
                "Sign-extends the 8 least-significant bits of a word to a larger integer type.",
        },
        extsh: {
            description:
                "Sign-extends the 16 least-significant bits of a word to a larger integer type.",
        },
        extsw: {
            description: "Sign-extends a word to a long.",
        },
        extub: {
            description:
                "Zero-extends the 8 least-significant bits of a word to a larger integer type.",
        },
        extuh: {
            description:
                "Zero-extends the 16 least-significant bits of a word to a larger integer type.",
        },
        extuw: {
            description: "Zero-extends a word to a long.",
        },
        sltof: {
            description:
                "Converts a signed long integer to a floating-point number.",
        },
        ultof: {
            description:
                "Converts an unsigned long integer to a floating-point number.",
        },
        stosi: {
            description:
                "Converts a single-precision float to a signed integer.",
        },
        stoui: {
            description:
                "Converts a single-precision float to an unsigned integer.",
        },
        swtof: {
            description:
                "Converts a signed word integer to a floating-point number.",
        },
        uwtof: {
            description:
                "Converts an unsigned word integer to a floating-point number.",
        },
        truncd: {
            description:
                "Truncates a double-precision float to a single-precision float.",
        },
        cast: {
            description:
                "Changes a value's type (e.g., int to float) without altering its underlying bits.",
        },
        copy: {
            description:
                "Returns the bits of its argument verbatim in a new temporary of the same type.",
        },
    },
    "Control Flow and Functions": {
        call: {
            description:
                "Calls a function with a specified list of arguments and types.",
        },
        vastart: {
            description:
                "Initializes a list to access the variable arguments of a variadic function.",
        },
        vaarg: {
            description:
                "Fetches the next argument from a variable argument list.",
        },
        phi: {
            description:
                "Selects a value based on the preceding control flow block, for use in SSA form.",
        },
        hlt: {
            description:
                "Halts program execution with a target-dependent error.",
        },
        jmp: {
            description:
                "Jumps unconditionally to another block within the same function.",
            args: {
                label: "string",
            },
        },
        jnz: {
            description:
                "Jumps conditionally to one of two blocks based on whether a value is non-zero.",
        },
        ret: {
            description:
                "Returns from the current function, optionally passing a value to the caller.",
        },
    },
} as any;
