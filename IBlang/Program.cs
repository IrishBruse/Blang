namespace IBlang;

using System;
using System.Collections.Generic;
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

        StreamReader fileStream = File.OpenText(file);

        Lexer lexer = new(fileStream, file, CompilationFlags.None);

        IEnumerator<Token> tokens = lexer.Lex();

        while (tokens.MoveNext())
        {
            Token token = tokens.Current;

            if (token.TokenType == TokenType.Garbage || token.TokenType == TokenType.Eof || token.TokenType == TokenType.Eol)
            {
                Console.WriteLine(token);
                break;
            }

            Console.WriteLine(token);
        }

        Console.WriteLine(File.ReadAllText(file));
    }
}
