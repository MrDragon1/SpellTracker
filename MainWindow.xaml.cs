using System;
using System.Text;
using System.Windows;
using System.Windows.Input;
using WebSocketSharp;
using System.Diagnostics;
using SpellTracker.Control;
using System.Net.Sockets;
using System.Net;
using System.Management;
using Newtonsoft.Json;

//TODO:setting page too empty
namespace SpellTracker
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public SpellWindow spellWindow;

        KeyBoardHook _keyboardHook;
        private Socket ValidationSocket;
        private IPAddress ips;
        private IPEndPoint ipNode;
        private bool Valid = false;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, RoutedEventArgs e)
        {
            //配置log4
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo("log4net.config"));
            Log.Info("===============Start log=================");

            Valid = true;
            ValidationText.Text = "验证成功！";

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
                case Key.F10:
                    {
                        spellWindow.Type();
                    }
                    break;
            }
        }
        private void SpellTrackerToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (Valid)
            {
                spellWindow = new SpellWindow((int)Slider_Shift.Value, (bool)FTOnly.IsChecked);
                spellWindow.Show();
                Slider_Shift.IsEnabled = false;
                FTOnly.IsEnabled = false;
            }
            else
            {
                //TODO:应该强调validation fail
            }

        }

        private void SpellTrackerToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            if (Valid)
            {
                spellWindow.Close();
                spellWindow = null;
                Slider_Shift.IsEnabled = true;
                FTOnly.IsEnabled = true;
            }
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

        private void Github_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try { System.Diagnostics.Process.Start("https://github.com/MrDragon1/SpellTracker"); } catch { }
        }

    }
    public class ValidJson
    {
        public bool IfValid { get; set; }
    }
}
