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

            try
            {
                ValidationSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                ips = IPAddress.Parse(Encoding.UTF8.GetString(Convert.FromBase64String("MzkuOTYuMTkuMTgy")));
                ipNode = new IPEndPoint(ips, 8210);
                IAsyncResult result = ValidationSocket.BeginConnect(ipNode, null, null);

                result.AsyncWaitHandle.WaitOne(2000);

                if (!result.IsCompleted)
                {
                    //Valid = false;
                    //ValidationText.Text = "连接失败，请重启程序。";
                    //ValidationText.FontSize = 18;
                    //ValidationSocket.Close();
                    //Log.fatal("Connected failed!");
                    Log.Info("Validation skip!");
                    Valid = true;
                    ValidationText.Text = "验证成功！";
                }
                else
                {
                    string rl = System.Environment.GetEnvironmentVariable("ComputerName");
                    //发送消息到服务端
                    rl = rl + "_" + Get_CPUID();
                    ValidationSocket.Send(Encoding.UTF8.GetBytes(rl));
                    byte[] buffer = new byte[1024];
                    int num = ValidationSocket.Receive(buffer);
                    string str = Encoding.UTF8.GetString(buffer, 0, num);
                    ValidationSocket.Disconnect(false);
                    if (str != rl)
                    {
                        Valid = false;
                        ValidationText.Text = "验证失败，请重启程序。";
                        ValidationText.FontSize = 18;
                        ValidationSocket.Close();
                        Log.fatal("Validation failed!");
                    }
                    else
                    {
                        Log.Info("Validation passed!");
                        Valid = true;
                        ValidationText.Text = "验证成功！";
                    }
                }
            }
            catch (Exception ex)
            {
                Log.fatal("Connected Error!" + ex.Message);
            }

            _keyboardHook = new KeyBoardHook();
            _keyboardHook.SetHook();
            _keyboardHook.SetOnKeyDownEvent(Win32_Keydown);
        }

        public static string Get_CPUID()
        {
            try
            {
                ManagementClass mc = new ManagementClass("Win32_Processor");
                ManagementObjectCollection moc = mc.GetInstances();
                string strCpuID = null;
                foreach (ManagementObject mo in moc)
                {
                    strCpuID = mo.Properties["ProcessorId"].Value.ToString();
                    mo.Dispose();
                    break;
                }
                return strCpuID;
            }
            catch
            {
                return "";
            }
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
}
