﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HideApp.Annotations;
using HideApp.Properties;

namespace HideApp
{
    public class CApp : INotifyPropertyChanged
    {
        public const char TagChar = '`';

        public CApp()
        {
        }

        public CApp(String settingString)
        {
            String[] ss = settingString.Split(TagChar);
            _name = ss[0];
            _path = ss[1];
            _args = ss[2];
        }

        public int Id
        {
            get { return CAppCollection.Collect.IndexOf(this); }
        }


        private String _name;

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        public String Command
        {
            get { return Path + " " + Args; }
        }

        private String _path;

        public string Path
        {
            get { return _path; }
            set
            {
                _path = value;
                OnPropertyChanged("Path");
                OnPropertyChanged("Command");
            }
        }

        private String _args;

        public string Args
        {
            get { return _args; }
            set
            {
                _args = value;
                OnPropertyChanged("Args");
                OnPropertyChanged("Command");
            }
        }

        public String State
        {
            get { return _isRunning ? ("Running/" + (_isVisible ? "Show" : "Hide")) : "Ready"; }
        }

        private bool _isRunning = false;

        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                _isVisible = value;
                CAppCollection.ShowWindow(_mainWindowHandle,
                    value ? ShowWindowCommands.Show : ShowWindowCommands.Hide);
                OnPropertyChanged("State");
            }
        }

        private IntPtr _mainWindowHandle = IntPtr.Zero;
        public bool RefreshMainWindowHandle()
        {
            if (this.Process.MainWindowHandle != IntPtr.Zero)
                _mainWindowHandle = this.Process.MainWindowHandle;
            else
            {
                List<IntPtr> hwnds = new List<IntPtr>();
                EnumReport.EnumWindows(new EnumWindowsCallBackPtr((hwnd, lp) =>
                {
                    int pid;
                    EnumReport.GetWindowThreadProcessId(hwnd, out pid);
                    if (pid == this.Process.Id)
                    {
                        int top = EnumReport.GetWindow(hwnd, EnumReport.GetWindow_Cmd.GW_OWNER);
                        Console.WriteLine("process {0}, hwnd {1}, top {2}", pid, hwnd, top);
                        hwnds.Add(hwnd);
                    }
                    //Console.WriteLine("process {0}, hwnd {1}", pid, hwnd);
                    return true;
                }), 0);
                if(hwnds.Count > 0)
                {
                    _mainWindowHandle = hwnds.Where(x => EnumReport.IsWindowVisible(x)).ToArray()[0];
                }
            }
            return _mainWindowHandle != IntPtr.Zero;
        }

        public bool IsRunning
        {
            get { return _isRunning; }
            set
            {
                _isRunning = value;
                OnPropertyChanged("State");
            }
        }

        private bool _isVisible = false;

        public Process Process;

        public String WorkDirectory
        {
            get
            {
                FileInfo file = new FileInfo(_path);
                return file.DirectoryName;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public String ToSettingString()
        {
            return String.Format("{1}{0}{2}{0}{3}", TagChar, Name, Path, Args);
        }

        public void Exit()
        {
            if (this.Process == null)
                return;
            try
            {
                this.Process.Kill();
            }
            catch (InvalidOperationException)
            {
            }
            this.IsRunning = false;
            this.Process = null;
            _mainWindowHandle = IntPtr.Zero;
            Console.WriteLine("Exited " + this.Name);
        }
    }

    public class CAppCollection
    {
        public static ObservableCollection<CApp> Collect;

        public static void Init()
        {
            if (Collect != null) return;
            StringCollection settingCollection = Settings.Default.CApps;
            if (settingCollection == null) settingCollection = new StringCollection();
            Collect = new ObservableCollection<CApp>();
            foreach (var ss in settingCollection)
            {
                Collect.Add(new CApp(ss));
            }
        }

        public static void Add(CApp app)
        {
            Collect.Add(app);
            Save();
        }

        public static void Update(CApp app, String name, String path, String args)
        {
            app.Name = name.Trim();
            app.Path = path.Trim();
            app.Args = args.Trim();
            Save();
        }

        public static void Remove(CApp app)
        {
            Collect.Remove(app);
            Save();
        }

        private static void Save()
        {
            StringCollection settingCollection = new StringCollection();
            settingCollection.AddRange(Collect.Select(app => app.ToSettingString()).ToArray());
            Settings.Default.CApps = settingCollection;
            Settings.Default.Save();
        }

        public static CApp GetAppByProcess(Process s)
        {
            return Collect.Single(a => a.Process == s);
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hWnd, ShowWindowCommands nCmdShow);
    }
}
