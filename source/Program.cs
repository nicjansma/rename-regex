// <copyright file="Program.cs" company="Nic Jansma">
//  Copyright (c) Nic Jansma 2022 All Right Reserved
// </copyright>
// <author>Nic Jansma</author>
// <email>nic@nicj.net</email>
namespace RenameRegex
{
    using System;
    using System.Diagnostics;
    using System.Reflection;

    /// <summary>
    /// RenameRegex command line program
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Main command line
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <returns>0 on success</returns>
        public static int Main(string[] args)
        {
            try
            {
                // get command-line arguments
                if (!Options.GetArguments(args))
                {
                    Usage();
                    return 1;
                }

                // do the work
                Worker.SearchDirectory();

                // show bad results if any
                if (Counters.Listed == 0)
                {
                    Console.WriteLine("No files or directories match!");
                    return 2;
                }

                // show final results
                Console.WriteLine(Counters.Total());
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR: {e.Message}");
                return 3;
            }
            finally
            {
#if DEBUG
                Console.WriteLine("\nDEBUG: Press a key to exit...");
                Console.ReadKey();
#endif
            }
        }

        /// <summary>
        /// Program usage
        /// </summary>
        private static void Usage()
        {
            // get the assembly version
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo i = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = $"v{i.FileMajorPart}.{i.FileMinorPart}.{i.FileBuildPart}";

            Console.WriteLine(
$@"Rename Regex (RR) {version} by Nic Jansma, https://nicj.net

Usage: RR.exe file-match search replace [/p] [/r] [/c] [/f] [/e] [/files] [/dirs]
        /p: pretend (show what will be renamed)
        /r: recursive
        /c: case insensitive
        /f: force overwrite if the file already exists
        /e: preserve file extensions
    /files: include files (default)
     /dirs: include directories
            (default is to include files only, to include both use /files /dirs)
       /fr: use regex for file name matching instead of Windows glob matching");

            return;
        }
    }
}
