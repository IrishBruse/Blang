#:property PublishAot=false
#:property DisableDynamicCode=false

namespace BLang.Targets.qbe;

using System.Text;
using System.Text.Json;

public class InstructionDefinition
{
    public string? name { get; set; }
    public string? description { get; set; }
    public Dictionary<string, string>? args { get; set; }
    public string? ret { get; set; }
    public string? retSize { get; set; }
    public string? overrideBody { get; set; }
}

public class InstructionGroup : Dictionary<string, InstructionDefinition> { }

public class QbeCodeGenerator
{
    private StringBuilder text = new();
    private int indentLevel;
    private const string indentString = "    ";


    private void WriteLine(string line = "")
    {
        if (line == "")
        {
            _ = text.Append('\n');
            return;
        }
        _ = text.Append(string.Concat(Enumerable.Repeat(indentString, indentLevel)) + line + '\n');
    }

    private void BeginLine()
    {
        _ = text.Append(string.Concat(Enumerable.Repeat(indentString, indentLevel)));
    }

    private void Write(string val)
    {
        _ = text.Append(val);
    }

    private void EndLine()
    {
        _ = text.Append('\n');
    }

    private void Indent()
    {
        indentLevel++;
    }

    private void Dedent()
    {
        indentLevel = Math.Max(0, indentLevel - 1);
    }

    private static string PascalCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return char.ToUpper(input[0]) + input[1..];
    }

    private void GenGroup(string groupName, InstructionGroup group)
    {
        WriteLine($"// {groupName}");
        WriteLine();
        foreach (string mnemonic in group.Keys)
        {
            InstructionDefinition? instruction = group[mnemonic];
            string name = instruction.name;
            Dictionary<string, string> args = instruction.args ?? new Dictionary<string, string>();
            List<KeyValuePair<string, string>> argsList = args.ToList();

            List<string> parameters = argsList
                .Select(static arg => $"{arg.Value} {arg.Key}")
                .ToList();

            if (instruction?.ret == "Reg" && instruction?.retSize == null)
                parameters.Add("Size regType = Size.L");

            string parameterString = string.Join(", ", parameters);
            string returnType = instruction?.ret ?? "void";

            WriteLine($"/// <summary> {instruction.description} </summary>");
            WriteLine($"public {returnType} {name}({parameterString})");
            WriteLine("{");
            Indent();
            WriteInstructionBody(mnemonic, instruction, argsList, returnType);
            Dedent();
            WriteLine("}");
            WriteLine();
        }
    }

    private void WriteInstructionBody(string mnemonic, InstructionDefinition instruction, List<KeyValuePair<string, string>> argsList, string returnType)
    {
        if (mnemonic == "ret")
        {
            WriteLine("if (returnValue == null)");
            WriteLine("{");
            WriteLine("    Write($\"ret\");");
            WriteLine("    return;");
            WriteLine("}");
            WriteLine("");
            WriteLine("Write($\"ret {returnValue}\");");
            return;
        }

        if (returnType != "void")
        {
            WriteLine("Reg reg = GetTempReg();");
        }

        if (instruction?.overrideBody != null)
        {
            WriteLine(instruction.overrideBody);
        }
        else
        {
            WriteInstruction(mnemonic, argsList, instruction);
        }

        if (returnType != "void")
        {
            WriteLine("return reg;");
        }
    }

    private void WriteInstruction(string mnemonic, List<KeyValuePair<string, string>> args, InstructionDefinition instruction)
    {
        BeginLine();
        Write("Write(");

        StringBuilder instructionString = new();

        if (instruction?.retSize != null)
            _ = instructionString.Append($"{{reg}} ={instruction.retSize} ");
        else if (instruction?.ret == "Reg")
        {
            _ = instructionString.Append("{reg} ={ToChar(regType)} ");
        }

        _ = instructionString.Append(mnemonic + " ");

        List<string> argStrings = new();
        foreach (KeyValuePair<string, string> arg in args)
        {
            string name = arg.Key;
            string type = arg.Value;

            if (type == "FunctionSymbol")
            {
                argStrings.Add("{" + name + "}");
            }
            else if (type == "Label")
            {
                argStrings.Add("@{" + name + "}");
            }
            else
            {
                argStrings.Add("{" + name + "}");
            }
        }
        _ = instructionString.Append(string.Join(", ", argStrings));

        Write("$\"" + instructionString + "\"");
        Write(");");
        EndLine();
    }

    public string Generate()
    {
        Dictionary<string, InstructionGroup> instructions = JsonSerializer.Deserialize<Dictionary<string, InstructionGroup>>(File.ReadAllText("misc/qbeInstructions.json"))!;

        WriteLine("namespace BLang.Targets.qbe;");
        WriteLine();
        WriteLine("using FunctionSymbol = string;");
        WriteLine("using Label = string;");
        WriteLine("using Reg = string;");
        WriteLine("using Val = string;");
        WriteLine();

        WriteLine("public partial class QbeOutput");
        WriteLine("{");
        Indent();

        foreach (KeyValuePair<string, InstructionGroup> groupEntry in instructions)
        {
            string group = groupEntry.Key;
            string groupName = PascalCase(group);
            GenGroup(groupName, groupEntry.Value);
        }

        Dedent();
        WriteLine("}");

        return text.ToString();
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        QbeCodeGenerator generator = new();
        string generatedText = generator.Generate();

        const string outputFileName = "./BLang/Targets/qbe/QbeOutput.gen.cs";
        try
        {
            File.WriteAllText(outputFileName, generatedText);
            Console.WriteLine($"Successfully generated: {outputFileName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing file: {ex.Message}");
        }
    }
}
