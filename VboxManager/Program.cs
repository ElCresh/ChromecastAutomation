using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace VboxManager
{
    class Program
    {
        private readonly static string VBOXM_PATH_X64 = "C:\\Program Files\\Oracle\\VirtualBox\\VBoxManage.exe";
        private readonly static string VBOXM_PATH_X86 = "C:\\Program Files(x86)\\Oracle\\VirtualBox\\VBoxManage.exe";
        private readonly static string VBOXM_PATH_WXP = "C:\\Programmi\\Oracle\\VirtualBox\\VBoxManage.exe";

        private static List<String> GetVms(String vboxm_path)
        {
            List<String> vms = new List<String>();

            Process process = null;
            ProcessStartInfo startInfo = new ProcessStartInfo(vboxm_path);
            startInfo.Arguments = "list vms";
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.CreateNoWindow = true;
            process = Process.Start(startInfo);
            while (!process.StandardOutput.EndOfStream)
            {
                vms.Add(process.StandardOutput.ReadLine().Split('{')[1].Split('}')[0]);
            }

            return vms;
        }

        private static List<String> GetVmsRunning(String vboxm_path)
        {
            List<String> vms = new List<String>();

            Process process = null;
            ProcessStartInfo startInfo = new ProcessStartInfo(vboxm_path);
            startInfo.Arguments = "list runningvms";
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.CreateNoWindow = true;
            process = Process.Start(startInfo);
            while (!process.StandardOutput.EndOfStream)
            {
                vms.Add(process.StandardOutput.ReadLine().Split('{')[1].Split('}')[0]);
            }

            return vms;
        }

        private static void StartVm(String vboxm_path, String vm)
        {
            Process process = null;
            ProcessStartInfo startInfo = new ProcessStartInfo(vboxm_path);
            startInfo.Arguments = "startvm " + vm;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.CreateNoWindow = true;
            process = Process.Start(startInfo);
            process.WaitForExit();
        }

        static void Main(string[] args)
        {
            Console.WriteLine("**************************************");
            Console.WriteLine("* Chromecast Automator - VboxManager *");
            Console.WriteLine("*        Notiosoft - Ver. 1.0        *");
            Console.WriteLine("**************************************");

            string current_directory = Directory.GetCurrentDirectory();
            string vboxm_path = "";
            Boolean first_cycle = true;

            if (File.Exists(VBOXM_PATH_X64))
            {
                vboxm_path = VBOXM_PATH_X64;
            }
            else if (File.Exists(VBOXM_PATH_X86))
            {
                vboxm_path = VBOXM_PATH_X86;
            }
            else if (File.Exists(VBOXM_PATH_WXP))
            {
                vboxm_path = VBOXM_PATH_WXP;
            }

            if (vboxm_path != "")
            {
                do {
                    List<String> vms = GetVms(vboxm_path);
                    List<String> vms_running = GetVmsRunning(vboxm_path);

                    vms.ForEach(delegate (String vm)
                    {
                        Boolean running = false;

                        vms_running.ForEach(delegate (String vm2)
                        {
                            if (vm == vm2)
                            {
                                running = true;
                            }
                        });

                        if (!running)
                        {
                            Console.WriteLine("Starting " + vm + "...");
                            StartVm(vboxm_path, vm);
                            Console.WriteLine(vm + " started");
                        }
                        else
                        {
                            if (first_cycle)
                                Console.WriteLine(vm + " already running");
                        }
                    });

                    first_cycle = false;

                    Thread.Sleep(2000);
                } while(true);

                //process.WaitForExit();
                Console.WriteLine("Premi un tasto per uscire");
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine("] Impossibile trovare VboxManager. Verrificare che VirtualBox sia correttamente installato nel dispositivo.");
                Console.WriteLine("Premi un tasto per uscire");
                Console.ReadKey();
            }
        }
    }
}
