using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Permissions;
using System.Threading;
using Core;
using System.Net;
using System.Reflection;
using System.IO;
using ProtoBuf;
using System.Net.Sockets;
using System.Collections.Specialized;
using System.Windows.Forms;
using ProtoBuf.Meta;

namespace AppMain
{

    [PermissionSet(SecurityAction.LinkDemand, Name = "Everything"), PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    class App
    {
        static App()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (se, ev) =>
            {
                Assembly asm = null;
                string comName = ev.Name.Split(',')[0];
                string resourceName = @"DLL\" + comName + ".dll";
                var assembly = Assembly.GetExecutingAssembly();
                resourceName = typeof(Program).Namespace + "." + resourceName.Replace(" ", "_").Replace("\\", ".").Replace("/", ".");
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        byte[] buffer = new byte[stream.Length];
                        using (MemoryStream ms = new MemoryStream())
                        {
                            int read;
                            while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                                ms.Write(buffer, 0, read);
                            buffer = ms.ToArray();
                        }
                        asm = Assembly.Load(buffer);
                    }
                }
                return asm;
            };
        }

        ////////////////////////////////////////
        static Log log;
        static HostListener host;  

        ////////////////////////////////////////
        static SystemTray icon_tray;
        static FormNotification fm_noti;
        static FormLogger fm_log;
        //static FormDemo fm_demo;


        public static void Start()
        {
            Application.EnableVisualStyles();

            //////////////////////////////////////////
            RuntimeTypeModel.Default.Add(typeof(Msg), false).SetSurrogate(typeof(MsgSurrogate));

            ////////////////////////////////////////
            log = new Log();  
            host = new HostListener(log);
            host.Start();
             
            //////////////////////////////////////////////
            noti_Init();
            icon_tray = new SystemTray("Host");
            fm_noti = new FormNotification();
            fm_log = new FormLogger();
            //fm_demo = new FormDemo(host); 
            //fm_log.Show();
            //fm_demo.Show();

            //log.Write(LogSystem.HOST_SYSTEM, LogType.NONE, string.Format("Host port public: {0}", host.Port));
            //log.Write(LogSystem.HOST_SYSTEM, LogType.NONE, string.Format("Host port HTTP: {0}", host.PortHTTP));
            //log.Write(LogSystem.HOST_SYSTEM, LogType.NONE, string.Format("Host port Websocket: {0}", host.PortWebSocket));

            Application.Run(icon_tray);
        }


        #region [ === FORM NOTIFICATION === ]

        // very simple method to create new forms, controls ... is to use Invoke of this control
        private static Control _invoker;
        public static void show_Notification(string msg, int duration_ = 0)
        {
            FormNotification form = new FormNotification(msg, duration_);
            _invoker.Invoke((MethodInvoker)delegate()
            {
                form.Show();
            });
        }

        static void noti_Init()
        {
            _invoker = new Control();
            _invoker.CreateControl();
        }

        #endregion
    }//end class 
}
