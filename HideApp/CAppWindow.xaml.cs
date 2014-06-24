using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace HideApp
{
    /// <summary>
    /// CAppWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CAppWindow : Window
    {
        private CApp _capp;
        public CAppWindow()
        {
            InitializeComponent();
        }
        public CAppWindow(CApp app)
        {
            InitializeComponent();
            _capp = app;
            tbxName.Text = app.Name;
            tbxPath.Text = app.Path;
            tbxArgs.Text = app.Args;
        }

        private void BrowserButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            Nullable<bool> result = dialog.ShowDialog();
            if (result == true)
            {
                tbxPath.Text = dialog.FileName;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_capp == null)
            {
                CApp app = new CApp {Name = tbxName.Text, Path = tbxPath.Text, Args = tbxArgs.Text};
                CAppCollection.Add(app);
            }
            else
            {
                CAppCollection.Update(_capp, tbxName.Text, tbxPath.Text, tbxArgs.Text);
            }

            Close();
        }
    }
}
