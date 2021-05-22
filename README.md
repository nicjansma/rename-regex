Copyright (c) 2021 Nic Jansma
[http://nicj.net](http://nicj.net)

# Introduction

RenameRegex (RR) is a Windows command-line bulk file and directory renamer, using regular expressions.  You can use it as a simple
file renamer or with a complex regular expression for matching and replacement.  See the Examples section for details.

# Usage

```
    RR.exe file-match search replace [/p] [/r] [/f] [/e] [/files] [/dirs]
        /p: pretend (show what will be renamed)
        /r: recursive
        /c: case insensitive
        /f: force overwrite if the file already exists
        /e: preserve file extensions
    /files: include only files
     /dirs: include only directories
            (default is to include files only, to include both use /files /dirs)
```

You can use [.NET regular expressions](http://msdn.microsoft.com/en-us/library/hs600312.aspx) for the search and
replacement strings, including [substitutions](http://msdn.microsoft.com/en-us/library/ewy2t5e0.aspx) (for example,
"$1" is the 1st capture group in the search term).

# Examples

Simple rename without a regular expression:

    RR.exe * .ext1 .ext2

Renaming with a replacement of all "-" characters to "_":

    RR.exe * "-" "_"

Remove all numbers from the file names:

    RR.exe * "[0-9]+" ""

Rename files in the pattern of "`124_xyz.txt`" to "`xyz_123.txt`":

    RR.exe *.txt "([0-9]+)_([a-z]+)" "$2_$1"

Rename directories (only)

    RR * "-" "_" /dirs

Rename files and directories

    RR * "-" "_" /files /dirs

# Version History

* v1.0 - 2012-01-30: Initial release
* v1.1 - 2012-12-15: Added `/r` option
* v1.2 - 2013-05-11: Allow `/p` and `/r` options before or after main arguments
* v1.3 - 2013-10-23: Added `/f` option
* v1.4 - 2018-04-06: Added `/e` option (via Marcel Peeters)
* v1.5 - 2020-07-02: Added support for directories, added length-check (via Alec S. @Synetech)
* v1.6 - 2021-05-22: Added `/c` support for case insensitivity (via Alec S. @Synetech)

# Credits

* Nic Jansma (http://nicj.net)
* Marcel Peeters
* Alec S. (http://synetech.freehostia.com/)
