using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Permissions;
using System.Threading;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Diagnostics;

namespace AppHost
{ 
    class Program
    { 
        static void Main(string[] args)
        {  
            HostManager.Start();   
        } 
    }//end class
}
