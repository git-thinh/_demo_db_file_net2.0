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

namespace AppGUI
{
    [PermissionSet(SecurityAction.LinkDemand, Name = "Everything"), PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    public class FormManager
    {
        static FormManager()
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
         
        public static void Start()
        { 

        }


    }//end class 
}

