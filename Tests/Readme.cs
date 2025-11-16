namespace Blang;

using System.IO;
using System.Linq;
using System.Text;

public class Readme
{
    public static void Main(string[] args)
    {
        StringBuilder readme = new();
        readme.AppendLine("# Table of Contents");
        readme.AppendLine();

        ProcessTests(readme, "ok", "OK Tests");
        readme.AppendLine();
        ProcessTests(readme, "error", "Error Tests");

        File.WriteAllText("./Readme.md", readme.ToString());
    }

    private static void ProcessTests(StringBuilder readme, string baseDir, string sectionName)
    {
        readme.AppendLine($"# {sectionName}");

        var subDirectories = Directory.EnumerateDirectories(baseDir, "*", SearchOption.TopDirectoryOnly)
                                     .OrderBy(d => d);

        foreach (var dir in subDirectories)
        {
            var folderName = Path.GetFileName(dir);
            ProcessFiles(readme, dir, folderName);
        }

        ProcessFiles(readme, baseDir, Path.GetFileName(baseDir), isRoot: true);
    }

    private static void ProcessFiles(StringBuilder readme, string dirPath, string dirName, bool isRoot = false)
    {
        var testFiles = Directory.EnumerateFiles(dirPath, "*.b", SearchOption.TopDirectoryOnly)
                                 .OrderBy(f => f)
                                 .ToList();

        if (testFiles.Any())
        {
            if (!isRoot)
            {
                readme.AppendLine($"  * **[{dirName}]({dirPath})**");
            }

            foreach (var t in testFiles)
            {
                var relativePath = Path.GetRelativePath(".", t).Replace('\\', '/');
                var fileName = Path.GetFileNameWithoutExtension(t);

                string indent = isRoot ? "  " : "    ";

                readme.AppendLine($"{indent}* [{fileName}]({relativePath})");
            }
        }
    }
}
