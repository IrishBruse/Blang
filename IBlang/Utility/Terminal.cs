namespace IBlang.Utility;

using System;

public class Terminal
{
    public static int ShowMenu(string[] options, string title = "Pick an options:\n")
    {
        Console.CursorVisible = false;

        int selectedIndex = 0;
        ConsoleKeyInfo key;

        while (true)
        {
            Console.Clear();

            Console.WriteLine(title);

            for (int i = 0; i < options.Length; i++)
            {
                if (i == selectedIndex)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write($"> ");
                }
                else
                {
                    Console.Write($"  ");
                }

                Console.WriteLine($"{options[i]}");
                Console.ResetColor();
            }

            key = Console.ReadKey(true);

            switch (key.Key)
            {
                case ConsoleKey.UpArrow:
                selectedIndex--;
                if (selectedIndex < 0)
                {
                    selectedIndex = options.Length - 1;
                }
                break;

                case ConsoleKey.DownArrow:
                selectedIndex++;
                if (selectedIndex >= options.Length)
                {
                    selectedIndex = 0;
                }
                break;

                case ConsoleKey.Enter:
                Console.Clear();
                Console.CursorVisible = true;
                return selectedIndex;
            }
        }
    }

    public static void Debug(string message)
    {
        if (Flags.Debug)
        {
            Console.WriteLine(message);
        }
    }
}
