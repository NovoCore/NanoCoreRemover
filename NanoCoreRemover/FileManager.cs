using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Reflection;
using System.Runtime.InteropServices;
using NanoCoreRemover;
using System.Threading.Tasks;
using System.Drawing;
using System.Collections.Generic;
using Microsoft.Win32;

public class FileManager
{
    private ProgressBar progressBar1;
    private ListView listView1;
    private Label label1;
    int count = 0;
    private object lockObj = new object();
    public FileManager(ProgressBar progressBar, ListView listView, Label textLabel)
    {
        progressBar1 = progressBar;
        listView1 = listView;
        label1 = textLabel;
    }
    string GetDownloadFolderPath()
    {
        return Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders", "{374DE290-123F-4565-9164-39C4925E467B}", String.Empty).ToString();
    }
    public void CheckAndScan()
    {
        string[] directories = {
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            GetDownloadFolderPath(),
            Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory),
            Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
            Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
            Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            Path.GetTempPath()
        };

        int totalFiles = directories.Sum(dir => CountFiles(dir));
        progressBar1.Invoke(new Action(() =>
        {
            progressBar1.Maximum = totalFiles;
            progressBar1.Value = 0;
        }));

        Parallel.ForEach(directories, (folder) =>
        {
            try
            {
                Console.WriteLine($"Scanning folder: {folder}");
                var files = GetFiles(folder, "*.*");
                foreach (string file in files)
                {
                    try
                    {
                        Console.WriteLine($"Processing file: {file}");
                        if (IsNanoCoreAssembly(file))
                        {
                            listView1.Invoke(new Action(() =>
                            {
                                ListViewItem item = new ListViewItem(Path.GetFileNameWithoutExtension(file));
                                item.SubItems.Add(file);
                                item.SubItems.Add(AsmName(file));
                                listView1.Items.Add(item);
                            }));
                        }

                        lock (lockObj)
                        {
                            count++;
                            progressBar1.Invoke(new Action(() => progressBar1.Value++));
                            label1.Invoke(new Action(() => label1.Text = "Files Counted: " + count));
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing file {file}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing folder {folder}: {ex.Message}");
            }
        });
    }

    private bool IsNanoCoreAssembly(string filePath)
    {
        try
        {
            var assemblyName = AssemblyName.GetAssemblyName(filePath);
            return (assemblyName.Name.Contains("NanoCore") && !(assemblyName.Name.Contains("NanoCoreRemover")));
        }
        catch
        {
            return false;
        }
    }

    private string AsmName(string filePath)
    {
        try
        {
            var assemblyName = AssemblyName.GetAssemblyName(filePath);
            return assemblyName.Name;
        }
        catch
        {
            return "error";
        }
    }

    private List<string> GetFiles(string path, string pattern)
    {
        var files = new List<string>();
        var stack = new Stack<string>();
        stack.Push(path);

        while (stack.Count > 0)
        {
            var currentDir = stack.Pop();
            try
            {
                files.AddRange(Directory.GetFiles(currentDir, pattern));
                foreach (var dir in Directory.GetDirectories(currentDir))
                {
                    stack.Push(dir);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Access denied to folder {currentDir}: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accessing folder {currentDir}: {ex.Message}");
            }
        }

        return files;
    }

    private int CountFiles(string directory)
    {
        int count = 0;
        try
        {
            var files = GetFiles(directory, "*.*");
            count = files.Count;
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Access denied to folder {directory}: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error counting files in {directory}: {ex.Message}");
        }
        return count;
    }
}
