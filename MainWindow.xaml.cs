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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using SpellTracker.Control;
using SpellTracker.Data;
using log4net;

namespace SpellTracker
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        /* Basic Data */
        public int GameTime;
        public string GameMode, PlayerName;
        public List<int> level = new List<int>();
        public List<string> summonerName = new List<string>();
        public List<string> championName = new List<string>();
        public List<string> summonerSpellOne = new List<string>();
        public List<string> summonerSpellTwo = new List<string>();
        public int[,] SpellTime = new int[5, 2];
        public int shift = 10;
        public string playerteam = "";
        public bool IsSync = false;
        KeyBoardHook _keyboardHook;
        static string webPath = "ws://127.0.0.1:2333";
        WebSocket webSocket = new WebSocket(webPath);
        Process process = new Process();

        public MainWindow()
        {
            //配置log4
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo("../../log4net.config"));
            InitializeComponent();
            _keyboardHook = new KeyBoardHook();
            _keyboardHook.SetHook();
            _keyboardHook.SetOnKeyDownEvent(Win32_Keydown);
            Log.Info("===============Startupfrom log=================");
        }

        private void Win32_Keydown(Key key)
        {
            switch (key)
            {
                case Key.F12:
                    {
                        Log.Info("hook f12");
                    }
                    break;
            }
        }

        protected override void OnClosed(EventArgs eventArgs)
        {
            Log.Info("================End================");
            _keyboardHook.UnHook();
            base.OnClosed(eventArgs);
        }

        public async Task ImageTic(int id)
        {
            Log.Info("click");
            SpellImage00.Source = await ImageCache.Instance.Get("img/CDSummonerFlash.png");
        }

        private void SpellImage00_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private async void SpellImage00_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            await ImageTic(0);
        }

    }
}
