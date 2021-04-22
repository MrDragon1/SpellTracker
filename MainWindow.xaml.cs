﻿using System;
using System.Windows;
using System.Windows.Input;
using SpellTracker.Control;
using Newtonsoft.Json;

//TODO:setting page too empty
//TODO:竖向排列
namespace SpellTracker
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public SpellWindow spellWindow;
        KeyBoardHook _keyboardHook;
        private bool Valid = false;
        ConfigJson json;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, RoutedEventArgs e)
        {
            //配置log4
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo("log4net.config"));
            Log.Info("===============Start log=================");

            /*read json file*/
            try
            {
                json = JsonConvert.DeserializeObject<ConfigJson>(System.IO.File.ReadAllText(@"config.json"));
            }
            catch
            {
                Log.error("Open json failed!");
                ValidationText.Text = "读取config文件失败！";
            }

            /*init user config*/
            try
            {
                _keyboardHook = new KeyBoardHook();
                _keyboardHook.SetHook();
                _keyboardHook.SetOnKeyDownEvent(Win32_Keydown);
                if (json.HotKey_Key != 0)
                {
                    HotKey.Text = ((Key)json.HotKey_Key).ToString();
                }
                else
                {
                    HotKey.Text = "点击设置快捷键";
                    ValidationText.Text = "快捷键未设置！";
                }
                if(json.shift >= Slider_Shift.Minimum && json.shift <= Slider_Shift.Maximum)
                {
                    Slider_Shift.Value = json.shift;
                }
                FTOnly.IsChecked = json.FTOnly;
                
                Valid = true;
                ValidationText.Text = "初始化完成！";
            }
            catch
            {
                Log.error("Hotkey init error!");
            }
            
        }
        private void Win32_Keydown(Key key)
        {
            if(key == (Key)json.HotKey_Key)
            {
                try
                {
                    spellWindow.Type();
                }
                catch
                {
                    ValidationText.Text = "请先启动软件！";
                }
            }
        }

        private void SpellTrackerToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (Valid)
            {
                spellWindow = new SpellWindow((int)Slider_Shift.Value, (bool)FTOnly.IsChecked,json.Win_width,json.Win_height);
                spellWindow.Show();
                Slider_Shift.IsEnabled = false;
                FTOnly.IsEnabled = false;
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
            //Log.Info("UnHooked");
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

        private void HotKey_KeyDown(object sender, KeyEventArgs e)
        {
            HotKey.Text = "";
            //if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            //{
            //    mod |= HotkeyModifiers.MOD_CONTROL;
            //}
            //if (e.Key == Key.LeftAlt || e.Key == Key.RightAlt)
            //{
            //    mod |= HotkeyModifiers.MOD_ALT;
            //}
            //if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            //{
            //    mod |= HotkeyModifiers.MOD_SHIFT;
            //}
            
            //if(mod == HotkeyModifiers.MOD_CONTROL)
            //{
            //    Log.Info(((int)mod).ToString() + e.Key.ToString());
            //}
            try
            {
                json.HotKey_Key = (uint)e.Key;
                string output = Newtonsoft.Json.JsonConvert.SerializeObject(json, Newtonsoft.Json.Formatting.Indented);
                System.IO.File.WriteAllText(@"config.json", output);
            }
            catch
            {
                Log.error("Register HotKey error!");
            }
            HotKey.Text = ((Key)json.HotKey_Key).ToString();
            Setting.Focus();
        }

    }
    public class ConfigJson
    {
        public uint HotKey_Mod { get; set; }
        public uint HotKey_Key { get; set; }
        public bool FTOnly { get; set; }
        public int shift { get; set; }
        public int Win_width { get; set; }
        public int Win_height { get; set; }
    }
}
