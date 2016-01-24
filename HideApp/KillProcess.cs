using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HideApp
{
    public class KillProcess
    {
        private static bool _isRunning = false;
        private static Thread _background;
        public static string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "killprocess.txt");
        public static void Run()
        {
            if (_isRunning)
                return;
            _isRunning = true;
            _background = new Thread(Start);
            _background.Start();
        }
        static void Start()
        {
            while (true)
            {
                Console.WriteLine("Kill Process Running");
                Load();
                Thread.Sleep(5000);
            }
        }

        public static void Stop()
        {
            if (_isRunning)
                _background.Abort();
            _isRunning = false;
            _background.Abort();
        }
        public static void Load()
        {
            String[] lines = File.ReadAllLines(ConfigFilePath);
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (line.StartsWith("#") || line.Length == 0)
                {
                    continue;
                }
                string[] pair = line.Split('|');
                if (pair.Length != 2)
                {
                    Console.WriteLine("Error line {0}: {1}", i + 1, line);
                    continue;
                }
                string type = pair[0].Trim();
                string name = pair[1].Trim();
                if (name.EndsWith(".exe"))
                {
                    name = name.Substring(0, name.Length - 4);
                }
                var process = Process.GetProcessesByName(name);
                foreach (var p in process)
                {
                    p.Kill();
                    Console.WriteLine("Kill process {0}, {1}", p.Id, name);
                }
            }
        }
    }
}
