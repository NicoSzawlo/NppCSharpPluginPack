// NPP plugin platform for .Net v0.94.00 by Kasper B. Graversen etc.
using Kbg.NppPluginNET.PluginInfrastructure;
using NppDemo.Utils;
using RGiesecke.DllExport;
using System;
using System.Runtime.InteropServices;

namespace Kbg.NppPluginNET
{
    class UnmanagedExports
    {
        [DllExport(CallingConvention=CallingConvention.Cdecl)]
        static bool isUnicode()
        {
            return true;
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static void setInfo(NppData notepadPlusData)
        {
            PluginBase.nppData = notepadPlusData;
            Main.CommandMenuInit();
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static IntPtr getFuncsArray(ref int nbF)
        {
            nbF = PluginBase._funcItems.Items.Count;
            return PluginBase._funcItems.NativePointer;
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static uint messageProc(uint Message, IntPtr wParam, IntPtr lParam)
        {
            return 1;
        }

        static IntPtr _ptrPluginName = IntPtr.Zero;
        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static IntPtr getName()
        {
            if (_ptrPluginName == IntPtr.Zero)
                _ptrPluginName = Marshal.StringToHGlobalUni(Main.PluginName);
            return _ptrPluginName;
        }

        [DllExport(CallingConvention = CallingConvention.StdCall)]
        public static void beNotified(IntPtr notifyCode)
        {
            ScNotification notification = (ScNotification)Marshal.PtrToStructure(notifyCode, typeof(ScNotification));

            if (notification.Header.Code == (uint)SciMsg.SCN_MODIFIED)
            {
                const int SC_MOD_INSERTTEXT = 0x1;
                const int SC_MOD_DELETETEXT = 0x2;

                if ((notification.ModificationType & (SC_MOD_INSERTTEXT | SC_MOD_DELETETEXT)) != 0)
                {
                    EditorEvents.RaiseEditorTextChanged();
                }
            }
        }
    }
}
