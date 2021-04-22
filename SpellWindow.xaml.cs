using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WebSocketSharp;
using System.Diagnostics;
using SpellTracker.Control;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SpellTracker
{
    /// <summary>
    /// SpelIWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SpellWindow : Window
    {
        /* Basic Data */

        static string webPath = "ws://127.0.0.1:2333";
        WebSocket webSocket = new WebSocket(webPath);
        Process process = new Process();
        public RiotParse RP;
        public bool IsInit = false;
        public System.Timers.Timer timer = new System.Timers.Timer(1000);
        public int shift;
        public bool FlashOnly;

        public SpellWindow(int shift = 10,bool FlashOnly = true,int width = 236,int height = 95)
        {
            InitializeComponent();
            SpellGrid.Visibility = Visibility.Hidden;
            InitButton.Visibility = Visibility.Visible;
            this.shift = shift;
            this.FlashOnly = FlashOnly;
            this.Width = width;
            this.Height = height;
        }

        private async void Main_Load(object sender, RoutedEventArgs e)
        {
            //配置log4
            Log.Info("---------------Start SpellTracker-----------------");
            await Init();
            Log.Info("Init data successfully!");
            InitButtonText.Text = "确保进入游戏了再点我";
            IsInit = true;
        }

        private async Task Init()
        {
           
            if (Process.GetProcessesByName("type").Length == 0)
            {
                Log.debug("Not found type.exe");
                //foreach (Process process in Process.GetProcessesByName("type")){
                //    KillProcess(process.ProcessName);
                //}
                try
                {
                    process.StartInfo.UseShellExecute = true;
                    process.StartInfo.FileName = "type\\type.exe";
                    //process.StartInfo.CreateNoWindow = true;
                    process.Start();
                }
                catch (Exception ex)
                {
                    Log.error("Init type.exe err! ",ex);
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
                Log.debug("Found type.exe");
            }

            Log.debug("Begin new RiotParse");
            RP = new RiotParse();
            RP.shift = this.shift;
            RP.FlashOnly = this.FlashOnly;
            await RP.GetSpells();
            Log.debug("new RiotParse successfully");
            RP.SpellImg[0] = SpellImage00; RP.SpellImg[1] = SpellImage01;
            RP.SpellImg[2] = SpellImage10; RP.SpellImg[3] = SpellImage11;
            RP.SpellImg[4] = SpellImage20; RP.SpellImg[5] = SpellImage21;
            RP.SpellImg[6] = SpellImage30; RP.SpellImg[7] = SpellImage31;
            RP.SpellImg[8] = SpellImage40; RP.SpellImg[9] = SpellImage41;

            timer.Elapsed += new System.Timers.ElapsedEventHandler(Timeout); //到达时间的时候执行事件；   
            timer.AutoReset = true;   //设置是执行一次（false）还是一直执行(true)；   
            timer.Enabled = true;     //是否执行System.Timers.Timer.Elapsed事件

            timer.Start();
        }

        public void Timeout(object source, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (RP.IsSync == false)
                {
                    this.Dispatcher.Invoke(new Action(delegate
                    {
                        SpellGrid.Visibility = Visibility.Hidden;
                        InitButton.Visibility = Visibility.Visible;
                    }));

                }
                else
                {
                    this.Dispatcher.Invoke(new Action(async delegate
                    {
                        await RP.Update();
                    }));
                }
            }
            catch (Exception ex)
            {
                Log.error("In timeout func ", ex);
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


        protected override void OnClosed(EventArgs eventArgs)
        {

            if (Process.GetProcessesByName("type").Length != 0)
            {
                foreach (Process process in Process.GetProcessesByName("type"))
                {
                    KillProcess(process.ProcessName);
                }
            }

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
                //return;
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
            if (IsInit)
            {
                await RP.Parse();
                InitButton.Visibility = Visibility.Hidden;
                SpellGrid.Visibility = Visibility.Visible;
            }
            
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        public void Type()
        {
            RP.Type();
            Senddata("update", RP.TypeStr);
            Log.Info(RP.TypeStr);
            Senddata("show", RP.TypeStr);
        }
    }

}
