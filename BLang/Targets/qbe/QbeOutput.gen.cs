namespace BLang.Targets.qbe;

using Val = string;
using Reg = string;
using Label = string;
using FunctionSymbol = string;

public partial class QbeOutput
{
    // Arithmetic and Bits

    /// <summary> Performs addition for both integer and floating-point types. </summary>
    public Reg Add(Val op1, Val op2)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w add {op1}, {op2}");
        return reg;
    }

    /// <summary> Performs subtraction for both integer and floating-point types. </summary>
    public Reg Sub(Val op1, Val op2)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w sub {op1}, {op2}");
        return reg;
    }

    /// <summary> Performs signed division for integers or standard division for floating-point numbers. </summary>
    public Reg Div(Val op1, Val op2)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w div {op1}, {op2}");
        return reg;
    }

    /// <summary> Performs multiplication for both integer and floating-point types. </summary>
    public Reg Mul(Val op1, Val op2)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w mul {op1}, {op2}");
        return reg;
    }

    /// <summary> Computes the arithmetic negative of an integer or floating-point value. </summary>
    public Reg Neg(Val op)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w neg {op}");
        return reg;
    }

    /// <summary> Performs unsigned division for integer types. </summary>
    public Reg Udiv(Val op1, Val op2)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w udiv {op1}, {op2}");
        return reg;
    }

    /// <summary> Computes the signed remainder for integer division. </summary>
    public Reg Rem(Val op1, Val op2)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w rem {op1}, {op2}");
        return reg;
    }

    /// <summary> Computes the unsigned remainder for integer division. </summary>
    public Reg Urem(Val op1, Val op2)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w urem {op1}, {op2}");
        return reg;
    }

    /// <summary> Performs a bitwise OR operation on integer types. </summary>
    public Reg Or(Val op1, Val op2)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w or {op1}, {op2}");
        return reg;
    }

    /// <summary> Performs a bitwise XOR operation on integer types. </summary>
    public Reg Xor(Val op1, Val op2)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w xor {op1}, {op2}");
        return reg;
    }

    /// <summary> Performs a bitwise AND operation on integer types. </summary>
    public Reg And(Val op1, Val op2)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w and {op1}, {op2}");
        return reg;
    }

    /// <summary> Performs an arithmetic (sign-preserving) right shift on an integer. </summary>
    public Reg Sar(int value, Val shiftAmount)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w sar {value}, {shiftAmount}");
        return reg;
    }

    /// <summary> Performs a logical (zero-fill) right shift on an integer. </summary>
    public Reg Shr(int value, Val shiftAmount)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w shr {value}, {shiftAmount}");
        return reg;
    }

    /// <summary> Performs a logical left shift on an integer, filling with zeroes. </summary>
    public Reg Shl(int value, Val shiftAmount)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w shl {value}, {shiftAmount}");
        return reg;
    }

    // Memory

    /// <summary> Stores a double-precision float value into memory. </summary>
    public Reg Stored(int value, Val address)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w stored {value}, {address}");
        return reg;
    }

    /// <summary> Stores a single-precision float value into memory. </summary>
    public Reg Stores(int value, Val address)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w stores {value}, {address}");
        return reg;
    }

    /// <summary> Stores a long (64-bit) integer value into memory. </summary>
    public Reg Storel(int value, Val address)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w storel {value}, {address}");
        return reg;
    }

    /// <summary> Stores a word (32-bit) integer value into memory. </summary>
    public void Storew(string value, Val address)
    {
        WriteGen($"storew {value}, {address}");
    }

    /// <summary> Stores the lower 16 bits of a word value into memory. </summary>
    public Reg Storeh(int value, Val address)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w storeh {value}, {address}");
        return reg;
    }

    /// <summary> Stores the lower 8 bits of a word value into memory. </summary>
    public Reg Storeb(int value, Val address)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w storeb {value}, {address}");
        return reg;
    }

    /// <summary> Loads a double-precision float value from memory. </summary>
    public Reg Loadd(Val address)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w loadd {address}");
        return reg;
    }

    /// <summary> Loads a single-precision float value from memory. </summary>
    public Reg Loads(Val address)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w loads {address}");
        return reg;
    }

    /// <summary> Loads a long (64-bit) integer value from memory. </summary>
    public Reg Loadl(Val address)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w loadl {address}");
        return reg;
    }

    /// <summary> Loads a word from memory and sign-extends it. </summary>
    public Reg Loadsw(Val address)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w loadsw {address}");
        return reg;
    }

    /// <summary> Loads a word from memory and zero-extends it. </summary>
    public Reg Loaduw(Val address)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w loaduw {address}");
        return reg;
    }

    /// <summary> Syntactic sugar to load a word from memory when the extension type is irrelevant. </summary>
    public Reg Loadw(Val address)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w loadw {address}");
        return reg;
    }

    /// <summary> Loads a half-word (16-bit) from memory and sign-extends it. </summary>
    public Reg Loadsh(Val address)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w loadsh {address}");
        return reg;
    }

    /// <summary> Loads a half-word (16-bit) from memory and zero-extends it. </summary>
    public Reg Loaduh(Val address)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w loaduh {address}");
        return reg;
    }

    /// <summary> Loads a byte (8-bit) from memory and sign-extends it. </summary>
    public Reg Loadsb(Val address)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w loadsb {address}");
        return reg;
    }

    /// <summary> Loads a byte (8-bit) from memory and zero-extends it. </summary>
    public Reg Loadub(Val address)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w loadub {address}");
        return reg;
    }

    /// <summary> Copies a constant number of bytes from a source memory address to a destination address. </summary>
    public Reg Blit(Val srcAddress, Val destAddress, int byteCount)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w blit {srcAddress}, {destAddress}, {byteCount}");
        return reg;
    }

    /// <summary> Allocates memory on the stack with 4-byte alignment. </summary>
    public Reg Alloc4(Val sizeInBytes)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w alloc4 {sizeInBytes}");
        return reg;
    }

    /// <summary> Allocates memory on the stack with 8-byte alignment. </summary>
    public Reg Alloc8(Val sizeInBytes)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w alloc8 {sizeInBytes}");
        return reg;
    }

    /// <summary> Allocates memory on the stack with 16-byte alignment. </summary>
    public Reg Alloc16(Val sizeInBytes)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w alloc16 {sizeInBytes}");
        return reg;
    }

    // Comparisons

    /// <summary> Compares two double-precision floats for equality. </summary>
    public Reg Ceqd(Val op1, Val op2)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w ceqd {op1}, {op2}");
        return reg;
    }

    /// <summary> Compares two long integers for equality. </summary>
    public Reg Ceql(Val op1, Val op2)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w ceql {op1}, {op2}");
        return reg;
    }

    /// <summary> Compares two single-precision floats for equality. </summary>
    public Reg Ceqs(Val op1, Val op2)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w ceqs {op1}, {op2}");
        return reg;
    }

    /// <summary> Compares two word integers for equality. </summary>
    public Reg Ceqw(Val op1, Val op2)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w ceqw {op1}, {op2}");
        return reg;
    }

    /// <summary> Compares if the first double is greater than or equal to the second. </summary>
    public Reg Cged(Val op1, Val op2)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w cged {op1}, {op2}");
        return reg;
    }

    /// <summary> Compares if the first single is greater than or equal to the second. </summary>
    public Reg Cges(Val op1, Val op2)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w cges {op1}, {op2}");
        return reg;
    }

    /// <summary> Compares if the first double is greater than the second. </summary>
    public Reg Cgtd(Val op1, Val op2)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w cgtd {op1}, {op2}");
        return reg;
    }

    /// <summary> Compares if the first single is greater than the second. </summary>
    public Reg Cgts(Val op1, Val op2)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w cgts {op1}, {op2}");
        return reg;
    }

    /// <summary> Compares if the first double is lower than or equal to the second. </summary>
    public Reg Cled(Val op1, Val op2)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w cled {op1}, {op2}");
        return reg;
    }

    /// <summary> Compares if the first single is lower than or equal to the second. </summary>
    public Reg Cles(Val op1, Val op2)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w cles {op1}, {op2}");
        return reg;
    }

    /// <summary> Compares if the first double is lower than the second. </summary>
    public Reg Cltd(Val op1, Val op2)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w cltd {op1}, {op2}");
        return reg;
    }

    /// <summary> Compares if the first single is lower than the second. </summary>
    public Reg Clts(Val op1, Val op2)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w clts {op1}, {op2}");
        return reg;
    }

    /// <summary> Compares two double-precision floats for inequality. </summary>
    public Reg Cned(Val op1, Val op2)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w cned {op1}, {op2}");
        return reg;
    }

    /// <summary> Compares two long integers for inequality. </summary>
    public Reg Cnel(Val op1, Val op2)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w cnel {op1}, {op2}");
        return reg;
    }

    /// <summary> Compares two single-precision floats for inequality. </summary>
    public void Cnes(Val op1, Val op2)
    {
        WriteGen($"cnes {op1}, {op2}");
    }

    /// <summary> Compares two word integers for inequality. </summary>
    public Reg Cnew(Val op1, Val op2)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w cnew {op1}, {op2}");
        return reg;
    }

    /// <summary> Checks if two double-precision floats are ordered (neither is NaN). </summary>
    public Reg Cod(Val op1, Val op2)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w cod {op1}, {op2}");
        return reg;
    }

    /// <summary> Checks if two single-precision floats are ordered (neither is NaN). </summary>
    public Reg Cos(Val op1, Val op2)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w cos {op1}, {op2}");
        return reg;
    }

    /// <summary> Compares if the first signed long is greater than or equal to the second. </summary>
    public Reg Csgel(Val op1, Val op2)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w csgel {op1}, {op2}");
        return reg;
    }

    /// <summary> Compares if the first signed word is greater than or equal to the second. </summary>
    public Reg Csgew(Val op1, Val op2)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w csgew {op1}, {op2}");
        return reg;
    }

    /// <summary> Compares if the first signed long is greater than the second. </summary>
    public Reg Csgtl(Val op1, Val op2)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w csgtl {op1}, {op2}");
        return reg;
    }

    /// <summary> Compares if the first signed word is greater than the second. </summary>
    public Reg Csgtw(Val op1, Val op2)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w csgtw {op1}, {op2}");
        return reg;
    }

    /// <summary> Compares if the first signed long is lower than or equal to the second. </summary>
    public Reg Cslel(Val op1, Val op2)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w cslel {op1}, {op2}");
        return reg;
    }

    /// <summary> Compares if the first signed word is lower than or equal to the second. </summary>
    public Reg Cslew(Val op1, Val op2)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w cslew {op1}, {op2}");
        return reg;
    }

    /// <summary> Compares if the first signed long is lower than the second. </summary>
    public Reg Csltl(Val op1, Val op2)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w csltl {op1}, {op2}");
        return reg;
    }

    /// <summary> Compares if the first signed word is lower than the second. </summary>
    public Reg Csltw(Val op1, Val op2)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w csltw {op1}, {op2}");
        return reg;
    }

    /// <summary> Compares if the first unsigned long is greater than or equal to the second. </summary>
    public Reg Cugel(Val op1, Val op2)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w cugel {op1}, {op2}");
        return reg;
    }

    /// <summary> Compares if the first unsigned word is greater than or equal to the second. </summary>
    public Reg Cugew(Val op1, Val op2)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w cugew {op1}, {op2}");
        return reg;
    }

    /// <summary> Compares if the first unsigned long is greater than the second. </summary>
    public Reg Cugtl(Val op1, Val op2)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w cugtl {op1}, {op2}");
        return reg;
    }

    /// <summary> Compares if the first unsigned word is greater than the second. </summary>
    public Reg Cugtw(Val op1, Val op2)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w cugtw {op1}, {op2}");
        return reg;
    }

    /// <summary> Compares if the first unsigned long is lower than or equal to the second. </summary>
    public Reg Culel(Val op1, Val op2)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w culel {op1}, {op2}");
        return reg;
    }

    /// <summary> Compares if the first unsigned word is lower than or equal to the second. </summary>
    public Reg Culew(Val op1, Val op2)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w culew {op1}, {op2}");
        return reg;
    }

    /// <summary> Compares if the first unsigned long is lower than the second. </summary>
    public Reg Cultl(Val op1, Val op2)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w cultl {op1}, {op2}");
        return reg;
    }

    /// <summary> Compares if the first unsigned word is lower than the second. </summary>
    public Reg Cultw(Val op1, Val op2)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w cultw {op1}, {op2}");
        return reg;
    }

    /// <summary> Checks if two double-precision floats are unordered (at least one is NaN). </summary>
    public Reg Cuod(Val op1, Val op2)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w cuod {op1}, {op2}");
        return reg;
    }

    /// <summary> Checks if two single-precision floats are unordered (at least one is NaN). </summary>
    public Reg Cuos(Val op1, Val op2)
    {
        Reg reg = GetTempReg();
        WriteGen($"{reg} =w cuos {op1}, {op2}");
        return reg;
    }

    // Conversions, Cast, and Copy

    /// <summary> Converts a double-precision float to a signed integer. </summary>
    public void Dtosi(int value)
    {
        WriteGen($"dtosi {value}");
    }

    /// <summary> Converts a double-precision float to an unsigned integer. </summary>
    public void Dtoui(int value)
    {
        WriteGen($"dtoui {value}");
    }

    /// <summary> Extends a single-precision float to a double-precision float. </summary>
    public void Exts(int value)
    {
        WriteGen($"exts {value}");
    }

    /// <summary> Sign-extends the 8 least-significant bits of a word to a larger integer type. </summary>
    public void Extsb(int value)
    {
        WriteGen($"extsb {value}");
    }

    /// <summary> Sign-extends the 16 least-significant bits of a word to a larger integer type. </summary>
    public void Extsh(int value)
    {
        WriteGen($"extsh {value}");
    }

    /// <summary> Sign-extends a word to a long. </summary>
    public void Extsw(int value)
    {
        WriteGen($"extsw {value}");
    }

    /// <summary> Zero-extends the 8 least-significant bits of a word to a larger integer type. </summary>
    public void Extub(int value)
    {
        WriteGen($"extub {value}");
    }

    /// <summary> Zero-extends the 16 least-significant bits of a word to a larger integer type. </summary>
    public void Extuh(int value)
    {
        WriteGen($"extuh {value}");
    }

    /// <summary> Zero-extends a word to a long. </summary>
    public void Extuw(int value)
    {
        WriteGen($"extuw {value}");
    }

    /// <summary> Converts a signed long integer to a floating-point number. </summary>
    public void Sltof(int value)
    {
        WriteGen($"sltof {value}");
    }

    /// <summary> Converts an unsigned long integer to a floating-point number. </summary>
    public void Ultof(int value)
    {
        WriteGen($"ultof {value}");
    }

    /// <summary> Converts a single-precision float to a signed integer. </summary>
    public void Stosi(int value)
    {
        WriteGen($"stosi {value}");
    }

    /// <summary> Converts a single-precision float to an unsigned integer. </summary>
    public void Stoui(int value)
    {
        WriteGen($"stoui {value}");
    }

    /// <summary> Converts a signed word integer to a floating-point number. </summary>
    public void Swtof(int value)
    {
        WriteGen($"swtof {value}");
    }

    /// <summary> Converts an unsigned word integer to a floating-point number. </summary>
    public void Uwtof(int value)
    {
        WriteGen($"uwtof {value}");
    }

    /// <summary> Truncates a double-precision float to a single-precision float. </summary>
    public void Truncd(int value)
    {
        WriteGen($"truncd {value}");
    }

    /// <summary> Changes a value's type (e.g., int to float) without altering its underlying bits. </summary>
    public void Cast(int value)
    {
        WriteGen($"cast {value}");
    }

    /// <summary> Returns the bits of its argument verbatim in a new temporary of the same type. </summary>
    public void Copy(int value)
    {
        WriteGen($"copy {value}");
    }

    // Control Flow and Functions

    /// <summary> Calls a function with a specified list of arguments and types. </summary>
    public void Call(FunctionSymbol functionSymbol, params string[] arguments)
    {
        WriteGen($"call ${functionSymbol}({string.Join(',', arguments)})");
    }

    /// <summary> Initializes a list to access the variable arguments of a variadic function. </summary>
    public void VaStart(Val variableArgumentListPtr)
    {
        WriteGen($"vastart {variableArgumentListPtr}");
    }

    /// <summary> Fetches the next argument from a variable argument list. </summary>
    public void VaArg(Val variableArgumentListPtr)
    {
        WriteGen($"vaarg {variableArgumentListPtr}");
    }

    /// <summary> Selects a value based on the preceding control flow block, for use in SSA form. </summary>
    public void Phi()
    {
        WriteGen($"phi ");
    }

    /// <summary> Halts program execution with a target-dependent error. </summary>
    public void Hlt()
    {
        WriteGen($"hlt ");
    }

    /// <summary> Jumps unconditionally to another block within the same function. </summary>
    public void Jmp(Label targetLabel)
    {
        WriteGen($"jmp {targetLabel}");
    }

    /// <summary> Jumps conditionally to one of two blocks based on whether a value is non-zero. </summary>
    public void Jnz(Val conditionVal, Label trueLabel, Label falseLabel)
    {
        WriteGen($"jnz {conditionVal}, {trueLabel}, {falseLabel}");
    }

    /// <summary> Returns from the current function, optionally passing a value to the caller. </summary>
    public void Ret(int? returnValue)
    {
        WriteGen($"ret {returnValue}");
    }

}
