using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

// Proxy.Set(new WebProxy("127.0.0.1", 9999)); //включить
// Proxy.Set(null); //выключить

namespace ctext
{
    public static class Proxy
    {
        private const int ERROR_INSUFFICIENT_BUFFER = 122;
        private const int INTERNET_OPTION_PROXY = 38;
        private const int INTERNET_OPEN_TYPE_DIRECT = 1;
        private const int INTERNET_OPEN_TYPE_PROXY = 3;

        [DllImport("wininet.dll", SetLastError = true)]
        private static extern bool InternetSetOption(IntPtr hInternet, int dwOption,
            INTERNET_PER_CONN_OPTION_LIST lpBuffer, int dwBufferLength);

        [DllImport("wininet.dll", SetLastError = true)]
        private static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer,
            int lpdwBufferLength);

        [DllImport("kernel32.dll")]
        private static extern int GetLastError();

        public static bool Set(WebProxy wProxy)
        {
            var bReturn = false;
            var info = new INTERNET_PROXY_INFO();
            var sPrx = string.Empty;
            if (wProxy != null)
            {
                sPrx = string.Format("{0}:{1}", wProxy.Address.DnsSafeHost, wProxy.Address.Port);
                info.dwAccessType = INTERNET_OPEN_TYPE_PROXY;
            }
            else
            {
                info.dwAccessType = INTERNET_OPEN_TYPE_DIRECT;
            }
            info.lpszProxy = Marshal.StringToHGlobalAnsi(sPrx);
            info.lpszProxyBypass = Marshal.StringToHGlobalAnsi("rado.ra-host.com");

            var intptrStruct = Marshal.AllocCoTaskMem(Marshal.SizeOf(info));

            Marshal.StructureToPtr(info, intptrStruct, true);
            bReturn = InternetSetOption(IntPtr.Zero, INTERNET_OPTION_PROXY, intptrStruct, Marshal.SizeOf(info));
            if (!bReturn)
            {
                //    Tools.SendErrorToBD(MessageType.FatalError, MethodBase.GetCurrentMethod().Name, string.Format("{0}\r\n", GetLastError()));
            }

            bReturn = InternetSetOption(IntPtr.Zero, (int)MyOptions.INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);
            if (!bReturn)
            {
                //   Tools.SendErrorToBD(MessageType.FatalError, MethodBase.GetCurrentMethod().Name, string.Format("{0}\r\n", GetLastError()));
            }
            Marshal.FreeCoTaskMem(intptrStruct);
            return bReturn;
        }

        //[Flags]
        //private enum Flags
        //{
        //    PROXY_TYPE_PROXY = 0x00000002
        //}

        //[StructLayout(LayoutKind.Explicit, Size = 12)]
        //private struct INTERNET_PER_CONN_OPTION
        //{
        //    [FieldOffset(0)]
        //    private readonly int dwOption;

        //    [FieldOffset(4)]
        //    private readonly int dwValue;

        //    [FieldOffset(4)]
        //    private IntPtr pszValue;

        //    [FieldOffset(4)]
        //    private readonly IntPtr ftValue;

        //    public byte[] GetBytes()
        //    {
        //        var b = new byte[12];
        //        BitConverter.GetBytes(dwOption).CopyTo(b, 0);
        //        switch (dwOption)
        //        {
        //            case (int) MyOptions.INTERNET_PER_CONN_FLAGS:
        //                BitConverter.GetBytes(dwValue).CopyTo(b, 4);
        //                break;
        //            case (int) MyOptions.INTERNET_PER_CONN_PROXY_BYPASS:
        //                BitConverter.GetBytes(pszValue.ToInt32()).CopyTo(b, 4);
        //                break;
        //            case (int) MyOptions.INTERNET_PER_CONN_PROXY_SERVER:
        //                BitConverter.GetBytes(pszValue.ToInt32()).CopyTo(b, 4);
        //                break;
        //        }
        //        return b;
        //    }
        //}

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private class INTERNET_PER_CONN_OPTION_LIST
        {
            public int dwSize;
            public string pszConnection;
            public int dwOptionCount;
            public IntPtr pOptions;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct INTERNET_PROXY_INFO
        {
            public int dwAccessType;
            public IntPtr lpszProxy;
            public IntPtr lpszProxyBypass;
        }

        private enum MyOptions
        {
            INTERNET_OPTION_PER_CONNECTION_OPTION = 75,
            INTERNET_OPTION_REFRESH = 37,
            INTERNET_PER_CONN_FLAGS = 1,
            INTERNET_PER_CONN_PROXY_BYPASS = 3,
            INTERNET_PER_CONN_PROXY_SERVER = 2,
        }

        [Flags]
        private enum ProxyFlags
        {
        }

        private enum ROptions
        {
        }
    }
}
