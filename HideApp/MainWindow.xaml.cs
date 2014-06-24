using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HideApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            CAppCollection.Init();
            LvApps.ItemsSource = CAppCollection.Collect;
            //LvApps.AddHandler(MouseDoubleClickEvent,new MouseButtonEventHandler (ItemDoubleClick));
            //LvApps.AddHandler(MouseRightButtonUpEvent, new MouseButtonEventHandler(ItemRightClick));
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
                }
                cm.Items.Insert(0, mi);
                //edit
                MenuItem mib = new MenuItem();
                mib.Header = "Edit";
                mib.Click += CAppEditCommand;
                cm.Items.Add(mib);
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

        void CAppRunCommand(object sender, RoutedEventArgs e)
        {
            var app = GetAppFromContextMenu(sender);
            if (!app.IsRunning)
            {
                try
                {
                    app.Process = Process.Start(app.Command);
                    while (app.Process.MainWindowHandle.ToInt64() == 0)
                    {
                        Thread.Sleep(100);
                    }
                    ShowWindow(app.Process.MainWindowHandle, ShowWindowCommands.Hide);
                    app.IsRunning = true;
                }
                catch (Exception)
                {

                }
            }
            Console.WriteLine("run " + app.Command);
        }
        private void CAppStopCommand(object sender, RoutedEventArgs args)
        {
            var app = GetAppFromContextMenu(sender);
            if (app.IsRunning)
            {
                app.Process.Kill();
                app.IsRunning = false;
            }
            Console.WriteLine("stop " + app.Command);
        }
        private void CAppVisiableCommand(object sender, RoutedEventArgs args)
        {
            var app = GetAppFromContextMenu(sender);
            if (app.IsVisible)
            {
                ShowWindow(app.Process.MainWindowHandle, ShowWindowCommands.Hide);
                app.IsVisible = false;
            }
            else
            {
                ShowWindow(app.Process.MainWindowHandle, ShowWindowCommands.Show);
                app.IsVisible = true;
            }
        }

        private void CAppEditCommand(object sender, RoutedEventArgs args)
        {
            var app = GetAppFromContextMenu(sender);
            CAppWindow appWindow = new CAppWindow(app);
            appWindow.ShowDialog();
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

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, ShowWindowCommands nCmdShow);

    }
}
