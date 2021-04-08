using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Diagnostics;
using SpellTracker.Control;

namespace SpellTracker
{
    public class KeyBoardHook
    {
        public delegate void KeyDownEvent(Key key);

        public delegate void KeyUpEvent(Key key);

        /// <summary>
        ///     键盘钩子句柄
        /// </summary>
        private int _hKeyboardHook;

        private Win32Helper.HookProc _keyboardHookDelegate;

        private KeyDownEvent _onKeyDown;
        private KeyUpEvent _onKeyUp;

        /// <summary>
        ///     安装键盘钩子
        /// </summary>
        public void SetHook()
        {
            _keyboardHookDelegate = KeyboardHookProc;
            var cModule = Process.GetCurrentProcess().MainModule;
            if (null == cModule) return;
            var moduleHandle = Win32Helper.GetModuleHandle(cModule.ModuleName);
            _hKeyboardHook =
                Win32Helper.SetWindowsHookEx(Win32Helper.WH_KEYBOARD_LL, _keyboardHookDelegate, moduleHandle, 0);
        }

        public void SetOnKeyDownEvent(KeyDownEvent callback)
        {
            _onKeyDown = callback;
        }

        public void SetOnKeyUpEvent(KeyUpEvent callback)
        {
            _onKeyUp = callback;
        }

        /// <summary>
        ///     卸载键盘钩子
        /// </summary>
        public void UnHook()
        {
            var retKeyboard = true;

            if (_hKeyboardHook != 0)
            {
                retKeyboard = Win32Helper.UnhookWindowsHookEx(_hKeyboardHook);
                _hKeyboardHook = 0;
            }

            //如果卸下钩子失败   
            if (!retKeyboard) throw new Exception("卸下钩子失败！");
        }

        /// <summary>
        ///     获取键盘消息
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private int KeyboardHookProc(int nCode, int wParam, IntPtr lParam)
        {
            // 如果该消息被丢弃 nCode<0
            if (nCode < 0) return Win32Helper.CallNextHookEx(_hKeyboardHook, nCode, wParam, lParam);

            var keyboardHookStruct =
                (Win32Helper.KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(Win32Helper.KeyboardHookStruct));
            var key = KeyInterop.KeyFromVirtualKey(keyboardHookStruct.vkCode);

            switch (wParam)
            {
                case Win32Helper.WM_KEYDOWN:
                case Win32Helper.WM_SYSKEYDOWN:
                    //WM_KEYDOWN和WM_SYSKEYDOWN消息，将会引发OnKeyDownEvent事件
                    // 此处触发键盘按下事件
                    _onKeyDown?.Invoke(key);
                    break;
                case Win32Helper.WM_KEYUP:
                case Win32Helper.WM_SYSKEYUP:
                    //WM_KEYUP和WM_SYSKEYUP消息，将引发OnKeyUpEvent事件
                    // 此处触发键盘抬起事件
                    _onKeyUp?.Invoke(key);
                    break;
            }

            return Win32Helper.CallNextHookEx(_hKeyboardHook, nCode, wParam, lParam);
        }
    }
}

