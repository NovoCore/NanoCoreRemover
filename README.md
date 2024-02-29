# NanoCore Remover

## About

I made NanoCore Remover because I was tired of antivirus programs that install a bunch of stuff you don't need and still don't get rid of NanoCore. Sometimes, NanoCore hides so well that it pretends to be something important on your computer, making it super tricky to remove.

## How It Works

Basically, NanoCore Remover looks through your computer to find anything related to NanoCore. Then, it uses a bit of code magic to make NanoCore not so tough anymore, allowing us to say goodbye to it for good.

Here's the piece of code that does the heavy lifting:

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

