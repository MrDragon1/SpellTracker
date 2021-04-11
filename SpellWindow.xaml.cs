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
    /// SpelIWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SpellWindow : Window
    {
        /* Basic Data */
        KeyBoardHook _keyboardHook;
        static string webPath = "ws://127.0.0.1:2333";
        WebSocket webSocket = new WebSocket(webPath);
        Process process = new Process();
        RiotParse RP;
        public bool IsInit = false;
        public System.Timers.Timer timer = new System.Timers.Timer(1000);

        public SpellWindow()
        {
            InitializeComponent();
        }

        private async void Main_Load(object sender, RoutedEventArgs e)
        {
            //配置log4
            Log.Info("---------------Start SpellTracker-----------------");
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo("../../log4net.config"));
            await Init();
            Log.Info("Init data successfully!");
            IsInit = true;
        }

        private async Task Init()
        {
            _keyboardHook = new KeyBoardHook();
            _keyboardHook.SetHook();
            _keyboardHook.SetOnKeyDownEvent(Win32_Keydown);


            if (Process.GetProcessesByName("type").Length == 0)
            {
                Log.debug("not found type.exe");
                //foreach (Process process in Process.GetProcessesByName("type")){
                //    KillProcess(process.ProcessName);
                //}
                try
                {
                    process.StartInfo.UseShellExecute = true;
                    process.StartInfo.FileName = "G:\\Desktop\\Type\\build\\win-unpacked\\type.exe";
                    //process.StartInfo.CreateNoWindow = true;
                    process.Start();
                }
                catch
                {
                    Log.error("Init type.exe err! ");
                }
                try
                {
                    Senddata("setname", System.Environment.GetEnvironmentVariable("ComputerName"));
                }
                catch
                {
                    Log.error("Connect to type.exe err! ");
                }
            }
            else
            {
                Log.debug("found type.exe");
            }


            Log.debug("begin new RiotParse");
            RP = new RiotParse();
            await RP.GetSpells();

            RP.SpellImg[0] = SpellImage00; RP.SpellImg[1] = SpellImage01;
            RP.SpellImg[2] = SpellImage10; RP.SpellImg[3] = SpellImage11;
            RP.SpellImg[4] = SpellImage20; RP.SpellImg[5] = SpellImage21;
            RP.SpellImg[6] = SpellImage30; RP.SpellImg[7] = SpellImage31;
            RP.SpellImg[8] = SpellImage40; RP.SpellImg[9] = SpellImage41;

            timer.Elapsed += new System.Timers.ElapsedEventHandler(Timeout); //到达时间的时候执行事件；   
            timer.AutoReset = true;   //设置是执行一次（false）还是一直执行(true)；   
            timer.Enabled = true;     //是否执行System.Timers.Timer.Elapsed事件

            SpellGrid.Visibility = Visibility.Hidden;

            timer.Start();
        }

        public void Timeout(object source, System.Timers.ElapsedEventArgs e)
        {
            if(RP.IsSync == false)
            {
                SpellGrid.Visibility = Visibility.Hidden;
                InitButton.Visibility = Visibility.Visible;
            }
            else
            {
                this.Dispatcher.Invoke(new Action(async delegate
                {
                    await RP.Update();
                }));
            }
        }


        private void KillProcess(string processName)
        {
            System.Diagnostics.Process myproc = new System.Diagnostics.Process();
            //得到所有打开的进程   
            try
            {
                foreach (Process thisproc in Process.GetProcessesByName(processName))
                {
                    //找到程序进程,kill之。
                    if (!thisproc.CloseMainWindow())
                    {
                        thisproc.Kill();
                        thisproc.WaitForExit();
                    }
                }

            }
            catch (Exception Exc)
            {
                Log.error("Kill " + processName + ".exe failed" + Exc.Message);
            }
            Log.Info("Successfully kill the " + processName + ".exe");
        }

        private void Win32_Keydown(Key key)
        {
            switch (key)
            {
                case Key.Add:
                    {
                        RP.Type();
                        Senddata("update", RP.TypeStr);
                        Log.Info(RP.TypeStr);
                        Senddata("show", RP.TypeStr);
                    }
                    break;
            }
        }

        protected override void OnClosed(EventArgs eventArgs)
        {

            if (Process.GetProcessesByName("type").Length != 0)
            {
                foreach (Process process in Process.GetProcessesByName("type"))
                {
                    KillProcess(process.ProcessName);
                }
            }

            _keyboardHook.UnHook();
            Log.Info("UnHooked");
            base.OnClosed(eventArgs);
            Log.Info("---------------Stop SpellTracker-----------------");
        }


        public async Task ImageTic(int id)
        {
            if (!IsInit)
            {
                Log.Info("The program is not initialized. Don't click the img.");
                return;
            }
            await RP.TicTok(id);
        }

        #region MouseEvent

        private void SpellImage00_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private async void SpellImage00_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            await ImageTic(0);
        }

        private void SpellImage01_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private async void SpellImage01_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            await ImageTic(1);
        }

        private void SpellImage10_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private async void SpellImage10_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            await ImageTic(2);
        }

        private void SpellImage11_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private async void SpellImage11_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            await ImageTic(3);
        }

        private void SpellImage20_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private async void SpellImage20_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            await ImageTic(4);
        }

        private void SpellImage21_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private async void SpellImage21_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            await ImageTic(5);
        }

        private void SpellImage30_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private async void SpellImage30_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            await ImageTic(6);
        }

        private void SpellImage31_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private async void SpellImage31_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            await ImageTic(7);
        }

        private void SpellImage40_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private async void SpellImage40_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            await ImageTic(8);
        }

        private void SpellImage41_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private async void SpellImage41_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            await ImageTic(9);
        }
        #endregion

        private void Senddata(string type, string text)
        {
            if (text == "")
            {
                Log.Info("Type empty string occur!");
                return;
            }
            webSocket.Connect();
            JObject msg = new JObject();
            // 设置json对象
            msg["text"] = text;
            msg["type"] = type;
            // 转为json字符串
            string data = JsonConvert.SerializeObject(msg);
            webSocket.Send(data);//发送消息的函数
        }

        private async void InitButton_Click(object sender, RoutedEventArgs e)
        {
            InitButton.Visibility = Visibility.Hidden;
            await RP.Parse();
            SpellGrid.Visibility = Visibility.Visible;
        }
    }

}
