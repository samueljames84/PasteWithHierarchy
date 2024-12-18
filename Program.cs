using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ClipboardFileTracker
{
class Program
{
     [STAThread] // Required for clipboard operations
     static void Main(string[] args)
     {
          if (args.Length < 1)
          {
               Console.WriteLine("Please provide a destination folder path as an argument.");
               return;
          }

          string destinationFolder = args[0];

          if (!Directory.Exists(destinationFolder))
          {
               Console.WriteLine("The specified destination folder does not exist.");
               return;
          }

          try
          {
               if (Clipboard.ContainsFileDropList())
               {
                    var fileDropList = Clipboard.GetFileDropList().Cast<string>().ToList();
                    string commonPath = GetCommonPath(fileDropList);

                    foreach (string sourcePath in fileDropList)
                    {
                    try
                    {
                         string relativePath = string.IsNullOrEmpty(commonPath) ? Path.GetFileName(sourcePath) : sourcePath.Substring(commonPath.Length).TrimStart(Path.DirectorySeparatorChar);
                         string destinationPath = Path.Combine(destinationFolder, relativePath);

                         if (File.Exists(sourcePath))
                         {
                              Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));
                              File.Copy(sourcePath, destinationPath, true);
                         }
                         else if (Directory.Exists(sourcePath))
                         {
                              CopyDirectory(sourcePath, destinationPath);
                         }
                    }
                    catch (Exception ex)
                    {
                         Console.WriteLine($"Error copying file {sourcePath}: {ex.Message}");
                    }
                    }

                    Console.WriteLine("Files pasted successfully.");
               }
               else
               {
                    Console.WriteLine("No files in the clipboard.");
               }
          }
          catch (Exception ex)
          {
               Console.WriteLine($"Error accessing clipboard: {ex.Message}");
          }
     }

     private static string GetCommonPath(List<string> paths)
     {
          if (paths == null || paths.Count == 0)
               return string.Empty;

          string commonPath = paths[0];
          foreach (string path in paths.Skip(1))
          {
               int minLength = Math.Min(commonPath.Length, path.Length);
               int lastSeparatorIndex = -1;

               for (int i = 0; i < minLength; i++)
               {
                    if (commonPath[i] != path[i])
                    break;
                    if (commonPath[i] == Path.DirectorySeparatorChar)
                    lastSeparatorIndex = i;
               }

               commonPath = commonPath.Substring(0, lastSeparatorIndex + 1);
          }

          return commonPath;
     }

     private static void CopyDirectory(string sourceDir, string destinationDir)
     {
          Directory.CreateDirectory(destinationDir);

          foreach (string file in Directory.GetFiles(sourceDir))
          {
               string destFile = Path.Combine(destinationDir, Path.GetFileName(file));
               File.Copy(file, destFile, true);
          }

          foreach (string directory in Directory.GetDirectories(sourceDir))
          {
               string destDirectory = Path.Combine(destinationDir, Path.GetFileName(directory));
               CopyDirectory(directory, destDirectory);
          }
     }
}
}
