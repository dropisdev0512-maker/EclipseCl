using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace EclipseClient.Services;

public static class InjectionService
{
    public static event Action<string, bool>? InjectionCompleted;

    private static readonly string DllName = "EclipseCore.dll";

    public static bool IsInjected { get; private set; }

    public static InjectionResult Inject()
    {
        var process = ProcessMonitorService.GetMinecraftProcess();
        if (process == null)
        {
            var fail = new InjectionResult(false, "Minecraft (javaw.exe) not found. Launch Minecraft first.");
            InjectionCompleted?.Invoke(fail.Message, false);
            return fail;
        }

        var dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "native", DllName);
        if (!File.Exists(dllPath))
            dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DllName);

        if (!File.Exists(dllPath))
        {
            process.Dispose();
            var fail = new InjectionResult(false, $"DLL not found: {DllName}. Run build.bat to compile EclipseCore.");
            InjectionCompleted?.Invoke(fail.Message, false);
            return fail;
        }

        dllPath = Path.GetFullPath(dllPath);

        try
        {
            var result = InjectDll(process.Id, dllPath);
            process.Dispose();

            if (result)
            {
                IsInjected = true;
                var success = new InjectionResult(true, "Successfully injected into Minecraft!");
                InjectionCompleted?.Invoke(success.Message, true);
                ModuleBridge.Initialize();
                return success;
            }

            var failResult = new InjectionResult(false, "Injection failed. Run as Administrator.");
            InjectionCompleted?.Invoke(failResult.Message, false);
            return failResult;
        }
        catch (Exception ex)
        {
            process.Dispose();
            var fail = new InjectionResult(false, $"Injection error: {ex.Message}");
            InjectionCompleted?.Invoke(fail.Message, false);
            return fail;
        }
    }

    private static bool InjectDll(int processId, string dllPath)
    {
        IntPtr hProcess = OpenProcess(
            ProcessAccessFlags.All,
            false,
            processId);

        if (hProcess == IntPtr.Zero) return false;

        try
        {
            var dllBytes = Encoding.Unicode.GetBytes(dllPath + "\0");
            IntPtr allocMem = VirtualAllocEx(
                hProcess,
                IntPtr.Zero,
                (uint)dllBytes.Length,
                AllocationType.Commit | AllocationType.Reserve,
                MemoryProtection.ReadWrite);

            if (allocMem == IntPtr.Zero) return false;

            if (!WriteProcessMemory(hProcess, allocMem, dllBytes, (uint)dllBytes.Length, out _))
                return false;

            IntPtr loadLibrary = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryW");
            if (loadLibrary == IntPtr.Zero) return false;

            IntPtr hThread = CreateRemoteThread(
                hProcess,
                IntPtr.Zero,
                0,
                loadLibrary,
                allocMem,
                0,
                IntPtr.Zero);

            if (hThread == IntPtr.Zero) return false;

            WaitForSingleObject(hThread, 5000);
            CloseHandle(hThread);
            return true;
        }
        finally
        {
            CloseHandle(hProcess);
        }
    }

    #region Win32

    [Flags]
    private enum ProcessAccessFlags : uint
    {
        All = 0x001F0FFF
    }

    [Flags]
    private enum AllocationType : uint
    {
        Commit = 0x1000,
        Reserve = 0x2000
    }

    [Flags]
    private enum MemoryProtection : uint
    {
        ReadWrite = 0x04
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize,
        AllocationType flAllocationType, MemoryProtection flProtect);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer,
        uint nSize, out int lpNumberOfBytesWritten);

    [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
    private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize,
        IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

    [DllImport("kernel32.dll")]
    private static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

    [DllImport("kernel32.dll")]
    private static extern bool CloseHandle(IntPtr hObject);

    #endregion
}

public record InjectionResult(bool Success, string Message);
