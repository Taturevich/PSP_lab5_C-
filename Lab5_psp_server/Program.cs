using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab5_psp_server
{
    class Program
    {
        static void Main(string[] args)
        {

            ThreadedServer ts = new ThreadedServer(80);
            ts.Start();
            Console.ReadLine();
            
        }
    }
}
