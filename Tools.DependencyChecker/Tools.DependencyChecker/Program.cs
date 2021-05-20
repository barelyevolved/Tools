using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Tools.DependencyChecker
{
    class Program
    {
        private static Dictionary<string, List<string>> _fileReferences = new Dictionary<string, List<string>>();
        private static List<string> _failedFiles = new List<string>();

        static void Main(string[] args)
        {
            string folderPath = GetFolderToAnalyse(args);

            foreach (var filePath in Directory.EnumerateFiles(folderPath, "*.dll"))
            {
                var filename = Path.GetFileNameWithoutExtension(filePath);

                if (filename.StartsWith(Assembly.GetExecutingAssembly().GetName().Name))
                {
                    continue;
                }

                try
                {
                    Assembly assembly = Assembly.LoadFrom(filePath);
                    var references = assembly.GetReferencedAssemblies();
                    foreach (var reference in references)
                    {
                        if (_fileReferences.ContainsKey(reference.FullName))
                        {
                            _fileReferences[reference.FullName].Add(filename);
                        }
                        else
                        {
                            _fileReferences.Add(reference.FullName, new List<string> { filename });
                        }
                    }
                }
                catch (Exception)
                {
                    _failedFiles.Add(filePath);
                }
            }

            var stringBuilder = new StringBuilder();

            if (_fileReferences.Any())
            {
                foreach (var referencedFile in _fileReferences.OrderBy(f => f.Key))
                {
                    stringBuilder.AppendLine($"Assembly '{referencedFile.Key}' referenced by:");
                    var referringFiles = string.Join(Environment.NewLine + "\t", referencedFile.Value);
                    stringBuilder.AppendLine("\t" + referringFiles);
                    stringBuilder.AppendLine();
                }
            }
            else
            {
                stringBuilder.AppendLine("No assemblies found to analyse.");
            }

            var outputFilePath = Path.Combine(folderPath, "Tools.DependencyChecker.Output.txt");

            File.WriteAllText(outputFilePath, stringBuilder.ToString());

            Console.WriteLine(stringBuilder.ToString());
            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }

        private static string GetFolderToAnalyse(string[] args)
        {
            var folderPath = args.FirstOrDefault();

            if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
            {
                bool hasValidPathOrEmpty = false;

                do
                {
                    Console.WriteLine("Folder options");
                    Console.WriteLine(" - Enter path to analyse,");
                    Console.WriteLine(" - Or blank to use current folder:");
                    Console.Write(":");

                    folderPath = Console.ReadLine();

                    if (string.IsNullOrEmpty(folderPath) || Directory.Exists(folderPath))
                    {
                        hasValidPathOrEmpty = true;
                    }
                    else
                    {
                        Console.WriteLine("Invalid Path. Please enter again:");
                    }

                } while (!hasValidPathOrEmpty);
            }

            if (string.IsNullOrEmpty(folderPath))
            {
                folderPath = Environment.CurrentDirectory;
            }

            return folderPath;
        }
    }
}
