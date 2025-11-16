export const instructions = {
    "Arithmetic and Bits": {
        add: {
            name: "Add",
            description:
                "Performs addition for both integer and floating-point types.",
            args: {
                op1: "Val",
                op2: "Val",
            },
            ret: "Reg",
        },
        sub: {
            name: "Sub",
            description:
                "Performs subtraction for both integer and floating-point types.",
            args: {
                op1: "Val",
                op2: "Val",
            },
            ret: "Reg",
        },
        div: {
            name: "Div",
            description:
                "Performs signed division for integers or standard division for floating-point numbers.",
            args: {
                op1: "Val",
                op2: "Val",
            },
            ret: "Reg",
        },
        mul: {
            name: "Mul",
            description:
                "Performs multiplication for both integer and floating-point types.",
            args: {
                op1: "Val",
                op2: "Val",
            },
            ret: "Reg",
        },
        neg: {
            name: "Neg",
            description:
                "Computes the arithmetic negative of an integer or floating-point value.",
            args: {
                op: "Val",
            },
            ret: "Reg",
        },
        udiv: {
            name: "Udiv",
            description: "Performs unsigned division for integer types.",
            args: {
                op1: "Val",
                op2: "Val",
            },
            ret: "Reg",
        },
        rem: {
            name: "Rem",
            description: "Computes the signed remainder for integer division.",
            args: {
                op1: "Val",
                op2: "Val",
            },
            ret: "Reg",
        },
        urem: {
            name: "Urem",
            description:
                "Computes the unsigned remainder for integer division.",
            args: {
                op1: "Val",
                op2: "Val",
            },
            ret: "Reg",
        },
        or: {
            name: "Or",
            description: "Performs a bitwise OR operation on integer types.",
            args: {
                op1: "Val",
                op2: "Val",
            },
            ret: "Reg",
        },
        xor: {
            name: "Xor",
            description: "Performs a bitwise XOR operation on integer types.",
            args: {
                op1: "Val",
                op2: "Val",
            },
            ret: "Reg",
        },
        and: {
            name: "And",
            description: "Performs a bitwise AND operation on integer types.",
            args: {
                op1: "Val",
                op2: "Val",
            },
            ret: "Reg",
        },
        sar: {
            name: "Sar",
            description:
                "Performs an arithmetic (sign-preserving) right shift on an integer.",
            args: {
                value: "int",
                shiftAmount: "Val",
            },
            ret: "Reg",
        },
        shr: {
            name: "Shr",
            description:
                "Performs a logical (zero-fill) right shift on an integer.",
            args: {
                value: "int",
                shiftAmount: "Val",
            },
            ret: "Reg",
        },
        shl: {
            name: "Shl",
            description:
                "Performs a logical left shift on an integer, filling with zeroes.",
            args: {
                value: "int",
                shiftAmount: "Val",
            },
            ret: "Reg",
        },
    },
    Memory: {
        stored: {
            name: "Stored",
            description: "Stores a double-precision float value into memory.",
            args: {
                value: "int",
                address: "Val",
            },
        },
        stores: {
            name: "Stores",
            description: "Stores a single-precision float value into memory.",
            args: {
                value: "int",
                address: "Val",
            },
        },
        storel: {
            name: "Storel",
            description: "Stores a long (64-bit) integer value into memory.",
            args: {
                value: "string",
                address: "Val",
            },
        },
        storew: {
            name: "Storew",
            description: "Stores a word (32-bit) integer value into memory.",
            args: {
                value: "string",
                address: "Val",
            },
        },
        storeh: {
            name: "Storeh",
            description:
                "Stores the lower 16 bits of a word value into memory.",
            args: {
                value: "int",
                address: "Val",
            },
        },
        storeb: {
            name: "Storeb",
            description: "Stores the lower 8 bits of a word value into memory.",
            args: {
                value: "int",
                address: "Val",
            },
        },
        loadd: {
            name: "Loadd",
            description: "Loads a double-precision float value from memory.",
            args: {
                address: "Val",
            },
            ret: "Reg",
        },
        loads: {
            name: "Loads",
            description: "Loads a single-precision float value from memory.",
            args: {
                address: "Val",
            },
            ret: "Reg",
        },
        loadl: {
            name: "Loadl",
            description: "Loads a long (64-bit) integer value from memory.",
            args: {
                address: "Val",
            },
            ret: "Reg",
        },
        loadsw: {
            name: "Loadsw",
            description: "Loads a word from memory and sign-extends it.",
            args: {
                address: "Val",
            },
            ret: "Reg",
        },
        loaduw: {
            name: "Loaduw",
            description: "Loads a word from memory and zero-extends it.",
            args: {
                address: "Val",
            },
            ret: "Reg",
        },
        loadw: {
            name: "Loadw",
            description:
                "Syntactic sugar to load a word from memory when the extension type is irrelevant.",
            args: {
                address: "Val",
            },
            ret: "Reg",
        },
        loadsh: {
            name: "Loadsh",
            description:
                "Loads a half-word (16-bit) from memory and sign-extends it.",
            args: {
                address: "Val",
            },
            ret: "Reg",
        },
        loaduh: {
            name: "Loaduh",
            description:
                "Loads a half-word (16-bit) from memory and zero-extends it.",
            args: {
                address: "Val",
            },
            ret: "Reg",
        },
        loadsb: {
            name: "Loadsb",
            description:
                "Loads a byte (8-bit) from memory and sign-extends it.",
            args: {
                address: "Val",
            },
            ret: "Reg",
        },
        loadub: {
            name: "Loadub",
            description:
                "Loads a byte (8-bit) from memory and zero-extends it.",
            args: {
                address: "Val",
            },
            ret: "Reg",
        },
        blit: {
            name: "Blit",
            description:
                "Copies a constant number of bytes from a source memory address to a destination address.",
            args: {
                srcAddress: "Val",
                destAddress: "Val",
                byteCount: "int",
            },
            ret: "Reg",
        },
        alloc4: {
            name: "Alloc4",
            description: "Allocates memory on the stack with 4-byte alignment.",
            args: {
                sizeInBytes: "int",
            },
            ret: "Reg",
        },
        alloc8: {
            name: "Alloc8",
            description: "Allocates memory on the stack with 8-byte alignment.",
            args: {
                sizeInBytes: "int",
            },
            ret: "Reg",
        },
        alloc16: {
            name: "Alloc16",
            description:
                "Allocates memory on the stack with 16-byte alignment.",
            args: {
                sizeInBytes: "int",
            },
            ret: "Reg",
        },
    },
    Comparisons: {
        ceqd: {
            name: "Ceqd",
            description: "Compares two double-precision floats for equality.",
            args: {
                op1: "Val",
                op2: "Val",
            },
            ret: "Reg",
        },
        ceql: {
            name: "Ceql",
            description: "Compares two long integers for equality.",
            args: {
                op1: "Val",
                op2: "Val",
            },
            ret: "Reg",
        },
        ceqs: {
            name: "Ceqs",
            description: "Compares two single-precision floats for equality.",
            args: {
                op1: "Val",
                op2: "Val",
            },
            ret: "Reg",
        },
        ceqw: {
            name: "Ceqw",
            description: "Compares two word integers for equality.",
            args: {
                op1: "Val",
                op2: "Val",
            },
            ret: "Reg",
        },
        cged: {
            name: "Cged",
            description:
                "Compares if the first double is greater than or equal to the second.",
            args: {
                op1: "Val",
                op2: "Val",
            },
            ret: "Reg",
        },
        cges: {
            name: "Cges",
            description:
                "Compares if the first single is greater than or equal to the second.",
            args: {
                op1: "Val",
                op2: "Val",
            },
            ret: "Reg",
        },
        cgtd: {
            name: "Cgtd",
            description:
                "Compares if the first double is greater than the second.",
            args: {
                op1: "Val",
                op2: "Val",
            },
            ret: "Reg",
        },
        cgts: {
            name: "Cgts",
            description:
                "Compares if the first single is greater than the second.",
            args: {
                op1: "Val",
                op2: "Val",
            },
            ret: "Reg",
        },
        cled: {
            name: "Cled",
            description:
                "Compares if the first double is lower than or equal to the second.",
            args: {
                op1: "Val",
                op2: "Val",
            },
            ret: "Reg",
        },
        cles: {
            name: "Cles",
            description:
                "Compares if the first single is lower than or equal to the second.",
            args: {
                op1: "Val",
                op2: "Val",
            },
            ret: "Reg",
        },
        cltd: {
            name: "Cltd",
            description:
                "Compares if the first double is lower than the second.",
            args: {
                op1: "Val",
                op2: "Val",
            },
            ret: "Reg",
        },
        clts: {
            name: "Clts",
            description:
                "Compares if the first single is lower than the second.",
            args: {
                op1: "Val",
                op2: "Val",
            },
            ret: "Reg",
        },
        cned: {
            name: "Cned",
            description: "Compares two double-precision floats for inequality.",
            args: {
                op1: "Val",
                op2: "Val",
            },
            ret: "Reg",
        },
        cnel: {
            name: "Cnel",
            description: "Compares two long integers for inequality.",
            args: {
                op1: "Val",
                op2: "Val",
            },
            ret: "Reg",
        },
        cnes: {
            name: "Cnes",
            description: "Compares two single-precision floats for inequality.",
            args: {
                op1: "Val",
                op2: "Val",
            },
        },
        cnew: {
            name: "Cnew",
            description: "Compares two word integers for inequality.",
            args: {
                op1: "Val",
                op2: "Val",
            },
            ret: "Reg",
        },
        cod: {
            name: "Cod",
            description:
                "Checks if two double-precision floats are ordered (neither is NaN).",
            args: {
                op1: "Val",
                op2: "Val",
            },
            ret: "Reg",
        },
        cos: {
            name: "Cos",
            description:
                "Checks if two single-precision floats are ordered (neither is NaN).",
            args: {
                op1: "Val",
                op2: "Val",
            },
            ret: "Reg",
        },
        csgel: {
            name: "Csgel",
            description:
                "Compares if the first signed long is greater than or equal to the second.",
            args: {
                op1: "Val",
                op2: "Val",
            },
            ret: "Reg",
        },
        csgew: {
            name: "Csgew",
            description:
                "Compares if the first signed word is greater than or equal to the second.",
            args: {
                op1: "Val",
                op2: "Val",
            },
            ret: "Reg",
        },
        csgtl: {
            name: "Csgtl",
            description:
                "Compares if the first signed long is greater than the second.",
            args: {
                op1: "Val",
                op2: "Val",
            },
            ret: "Reg",
        },
        csgtw: {
            name: "Csgtw",
            description:
                "Compares if the first signed word is greater than the second.",
            args: {
                op1: "Val",
                op2: "Val",
            },
            ret: "Reg",
        },
        cslel: {
            name: "Cslel",
            description:
                "Compares if the first signed long is lower than or equal to the second.",
            args: {
                op1: "Val",
                op2: "Val",
            },
            ret: "Reg",
        },
        cslew: {
            name: "Cslew",
            description:
                "Compares if the first signed word is lower than or equal to the second.",
            args: {
                op1: "Val",
                op2: "Val",
            },
            ret: "Reg",
        },
        csltl: {
            name: "Csltl",
            description:
                "Compares if the first signed long is lower than the second.",
            args: {
                op1: "Val",
                op2: "Val",
            },
            ret: "Reg",
        },
        csltw: {
            name: "Csltw",
            description:
                "Compares if the first signed word is lower than the second.",
            args: {
                op1: "Val",
                op2: "Val",
            },
            ret: "Reg",
        },
        cugel: {
            name: "Cugel",
            description:
                "Compares if the first unsigned long is greater than or equal to the second.",
            args: {
                op1: "Val",
                op2: "Val",
            },
            ret: "Reg",
        },
        cugew: {
            name: "Cugew",
            description:
                "Compares if the first unsigned word is greater than or equal to the second.",
            args: {
                op1: "Val",
                op2: "Val",
            },
            ret: "Reg",
        },
        cugtl: {
            name: "Cugtl",
            description:
                "Compares if the first unsigned long is greater than the second.",
            args: {
                op1: "Val",
                op2: "Val",
            },
            ret: "Reg",
        },
        cugtw: {
            name: "Cugtw",
            description:
                "Compares if the first unsigned word is greater than the second.",
            args: {
                op1: "Val",
                op2: "Val",
            },
            ret: "Reg",
        },
        culel: {
            name: "Culel",
            description:
                "Compares if the first unsigned long is lower than or equal to the second.",
            args: {
                op1: "Val",
                op2: "Val",
            },
            ret: "Reg",
        },
        culew: {
            name: "Culew",
            description:
                "Compares if the first unsigned word is lower than or equal to the second.",
            args: {
                op1: "Val",
                op2: "Val",
            },
            ret: "Reg",
        },
        cultl: {
            name: "Cultl",
            description:
                "Compares if the first unsigned long is lower than the second.",
            args: {
                op1: "Val",
                op2: "Val",
            },
            ret: "Reg",
        },
        cultw: {
            name: "Cultw",
            description:
                "Compares if the first unsigned word is lower than the second.",
            args: {
                op1: "Val",
                op2: "Val",
            },
            ret: "Reg",
        },
        cuod: {
            name: "Cuod",
            description:
                "Checks if two double-precision floats are unordered (at least one is NaN).",
            args: {
                op1: "Val",
                op2: "Val",
            },
            ret: "Reg",
        },
        cuos: {
            name: "Cuos",
            description:
                "Checks if two single-precision floats are unordered (at least one is NaN).",
            args: {
                op1: "Val",
                op2: "Val",
            },
            ret: "Reg",
        },
    },
    "Conversions, Cast, and Copy": {
        dtosi: {
            name: "Dtosi",
            description:
                "Converts a double-precision float to a signed integer.",
            args: {
                value: "int",
            },
        },
        dtoui: {
            name: "Dtoui",
            description:
                "Converts a double-precision float to an unsigned integer.",
            args: {
                value: "int",
            },
        },
        exts: {
            name: "Exts",
            description:
                "Extends a single-precision float to a double-precision float.",
            args: {
                value: "int",
            },
        },
        extsb: {
            name: "Extsb",
            description:
                "Sign-extends the 8 least-significant bits of a word to a larger integer type.",
            args: {
                value: "int",
            },
        },
        extsh: {
            name: "Extsh",
            description:
                "Sign-extends the 16 least-significant bits of a word to a larger integer type.",
            args: {
                value: "int",
            },
        },
        extsw: {
            name: "Extsw",
            description: "Sign-extends a word to a long.",
            args: {
                value: "int",
            },
        },
        extub: {
            name: "Extub",
            description:
                "Zero-extends the 8 least-significant bits of a word to a larger integer type.",
            args: {
                value: "int",
            },
        },
        extuh: {
            name: "Extuh",
            description:
                "Zero-extends the 16 least-significant bits of a word to a larger integer type.",
            args: {
                value: "int",
            },
        },
        extuw: {
            name: "Extuw",
            description: "Zero-extends a word to a long.",
            args: {
                value: "Val",
            },
            ret: "Reg",
            retSize: "l",
        },
        sltof: {
            name: "Sltof",
            description:
                "Converts a signed long integer to a floating-point number.",
            args: {
                value: "int",
            },
        },
        ultof: {
            name: "Ultof",
            description:
                "Converts an unsigned long integer to a floating-point number.",
            args: {
                value: "int",
            },
        },
        stosi: {
            name: "Stosi",
            description:
                "Converts a single-precision float to a signed integer.",
            args: {
                value: "int",
            },
        },
        stoui: {
            name: "Stoui",
            description:
                "Converts a single-precision float to an unsigned integer.",
            args: {
                value: "int",
            },
        },
        swtof: {
            name: "Swtof",
            description:
                "Converts a signed word integer to a floating-point number.",
            args: {
                value: "int",
            },
        },
        uwtof: {
            name: "Uwtof",
            description:
                "Converts an unsigned word integer to a floating-point number.",
            args: {
                value: "int",
            },
        },
        truncd: {
            name: "Truncd",
            description:
                "Truncates a double-precision float to a single-precision float.",
            args: {
                value: "int",
            },
        },
        cast: {
            name: "Cast",
            description:
                "Changes a value's type (e.g., int to float) without altering its underlying bits.",
            args: {
                value: "int",
            },
        },
        copy: {
            name: "Copy",
            description:
                "Returns the bits of its argument verbatim in a new temporary of the same type.",
            args: {
                value: "int",
            },
        },
    },
    "Control Flow and Functions": {
        call: {
            name: "Call",
            description:
                "Calls a function with a specified list of arguments and types.",
            args: {
                functionSymbol: "FunctionSymbol",
                arguments: "params string[]",
            },
            overrideBody:
                "Write($\"call ${functionSymbol}({string.Join(',', arguments)})\");",
        },
        vastart: {
            name: "VaStart",
            description:
                "Initializes a list to access the variable arguments of a variadic function.",
            args: {
                variableArgumentListPtr: "Val",
            },
        },
        vaarg: {
            name: "VaArg",
            description:
                "Fetches the next argument from a variable argument list.",
            args: {
                variableArgumentListPtr: "Val",
            },
        },
        phi: {
            name: "Phi",
            description:
                "Selects a value based on the preceding control flow block, for use in SSA form.",
            args: {
                // TODO:
            },
        },
        hlt: {
            name: "Hlt",
            description:
                "Halts program execution with a target-dependent error.",
            args: {},
        },
        jmp: {
            name: "Jmp",
            description:
                "Jumps unconditionally to another block within the same function.",
            args: {
                targetLabel: "Label",
            },
        },
        jnz: {
            name: "Jnz",
            description:
                "Jumps conditionally to one of two blocks based on whether a value is non-zero.",
            args: {
                conditionVal: "Val",
                trueLabel: "Label",
                falseLabel: "Label",
            },
        },
        ret: {
            name: "Ret",
            description:
                "Returns from the current function, optionally passing a value to the caller.",
            args: {
                returnValue: "int?",
            },
        },
    },
} as any;
