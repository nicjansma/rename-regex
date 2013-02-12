Copyright (c) 2013 Nic Jansma
[http://nicj.net](http://nicj.net)

# Introduction

RenameRegex (RR) is a Windows command-line bulk file renamer, using regular expressions.  You can use it as a simple
file renamer or with a complex regular expression for matching and replacement.  See the Examples section for details.

# Usage

    RR.exe file-match search replace [/p] [/r]
     /p: pretend (show what will be renamed)
     /r: recursive

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

Rename files in the pattern of "````124_xyz.txt````" to "````xyz_123.txt````":

    RR.exe *.txt "([0-9]+)_([a-z]+)" "$2_$1"

# Version History

* v1.0 - 2012-01-30: Initial release
* v1.1 - 2012-12-15: Added /r option

# Credits

Nic Jansma (http://nicj.net)