# NanoCore Remover

## About

I developed NanoCore Remover out of frustration with antivirus solutions that overload you with unnecessary features yet fail to effectively eliminate NanoCore. The truly insidious aspect lies in how crypters can evade detection by your antivirus, disable it, and subsequently allow NanoCore to operate unimpeded.

## How Does NanoCore Remover Disable NanoCore's Critical Process Privileges?

The NanoCore Remover identifies NanoCore malware on your system and then strips it of its critical process status by setting this attribute to false. After demoting NanoCore from its critical status, the tool proceeds to safely terminate it. Below is the code snippet that facilitates this crucial step:

```csharp
const int STATUS_SUCCESS = 0;
const int ProcessBreakOnTermination = 0x1D;

[DllImport("ntdll.dll", SetLastError = true)]
static extern int NtSetInformationProcess(IntPtr processHandle, int processInformationClass, ref int processInformation, int processInformationLength);

static void SetProcessCriticalStatus(int pid, bool setStatus, Action<string, Color> logAction)
{
    try
    {
        Process process = Process.GetProcessById(pid);
        if (process == null)
        {
            logAction("Couldn't find the process.", Color.Red);
            return;
        }

        int isCritical = setStatus ? 1 : 0;
        int result = NtSetInformationProcess(process.Handle, ProcessBreakOnTermination, ref isCritical, sizeof(int));
        if (result == STATUS_SUCCESS)
        {
            logAction($"Process {(setStatus ? "is now vulnerable" : "is back to normal")} successfully.", Color.Green);
        }
        else
        {
            logAction($"Couldn't change the process. Error: {result}", Color.Red);
        }
    }
    catch (Exception ex)
    {
        logAction($"Something went wrong: {ex.Message}", Color.Red);
    }
}
```
Since you don't have the same logging setup as me, I've tweaked the code to use Console.WriteLine instead. Check it out:
```csharp
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

const int STATUS_SUCCESS = 0;
const int ProcessBreakOnTermination = 0x1D;

[DllImport("ntdll.dll", SetLastError = true)]
static extern int NtSetInformationProcess(IntPtr processHandle, int processInformationClass, ref int processInformation, int processInformationLength);

static void SetProcessCriticalStatus(int pid, bool setStatus)
{
    try
    {
        Process process = Process.GetProcessById(pid);
        if (process == null)
        {
            Console.WriteLine("Couldn't find the process.");
            return;
        }

        int isCritical = setStatus ? 1 : 0;
        int result = NtSetInformationProcess(process.Handle, ProcessBreakOnTermination, ref isCritical, sizeof(int));
        if (result == STATUS_SUCCESS)
        {
            Console.WriteLine($"Process {(setStatus ? "is now vulnerable" : "is back to normal")} successfully.");
        }
        else
        {
            Console.WriteLine($"Couldn't change the process. Error: {result}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Something went wrong: {ex.Message}");
    }
}

