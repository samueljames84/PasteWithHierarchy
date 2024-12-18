using System;
using System.Collections.Generic;
using System.Drawing;
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
                    List<string> copiedFiles = new List<string>();

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
                              copiedFiles.Add(relativePath);
                         }
                         else if (Directory.Exists(sourcePath))
                         {
                              CopyDirectory(sourcePath, destinationPath);
                              copiedFiles.Add(relativePath);
                         }
                    }
                    catch (Exception ex)
                    {
                         Console.WriteLine($"Error copying file {sourcePath}: {ex.Message}");
                    }
                    }

                    Console.WriteLine("Files pasted successfully.");
                    ShowTooltip(copiedFiles.ToArray());
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

     static void ShowTooltip(string[] filePaths)
     {
          Application.EnableVisualStyles();
          // Create tooltip form
          Form tooltipForm = new Form
          {
               FormBorderStyle = FormBorderStyle.None,
               ShowInTaskbar = false,
               StartPosition = FormStartPosition.Manual,
               Size = new Size(Screen.PrimaryScreen.WorkingArea.Width / 3, Screen.PrimaryScreen.WorkingArea.Height / 8),
               BackColor = Color.Black,
               Opacity = 0.9
          };

          // Position the form at bottom right of screen
          tooltipForm.Location = new Point(
               Screen.PrimaryScreen.WorkingArea.Width - tooltipForm.Width - 5,
               Screen.PrimaryScreen.WorkingArea.Height - tooltipForm.Height - 5
          );

          // Create label for file paths
          Label label = new Label
          {
               Dock = DockStyle.Fill,
               Text = $"{filePaths.Length} Copied files:\n" + string.Join("\n", filePaths),
               ForeColor = Color.White,
               TextAlign = ContentAlignment.TopLeft,
               Font = new Font("Consolas", 10)
          };

          tooltipForm.Controls.Add(label);

          // Show the form
          tooltipForm.Show();

          // Close the form after 500ms
          System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer
          {
               Interval = 1000
          };
          timer.Tick += (sender, e) =>
          {
               tooltipForm.Close();
               timer.Stop();
          };
          timer.Start();

          // Run the message loop to ensure the form displays
          Application.Run(tooltipForm);
     }
}
}
