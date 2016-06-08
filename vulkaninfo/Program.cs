using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vulkan;

namespace vulkaninfo
{
    class Program
    {
        static void Main(string[] args)
        {            
            InfoGenerator gen = new InfoGenerator();
            gen.DumpInfo(new System.IO.StreamWriter(Console.OpenStandardOutput()));

            Console.ReadLine();
        }
    }
}
