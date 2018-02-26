using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Permissions;
using System.Threading;
using System.IO;
using System.Reflection;

namespace AppDB
{ 
    class Program
    { 
        static void Main(string[] args)
        {
            DB.Start();

            Console.ReadKey();
        }
    }//end class
}
