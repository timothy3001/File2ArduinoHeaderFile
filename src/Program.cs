using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace File2ArduinoHeaderFile
{
    class Program
    {
        static void Main(string[] args)
        {
            AnalyzeStartArguments(args, out string filePath, out bool doNotIgnoreHeaderFiles, out string directory, out bool doGzip);

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
                ConvertFileToHeaderFile(file, doGzip);
            }
        }

        private static string GetAllExtensionsFromFileName(string fileName)
        {
            var result = "";
            var parts = fileName.Split(".");

            for (int i = 1; i < parts.Length; i++)
                result += $".{parts[i]}";

            return result;
        }

        private static byte[] GzipBytes(byte[] inputBytes)
        {
            using (var ms = new MemoryStream())
            {
                using (GZipStream gzipStream = new GZipStream(ms, CompressionMode.Compress, true))
                {
                    gzipStream.Write(inputBytes, 0, inputBytes.Length);
                }

                return ms.ToArray();
            }
        }

        private static void ConvertFileToHeaderFile(string filePath, bool doGzip)
        {
            var fileName = Path.GetFileName(filePath);
            var fileNameWithoutExtension = fileName.Split(".")[0];
            var directoryPath = Path.GetDirectoryName(filePath);
            var fileExtension = GetAllExtensionsFromFileName(filePath).Replace(".", "-");

            if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(fileNameWithoutExtension))
                throw new Exception($"Something went wrong, fileName not found in filePath '{filePath}'");

            var headerFileName = $"{fileNameWithoutExtension}{fileExtension}.h";
            var headerFilePath = Path.Combine(directoryPath, headerFileName);

            if (File.Exists(headerFilePath))
                Console.WriteLine($"File already exists and will not be overridden: '{headerFilePath}'");
            else
            {
                var inputBytes = File.ReadAllBytes(filePath);
                if (doGzip)
                    inputBytes = GzipBytes(inputBytes);

                using (var headerFileStream = File.Create(headerFilePath))
                {
                    var prefix = $"const byte {FirstCharToLower(fileNameWithoutExtension)}{GetFileExtensionHeaderFile(fileExtension)}[] PROGMEM = {{";
                    headerFileStream.Write(Encoding.ASCII.GetBytes(prefix));

                    for (int i = 0; i < inputBytes.Length; i++)
                    {
                        var stringRepresentation = $" 0x{inputBytes[i].ToString("X2")}";
                        if (i != inputBytes.Length - 1)
                            stringRepresentation += ", ";
                        if (i % 20 == 0)
                            stringRepresentation = "\n" + stringRepresentation;

                        headerFileStream.Write(Encoding.ASCII.GetBytes(stringRepresentation));
                    }

                    var suffix = "\n};";

                    headerFileStream.Write(Encoding.ASCII.GetBytes(suffix));
                }
            }
        }

        private static object GetFileExtensionHeaderFile(string fileExtension)
        {
            var parts = fileExtension.Split("-");
            var result = FirstCharToLower(parts[0]);

            for (int i = 1; i < parts.Length; i++)
            {
                result += FirstCharToUpper(parts[i]);
            }

            return result;
        }

        private static List<string> GetFilesInDirConsideringHeaderFiles(string directory, bool doNotIgnoreHeaderFiles)
        {
            var files = new List<string>();

            files.AddRange(Directory.GetFiles(directory, "*"));
            if (!doNotIgnoreHeaderFiles)
                files.RemoveAll(x => x.EndsWith(".h", StringComparison.InvariantCultureIgnoreCase) || x.EndsWith(".hpp", StringComparison.InvariantCultureIgnoreCase));

            return files;
        }

        private static void AnalyzeStartArguments(string[] args, out string filePath, out bool doNotIgnoreHeaderFiles, out string directory, out bool doGzip)
        {
            filePath = null;
            doNotIgnoreHeaderFiles = false;
            directory = null;
            doGzip = false;

            for (int i = 0; i < args.Length; i++)
            {
                string currentArg = args[i];
                string nextArg = i + 1 < args.Length ? args[i + 1] : null;

                if (currentArg.StartsWith("-f", StringComparison.InvariantCultureIgnoreCase) || currentArg.StartsWith("--file", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (nextArg != null)
                        filePath = nextArg.Replace("\"", "");
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
                        directory = nextArg.Replace("\"", "");
                    else
                        throw new ArgumentException("You must provide a valid directory when using parameter '-d'!");
                }
                else if (currentArg.StartsWith("-g", StringComparison.InvariantCultureIgnoreCase) || currentArg.StartsWith("--gzip", StringComparison.InvariantCultureIgnoreCase))
                {
                    doGzip = true;
                }
            }
        }

        private static string FirstCharToUpper(string s)
        {
            // Check for empty string.  
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            // Return char and concat substring.  
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        private static string FirstCharToLower(string s)
        {
            // Check for empty string.  
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            // Return char and concat substring.  
            return char.ToLower(s[0]) + s.Substring(1);
        }
    }
}
