﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CToolkit.v1_1.WinApiNative;
using System.Threading;


namespace CToolkit.v1_1.WinApi
{
    public class CtkHookKeyboard
    {
        IntPtr intPtrHook;
        CtkUser32Lib.HookProc hookProc;

        Dictionary<int, bool> keepKeys = new Dictionary<int, bool>();
        public bool IsKeepCtrl { get { return keepKeys[162]; } }
        public bool IsKeepAlt { get { return keepKeys[164]; } }


        ~CtkHookKeyboard()
        {
            this.Unhook();
        }

        public void Hook()
        {
            //Hook Keyboard
            Unhook();
            if (intPtrHook == IntPtr.Zero)
            {
                hookProc = new CtkUser32Lib.HookProc(HookProcCallback);
                intPtrHook = CtkUser32Lib.SetWindowsHookEx(CtkEnumHookType.WH_KEYBOARD_LL,
                    hookProc,
                    IntPtr.Zero,
                    0);

                if (intPtrHook == IntPtr.Zero)
                    throw new CtkException("WinApi Error-" + System.Runtime.InteropServices.Marshal.GetLastWin32Error());
            }
        }
        public void Unhook()
        {
            if (intPtrHook != IntPtr.Zero)
            {
                CtkUser32Lib.UnhookWindowsHookEx(intPtrHook);
                intPtrHook = IntPtr.Zero;
            }
        }

        protected int HookProcCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                try { this.OnHookCallback(new CtkEventArgsHookCallback() { nCode = nCode, wParam = wParam, lParam = lParam }); }
                catch (Exception ex) { ThreadPool.QueueUserWorkItem(delegate { this.OnHookCallbackException(new CtkEventArgsException() { exception = ex }); }); }

                try
                {

                    int vkCode = System.Runtime.InteropServices.Marshal.ReadInt32(lParam);
                    CtkEnumConst kbInput = (CtkEnumConst)wParam;

                    if (kbInput == CtkEnumConst.WM_KEYDOWN || kbInput == CtkEnumConst.WM_SYSKEYDOWN) { keepKeys[vkCode] = true; }
                    if (kbInput == CtkEnumConst.WM_KEYUP || kbInput == CtkEnumConst.WM_SYSKEYUP) { keepKeys[vkCode] = false; }


                }
                catch (Exception ex)
                {
                    //給背景執行緒處理, 再出Exception也與原執行緒無關, 可以正常工作
                    ThreadPool.QueueUserWorkItem(delegate { this.OnHookCallbackException(new CtkEventArgsException() { exception = ex }); });
                }
            }
            return CtkUser32Lib.CallNextHookEx(intPtrHook, nCode, wParam, lParam);
        }





        #region Event


        //---HookCallback----------------------------------------------------------------
        public event EventHandler<CtkEventArgsHookCallback> EhHookCallback;
        protected void OnHookCallback(CtkEventArgsHookCallback ehargs)
        {
            if (EhHookCallback == null) return;
            this.EhHookCallback(this, ehargs);
        }




        public event EventHandler<CtkEventArgsException> EhHookCallbackException;
        protected void OnHookCallbackException(CtkEventArgsException ex)
        {
            if (EhHookCallbackException == null) return;
            this.EhHookCallbackException(this, ex);
        }


        #endregion

    }
}
