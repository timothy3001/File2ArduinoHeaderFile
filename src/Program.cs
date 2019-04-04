using System;
using System.Collections.Generic;
using System.IO;

namespace File2ArduinoHeaderFile
{
    class Program
    {
        static void Main(string[] args)
        {
            AnalyzeStartArguments(args, out string filePath, out bool doNotIgnoreHeaderFiles, out string directory);

            List<string> files = new List<string>();

            if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath))
            {
                files.Add(filePath);
            }
            else if (!string.IsNullOrWhiteSpace(directory))
            {
                if (Directory.Exists(directory))
                {
                    files.AddRange(GetFilesInDirConsideringHeaderFiles(directory, doNotIgnoreHeaderFiles));
                }
                else
                {
                    throw new ArgumentException($"Directory '{directory}' does not exist!");
                }
            }
            else
            {
                var rootDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);

                if (Directory.Exists(rootDir))
                {
                    files.AddRange(GetFilesInDirConsideringHeaderFiles(rootDir, doNotIgnoreHeaderFiles));
                }
                else
                {
                    throw new Exception($"Something went wrong! Directory '{rootDir}' does not exist!");
                }
            }

            foreach (var file in files)
            {
                ConvertFileToHeaderFile(file);
            }
        }

        private static void ConvertFileToHeaderFile(string filePath)
        {

        }

        private static List<string> GetFilesInDirConsideringHeaderFiles(string directory, bool doNotIgnoreHeaderFiles)
        {
            var files = new List<string>();

            files.AddRange(Directory.GetFiles(directory, "*"));
            if (!doNotIgnoreHeaderFiles)
                files.RemoveAll(x => x.EndsWith(".h", StringComparison.InvariantCultureIgnoreCase) || x.EndsWith(".hpp", StringComparison.InvariantCultureIgnoreCase));

            return files;
        }

        private static void AnalyzeStartArguments(string[] args, out string filePath, out bool doNotIgnoreHeaderFiles, out string directory)
        {
            filePath = null;
            doNotIgnoreHeaderFiles = false;
            directory = null;

            for (int i = 0; i < args.Length; i++)
            {
                string currentArg = args[i];
                string nextArg = i + 1 < args.Length ? args[i + 1] : null;

                if (currentArg.StartsWith("-f", StringComparison.InvariantCultureIgnoreCase) || currentArg.StartsWith("--file", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (nextArg != null)
                        filePath = nextArg;
                    else
                        throw new ArgumentException("You must provide a valid filePath when using parameter '-f'!");
                }
                else if (currentArg.StartsWith("-u", StringComparison.InvariantCultureIgnoreCase) || currentArg.StartsWith("--doNotIgnoreHeaderFiles", StringComparison.InvariantCultureIgnoreCase))
                {
                    doNotIgnoreHeaderFiles = true;
                }
                else if (currentArg.StartsWith("-d", StringComparison.InvariantCultureIgnoreCase) || currentArg.StartsWith("--directory", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (nextArg != null)
                        directory = nextArg;
                    else
                        throw new ArgumentException("You must provide a valid directory when using parameter '-d'!");
                }
            }
        }
    }
}
