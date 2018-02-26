using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Permissions;
using System.Threading;
using Core;
using ProtoBuf.Meta;
using model;
using System.Net;
using System.IO;
using System.Reflection;
using ProtoBuf;
using System.Runtime.InteropServices;
namespace AppDB
{
    [PermissionSet(SecurityAction.LinkDemand, Name = "Everything"),
    PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
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


        static void Send(Msg m)
        {
            byte[] buf = m.Serialize_Msg();

            string uri = "http://127.0.0.1:10101/";
            WebRequest request = WebRequest.Create(uri);
            request.Method = "POST";

            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = buf.Length;

            //request.ContentType = "application/x-www-form-urlencoded";
            //request.ContentLength = 0;

            Stream streamPUT = request.GetRequestStream();
            streamPUT.Write(buf, 0, buf.Length);
            streamPUT.Close();

            WebResponse response = request.GetResponse();

            if (((HttpWebResponse)response).StatusCode == HttpStatusCode.OK)
            {
                string status = ((HttpWebResponse)response).StatusDescription;
                if (status == "OK")
                {
                    Console.WriteLine("OK");
                }
            }

            response.Close();
        }

        static Log log;

        public static void Start()
        {
            RuntimeTypeModel.Default.Add(typeof(Msg), false).SetSurrogate(typeof(MsgSurrogate));
            log = new Log();

            //Console.ReadKey();


            Send(new Msg()
            {
                Data = typeof(mpUser),
            });

            Send(new Msg()
            {
                DataAction = DataAction.DB_ADD,
                Data = new mpUser() { date = DateTime.Now, Username = "thinh", Password = "12345", Fullname = "Nguyễn Văn Thịnh" }
            });

            Send(new Msg()
            {
                DataAction = DataAction.DB_ADD,
                Data = new mpUser() { date = DateTime.Now.AddDays(-99), Username = "tu", Password = "99999", Fullname = "Nguyễn Cam Tu" }
            });

            Send(new Msg()
            {
                DataAction = DataAction.DB_ADD,
                Data = new mpUser() { date = DateTime.Now.AddDays(-99), Username = "doanh", Password = "99999", Fullname = "Nguyễn Doanh" }
            });
            Send(new Msg()
            {
                DataAction = DataAction.DB_ADD,
                Data = new mpUser() { date = DateTime.Now.AddDays(-99), Username = "giang", Password = "99999", Fullname = "Nguyễn Giang" }
            });
            Send(new Msg()
            {
                DataAction = DataAction.DB_ADD,
                Data = new mpUser() { date = DateTime.Now.AddDays(-99), Username = "tuan", Password = "99999", Fullname = "Nguyễn Tuan" }
            });
            Send(new Msg()
            {
                DataAction = DataAction.DB_ADD,
                Data = new mpUser() { date = DateTime.Now.AddDays(-99), Username = "truong", Password = "99999", Fullname = "Nguyễn Truong" }
            });

            ////////var w = new Func<mpUser, bool>(x => x.Username == "thinh");


            Send(new Msg()
            {
                DataAction = DataAction.DB_SELECT,
                DataType = typeof(mpUser).FullName,
                Data = @" it.Username == ""thinh"" "
            });

            ////new Thread(() =>
            ////{
            ////    //while (true)
            ////    //{
            ////    //    msg.SendText(Guid.NewGuid().ToString());
            ////    //    Thread.Sleep(3000);
            ////    //}
            ////}).Start();

            Console.ReadLine();
        }
    }//end class 
}

namespace model
{
    [ProtoContract]
    public class Foo
    {
        [ProtoMember(1)]
        public Type Type { get; set; }
    }

    [Serializable]
    public class mpUser
    {
        public int _index { set; get; }



        public int Id { set; get; }
        public long Value { set; get; }
        public DateTime date { set; get; }

        public string Username { set; get; }
        public string Password { set; get; }
        public string Fullname { set; get; }
    }
}
