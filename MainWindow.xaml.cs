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

//TODO:设置语言
//TODO:添加版本及版权信息
//TODO:测试发布
//TODO:finish the user guide
namespace SpellTracker
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public SpellWindow spellWindow;

        KeyBoardHook _keyboardHook;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, RoutedEventArgs e)
        {
            //配置log4
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo("log4net.config"));
            Log.Info("===============Start log=================");

            _keyboardHook = new KeyBoardHook();
            _keyboardHook.SetHook();
            _keyboardHook.SetOnKeyDownEvent(Win32_Keydown);
        }

        private void Win32_Keydown(Key key)
        {
            switch (key)
            {
                case Key.Add:
                    {
                        spellWindow.Type();
                    }
                    break;
            }
        }
        private void SpellTrackerToggle_Checked(object sender, RoutedEventArgs e)
        {
            spellWindow = new SpellWindow((int)Slider_Shift.Value,(bool) FTOnly.IsChecked);
            spellWindow.Show();
            Slider_Shift.IsEnabled = false;
            FTOnly.IsEnabled = false;
        }

        private void SpellTrackerToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            spellWindow.Close();
            spellWindow = null;
            Slider_Shift.IsEnabled = true;
            FTOnly.IsEnabled = true;
        }

        private void Window_Closing(object sender, EventArgs e)
        {

            if (!Slider_Shift.IsEnabled)
            {
                spellWindow.Close();
            }
            _keyboardHook.UnHook();
            Log.Info("UnHooked");
            Log.Info("===============End log=================");
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }

        private void PackIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }
    }
}
