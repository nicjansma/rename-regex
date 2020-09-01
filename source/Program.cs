// <copyright file="Program.cs" company="Nic Jansma">
//  Copyright (c) Nic Jansma 2020 All Right Reserved
// </copyright>
// <author>Nic Jansma</author>
// <email>nic@nicj.net</email>
namespace RenameRegex
{
    using System;
    using System.Collections.Generic;
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
        /// Maximum Windows / DOS path length
        /// </summary>
        public const int MaxPath = 260;

        /// <summary>
        /// Include files
        /// </summary>
        public const int IncludeFiles = 1;

        /// <summary>
        /// Include directories
        /// </summary>
        public const int IncludeDirs = 2;

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
            bool force;
            bool preserveExt;
            int  includeMask;

            if (!GetArguments(
                    args,
                    out fileMatch,
                    out nameSearch,
                    out nameReplace,
                    out pretend,
                    out recursive,
                    out force,
                    out preserveExt,
                    out includeMask))
            {
                Usage();

                return 1;
            }

            // enumerate all files and directories
            List<string> allItems = new List<string>();

            // include all files by default
            if ((includeMask == 0) || ((includeMask & IncludeFiles) != 0))
            {
                string[] files = Directory.GetFiles(
                    Environment.CurrentDirectory,
                    fileMatch,
                    recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

                allItems.AddRange(files);
            }

            // include all directories if requested
            if ((includeMask & IncludeDirs) != 0)
            {
                string[] dirs = Directory.GetDirectories(
                    Environment.CurrentDirectory,
                    fileMatch,
                    recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

                allItems.AddRange(dirs);
            }
    
            if (allItems.Count == 0)
            {
                Console.WriteLine(@"No files or directories match!");

                return 1;
            }

            string pretendModeNotification = pretend ? " (pretend)" : String.Empty;

            //
            // loop through each file, renaming via a regex
            //
            foreach (string fullFile in allItems)
            {
                if (fullFile.Length > MaxPath || Path.GetDirectoryName(fullFile).Length > MaxPath - 12)
                {
                    Console.WriteLine(@"""{0}"" cannot be accessed; too long.", fullFile);
                    continue;
                }

                // split into filename, extension and path
                string fileName = Path.GetFileNameWithoutExtension(fullFile);
                string fileExt  = Path.GetExtension(fullFile);
                string fileDir  = Path.GetDirectoryName(fullFile);

                if (!preserveExt)
                {
                    // if file extension should NOT be preserverd
                    // append extension to filename BEFORE renaming
                    fileName += fileExt;
                }

                // rename via a regex
                string fileNameAfter = Regex.Replace(fileName, nameSearch, nameReplace, RegexOptions.IgnoreCase);

                if (preserveExt)
                {
                    // if file extension SHOULD be preserved
                    // append extension to filenames AFTER renaming
                    fileName += fileExt;
                    fileNameAfter += fileExt;
                }

                bool newFileAlreadyExists = File.Exists(fileDir + @"\" + fileNameAfter);

                // write what we changed (or would have)
                if (fileName != fileNameAfter)
                {
                    // show the relative file path if not the current directory
                    string fileNameToShow = (System.Environment.CurrentDirectory == fileDir) ?
                        fileName :
                        (fileDir + @"\" + fileName).Replace(System.Environment.CurrentDirectory + @"\", String.Empty);

                    Console.WriteLine(
                        @"{0} -> {1}{2}{3}",
                        fileNameToShow,
                        fileNameAfter,
                        pretendModeNotification,
                        newFileAlreadyExists ? @" (already exists)" : String.Empty);
                }

                // move file
                if (!pretend && fileName != fileNameAfter)
                {
                    try
                    {
                        if (newFileAlreadyExists && force)
                        {
                            // remove old file on force overwrite
                            File.Delete(fileNameAfter);
                        }

                        if (File.Exists(fileName))
                        {
                            File.Move(fileDir + @"\" + fileName, fileDir + @"\" + fileNameAfter);
                        }
                        else if (Directory.Exists(fileName))
                        {
                            Directory.Move(fileDir + @"\" + fileName, fileDir + @"\" + fileNameAfter);
                        }
                        else
                        {
                            Console.WriteLine(@"Could not rename {0}",  fileName);
                        }
                    }
                    catch (IOException)
                    {
                        Console.WriteLine(@"WARNING: Could not move {0} to {1}", fileName, fileNameAfter);
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
        /// <param name="force">Whether or not to force overwrites</param>
        /// <param name="preserveExt">Whether or not to preserve file extensions</param>
        /// <param name="includeMask">Whether to include directories, files or both</param>
        /// <returns>True if argument parsing was successful</returns>
        private static bool GetArguments(
            string[] args,
            out string fileMatch,
            out string nameSearch,
            out string nameReplace,
            out bool pretend,
            out bool recursive,
            out bool force,
            out bool preserveExt,
            out int  includeMask)
        {
            // defaults
            fileMatch   = String.Empty;
            nameSearch  = String.Empty;
            nameReplace = String.Empty;

            bool foundNameReplace = false;

            pretend     = false;
            recursive   = false;
            force       = false;
            preserveExt = false;
            includeMask = 0;

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
                else if (args[i].Equals("/f", StringComparison.OrdinalIgnoreCase))
                {
                    force = true;
                }
                else if (args[i].Equals("/e", StringComparison.OrdinalIgnoreCase))
                {
                    preserveExt = true;
                }
                else if (args[i].Equals("/files", StringComparison.OrdinalIgnoreCase))
                {
                    includeMask |= IncludeFiles;
                }
                else if (args[i].Equals("/dirs", StringComparison.OrdinalIgnoreCase))
                {
                    includeMask |= IncludeDirs;
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        private static void Usage()
        {
            // get the assembly version
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.ProductVersion;

            Console.WriteLine(@"Rename Regex (RR) v{0} by Nic Jansma, http://nicj.net", version);
            Console.WriteLine();
            Console.WriteLine(@"Usage: RR.exe file-match search replace [/p] [/r] [/f] [/e] [/files] [/dirs]");
            Console.WriteLine(@"        /p: pretend (show what will be renamed)");
            Console.WriteLine(@"        /r: recursive");
            Console.WriteLine(@"        /f: force overwrite if the file already exists");
            Console.WriteLine(@"        /e: preserve file extensions");
            Console.WriteLine(@"    /files: include files (default)");
            Console.WriteLine(@"     /dirs: include directories");
            Console.WriteLine(@"            (default is to include files only, to include both use /files /dirs)");
            return;
        }
    }
}
