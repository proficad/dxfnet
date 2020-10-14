using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

using System.Drawing;
using WW.Math;
using WW.Math.Geometry;
using WW.Cad.IO;

//using WW.ComponentModel;
using WW.Cad.Model;
using WW.Cad.Model.Entities;
using WW.Cad.Drawing;
using DxfNet;
using System.IO;
using System.Reflection;
using System.Xml;
using Core;
using WW.Cad.Model.Tables;

namespace Dxf2ProfiCAD
{
    class Program
    {
        private enum OutputFormat { output_format_sxe, output_format_pxf, output_format_ppd };

        static OutputFormat l_output_format;

        private static string m_path_log;

        private static void Main(string[] args)
        {
            ShowIntro();
            if (args.Length < 3)
            {
                Report_Error_And_Quit("input parameter missing");
                ShowUsage();
                return;
            }
            if (args.Length > 4)
            {
                Report_Error_And_Quit("too many parameters");
                ShowUsage();
                return;
            }

            if (args.Length > 3)
            {
                m_path_log = args[3];
            }

            string ls_in  = args[0];
            string ls_out = args[1];

            if (!Directory.Exists(ls_out))
            {
                Report_Error_And_Quit("output path does not exist");
                ShowUsage();
                return;
            }


            string ls_out_format = args[2];
            if (ls_out_format == "sxe")
            {
                l_output_format = OutputFormat.output_format_sxe;
            }
            else if (ls_out_format == "pxf")
            {
                l_output_format = OutputFormat.output_format_pxf;
            }
            else if (ls_out_format == "ppd")
            {
                l_output_format = OutputFormat.output_format_ppd;
            }
            else
            {
                ShowUsage();
                return;
            }

            try
            {
                if (File.Exists(ls_in))
                {
                    ConvertOneFile(ls_in, ls_out, l_output_format);
                }
                else if (Directory.Exists(ls_in))
                {
                    string[] ls_filenames = Directory.GetFiles(ls_in, "*.dxf", SearchOption.AllDirectories);
                    foreach (string ls_path in ls_filenames)
                    {
                        string ls_out_path = Calculate_Output_Path(ls_in, ls_path, ls_out);

                        System.IO.Directory.CreateDirectory(ls_out_path);

                        ConvertOneFile(ls_path, ls_out_path, l_output_format);
                    }
                }
                else
                {
                    ShowUsage();
                }
            }
            catch (Exception ex)
            {
                string ls_error = ex.Message;
                ls_error += ex.InnerException;
                ls_error += ex.StackTrace;

                if (!string.IsNullOrEmpty(m_path_log))
                {
                    WriteToLogFile(m_path_log, ls_error);
                }
                else
                {
                    Console.Write(ls_error);
                }
                return;
            }

            if (!string.IsNullOrEmpty(m_path_log))
            {
                WriteToLogFile(m_path_log, "ok");
            }

        }

        private static string Calculate_Output_Path(string as_in_folder, string as_in_file, string as_out_folder)
        {
            string ls_folder_of_actual_file = Path.GetDirectoryName(as_in_file);

            int li_len_in = 1 + as_in_folder.Length;

            string ls_relative_path_out = ls_folder_of_actual_file.Substring(li_len_in);

            return Path.Combine(as_out_folder, ls_relative_path_out);

        }

        private static void Report_Error_And_Quit(string as_message)
        {
            if (!string.IsNullOrEmpty(m_path_log))
            {
                WriteToLogFile(m_path_log, as_message);
            }

            Console.WriteLine(as_message);
        }


        private static void WriteToLogFile(string as_path, string as_msg)
        {
            FileStream l_log_file = File.Create(as_path);
            StreamWriter l_writer = new StreamWriter(l_log_file);
            //l_writer.NewLine = "\n";
            l_writer.Write(as_msg);
            l_writer.Close();
        }

        private static void WriteToLogFileXml(string as_path, string as_msg)
        {
            XmlWriterSettings l_settings = new XmlWriterSettings();
            using (XmlWriter l_writer = XmlWriter.Create(as_path, l_settings))
            {
                l_writer.WriteStartElement("msg");
                l_writer.WriteElementString("status", as_msg);
           
                l_writer.WriteEndElement();
            }
        }


        private static void AdjustScaleAndShift(DxfModel a_model, Size a_size_target)
        {
            BoundsCalculator l_bc = new BoundsCalculator();
            l_bc.GetBounds(a_model);
            Bounds3D l_bounds3D = l_bc.Bounds;

            double li_scale_x = a_size_target.Width / l_bounds3D.Delta.X;
            double li_scale_y = a_size_target.Height / l_bounds3D.Delta.Y;

            if ((li_scale_x == 0) || (li_scale_y == 0))
            {
                throw new Exception("problem with model bounds");
            }

            double li_scale = Math.Min(li_scale_x, li_scale_y);
            Converter.SetScale(li_scale);

            int li_shift_x = (int)(-l_bounds3D.Center.X);
            int li_shift_y = (int)(-l_bounds3D.Center.Y);


            //            Converter.SetShift(li_shift_x, li_shift_y, a_size_target.Height);
            Converter.SetShift(li_shift_x, li_shift_y, 0);
        }

        private static void ConvertRepo(Repo a_repo, DxfModel model)
        {
            foreach (WW.Cad.Model.Tables.DxfBlock l_block in model.Blocks)
            {
                if (string.IsNullOrEmpty(l_block.Name))
                {
                    continue;
                }

                //vyrob PpdDoc
                PpdDoc l_ppdDoc = new PpdDoc();
                l_ppdDoc.m_name = l_block.Name;
                l_ppdDoc.m_lG = l_block.Name; //yes, we do not have GUID, let us try name instead

                System.Diagnostics.Debug.Assert(l_ppdDoc.m_name.Length > 0);


                foreach (DxfEntity l_entity in l_block.Entities)
                {
                    DrawObj l_drawObj = Converter.Convert(l_entity);
                    if (l_drawObj != null)
                    {
                        if (l_drawObj is Insert l_insert)
                        {
                            Add_To_Repo(l_ppdDoc.m_repo, model, l_insert.m_lG);
                        }

                        l_ppdDoc.Add(l_drawObj, l_entity.Layer.Name);
                    }
                }

                //calc center point of this ppd
                int l_baseX = Converter.MyShiftScaleX(l_block.BasePoint.X);
                int l_baseY = Converter.MyShiftScaleY(l_block.BasePoint.Y);
                Point l_basePoint = new Point(l_baseX, l_baseY);
                l_ppdDoc.RecalcToBeInCenterPoint(l_basePoint);


                //add it to PCadDoc
                a_repo.AddPpd(l_ppdDoc);
            }

        }

        private static void PrintSep()
        {
            System.Console.WriteLine("------------------------------------");
        }

 
        private static void ShowIntro()
        {
            AssemblyName l_an = Assembly.GetExecutingAssembly().GetName(false);
            string ls_echo = $"ProfiCAD\nA2P version {l_an.Version.ToString()}";
            Console.WriteLine(ls_echo);
        }


        private static void ShowUsage()
        {
            Console.WriteLine("Syntax:");
            Console.WriteLine("A2P input_path output_path format(sxe|pxf|ppd) [log_path]");
        }


        private static void ConvertOneFile(string as_input_path, string as_output_path, OutputFormat a_format)
        {
            string ls_extension;

            Repo l_repo;
            DrawDoc l_drawDoc;

            switch (a_format)
            {
                case OutputFormat.output_format_sxe:
                    ls_extension = "sxe";
                    CollPages l_collPages = new CollPages();
                    PCadDoc l_pcadDoc = new PCadDoc(l_collPages);
                    l_repo = l_pcadDoc.GetRepo();
                    l_drawDoc = l_pcadDoc;
                    break;
                case OutputFormat.output_format_pxf:
                    ls_extension = "pxf";
                    Core.PxfDoc l_pxfDoc = new Core.PxfDoc();
                    l_repo = l_pxfDoc.GetRepo();
                    l_drawDoc = l_pxfDoc;
                    break;
                case OutputFormat.output_format_ppd:
                    ls_extension = "ppd";
                    PpdDoc l_ppdDoc = new PpdDoc();
                    l_repo = l_ppdDoc.GetRepo();
                    l_drawDoc = l_ppdDoc;
                    break;
                default:
                    return;
            }


            DxfModel model;
            string extension = Path.GetExtension(as_input_path);
            if (string.Compare(extension, ".dwg", true) == 0)
            {
                model = DwgReader.Read(as_input_path);
            }
            else
            {
                model = DxfReader.Read(as_input_path);
            }


   
            const int li_size = 2800;
            Size l_size_target = new Size(li_size, li_size);
            AdjustScaleAndShift(model, l_size_target);

                                     


            ConvertRepo(l_repo, model);

            foreach (DxfEntity l_entity in model.Entities)
            {
                DrawObj l_drawObj = Converter.Convert(l_entity);

                if (l_drawObj != null)
                {
                    // if it is an insert move it by the offset of its PPD
                    if (l_drawObj is Insert l_insert)
                    {
                        string ls_lastGuid = l_insert.m_lG;

                        /*
                        if (ls_lastGuid == "A32")
                        {
                            int o = 5;
                        }
                        */

                        System.Diagnostics.Debug.Assert(ls_lastGuid.Length > 0);
                        PpdDoc l_ppdDoc = l_repo.FindPpdDocInRepo(ls_lastGuid);
                        if (l_ppdDoc != null)
                        {
                            l_insert.SetPpdDoc(l_ppdDoc);

                            System.Drawing.Size l_offset = l_ppdDoc.m_offset;
                            l_insert.Offset(l_offset);
                        }
                    }


                    l_drawDoc.Add(l_drawObj, l_entity.Layer.Name);
                }
            }

            l_drawDoc.SetSize(l_size_target);

            string ls_file_name = Path.GetFileNameWithoutExtension(as_input_path);
            string ls_output_path = Path.Combine(as_output_path, ls_file_name) + "." + ls_extension;


            l_drawDoc.Save(ls_output_path);

            Console.WriteLine("Conversion completed");
        }

        private static void Add_To_Repo(Repo a_repo, DxfModel a_model, string as_block_name)
        {
            if (!a_model.Blocks.Contains(as_block_name))
            {
                return;
            }


            DxfBlock l_block = a_model.Blocks[as_block_name];
            if (l_block != null)
            {
                //vyrob PpdDoc
                PpdDoc l_ppdDoc = new PpdDoc();
                l_ppdDoc.m_name = l_block.Name;
                l_ppdDoc.m_lG = l_block.Name; //yes, we do not have GUID, let us try name instead

                System.Diagnostics.Debug.Assert(l_ppdDoc.m_name.Length > 0);


                foreach (DxfEntity l_entity in l_block.Entities)
                {
                    DrawObj l_drawObj = Converter.Convert(l_entity);
                    if (l_drawObj != null)
                    {
                        if (l_drawObj is Insert l_insert)
                        {
                            Add_To_Repo(l_ppdDoc.m_repo, a_model, l_insert.m_lG);
                        }

                        l_ppdDoc.Add(l_drawObj, l_entity.Layer.Name);
                    }
                }

                //calc center point of this ppd
                int l_baseX = Converter.MyShiftScaleX(l_block.BasePoint.X);
                int l_baseY = Converter.MyShiftScaleY(l_block.BasePoint.Y);
                Point l_basePoint = new Point(l_baseX, l_baseY);
                l_ppdDoc.RecalcToBeInCenterPoint(l_basePoint);


                //add it to PCadDoc
                a_repo.AddPpd(l_ppdDoc);
            }

        }

    }



}
