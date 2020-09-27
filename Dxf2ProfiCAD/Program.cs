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

namespace Dxf2ProfiCAD
{
    class Program
    {
        enum OutputFormat { output_format_sxe, output_format_pxf };

        static OutputFormat l_output_format;


        static void Main(string[] args)
        {
            ShowIntro();
            if (args.Length < 2)
            {
                Console.WriteLine("input parameter missing");
                ShowUsage();
                return;
            }
            if (args.Length > 2)
            {
                Console.WriteLine("too many parameters");
                ShowUsage();
                return;
            }


            string ls_in = args[0];

            string ls_out_format = args[1];
            if (ls_out_format == "sxe")
            {
                l_output_format = OutputFormat.output_format_sxe;
            }
            else if (ls_out_format == "pxf")
            {
                l_output_format = OutputFormat.output_format_pxf;
            }
            else
            {
                ShowUsage();
                return;
            }

            if (System.IO.File.Exists(ls_in))
            {
                ConvertOneFile(ls_in, l_output_format);
            }
            else if (System.IO.Directory.Exists(ls_in))
            {
                string[] ls_filenames = Directory.GetFiles(ls_in, "*.dxf", SearchOption.AllDirectories);
                foreach (string ls_path in ls_filenames)
                {
                    ConvertOneFile(ls_path, l_output_format);
                }
            }
            else
            {
                ShowUsage();
                return;
            }



        }






        private static void AdjustScaleAndShift(DxfModel a_model, Size a_size_target)
        {
            BoundsCalculator l_bc = new BoundsCalculator();
            l_bc.GetBounds(a_model);
            Bounds3D l_bounds3D = l_bc.Bounds;

            double li_scale_x = a_size_target.Width / l_bounds3D.Delta.X;
            double li_scale_y = a_size_target.Height / l_bounds3D.Delta.Y;
            double li_scale = Math.Min(li_scale_x, li_scale_y);
            Converter.SetScale(li_scale);

            int li_shift_x = (int)(-l_bounds3D.Corner1.X);
            int li_shift_y = (int)(-l_bounds3D.Corner2.Y);
            //            Converter.SetShift(li_shift_x, li_shift_y, a_size_target.Height);
            Converter.SetShift(li_shift_x, li_shift_y, 0);
        }

        private static void ConvertRepo(Repo a_repo, DxfModel model)
        {
            foreach (WW.Cad.Model.Tables.DxfBlock l_block in model.Blocks)
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

        private static double PrintStats(DxfModel model)
        {

            System.Console.WriteLine("Layouts");
            foreach (var v in model.Layouts)
            {
                System.Console.WriteLine("{0}, Denominator={1}, Numerator={2}", v.Name, v.CustomPrintScaleDenominator, v.CustomPrintScaleNumerator);

            }
            System.Console.WriteLine("-------");


            System.Console.WriteLine("Views");
            foreach (var v in model.Views)
            {
                System.Console.WriteLine("{0}", v.Name);

            }
            System.Console.WriteLine("-------");


            System.Collections.Hashtable l_hash_entities = new Hashtable(500);
            System.Collections.Hashtable l_hash_text_heights = new Hashtable(500);


            double ld_smallest_text_height = -1;
            string ls_smallest_text_wording = "";

            foreach (DxfEntity l_entity in model.Entities)
            {
                int li_count = 0;
                if (l_hash_entities.Contains(l_entity.GetType()))
                {
                    li_count = (int)l_hash_entities[l_entity.GetType()];
                }
                l_hash_entities[l_entity.GetType()] = li_count + 1;



                DxfText l_dxfText = l_entity as DxfText;
                if (l_dxfText != null)
                {
                    CalculateTextSize(l_dxfText.Height, l_dxfText.Text,
                        ref l_hash_entities, ref l_hash_text_heights,
                        ref ld_smallest_text_height, ref ls_smallest_text_wording);
                }
                DxfMText l_dxfMText = l_entity as DxfMText;
                if (l_dxfMText != null)
                {
                    CalculateTextSize(l_dxfMText.Height, l_dxfMText.Text,
                        ref l_hash_entities, ref l_hash_text_heights,
                        ref ld_smallest_text_height, ref ls_smallest_text_wording);
                }



            }
            foreach (object l_key in l_hash_entities.Keys)
            {
                System.Console.WriteLine("{0} : {1}", l_key.ToString(), l_hash_entities[l_key].ToString());
            }
            PrintSep();
            foreach (object l_key in l_hash_text_heights.Keys)
            {
                System.Console.WriteLine("{0} : {1}", l_key.ToString(), l_hash_text_heights[l_key].ToString());
            }
            PrintSep();
            Console.WriteLine("smallest size: {0} for text \"{1}\"", ld_smallest_text_height, ls_smallest_text_wording);
            PrintSep();


            return ld_smallest_text_height;
        }

        private static void CalculateTextSize(double ai_height, string as_text,
            ref System.Collections.Hashtable l_hash_entities,
            ref System.Collections.Hashtable l_hash_text_heights,
            ref double ld_smallest_text_height, ref string ls_smallest_text_wording)
        {
            int li_count_height = 0;

            if (l_hash_text_heights.Contains(ai_height))
            {
                li_count_height = (int)l_hash_text_heights[ai_height];
            }
            l_hash_text_heights[ai_height] = li_count_height + 1;

            if (ld_smallest_text_height == -1)
            {
                ld_smallest_text_height = ai_height;
                ls_smallest_text_wording = as_text;
            }
            else
            {
                if (ld_smallest_text_height > ai_height)
                {
                    ld_smallest_text_height = ai_height;
                    ls_smallest_text_wording = as_text;
                }
            }
        }

        /*
                private static void Covert_To_ProfiCAD_Lines(DxfModel model, String as_path)
                {
                    List<Polyline2D> collectedPolylines = PolygonWireframeGraphicsFactory.ConvertModelToPolylines(model, GraphicsConfig.BlackBackground);

                    double l_min_x = 0, l_max_x = 0, l_min_y = 0, l_max_y = 0;

                    bool lb_first = true;
                    foreach (Polyline2D x in collectedPolylines)
                    {
                        foreach(WW.Math.Point2D l_point in x)
                        {
                            if (lb_first)
                            {
                                l_min_x = l_max_x = l_point.X;
                                l_min_y = l_max_y = l_point.Y;
                                lb_first = false;
                            }
                            else
                            {
                                l_min_x = System.Math.Min(l_min_x, l_point.X);
                                l_max_x = System.Math.Max(l_max_x, l_point.X);
                                l_min_y = System.Math.Min(l_min_y, l_point.Y);
                                l_max_y = System.Math.Max(l_max_y, l_point.Y);
                            }
                        }
                    }

                    double li_size_x = l_max_x - l_min_x;
                    double li_size_y = l_max_y - l_min_y;

                    if (li_size_x == 0)
                    {
                        li_size_x = 1;
                    }
                    if (li_size_y == 0)
                    {
                        li_size_y = 1;
                    }

                    const int li_paper_x = 3000;
                    const int li_paper_y = 3000;

                    double l_ratio_x = li_paper_x / li_size_x;
                    double l_ratio_y = li_paper_y / li_size_y;

                    double l_ratio = System.Math.Min(l_ratio_x, l_ratio_y);

                    CollPages l_collPages = new CollPages();
                    PCadDoc l_pcadDoc = new PCadDoc(l_collPages);

                    foreach (Polyline2D l_polyline in collectedPolylines)
                    {
                        DrawObj l_drawObj = Convert(l_polyline, l_ratio, l_min_x, l_min_y);
                        if (l_drawObj != null)
                        {
                            l_pcadDoc.Add(l_drawObj, null);
                        }
                    }

                    l_pcadDoc.Save(as_path);

                }
        */
        static DrawObj Convert(Polyline2D a_line, double a_ratio, double ai_min_x, double ai_min_y)
        {
            DrawPoly l_drawPoly = new DrawPoly(Shape.polyline);
            foreach (WW.Math.Point2D l_point in a_line)
            {
                int li_x = (int)((l_point.X - ai_min_x) * a_ratio);
                int li_y = (int)((l_point.Y - ai_min_y) * a_ratio);

                l_drawPoly.AddPoint(li_x, li_y);
            }

            if (l_drawPoly.m_points.Count < 2)
            {
                return null;
            }

            return l_drawPoly;
        }

        private static void ShowIntro()
        {
            AssemblyName l_an = Assembly.GetExecutingAssembly().GetName(false);
            string ls_echo = string.Format("Dxf2ProfiCAD version {0}", l_an.Version.ToString());
            Console.WriteLine(ls_echo);
        }

        private static void ShowUsage()
        {
            Console.WriteLine("Syntax:");
            Console.WriteLine("Dxf2ProfiCAD input(path) output(sxe|pxf)");

        }

        private static void ConvertOneFile(string as_input_path, OutputFormat a_format)
        {
            string ls_extension = a_format == OutputFormat.output_format_sxe ? "sxe" : "pxf";
            string ls_outputPath = Path.ChangeExtension(as_input_path, ls_extension);

            DxfModel model = null;
            string extension = Path.GetExtension(as_input_path);
            if (string.Compare(extension, ".dwg", true) == 0)
            {
                model = DwgReader.Read(as_input_path);
            }
            else
            {
                model = DxfReader.Read(as_input_path);
            }


            //try to convert using the WireFrameGraphicsFactory
            //Covert_To_ProfiCAD_Lines(model, @"H:\lines.sxe");

            //            double ld_smallest_font_size = PrintStats(model);



            const int li_size = 2800;
            Size l_size_target = new Size(li_size, li_size);
            AdjustScaleAndShift(model, l_size_target);


            Repo l_repo = null;
            DrawDoc l_drawDoc = null;

            if(a_format == OutputFormat.output_format_sxe)
            {
                CollPages l_collPages = new CollPages();
                PCadDoc l_pcadDoc = new PCadDoc(l_collPages);
                l_repo = l_pcadDoc.GetRepo();
                l_drawDoc = l_pcadDoc;
            }
            else
            {
                Core.PxfDoc l_pxfDoc = new Core.PxfDoc();
                l_repo = l_pxfDoc.GetRepo();
                l_drawDoc = l_pxfDoc;
            }



            ConvertRepo(l_repo, model);

            foreach (DxfEntity l_entity in model.Entities)
            {
                DrawObj l_drawObj = Converter.Convert(l_entity);

                if (l_drawObj != null)
                {
                    // if it is an insert move it by the offset of its PPD
                    if (l_drawObj is Insert)
                    {
                        Insert l_insert = l_drawObj as Insert;
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


            l_drawDoc.Save(ls_outputPath);

            Console.WriteLine("Conversion completed");
        }
    }



}
