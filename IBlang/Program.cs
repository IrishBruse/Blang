namespace IBlang;

using System;
using System.IO;
using IBlang.Utility;

public class Program
{
    public static void Main()
    {
        string[] files = Directory.GetFiles("Examples/");
        string[] fileNames = new string[files.Length];

        for (int i = 0; i < files.Length; i++)
        {
            fileNames[i] = Path.GetFileNameWithoutExtension(files[i]);
        }

        int index = Terminal.ShowMenu(fileNames, "Pick Example:\n");

        string file = files[index];

        var fileStream = File.OpenText(file);

        Lexer lexer = new(fileStream, file, CompilationFlags.None);

        lexer.Lex()

        Console.WriteLine(File.ReadAllText(file));
    }
}
