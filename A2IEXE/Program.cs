using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



namespace ACAD2Img
{
    class Program
    {
        static void Main(string[] args)
        {
            string ls_issue = string.Empty;

            
string[] ls_formats = new string[] { "png", "tiff", "jpeg", "bmp", "gif", "pdf", "pdf", "xaml", "svg" };

            if (args.Length < 5)
            {
                ls_issue = "ProfiCAD www.proficad.com\nUsage: A2IEXE <png|tiff|jpeg|bmp|gif|pdf> <yes|no> <size> <input file> <output file> [log file]";
                Console.WriteLine(ls_issue);
                Environment.Exit(0);
            }
            string ls_format = args[0];
            if (!ls_formats.Contains(ls_format))
            {
                Console.WriteLine("1st param must be png|tiff|jpeg|bmp|gif|pdf");
                Environment.Exit(0);
            }

            string ls_invertColors = args[1];
            if (!new string[] { "yes", "no" }.Contains(ls_invertColors))
            {
                Console.WriteLine("2nd param must be yes|no");
                Environment.Exit(0);
            }
            bool lb_invertColors = ls_invertColors.ToLowerInvariant().Equals("yes");

            string ls_size = args[2]; int li_size;
            if (!int.TryParse(ls_size, out li_size))
            {
                Console.WriteLine("3rd param must be max size (pixels)");
                Environment.Exit(0);
            }

            string ls_fileIn = args[3];
            if (!System.IO.File.Exists(ls_fileIn))
            {
                Console.WriteLine("4th param must be input file");
                Environment.Exit(0);
            }

            string ls_fileOut = args[4];
            if (ls_fileOut.Length == 0)
            {
                Console.WriteLine("5th param must be output file");
                Environment.Exit(0);
            }

            string ls_fileLog = args[5];
            string ls_echo = "ok";

            try
            {
                A2IDLL.Program.Convert(ls_format, lb_invertColors, li_size, ls_fileIn, ls_fileOut);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);

                ls_echo = e.Message + "\n" + e.StackTrace + "\n";
                if (e.InnerException != null)
                {
                    ls_echo += e.InnerException.Message + "\n" + e.InnerException.StackTrace;
                }
                
            }


            if (!string.IsNullOrEmpty(ls_fileLog))
            {
                System.IO.File.WriteAllText(ls_fileLog, ls_echo);
            }
        }
    }
}
