using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CToolkit.v1_0.WinApiNative;
using System.Runtime.InteropServices;
using System.Threading;


namespace CToolkit.v1_0.WinApi
{
    public class CtkHookMouse
    {
        IntPtr intPtrHook;
        CtkUser32Lib.HookProc HookProcdure;

        /// <summary>
        /// 取得或設定是否獨佔所有滑鼠事件。
        /// </summary>
        public bool Monopolize = false;

        //記憶上次MouseDonw的引發位置，如果與MouseUp的位置不同則不引發Click事件。
        int m_LastBTDownX = 0;
        int m_LastBTDownY = 0;

        //記憶游標上一次的位置，避免MouseMove事件一直引發。
        int m_OldX = 0;
        int m_OldY = 0;

        ~CtkHookMouse()
        {
            this.Unhook();
        }

        public void Hook()
        {
            //Hook Keyboard
            Unhook();
            if (intPtrHook == IntPtr.Zero)
            {
                HookProcdure = new CtkUser32Lib.HookProc(HookProcCallback);
                intPtrHook = CtkUser32Lib.SetWindowsHookEx(CtkEnumHookType.WH_MOUSE_LL,
                    HookProcdure,
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


        int HookProcCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            EventArgsMouse evtargs = null;
            if (nCode >= 0)
            {
                try
                {
                    var hookType = (CtkEnumConst)wParam;
                    var mouseHookStruct = (CtkMdlHookMouseStruct)Marshal.PtrToStructure(lParam, typeof(CtkMdlHookMouseStruct));

                    short mouseDelta = 0;

                    if (hookType == CtkEnumConst.WM_MOUSEWHEEL)
                        mouseDelta = (short)((mouseHookStruct.mouseData >> 16) & 0xffff);

                    evtargs = new EventArgsMouse(hookType, mouseHookStruct.dx, mouseHookStruct.dy, mouseDelta);

                    if (hookType == CtkEnumConst.WM_MOUSEWHEEL)
                        this.OnMouseWheel(evtargs);

                    else if (hookType == CtkEnumConst.WM_LBUTTONUP || hookType == CtkEnumConst.WM_RBUTTONUP || hookType == CtkEnumConst.WM_MBUTTONUP)
                    {
                        this.OnMouseUp(evtargs);
                        if (mouseHookStruct.dx == m_LastBTDownX && mouseHookStruct.dy == m_LastBTDownY)
                            this.OnMouseClick(evtargs);
                    }
                    else if (hookType == CtkEnumConst.WM_LBUTTONDOWN || hookType == CtkEnumConst.WM_RBUTTONDOWN || hookType == CtkEnumConst.WM_MBUTTONDOWN)
                    {
                        m_LastBTDownX = mouseHookStruct.dx;
                        m_LastBTDownY = mouseHookStruct.dy;
                        this.OnMouseDown(evtargs);
                    }
                    else if (m_OldX != mouseHookStruct.dx || m_OldY != mouseHookStruct.dy)
                    {
                        m_OldX = mouseHookStruct.dx;
                        m_OldY = mouseHookStruct.dy;
                        this.OnMouseMove(evtargs);
                    }
                }
                catch (Exception ex)
                {
                    //給背景執行緒處理, 再出Exception也與原執行緒無關, 可以正常工作
                    ThreadPool.QueueUserWorkItem(delegate { this.OnHookCallbackException(new CtkEventArgsException() { exception = ex }); });
                }
            }

            if (Monopolize || (evtargs != null && evtargs.Handled))
                return -1;


            return CtkUser32Lib.CallNextHookEx(intPtrHook, nCode, wParam, lParam);
        }



        #region Event


        //---HookCallback----------------------------------------------------------------
        public event EventHandler<CtkEventArgsHookCallback> evtHookCallback;
        protected void OnHookCallback(CtkEventArgsHookCallback evtargs)
        {
            if (evtHookCallback == null) return;
            this.evtHookCallback(this, evtargs);
        }



        public event EventHandler<CtkEventArgsException> evtHookCallbackException;
        protected void OnHookCallbackException(CtkEventArgsException ex)
        {
            if (evtHookCallbackException == null) return;
            this.evtHookCallbackException(this, ex);
        }




        //---Mouse Event----------------------------------------------------------------


        /// <summary>
        /// 提供 GlobalMouseUp、GlobalMouseDown 和 GlobalMouseMove 事件的資料。
        /// </summary>
        public class EventArgsMouse : EventArgs
        {
            /// <summary>
            /// 取得按下哪個滑鼠鍵的資訊。
            /// </summary>
            public CtkEnumMouseLMR Button { get; private set; }
            /// <summary>
            /// 取得滑鼠滾輪滾動時帶有正負號的刻度數乘以 WHEEL_DELTA 常數。 一個刻度是一個滑鼠滾輪的刻痕。
            /// </summary>
            public int Delta { get; private set; }
            /// <summary>
            /// 取得滑鼠在產生滑鼠事件期間的 X 座標。
            /// </summary>
            public int X { get; private set; }
            /// <summary>
            /// 取得滑鼠在產生滑鼠事件期間的 Y 座標。
            /// </summary>
            public int Y { get; private set; }
            internal EventArgsMouse(CtkEnumConst wParam, int x, int y, int delta)
            {
                Button = CtkEnumMouseLMR.None;
                switch (wParam)
                {
                    case CtkEnumConst.WM_LBUTTONDOWN:
                    case CtkEnumConst.WM_LBUTTONUP:
                        Button = CtkEnumMouseLMR.Left;
                        break;
                    case CtkEnumConst.WM_RBUTTONDOWN:
                    case CtkEnumConst.WM_RBUTTONUP:
                        Button = CtkEnumMouseLMR.Right;
                        break;
                    case CtkEnumConst.WM_MBUTTONDOWN:
                    case CtkEnumConst.WM_MBUTTONUP:
                        Button = CtkEnumMouseLMR.Middle;
                        break;
                }
                this.X = x;
                this.Y = y;
                this.Delta = delta;
            }
            private bool m_Handled;
            /// <summary>
            /// 取得或設定值，指出是否處理事件。
            /// </summary>
            public bool Handled
            {
                get { return m_Handled; }
                set { m_Handled = value; }
            }
        }
        public event EventHandler<EventArgsMouse> evtMouseWheel;
        protected void OnMouseWheel(EventArgsMouse evtargs)
        {
            if (evtMouseWheel == null) return;
            this.evtMouseWheel(this, evtargs);
        }

        public event EventHandler<EventArgsMouse> evtMouseDown;
        protected void OnMouseDown(EventArgsMouse evtargs)
        {
            if (evtMouseDown == null) return;
            this.evtMouseDown(this, evtargs);
        }
        public event EventHandler<EventArgsMouse> evtMouseUp;
        protected void OnMouseUp(EventArgsMouse evtargs)
        {
            if (evtMouseUp == null) return;
            this.evtMouseUp(this, evtargs);
        }
        public event EventHandler<EventArgsMouse> evtMouseClick;
        protected void OnMouseClick(EventArgsMouse evtargs)
        {
            if (evtMouseClick == null) return;
            this.evtMouseClick(this, evtargs);
        }
        public event EventHandler<EventArgsMouse> evtMouseMove;
        protected void OnMouseMove(EventArgsMouse evtargs)
        {
            if (evtMouseMove == null) return;
            this.evtMouseMove(this, evtargs);
        }

        #endregion


    }
}
