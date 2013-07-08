// <copyright file="Program.cs" company="Nic Jansma">
//  Copyright (c) Nic Jansma 2013 All Right Reserved
// </copyright>
// <author>Nic Jansma</author>
// <email>nic@nicj.net</email>
namespace RenameRegex
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Text.RegularExpressions;

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
            // get command-line arguments
            string nameSearch;
            string nameReplace;
            string fileMatch;
            bool recursive;
            bool pretend;
            if (!GetArguments(args, out fileMatch, out nameSearch, out nameReplace, out pretend, out recursive))
            {
                Usage();
                return 1;
            }

            // enumerate all files
            string[] files = Directory.GetFiles(
                System.Environment.CurrentDirectory, 
                fileMatch, 
                recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            if (files.Length == 0)
            {
                Console.WriteLine(@"No files match!");
                return 1;
            }

            string pretendModeNotification = pretend ? " (pretend)" : String.Empty;

            //
            // loop through each file, renaming via a regex
            //
            foreach (string fullFile in files)
            {
                // split into file and path
                string fileName = Path.GetFileName(fullFile);
                string fileDir  = Path.GetDirectoryName(fullFile);

                // rename via a regex
                string fileNameAfter = Regex.Replace(fileName, nameSearch, nameReplace, RegexOptions.IgnoreCase);

                // write what we changed (or would have)
                if (fileName != fileNameAfter)
                {
                    // show the relative file path if not the current directory
                    string fileNameToShow = (System.Environment.CurrentDirectory == fileDir) ?
                        fileName :
                        (fileDir + @"\" + fileName).Replace(System.Environment.CurrentDirectory + @"\", String.Empty);

                    Console.WriteLine(@"{0} -> {1}{2}", fileNameToShow, fileNameAfter, pretendModeNotification);
                }

                // move file
                if (!pretend && fileName != fileNameAfter)
                {
                    try
                    {
                        File.Move(fileDir + @"\" + fileName, fileDir + @"\" + fileNameAfter);
                    }
                    catch (IOException)
                    {
                        Console.WriteLine(@"WARNING: Could note move {0} to {1}", fileName, fileNameAfter);
                    }
                }
            }

            return 0;
        }

        /// <summary>
        /// Gets the program arguments
        /// </summary>
        /// <param name="args">Command-line arguments</param>
        /// <param name="fileMatch">File matching pattern</param>
        /// <param name="nameSearch">Search expression</param>
        /// <param name="nameReplace">Replace expression</param>
        /// <param name="pretend">Whether or not to only show what would happen</param>
        /// <param name="recursive">Whether or not to recursively look in directories</param>
        /// <returns>True if argument parsing was successful</returns>
        private static bool GetArguments(
            string[] args,
            out string fileMatch,
            out string nameSearch,
            out string nameReplace,
            out bool pretend,
            out bool recursive)
        {
            // defaults
            fileMatch   = String.Empty;
            nameSearch  = String.Empty;
            nameReplace = String.Empty;

            bool foundNameReplace = false;

            pretend     = false;
            recursive   = false;

            // check for all arguments
            if (args == null || args.Length < 3)
            {
                return false;
            }

            //
            // Loop through all of the command line arguments.
            //
            // Look for options first:
            //  /p: pretend (show what will be renamed)
            //  /r: recursive
            //
            // If not an option, assume it's one of the three main arguments (filename, search, replace)
            //
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals("/p", StringComparison.OrdinalIgnoreCase))
                {
                    pretend = true;
                }
                else if (args[i].Equals("/r", StringComparison.OrdinalIgnoreCase))
                {
                    recursive = true;
                }
                else
                {
                    // if not an option, the rest of the arguments are filename, search, replace
                    if (String.IsNullOrEmpty(fileMatch))
                    {
                        fileMatch = args[i];
                    }
                    else if (String.IsNullOrEmpty(nameSearch))
                    {
                        nameSearch = args[i];
                    }
                    else if (String.IsNullOrEmpty(nameReplace))
                    {
                        nameReplace = args[i];
                        foundNameReplace = true;
                    }
                }
            }

            return !String.IsNullOrEmpty(fileMatch)
                && !String.IsNullOrEmpty(nameSearch)
                && foundNameReplace;
        }

        /// <summary>
        /// Program usage
        /// </summary>
        private static void Usage()
        {
            // get the assembly version
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.ProductVersion;

            Console.WriteLine(@"Rename Regex (RR) v{0} by Nic Jansma, http://nicj.net", version);
            Console.WriteLine();
            Console.WriteLine(@"Usage: RR.exe file-match search replace [/p] [/r]");
            Console.WriteLine(@"        /p: pretend (show what will be renamed)");
            Console.WriteLine(@"        /r: recursive");
            return;
        }
    }
}
