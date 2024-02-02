using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace NanoCoreRemover
{
    public partial class Form1 : Form
    {
        const int STATUS_SUCCESS = 0;
        const int ProcessBreakOnTermination = 0x1D;

        [DllImport("ntdll.dll", SetLastError = true)]
        static extern int NtSetInformationProcess(IntPtr processHandle, int processInformationClass, ref int processInformation, int processInformationLength);
        private BackgroundWorker backgroundWorker1;
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        public Form1()
        {
            InitializeComponent();
            InitializeBackgroundWorker();
        }

        private void InitializeBackgroundWorker()
        {
            backgroundWorker1 = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };

            backgroundWorker1.DoWork += new DoWorkEventHandler(backgroundWorker1_DoWork);
            backgroundWorker1.ProgressChanged += new ProgressChangedEventHandler(backgroundWorker1_ProgressChanged);
            backgroundWorker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker1_RunWorkerCompleted);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            progressBar1.Value = 0;
            button2.Enabled = false;
            listView1.Items.Clear();
            if (!backgroundWorker1.IsBusy)
            {

                backgroundWorker1.RunWorkerAsync(argument: null);
                button1.Enabled = false;
            }
        }
        static void SetProcessCriticalStatus(int pid, bool setStatus, Action<string, Color> logAction)
        {
            try
            {
                Process process = Process.GetProcessById(pid);
                if (process == null)
                {
                    logAction("Process not found.", Color.Red);
                    return;
                }

                int isCritical = setStatus ? 1 : 0;
                int result = NtSetInformationProcess(process.Handle, ProcessBreakOnTermination, ref isCritical, sizeof(int));
                if (result == STATUS_SUCCESS)
                {
                    logAction($"Process {(setStatus ? "set as" : "removed from")} critical status successfully.", Color.Green);
                }
                else
                {
                    logAction($"Failed to change process critical status. Error: {result}", Color.Red);
                }
            }
            catch (Exception ex)
            {
                logAction($"An error occurred: {ex.Message}", Color.Red);
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {

            FileManager fileManager = new FileManager(progressBar1, listView1, label1);
            fileManager.CheckAndScan();
            button1.Enabled=true;
            button2.Enabled = true;

        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

            progressBar1.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                MessageBox.Show("Operation was canceled.");
            }
            else if (e.Error != null)
            {
                MessageBox.Show("An error occurred: " + e.Error.Message);
            }
            else
            {
                MessageBox.Show("Operation completed successfully.");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listView1.Items.Count == 0)
            {
                LogMessage("The list is empty.", Color.Red);
                return;
            }
            foreach (ListViewItem item in listView1.CheckedItems)
            {
                string filePath = item.SubItems[1].Text;
                try
                {
                    var runningProcesses = Process.GetProcesses();
                    foreach (var process in runningProcesses)
                    {
                        try
                        {
                            if (string.Equals(process.MainModule.FileName, filePath, StringComparison.OrdinalIgnoreCase))
                            {
                                SetProcessCriticalStatus(process.Id, false, LogMessage);
                                process.Kill();
                                process.WaitForExit();
                                LogMessage($"Process {process.ProcessName} killed.", Color.DarkGreen);
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error checking process: {ex.Message}");
                            LogMessage($"Error checking process: {ex.Message}", Color.Red);
                            LogMessage($"Process is probably not active.", Color.DarkOrange);
                        }
                    }

                    try
                    {
                        File.Delete(filePath);
                        LogMessage($"File {filePath} deleted.", Color.DarkGreen);
                    } catch
                    {

                    }
                }
                catch (Exception ex)
                {
                    LogMessage($"An error occurred: {ex.Message}", Color.Red);
                }
            }
        }


        public void LogMessage(string message, Color color)
        {
            richTextBox1.SelectionStart = richTextBox1.TextLength;
            richTextBox1.SelectionLength = 0;
            richTextBox1.SelectionColor = color;
            richTextBox1.AppendText(message + "\n");
            richTextBox1.SelectionColor = richTextBox1.ForeColor;
        }



        private void Form1_Load(object sender, EventArgs e)
        {
            this.listView1.CheckBoxes = true;
            LogMessage("Welcome to NanoCore Remover!\nWhen you click on Scan, it will take more than a minute or even an hour in some cases to calculate the file count, so the count might remain at 0 for a while.\nWe appreciate your patience.", Color.DarkGoldenrod);
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}