namespace BLang.Targets.qbe;

public partial class QbeOutput
{
    // Arithmetic and Bits

    /// <summary> Performs addition for both integer and floating-point types. </summary>
    public void Add(int a, int b)
    {
        Write($"add {a}, {b}");
    }

    /// <summary> Performs subtraction for both integer and floating-point types. </summary>
    public void Sub()
    {
        Write($"sub ");
    }

    /// <summary> Performs signed division for integers or standard division for floating-point numbers. </summary>
    public void Div()
    {
        Write($"div ");
    }

    /// <summary> Performs multiplication for both integer and floating-point types. </summary>
    public void Mul()
    {
        Write($"mul ");
    }

    /// <summary> Computes the arithmetic negative of an integer or floating-point value. </summary>
    public void Neg()
    {
        Write($"neg ");
    }

    /// <summary> Performs unsigned division for integer types. </summary>
    public void Udiv()
    {
        Write($"udiv ");
    }

    /// <summary> Computes the signed remainder for integer division. </summary>
    public void Rem()
    {
        Write($"rem ");
    }

    /// <summary> Computes the unsigned remainder for integer division. </summary>
    public void Urem()
    {
        Write($"urem ");
    }

    /// <summary> Performs a bitwise OR operation on integer types. </summary>
    public void Or()
    {
        Write($"or ");
    }

    /// <summary> Performs a bitwise XOR operation on integer types. </summary>
    public void Xor()
    {
        Write($"xor ");
    }

    /// <summary> Performs a bitwise AND operation on integer types. </summary>
    public void And()
    {
        Write($"and ");
    }

    /// <summary> Performs an arithmetic (sign-preserving) right shift on an integer. </summary>
    public void Sar()
    {
        Write($"sar ");
    }

    /// <summary> Performs a logical (zero-fill) right shift on an integer. </summary>
    public void Shr()
    {
        Write($"shr ");
    }

    /// <summary> Performs a logical left shift on an integer, filling with zeroes. </summary>
    public void Shl()
    {
        Write($"shl ");
    }

    // Memory

    /// <summary> Stores a double-precision float value into memory. </summary>
    public void Stored()
    {
        Write($"stored ");
    }

    /// <summary> Stores a single-precision float value into memory. </summary>
    public void Stores()
    {
        Write($"stores ");
    }

    /// <summary> Stores a long (64-bit) integer value into memory. </summary>
    public void Storel()
    {
        Write($"storel ");
    }

    /// <summary> Stores a word (32-bit) integer value into memory. </summary>
    public void Storew(int val, string address)
    {
        Write($"storew {val}, {address}");
    }

    /// <summary> Stores the lower 16 bits of a word value into memory. </summary>
    public void Storeh()
    {
        Write($"storeh ");
    }

    /// <summary> Stores the lower 8 bits of a word value into memory. </summary>
    public void Storeb()
    {
        Write($"storeb ");
    }

    /// <summary> Loads a double-precision float value from memory. </summary>
    public void Loadd()
    {
        Write($"loadd ");
    }

    /// <summary> Loads a single-precision float value from memory. </summary>
    public void Loads()
    {
        Write($"loads ");
    }

    /// <summary> Loads a long (64-bit) integer value from memory. </summary>
    public void Loadl()
    {
        Write($"loadl ");
    }

    /// <summary> Loads a word from memory and sign-extends it. </summary>
    public void Loadsw()
    {
        Write($"loadsw ");
    }

    /// <summary> Loads a word from memory and zero-extends it. </summary>
    public void Loaduw()
    {
        Write($"loaduw ");
    }

    /// <summary> Syntactic sugar to load a word from memory when the extension type is irrelevant. </summary>
    public void Loadw()
    {
        Write($"loadw ");
    }

    /// <summary> Loads a half-word (16-bit) from memory and sign-extends it. </summary>
    public void Loadsh()
    {
        Write($"loadsh ");
    }

    /// <summary> Loads a half-word (16-bit) from memory and zero-extends it. </summary>
    public void Loaduh()
    {
        Write($"loaduh ");
    }

    /// <summary> Loads a byte (8-bit) from memory and sign-extends it. </summary>
    public void Loadsb()
    {
        Write($"loadsb ");
    }

    /// <summary> Loads a byte (8-bit) from memory and zero-extends it. </summary>
    public void Loadub()
    {
        Write($"loadub ");
    }

    /// <summary> Copies a constant number of bytes from a source memory address to a destination address. </summary>
    public void Blit()
    {
        Write($"blit ");
    }

    /// <summary> Allocates memory on the stack with 4-byte alignment. </summary>
    public void Alloc4()
    {
        Write($"alloc4 ");
    }

    /// <summary> Allocates memory on the stack with 8-byte alignment. </summary>
    public void Alloc8()
    {
        Write($"alloc8 ");
    }

    /// <summary> Allocates memory on the stack with 16-byte alignment. </summary>
    public void Alloc16()
    {
        Write($"alloc16 ");
    }

    // Comparisons

    /// <summary> Compares two double-precision floats for equality. </summary>
    public void Ceqd()
    {
        Write($"ceqd ");
    }

    /// <summary> Compares two long integers for equality. </summary>
    public void Ceql()
    {
        Write($"ceql ");
    }

    /// <summary> Compares two single-precision floats for equality. </summary>
    public void Ceqs()
    {
        Write($"ceqs ");
    }

    /// <summary> Compares two word integers for equality. </summary>
    public void Ceqw()
    {
        Write($"ceqw ");
    }

    /// <summary> Compares if the first double is greater than or equal to the second. </summary>
    public void Cged()
    {
        Write($"cged ");
    }

    /// <summary> Compares if the first single is greater than or equal to the second. </summary>
    public void Cges()
    {
        Write($"cges ");
    }

    /// <summary> Compares if the first double is greater than the second. </summary>
    public void Cgtd()
    {
        Write($"cgtd ");
    }

    /// <summary> Compares if the first single is greater than the second. </summary>
    public void Cgts()
    {
        Write($"cgts ");
    }

    /// <summary> Compares if the first double is lower than or equal to the second. </summary>
    public void Cled()
    {
        Write($"cled ");
    }

    /// <summary> Compares if the first single is lower than or equal to the second. </summary>
    public void Cles()
    {
        Write($"cles ");
    }

    /// <summary> Compares if the first double is lower than the second. </summary>
    public void Cltd()
    {
        Write($"cltd ");
    }

    /// <summary> Compares if the first single is lower than the second. </summary>
    public void Clts()
    {
        Write($"clts ");
    }

    /// <summary> Compares two double-precision floats for inequality. </summary>
    public void Cned()
    {
        Write($"cned ");
    }

    /// <summary> Compares two long integers for inequality. </summary>
    public void Cnel()
    {
        Write($"cnel ");
    }

    /// <summary> Compares two single-precision floats for inequality. </summary>
    public void Cnes()
    {
        Write($"cnes ");
    }

    /// <summary> Compares two word integers for inequality. </summary>
    public void Cnew()
    {
        Write($"cnew ");
    }

    /// <summary> Checks if two double-precision floats are ordered (neither is NaN). </summary>
    public void Cod()
    {
        Write($"cod ");
    }

    /// <summary> Checks if two single-precision floats are ordered (neither is NaN). </summary>
    public void Cos()
    {
        Write($"cos ");
    }

    /// <summary> Compares if the first signed long is greater than or equal to the second. </summary>
    public void Csgel()
    {
        Write($"csgel ");
    }

    /// <summary> Compares if the first signed word is greater than or equal to the second. </summary>
    public void Csgew()
    {
        Write($"csgew ");
    }

    /// <summary> Compares if the first signed long is greater than the second. </summary>
    public void Csgtl()
    {
        Write($"csgtl ");
    }

    /// <summary> Compares if the first signed word is greater than the second. </summary>
    public void Csgtw()
    {
        Write($"csgtw ");
    }

    /// <summary> Compares if the first signed long is lower than or equal to the second. </summary>
    public void Cslel()
    {
        Write($"cslel ");
    }

    /// <summary> Compares if the first signed word is lower than or equal to the second. </summary>
    public void Cslew()
    {
        Write($"cslew ");
    }

    /// <summary> Compares if the first signed long is lower than the second. </summary>
    public void Csltl()
    {
        Write($"csltl ");
    }

    /// <summary> Compares if the first signed word is lower than the second. </summary>
    public void Csltw()
    {
        Write($"csltw ");
    }

    /// <summary> Compares if the first unsigned long is greater than or equal to the second. </summary>
    public void Cugel()
    {
        Write($"cugel ");
    }

    /// <summary> Compares if the first unsigned word is greater than or equal to the second. </summary>
    public void Cugew()
    {
        Write($"cugew ");
    }

    /// <summary> Compares if the first unsigned long is greater than the second. </summary>
    public void Cugtl()
    {
        Write($"cugtl ");
    }

    /// <summary> Compares if the first unsigned word is greater than the second. </summary>
    public void Cugtw()
    {
        Write($"cugtw ");
    }

    /// <summary> Compares if the first unsigned long is lower than or equal to the second. </summary>
    public void Culel()
    {
        Write($"culel ");
    }

    /// <summary> Compares if the first unsigned word is lower than or equal to the second. </summary>
    public void Culew()
    {
        Write($"culew ");
    }

    /// <summary> Compares if the first unsigned long is lower than the second. </summary>
    public void Cultl()
    {
        Write($"cultl ");
    }

    /// <summary> Compares if the first unsigned word is lower than the second. </summary>
    public void Cultw()
    {
        Write($"cultw ");
    }

    /// <summary> Checks if two double-precision floats are unordered (at least one is NaN). </summary>
    public void Cuod()
    {
        Write($"cuod ");
    }

    /// <summary> Checks if two single-precision floats are unordered (at least one is NaN). </summary>
    public void Cuos()
    {
        Write($"cuos ");
    }

    // Conversions, Cast, and Copy

    /// <summary> Converts a double-precision float to a signed integer. </summary>
    public void Dtosi()
    {
        Write($"dtosi ");
    }

    /// <summary> Converts a double-precision float to an unsigned integer. </summary>
    public void Dtoui()
    {
        Write($"dtoui ");
    }

    /// <summary> Extends a single-precision float to a double-precision float. </summary>
    public void Exts()
    {
        Write($"exts ");
    }

    /// <summary> Sign-extends the 8 least-significant bits of a word to a larger integer type. </summary>
    public void Extsb()
    {
        Write($"extsb ");
    }

    /// <summary> Sign-extends the 16 least-significant bits of a word to a larger integer type. </summary>
    public void Extsh()
    {
        Write($"extsh ");
    }

    /// <summary> Sign-extends a word to a long. </summary>
    public void Extsw()
    {
        Write($"extsw ");
    }

    /// <summary> Zero-extends the 8 least-significant bits of a word to a larger integer type. </summary>
    public void Extub()
    {
        Write($"extub ");
    }

    /// <summary> Zero-extends the 16 least-significant bits of a word to a larger integer type. </summary>
    public void Extuh()
    {
        Write($"extuh ");
    }

    /// <summary> Zero-extends a word to a long. </summary>
    public void Extuw()
    {
        Write($"extuw ");
    }

    /// <summary> Converts a signed long integer to a floating-point number. </summary>
    public void Sltof()
    {
        Write($"sltof ");
    }

    /// <summary> Converts an unsigned long integer to a floating-point number. </summary>
    public void Ultof()
    {
        Write($"ultof ");
    }

    /// <summary> Converts a single-precision float to a signed integer. </summary>
    public void Stosi()
    {
        Write($"stosi ");
    }

    /// <summary> Converts a single-precision float to an unsigned integer. </summary>
    public void Stoui()
    {
        Write($"stoui ");
    }

    /// <summary> Converts a signed word integer to a floating-point number. </summary>
    public void Swtof()
    {
        Write($"swtof ");
    }

    /// <summary> Converts an unsigned word integer to a floating-point number. </summary>
    public void Uwtof()
    {
        Write($"uwtof ");
    }

    /// <summary> Truncates a double-precision float to a single-precision float. </summary>
    public void Truncd()
    {
        Write($"truncd ");
    }

    /// <summary> Changes a value's type (e.g., int to float) without altering its underlying bits. </summary>
    public void Cast()
    {
        Write($"cast ");
    }

    /// <summary> Returns the bits of its argument verbatim in a new temporary of the same type. </summary>
    public void Copy()
    {
        Write($"copy ");
    }

    // Control Flow and Functions

    /// <summary> Calls a function with a specified list of arguments and types. </summary>
    public void Call()
    {
        Write($"call ");
    }

    /// <summary> Initializes a list to access the variable arguments of a variadic function. </summary>
    public void Vastart()
    {
        Write($"vastart ");
    }

    /// <summary> Fetches the next argument from a variable argument list. </summary>
    public void Vaarg()
    {
        Write($"vaarg ");
    }

    /// <summary> Selects a value based on the preceding control flow block, for use in SSA form. </summary>
    public void Phi()
    {
        Write($"phi ");
    }

    /// <summary> Halts program execution with a target-dependent error. </summary>
    public void Hlt()
    {
        Write($"hlt ");
    }

    /// <summary> Jumps unconditionally to another block within the same function. </summary>
    public void Jmp(string label)
    {
        Write($"jmp {label}");
    }

    /// <summary> Jumps conditionally to one of two blocks based on whether a value is non-zero. </summary>
    public void Jnz()
    {
        Write($"jnz ");
    }

    /// <summary> Returns from the current function, optionally passing a value to the caller. </summary>
    public void Ret()
    {
        Write($"ret ");
    }

}
