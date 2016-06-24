using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Collections.Specialized;
using System.Xml;


namespace Export2Dxf
{
    class Program
    {
        //static decimal ld_version = 2.0m;


        static void Main(string[] args)
        {
            
            ShowIntro();
            if (args.Length == 0)
            {
                Console.WriteLine("input parameter missing");
                ShowUsage();
                return;
            }
            if (args.Length > 3)
            {
                Console.WriteLine("too many parameters");
                ShowUsage();
                return;
            }

            string ls_in = args[0];
            if (!System.IO.File.Exists(ls_in))
            {
                Console.WriteLine("input file does not exist");
                ShowUsage();
                return;
            }

            string ls_out = null;
            if (args.Length > 1)
            {
                ls_out = args[1];
            }
            else 
            {
                ls_out = Path.ChangeExtension(ls_in, "dxf");
            }

            string ls_log = null;
            if (args.Length > 2)
            {
                ls_log = args[2];
            }

            Console.WriteLine("Converting file");
            Console.WriteLine(ls_in);

            StringCollection l_coll = new StringCollection();
            try
            {
                Loader.Loader.Sxe2Dxf(ls_in, ls_out, l_coll);
            }
            catch (System.Exception ex)
            {
                string ls_error = ex.Message;
                ls_error += ex.InnerException;
                ls_error += ex.StackTrace;

                if (!string.IsNullOrEmpty(ls_log))
                {
                    WriteToLogFileXml(ls_log, ls_error, null);
                }
                else
                {
                    Console.Write(ls_error);
                }
                return;
            }

            if (!string.IsNullOrEmpty(ls_log))
            {
                WriteToLogFileXml(ls_log, "ok", l_coll);
            }

            Console.WriteLine("Conversion completed");

        }

        private static void WriteToLogFile(string as_path, string as_msg)
        {
            FileStream l_log_file = File.Create(as_path);
            StreamWriter l_writer = new StreamWriter(l_log_file);
            //l_writer.NewLine = "\n";
            l_writer.Write(as_msg);
            l_writer.Close();
        }

        private static void WriteToLogFileXml(string as_path, string as_msg, StringCollection a_coll)
        {
            XmlWriterSettings l_settings = new XmlWriterSettings();
            using (XmlWriter l_writer = XmlWriter.Create(as_path, l_settings))
            {
                l_writer.WriteStartElement("msg");
                l_writer.WriteElementString("status", as_msg);
                if (a_coll != null)
                {
                    l_writer.WriteStartElement("paths");
                    foreach (string ls_path in a_coll)
                    {
                        l_writer.WriteElementString("path", ls_path);
                    }
                    l_writer.WriteEndElement();
                }
                l_writer.WriteEndElement();
            }
        }

        private static void ShowIntro()
        {
            AssemblyName l_an = Assembly.GetExecutingAssembly().GetName(false);
            string ls_echo = string.Format("ProfiCAD2Dxf version {0}", l_an.Version.ToString());
            Console.WriteLine(ls_echo);
        }

        private static void ShowUsage()
        {
            Console.WriteLine("Syntax:");
            Console.WriteLine("ProfiCAD2Dxf input [output]");
        }
    }
}
