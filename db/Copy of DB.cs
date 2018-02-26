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
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Linq;
using ProtoBuf.Meta;
using model;

namespace AppDB
{
    [PermissionSet(SecurityAction.LinkDemand, Name = "Everything"), PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    class DB
    {
        static DB()
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

        static Log log; 
        static ClientConnect client;
         

        public static void Start()
        {
            RuntimeTypeModel.Default.Add(typeof(Msg), false).SetSurrogate(typeof(MsgSurrogate));
            log = new Log(); 

            client = new ClientConnect(log);
            client.OnMessage += (type, obj) =>
            {
                switch (type)
                {
                    case MsgConnectEvent.OPEN:


                        client.Send(new Msg()
                        {
                            Data = typeof(mpUser),
                        });

                        //client.Send(new Msg()
                        //{
                        //    DataAction = DataAction.DB_ADD,
                        //    Data = new mpUser() { Username = "thinh", Password = "12345", Fullname = "Nguyễn Văn Thịnh" }
                        //});
                        //msg.Send(new Msg()
                        //{
                        //    DataAction = DataAction.DB_ADD,
                        //    Data = new mpUser() { Username = "tu", Password = "99999", Fullname = "Nguyễn Cam Tu" }
                        //});

                        ////new Thread(() =>
                        ////{
                        ////    //while (true)
                        ////    //{
                        ////    //    msg.SendText(Guid.NewGuid().ToString());
                        ////    //    Thread.Sleep(3000);
                        ////    //}
                        ////}).Start();
                        break;
                    case MsgConnectEvent.CLOSE:
                        break;
                    case MsgConnectEvent.MESSAGE_TEXT:
                        string text = obj as string;
                        if (!string.IsNullOrEmpty(text))
                        {
                            string code = string.Empty;
                            if (text.Length > 1) code = text.Substring(0, 2);
                            switch (code)
                            {
                                case "@@":
                                    ////////string src = text.Substring(2).Trim();

                                    ////////CSharpCodeProvider provider = new CSharpCodeProvider(new Dictionary<string, string>() { { "CompilerVersion", "v2.0" } }); 
                                    ////////////CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp"); 
                                    //////////CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
                                    ////////CompilerParameters parameter = new CompilerParameters(); 
                                    ////////// True - memory generation, false - external file generation
                                    ////////parameter.GenerateInMemory = true;
                                    ////////// True - exe file generation, false - dll file generation
                                    ////////parameter.GenerateExecutable = false;
                                    ////////parameter.ReferencedAssemblies.Add(@"System.dll"); 
                                    ////////parameter.IncludeDebugInformation = false;

                                    ////////CompilerResults result = provider.CompileAssemblyFromSource(parameter, src); 
                                    ////////if (result.Errors.HasErrors)
                                    ////////{
                                    ////////    StringBuilder sb = new StringBuilder();
                                    ////////    foreach (CompilerError error in result.Errors)
                                    ////////        sb.AppendLine(String.Format("Error ({0}): {1}", error.ErrorNumber, error.ErrorText));
                                    ////////    string err = sb.ToString();
                                    ////////}
                                    ////////else
                                    ////////{
                                    ////////    Assembly asm = result.CompiledAssembly;
                                    ////////    string[] aName = asm.GetTypes().Select(x => x.FullName).ToArray();
                                    ////////    modelUser = asm.GetType(aName[0], false); 
                                    ////////} 
                                    break;
                            }
                        }
                        break;
                    case MsgConnectEvent.MESSAGE_BINARY:
                        Msg m;
                        try
                        {
                            byte[] buf = (byte[])obj;
                            m = buf.Deserialize_Msg();
                        }
                        catch (Exception ex)
                        {
                        }
                        break;
                    case MsgConnectEvent.ERROR:
                        break;
                }
            }; 

            ////////////////////////////////////////////////////
            // Start wait send and recieve message socket
            client.Start();
        }


    }//end class 
}

namespace model
{
    [Serializable]
    public class mpUser
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Fullname { get; set; }
    }
}
