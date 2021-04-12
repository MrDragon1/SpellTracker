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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WebSocketSharp;
using System.Diagnostics;
using SpellTracker.Control;
using SpellTracker.Data;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;

namespace SpellTracker
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public SpellWindow spellWindow;
        public MainWindow()
        {
            InitializeComponent();
            this.AllowsTransparency = true;
            this.WindowStyle = WindowStyle.None;
        }

        private void Main_Load(object sender, RoutedEventArgs e)
        {
            //配置log4
            Log.Info("===============Start log=================");
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo("../../log4net.config"));

        }



        private void SpellTrackerToggle_Checked(object sender, RoutedEventArgs e)
        {
            spellWindow = new SpellWindow((int)Slider_Shift.Value);
            spellWindow.Show();
        }

        private void SpellTrackerToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            spellWindow.Close();
            spellWindow = null;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (spellWindow.IsActive)
            {
                spellWindow.Close();
                spellWindow = null;
            }
            Log.Info("===============End log=================");
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }
    }
}
