using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HideApp
{
    public partial class MainWindow : Window
    {
        private System.Windows.Forms.NotifyIcon _notifyIcon;

        public MainWindow()
        {
            InitializeComponent();
            CAppCollection.Init();
            LvApps.ItemsSource = CAppCollection.Collect;
            //LvApps.AddHandler(MouseDoubleClickEvent,new MouseButtonEventHandler (ItemDoubleClick));
            //LvApps.AddHandler(MouseRightButtonUpEvent, new MouseButtonEventHandler(ItemRightClick));

            _notifyIcon = new System.Windows.Forms.NotifyIcon();
            _notifyIcon.BalloonTipText = "The app has been minimised. Click the tray icon to show.";
            _notifyIcon.BalloonTipTitle = Title;
            _notifyIcon.Text = Title;
            _notifyIcon.Icon = new System.Drawing.Icon(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico"));
            _notifyIcon.Click += NotifyIcon_Click;
        }

        public void onStart()
        {

        }

        protected void ItemDoubleClick(object sender, MouseButtonEventArgs routedEventArgs)
        {
            if (sender is ListViewItem)
            {
                var app = ((ListViewItem)sender).Content as CApp;
                if (app.IsRunning) CAppVisiableCommand(app, null);
                else CAppRunCommand(app, null);
            }
        }

        protected void ItemRightClick(object sender, MouseButtonEventArgs args)
        {
            if (sender is ListViewItem)
            {
                ListViewItem lvItem = (ListViewItem)sender;
                var app = lvItem.Content as CApp;
                Console.WriteLine("right" + app.Id);
                ContextMenu cm = new ContextMenu();
                cm.PlacementTarget = lvItem;
                MenuItem mi = new MenuItem();
                if (!app.IsRunning)
                {
                    mi.Header = "Run";
                    mi.Click += CAppRunCommand;
                }
                else
                {
                    mi.Header = "Stop";
                    mi.Click += CAppStopCommand;
                    MenuItem mia = new MenuItem();
                    mia.Header = app.IsVisible ? "Hide" : "Show";
                    mia.Click += CAppVisiableCommand;
                    cm.Items.Add(mia);
                    //restart
                    MenuItem miRestar = new MenuItem();
                    miRestar.Header = "Restar";
                    miRestar.Click += CAppRestarCommand;
                    cm.Items.Add(miRestar);
                }
                cm.Items.Insert(0, mi);
                //edit
                MenuItem mib = new MenuItem();
                mib.Header = "Edit";
                mib.Click += CAppEditCommand;
                cm.Items.Add(mib);
                //open Folder
                MenuItem mid = new MenuItem();
                mid.Header = "Open Folder";
                mid.Click += CAppOpenFolderCommand;
                cm.Items.Add(mid);
                //delet
                MenuItem mic = new MenuItem();
                mic.Header = "Delete";
                mic.Click += CAppDeleteCommand;
                cm.Items.Add(mic);

                cm.IsOpen = true;
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            CAppWindow appWindow = new CAppWindow();
            appWindow.ShowDialog();
        }



        private void CAppRunCommand(object sender, RoutedEventArgs e)
        {
            var app = GetAppFromContextMenu(sender);
            if (!app.IsRunning)
            {

                ProcessStartInfo info = new ProcessStartInfo
                {
                    FileName = app.Path,
                    Arguments = app.Args,
                    WorkingDirectory = app.WorkDirectory
                };
                app.Process = Process.Start(info);
                app.Process.EnableRaisingEvents = true;
                app.Process.Exited += Process_Exited;
                while (!app.RefreshMainWindowHandle())
                {
                    Thread.Sleep(300);
                }
                app.IsVisible = false;

            }
            Console.WriteLine("run " + app.Command);
        }

        void Process_Exited(object sender, EventArgs e)
        {
            var app = CAppCollection.GetAppByProcess((Process)sender);
            if (app != null)
            {
                app.Exit();
            }

        }

        private void CAppRestarCommand(object sender, RoutedEventArgs e)
        {
            CAppStopCommand(sender, e);
            var app = GetAppFromContextMenu(sender);
            while (app.IsRunning)
            {
                Thread.Sleep(100);
            }
            CAppRunCommand(sender, e);
        }

        private void CAppStopCommand(object sender, RoutedEventArgs args)
        {
            var app = GetAppFromContextMenu(sender);
            app.Exit();
            Console.WriteLine("stop " + app.Command);
        }

        private void CAppVisiableCommand(object sender, RoutedEventArgs args)
        {
            var app = GetAppFromContextMenu(sender);
            app.IsVisible = !app.IsVisible;
        }

        private void CAppEditCommand(object sender, RoutedEventArgs args)
        {
            var app = GetAppFromContextMenu(sender);
            CAppWindow appWindow = new CAppWindow(app);
            appWindow.ShowDialog();
        }
        private void CAppOpenFolderCommand(object sender, RoutedEventArgs args)
        {
            var app = GetAppFromContextMenu(sender);
            CAppWindow appWindow = new CAppWindow(app);
            Process.Start(Path.GetDirectoryName(app.Path));
        }

        private void CAppDeleteCommand(object sender, RoutedEventArgs args)
        {
            var app = GetAppFromContextMenu(sender);
            CAppStopCommand(app, null);
            CAppCollection.Remove(app);
        }

        private CApp GetAppFromContextMenu(object sender)
        {
            if (sender is MenuItem)
            {
                MenuItem mnu = sender as MenuItem;
                ListViewItem lvItem = ((ContextMenu)mnu.Parent).PlacementTarget as ListViewItem;
                return lvItem.Content as CApp;
            }
            else if (sender is CApp)
            {
                return sender as CApp;
            }
            return null;
        }


        #region close to tray

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = true;
                e.Cancel = true;
                Hide();
            }
            else
            {
                foreach (var app in CAppCollection.Collect)
                {
                    if (app.Process != null && !app.Process.HasExited)
                    {
                        app.Process.Kill();
                    }
                }
            }
        }

        private void NotifyIcon_Click(object sender, EventArgs e)
        {
            Show();
            _notifyIcon.Visible = false;

        }

        #endregion

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            _notifyIcon = null;
            if (MessageBox.Show("Close all running processes?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                CAppCollection.CloseAll();
            }
            KillProcess.Stop();
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            KillProcess.Run();
        }

        private void EditKP_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(KillProcess.ConfigFilePath);
        }
    }
}
