using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Loader;

namespace Tester
{
    class Program
    {
        static void Main(string[] args)
        {

            for(int li_i = 0; li_i < 50; ++li_i)
            {
                Console.WriteLine("{0} -> {1}", li_i, Exporter.Calculate_Line_Thickness_Ellipse(li_i));
            }
            






        }
    }
}
