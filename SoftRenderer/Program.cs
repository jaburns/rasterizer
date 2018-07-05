using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Sharpy
{
    class Program
    {
        [DllImport("GFXLesson.dll", CharSet=CharSet.Unicode)]
        static extern void run_it();
            
        static void Main(string[] args)
        {
            run_it();
        }
    }
}
