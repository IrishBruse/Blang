namespace BLang.Utility;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

public class FlagList<T>
{
    public List<T> Items { get; } = new List<T>();
}

public class FlagListMut : FlagList<string>
{
}

public enum FlagType
{
    BoolFlag,
    IntegerFlag,
    StringFlag,
}

public enum FlagError
{
    NoError,
    Unknown,
    NoValue,
    InvalidNumber,
    IntegerOverflow,
    InvalidSizeSuffix,
}

public abstract class CommandFlag
{
    public FlagType Type { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}

public class CommandFlag<T> : CommandFlag
{
    public T Value { get; set; }
    public T DefaultValue { get; set; }
}

public class FlagContext
{
    public List<CommandFlag> Flags { get; } = new List<CommandFlag>();
    public FlagError Error { get; set; } = FlagError.NoError;
    public string ErrorName { get; set; }
    public string ProgramName { get; set; }
    public string[] RestArguments { get; set; }
}

public static class Flag
{
    private static FlagContext GlobalContext = new();

    private static CommandFlag<T> AddNewFlag<T>(FlagContext context, FlagType type, string name, string desc)
    {
        CommandFlag<T> flag = new()
        {
            Type = type,
            Name = name,
            Description = desc
        };
        context.Flags.Add(flag);
        return flag;
    }

    // Utility method to get a value by reference in C#
    public static T GetValue<T>(object value)
    {
        return (T)value;
    }

    public static bool Parse(string[] args)
    {
        return Parse(GlobalContext, args);
    }

    public static bool Parse(FlagContext context, string[] args)
    {
        if (context.ProgramName == null && args.Length > 0)
        {
            context.ProgramName = args[0];
            args = args.Skip(1).ToArray();
        }

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];

            if (!arg.StartsWith("-", true, CultureInfo.CurrentCulture))
            {
                context.RestArguments = args.Skip(i).ToArray();
                return true;
            }

            if (arg == "--")
            {
                // FLAG_PUSH_DASH_DASH_BACK is a C macro, C# version keeps the argument
                context.RestArguments = args.Skip(i).ToArray();
                return true;
            }

            string flag = arg[1..];
            int equalsIndex = flag.IndexOf('=');
            string flagName, flagValue = null;

            if (equalsIndex != -1)
            {
                flagName = flag[..equalsIndex];
                flagValue = flag[(equalsIndex + 1)..];
            }
            else
            {
                flagName = flag;
            }

            CommandFlag foundFlag = context.Flags.Find(f => f.Name == flagName);
            if (foundFlag == null)
            {
                context.Error = FlagError.Unknown;
                context.ErrorName = flagName;
                return false;
            }

            switch (foundFlag.Type)
            {
                case FlagType.BoolFlag:
                    CommandFlag<bool> boolFlag = (CommandFlag<bool>)foundFlag;
                    boolFlag.Value = true;
                    break;
                case FlagType.StringFlag:
                    CommandFlag<string> stringFlag = (CommandFlag<string>)foundFlag;
                    if (flagValue == null)
                    {
                        if (i + 1 >= args.Length)
                        {
                            context.Error = FlagError.NoValue;
                            context.ErrorName = flagName;
                            return false;
                        }
                        flagValue = args[++i];
                    }
                    stringFlag.Value = flagValue;
                    break;
                case FlagType.IntegerFlag:
                    CommandFlag<int> intFlag = (CommandFlag<int>)foundFlag;
                    if (flagValue == null)
                    {
                        if (i + 1 >= args.Length)
                        {
                            context.Error = FlagError.NoValue;
                            context.ErrorName = flagName;
                            return false;
                        }
                        flagValue = args[++i];
                    }
                    if (!int.TryParse(flagValue, out int intResult))
                    {
                        context.Error = FlagError.InvalidNumber;
                        context.ErrorName = flagName;
                        return false;
                    }
                    intFlag.Value = intResult;
                    break;
                default:
                    throw new NotImplementedException("");
            }
        }

        context.RestArguments = Array.Empty<string>();
        return true;
    }

    private static bool ParseSize(string sizeString, out ulong result)
    {
        result = 0;
        if (string.IsNullOrEmpty(sizeString)) return false;

        char suffix = sizeString.Last();
        string valueString = sizeString[..^1];

        ulong multiplier = 1;
        bool hasSuffix = true;

        switch (suffix)
        {
            case 'K':
                multiplier = 1024;
                break;
            case 'M':
                multiplier = 1024 * 1024;
                break;
            case 'G':
                multiplier = 1024 * 1024 * 1024;
                break;
            default:
                hasSuffix = false;
                valueString = sizeString;
                break;
        }

        if (!ulong.TryParse(valueString, out result)) return false;

        if (hasSuffix)
        {
            try
            {
                result = checked(result * multiplier);
            }
            catch (OverflowException)
            {
                return false;
            }
        }

        return true;
    }


    public static CommandFlag<bool> GetBool(string name, bool defaultValue, string desc)
    {
        CommandFlag<bool> flag = AddNewFlag<bool>(GlobalContext, FlagType.BoolFlag, name, desc);
        flag.Value = defaultValue;
        flag.DefaultValue = defaultValue;
        return flag;
    }

    public static void PrintOptions(TextWriter stream)
    {
        PrintOptions(GlobalContext, stream);
    }

    public static void PrintOptions(FlagContext context, TextWriter stream)
    {
        foreach (CommandFlag<object> flag in context.Flags.Cast<CommandFlag<object>>())
        {
            stream.WriteLine($"  -{flag.Name} <{flag.Type.ToString().ToLower(CultureInfo.CurrentCulture)}>");
            stream.WriteLine($"    {flag.Description}");
            if (flag.DefaultValue != null)
            {
                stream.WriteLine($"    Default: {flag.DefaultValue}");
            }
            stream.WriteLine();
        }
    }

    public static void PrintError(TextWriter stream)
    {
        PrintError(GlobalContext, stream);
    }

    public static void PrintError(FlagContext context, TextWriter stream)
    {
        switch (context.Error)
        {
            case FlagError.NoError:
                stream.WriteLine("Operation Failed Successfully! Please tell the developer of this software that they don't know what they are doing! :)");
                break;
            case FlagError.Unknown:
                stream.WriteLine($"ERROR: -{context.ErrorName}: unknown flag");
                break;
            case FlagError.NoValue:
                stream.WriteLine($"ERROR: -{context.ErrorName}: no value provided");
                break;
            case FlagError.InvalidNumber:
                stream.WriteLine($"ERROR: -{context.ErrorName}: invalid number");
                break;
            case FlagError.IntegerOverflow:
                stream.WriteLine($"ERROR: -{context.ErrorName}: integer overflow");
                break;
            case FlagError.InvalidSizeSuffix:
                stream.WriteLine($"ERROR: -{context.ErrorName}: invalid size suffix");
                break;
            default:
                break;
        }
    }
}
