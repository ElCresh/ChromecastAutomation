using System;
using System.IO;
using System.Diagnostics;
using System.Xml;
using System.Threading;
using System.Runtime.InteropServices;
using System.Net.NetworkInformation;

namespace VLCAutoChromecast
{
    class Program
    {
        private readonly static string MEDIA_PATH = "media" + Path.DirectorySeparatorChar;
        private readonly static string VLC_PATH_X64 = "C:\\Program Files\\VideoLAN\\VLC\\vlc.exe";
        private readonly static string VLC_PATH_X86 = "C:\\Program Files(x86)\\VideoLAN\\VLC\\vlc.exe";
        private readonly static string VLC_PATH_WXP = "C:\\Programmi\\VideoLAN\\VLC\\vlc.exe";
        private readonly static string VLC_PATH_UBT = "/usr/bin/vlc";
#if DEBUG
        private readonly static string VLC_MINIMIZED_ARGS = "-I rc --verbose=1 --qt-start-minimized";
#else
        private readonly static string VLC_MINIMIZED_ARGS = "--qt-start-minimized";
#endif

        public static void GeneratePlaylist()
        {
            string current_directory = Directory.GetCurrentDirectory();
            string[] media_files = Directory.GetFiles(MEDIA_PATH);
            string[] lines = new string[(media_files.Length * 2) + 1];

            int index = 0;

            // Cleaning old playlist
            File.Delete(current_directory + "\\Playlist.m3u");


            // Generating new playlist
            foreach (string media_file in media_files)
            {
                if (
                    !media_file.Equals("media" + Path.DirectorySeparatorChar + "STOP") &&
                    !media_file.Equals("media" + Path.DirectorySeparatorChar + "stop") &&
                    !media_file.Equals("media" + Path.DirectorySeparatorChar + "STOP.ntsf") &&
                    !media_file.Equals("media" + Path.DirectorySeparatorChar + "stop.ntsf")
                )
                {
                    lines[index] = "#EXTINF:-1,.";
                    lines[index + 1] = current_directory + Path.DirectorySeparatorChar + media_file;
                    index += 2;
                }
            }

            lines[index] = "vlc://quit";


            File.WriteAllLines(current_directory + Path.DirectorySeparatorChar + "Playlist.m3u", lines);
        }

        public static void VlcKiller()
        {
            foreach (var process_vlc in Process.GetProcessesByName("vlc"))
            {
                process_vlc.Kill();
            }
        }

        public static void VlcRunner(String chromecast_ip, String chromecast_vlc_args)
        {
            string current_directory = Directory.GetCurrentDirectory();
            string vlc_path = "";

            if (File.Exists(VLC_PATH_UBT))
            {
                vlc_path = VLC_PATH_UBT;
            }
            else if (File.Exists(VLC_PATH_X64))
            {
                vlc_path = VLC_PATH_X64;
            }
            else if (File.Exists(VLC_PATH_X86))
            {
                vlc_path = VLC_PATH_X86;
            }
            else if (File.Exists(VLC_PATH_WXP))
            {
                vlc_path = VLC_PATH_WXP;
            }

            if (vlc_path != "")
            {
                while (true)
                {
                    GeneratePlaylist();

                    Process process = null;

                    ProcessStartInfo startInfo = new ProcessStartInfo(vlc_path);
                    startInfo.Arguments = VLC_MINIMIZED_ARGS + " \"" + current_directory + Path.DirectorySeparatorChar + "Playlist.m3u\" " + chromecast_vlc_args;
                    process = Process.Start(startInfo);
                    process.WaitForExit();
                }
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("************************");
            Console.WriteLine("* Chromecast Automator *");
            Console.WriteLine("* Notiosoft - Ver. 1.2 *");
            Console.WriteLine("************************");

            string vlc_path = "";
            bool is_running = false;
            bool is_online = false;
            bool is_stop_requested = false;
            Thread t = null;
            Ping ping = new Ping();

            if (File.Exists("settings.xml"))
            {
                if (File.Exists(VLC_PATH_UBT))
                {
                    vlc_path = VLC_PATH_UBT;
                }
                else if (File.Exists(VLC_PATH_X64))
                {
                    vlc_path = VLC_PATH_X64;
                }
                else if (File.Exists(VLC_PATH_X86))
                {
                    vlc_path = VLC_PATH_X86;
                }
                else if (File.Exists(VLC_PATH_WXP))
                {
                    vlc_path = VLC_PATH_WXP;
                }

                if (vlc_path != "")
                {
                    // Terminating all existing vlc istances
                    VlcKiller();

                    XmlDocument doc = new XmlDocument();
                    doc.Load("settings.xml");
                    XmlNode node = doc.DocumentElement.SelectSingleNode("/settings/chromecast");

                    string chromecast_ip = node.Attributes["ip"].Value;
                    string chromecast_vlc_args = "--sout \"#chromecast\" --sout-chromecast-ip=" + chromecast_ip + " --demux-filter=demux_chromecast";

                    Console.WriteLine("> Avvio stream chromecast");
                    Console.WriteLine("\nPer interrompere l'esecuzione chiudere la  finestra...");

                    while (true)
                    {
                        if(
                            File.Exists("media" + Path.DirectorySeparatorChar + "STOP") ||
                            File.Exists("media" + Path.DirectorySeparatorChar + "stop") ||
                            File.Exists("media" + Path.DirectorySeparatorChar + "STOP.ntsf") ||
                            File.Exists("media" + Path.DirectorySeparatorChar + "stop.ntsf"))
                        {
                            is_stop_requested = true;
                        }
                        else
                        {
                            is_stop_requested = false;
                        }

                        try
                        {
                            PingReply pingReply = ping.Send(chromecast_ip);

                            if (pingReply.Status == IPStatus.Success)
                            {
                                is_online = true;
                            }
                            else
                            {
                                is_online = false;
                            }
                        }
                        catch (PingException e)
                        {
                            is_online = false;
                        }

                        if (is_online && !is_stop_requested) {
                            if (!is_running)
                            {
                                t = new Thread(() => VlcRunner(chromecast_ip, chromecast_vlc_args));
                                t.Start();
                                is_running = true;
                            }
                        }
                        else
                        {
                            if (is_running)
                            {
                                t.Abort();
                                VlcKiller();
                                is_running = false;
                            }
                        }

                        Thread.Sleep(1000);
                    }

                }
                else
                {
                    Console.WriteLine("] Impossibile trovare VLC. Verrificare che sia correttamente installato nel dispositivo.");
                    Console.WriteLine("Premi un tasto per uscire");
                    Console.ReadKey();
                }
            }
            else
            {
                // Code to create basic XML settings file
                XmlDocument xmlDoc = new XmlDocument();
                XmlNode rootNode = xmlDoc.CreateElement("settings");
                xmlDoc.AppendChild(rootNode);

                XmlNode chromecastNode = xmlDoc.CreateElement("chromecast");
                XmlAttribute attribute = xmlDoc.CreateAttribute("ip");
                attribute.Value = "192.168.1.10";
                chromecastNode.Attributes.Append(attribute);
                rootNode.AppendChild(chromecastNode);

                xmlDoc.Save("settings.xml");

                Console.WriteLine("] Impossibile trovare il file di configurazione. Ne sarà generato uno nuovo. Per favore verifica le impostazioni e riavvia il programma.");
                Console.WriteLine("Premi un tasto per uscire");
                Console.ReadKey();
            }
        }
    }
}
