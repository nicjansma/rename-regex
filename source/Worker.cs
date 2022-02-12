// <copyright file="Worker.cs" company="Nic Jansma">
//  Copyright (c) Nic Jansma 2022 All Right Reserved
// </copyright>
// <author>Nic Jansma</author>
// <email>nic@nicj.net</email>
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace RenameRegex
{
    /// <summary>
    /// Main processing unit
    /// </summary>
    internal class Worker
    {
        #region Cashed options

        private static string _searchDirectory;
        private static int _cut;

        private static string _searchPattern;
        private static string _pretendModeNotification;
        private static Regex _regex;

        private static bool _includeFiles;
        private static bool _includeDirs;
        private static bool _recursive;

        private static bool _preserveExt;
        private static bool _pretend;
        private static bool _force;

        private static string _nameSearch;
        private static string _nameReplace;
        private static RegexOptions _regexOptions;

        #endregion Cashed options

        /// <summary>
        /// Process a directory with the Options, safely for changing dir tree renames and network rights
        /// </summary>
        public static void SearchDirectory()
        {
            _includeFiles = Options.IncludeFiles;
            _includeDirs = Options.IncludeDirs;
            _recursive = Options.Recursive;

            _preserveExt = Options.PreserveExt;
            _force = Options.Force;

            // filter items
            if (Options.FileMatchRegEx)
            {
                _regex = new Regex(Options.FileMatch);
                _searchDirectory = @"\\?\" + Environment.CurrentDirectory;
                _searchPattern = "*";
            }
            else
            {
                _regex = null;
                _searchPattern = Options.FileMatch;

                if (_searchPattern.Contains("%"))
                {
                    // expand %TEMP%, %USERNAME%, etc
                    _searchPattern = Environment.ExpandEnvironmentVariables(_searchPattern);
                }

                if (_searchPattern.Contains(Path.DirectorySeparatorChar.ToString()))
                {
                    _searchDirectory = @"\\?\" + Path.GetFullPath(Path.GetDirectoryName(_searchPattern));
                    _searchPattern = Path.GetFileName(_searchPattern);
                }
                else
                {
                    _searchDirectory = @"\\?\" + Environment.CurrentDirectory;
                }
            }

            // to show only
            _pretend = Options.Pretend;
            _pretendModeNotification = _pretend ? " (pretend)" : string.Empty;

            // to rename via a regex
            _nameSearch = Options.NameSearch;
            _nameReplace = Options.NameReplace;
            _regexOptions = Options.CaseInsensitive ? RegexOptions.IgnoreCase : RegexOptions.None;

            // to show the relative path only
            _cut = _searchDirectory.Length + 1;

            // start processing
            ProcessDir(new DirectoryInfo(_searchDirectory), 0);
        }

        /// <summary>
        /// Process a directory recursively
        /// </summary>
        /// <param name="dir">Top or a next recursive directory</param>
        /// <param name="level">Level of recursion for some info</param>
        private static void ProcessDir(DirectoryInfo dir, int level)
        {
            if (_includeFiles)
            {
                try
                {
                    // no fast EnumerateFiles(), dangerously for wrong renames
                    foreach (FileInfo fi in dir.GetFiles(_searchPattern))
                    {
                        try
                        {
                            if (_regex is null || _regex.IsMatch(fi.Name))
                            {
                                ProcessItem(fi, true);
                            }
                        }
                        catch (UnauthorizedAccessException e)
                        {
                            Counters.Errors++;
                            Console.WriteLine($"ERROR: File Access: {e.Message}");
                        }
                    }
                }
                catch (UnauthorizedAccessException e)
                {
                    Counters.Errors++;
                    Console.WriteLine($"ERROR: Files Access: {e.Message}");
                }
            }

            if (_includeDirs)
            {
                try
                {
                    // no fast EnumerateDirectories(), dangerously for wrong renames
                    foreach (DirectoryInfo di in dir.GetDirectories(_searchPattern))
                    {
                        try
                        {
                            if (_regex is null || _regex.IsMatch(di.Name))
                            {
                                // rename dir, danderously for deep trees walking
                                ProcessItem(di, false);
                            }
                        }
                        catch (UnauthorizedAccessException e)
                        {
                            // i.e. "C:\System Volume Information"
                            Counters.Errors++;
                            Console.WriteLine($"ERROR: Dir Access: {e.Message}");
                        }
                    }
                }
                catch (UnauthorizedAccessException e)
                {
                    // i.e. "C:\System Volume Information" with /r option
                    Counters.Errors++;
                    Console.WriteLine($"ERROR: Dirs Access: {e.Message}");
                }

                dir.Refresh(); // before deeping into recursion of some changed dirs
            }

            if (_recursive)
            {
                try
                {
                    foreach (DirectoryInfo di in dir.GetDirectories(_searchPattern))
                    {
                        try
                        {
                            // call itself recursively
                            ProcessDir(di, ++level);
                        }
                        catch (UnauthorizedAccessException e)
                        {
                            Counters.Errors++;
                            Console.WriteLine($"ERROR: Recurse Access: {e.Message}");
                        }
                    }
                }
                catch (UnauthorizedAccessException e)
                {
                    // i.e. "C:\System Volume Information" with /r option
                    Counters.Errors++;
                    Console.WriteLine($"ERROR: Recurses Access: {e.Message}");
                }
            }
        }

        /// <summary>
        /// Process one file or dir
        /// </summary>
        /// <param name="item">File or Dir</param>
        /// <param name="isFile">Whether FileInfo or DirectoryInfo</param>
        private static void ProcessItem(FileSystemInfo item, bool isFile)
        {
            Counters.Listed++;
            FileSystemInfo itemAfter;

            if (isFile)
            {
                // rename via a regex
                if (_preserveExt)
                {
                    string name = Path.GetFileNameWithoutExtension(item.Name);
                    string nameAfter = Regex.Replace(name, _nameSearch, _nameReplace, _regexOptions);

                    // if file extension SHOULD be preserved
                    // append extension to filename AFTER renaming
                    itemAfter = new FileInfo(nameAfter + item.Extension);
                }
                else
                {
                    // if file extension should NOT be preserved
                    // use with extension BEFORE renaming
                    itemAfter = new FileInfo(Regex.Replace(item.Name, _nameSearch, _nameReplace, _regexOptions));
                }
            }
            else
            {
                // rename via a regex, ignore possible extension(s)
                itemAfter = new DirectoryInfo(Regex.Replace(item.Name, _nameSearch, _nameReplace, _regexOptions));
            }

            if (item.Name == itemAfter.Name)
            {
                return;
            }

            ShowItems(item, itemAfter);

            // show only
            if (_pretend)
            {
                return;
            }

            if (itemAfter.Exists && !DeleteItem(itemAfter))
            {
                return;
            }

            MoveItem(item, itemAfter, isFile);
        }

        /// <summary>
        /// Output some info about processed files or dirs
        /// </summary>
        /// <param name="item">Source File or Dir</param>
        /// <param name="itemAfter">Destnation File or Dir</param>
        private static void ShowItems(FileSystemInfo item, FileSystemInfo itemAfter)
        {
            const string message = " (already exists)";
            string output = $"{item.FullName.Substring(_cut)} -> {itemAfter.Name}{_pretendModeNotification}";

            Console.WriteLine(itemAfter.Exists ? output + message : output);
        }

        /// <summary>
        /// Delete a file or a dir to overwrite it
        /// </summary>
        /// <param name="item">File or Dir</param>
        /// <returns>True if no exceptions occured</returns>
        private static bool DeleteItem(FileSystemInfo item)
        {
            if (_force)
            {
                // remove old file/dir to overwrite
                try
                {
                    item.Delete();
                    return true;
                }
                catch (UnauthorizedAccessException e)
                {
                    Counters.Errors++;
                    Console.WriteLine($"ERROR: Delete Access: {e.Message}");
                }
            }
            else
            {
                Counters.Fails++;
                //Console.WriteLine("WARNING: Use /f to overwrite it.");
            }

            return false;
        }

        /// <summary>
        /// Rename (move) a file or a dir
        /// </summary>
        /// <param name="item">Source File or Dir</param>
        /// <param name="itemAfter">Destination File or Dir</param>
        /// <param name="isFile">Whether FileInfo or DirectoryInfo</param>
        private static void MoveItem(FileSystemInfo item, FileSystemInfo itemAfter, bool isFile)
        {
            try
            {
                if (isFile)
                {
                    (item as FileInfo).MoveTo(itemAfter.FullName);
                }
                else
                {
                    (item as DirectoryInfo).MoveTo(itemAfter.FullName);
                }

                Counters.Done++;
            }
            catch (IOException)
            {
                Counters.Fails++;
                Console.WriteLine($"WARNING: Could not move \"{item.FullName}\" to \"{itemAfter.Name}\"");
            }
        }
    }
}
