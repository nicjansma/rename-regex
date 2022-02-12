// <copyright file="Options.cs" company="Nic Jansma">
//  Copyright (c) Nic Jansma 2022 All Right Reserved
// </copyright>
// <author>Nic Jansma</author>
// <email>nic@nicj.net</email>
using System;
using System.Text.RegularExpressions;

namespace RenameRegex
{
    /// <summary>
    /// Program options from command line arguments
    /// </summary>
    internal static class Options
    {
        #region Args

        /// <summary>
        /// File matching pattern
        /// </summary>
        public static string FileMatch = null;

        /// <summary>
        /// Search expression
        /// </summary>
        public static string NameSearch = null;

        /// <summary>
        /// Replace expression
        /// </summary>
        public static string NameReplace = null;

        /// <summary>
        /// Whether or not to only show what would happen
        /// </summary>
        public static bool Pretend = false;

        /// <summary>
        /// Whether or not to recursively look in directories
        /// </summary>
        public static bool Recursive = false;

        /// <summary>
        /// Whether or not to search case insensitive
        /// </summary>
        public static bool CaseInsensitive = false;

        /// <summary>
        /// Whether or not to force overwrites
        /// </summary>
        public static bool Force = false;

        /// <summary>
        /// Whether or not to preserve file extensions
        /// </summary>
        public static bool PreserveExt = false;

        /// <summary>
        /// Whether to include directories
        /// </summary>
        public static bool IncludeDirs = false;

        /// <summary>
        /// Whether to include files
        /// </summary>
        public static bool IncludeFiles = false;

        /// <summary>
        /// Whether to use a RegEx for the file match. If false, Windows glob patterns are used.
        /// </summary>
        public static bool FileMatchRegEx = false;

        #endregion Args

        /// <summary>
        /// Whether or not all args read
        /// </summary>
        public static bool Ready => !(FileMatch is null || NameSearch is null || NameReplace is null);

        /// <summary>
        /// Gets the program arguments
        /// </summary>
        /// <param name="args">Command-line arguments</param>
        /// <returns>True if argument parsing was successful</returns>
        public static bool GetArguments(string[] args)
        {
            // check for all arguments
            if (args is null || args.Length < 3)
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
            foreach (string arg in args)
            {
                switch (arg.ToLowerInvariant())
                {
                    case "/p":
                        Pretend = true;
                        break;

                    case "/r":
                        Recursive = true;
                        break;

                    case "/c":
                        CaseInsensitive = true;
                        break;

                    case "/f":
                        Force = true;
                        break;

                    case "/e":
                        PreserveExt = true;
                        break;

                    case "/files":
                        IncludeFiles = true;
                        break;

                    case "/dirs":
                        IncludeDirs = true;
                        break;

                    case "/fr":
                        FileMatchRegEx = true;
                        break;

                    default:
                        // if not an option, the rest of the arguments are filename, search, replace

                        if (FileMatch is null)
                        {
                            FileMatch = arg;
                            break;
                        }

                        if (NameSearch is null)
                        {
                            NameSearch = arg;
                            break;
                        }

                        if (NameReplace is null)
                        {
                            NameReplace = arg;
                            break;
                        }

                        Console.WriteLine($"ERROR: Unknown option: \"{arg}\"\n");
                        return false;
                }
            }

            // args finished
            if (!Ready)
            {
                return false;
            }

            if (FileMatchRegEx)
            {
                try
                {
                    Regex regex = new Regex(FileMatch);
                }
                catch (ArgumentException)
                {
                    Console.WriteLine("ERROR: File match is not a regular expression!\n");
                    return false;
                }
            }

            // default: files only
            if (!IncludeDirs)
            {
                IncludeFiles = true;
            }

            return true;
        }
    }
}
