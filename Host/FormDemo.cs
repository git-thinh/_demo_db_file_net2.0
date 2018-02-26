using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Core;
using ProtoBuf;
using model;

namespace AppHost
{
    public partial class FormDemo : Form
    {  
        public FormDemo( ) 
        { 
            InitializeComponent(); 
















            textBox1.Text =
@" 
using System;
namespace model 
{ 
    [Serializable]
    public class mpUser
    {
        private string _Username;
        public string Username { get{ return _Username; } set{ _Username = value; } }
        private string _Password;  
        public string Password { get{ return _Password; } set{ _Password = value; } }
        private string _Fullname;  
        public string Fullname { get{ return _Fullname; } set{ _Fullname = value; } }
    } 
}
";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //host.BroadCast("@@" +textBox1.Text);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //host.BroadCast(new Msg()
            //{ 
            //    Data = typeof(mpUser),
            //});
            //host.BroadCast(new Msg()
            //{
            //    DataAction = Core.DataAction.DB_ADD,
            //    Data = new mpUser() { Username = "thinh", Password = "12345", Fullname = "Nguyễn Văn Thịnh" }
            //});
            //host.BroadCast(new Msg()
            //{
            //    DataAction = Core.DataAction.DB_ADD,
            //    Data = new mpUser() { Username = "tu", Password = "99999", Fullname = "Nguyễn Cam Tu" }
            //});
        }
    }


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
