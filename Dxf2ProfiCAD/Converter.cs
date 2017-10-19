using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;


//using WW.ComponentModel;
using WW.Cad.Model.Entities;

using DxfNet;



namespace Dxf2ProfiCAD
{
    public static class Converter
    {
        static double m_scaleX = 1;
        static double m_scaleY = -m_scaleX;

        static int m_shift_x = 0;
        static int m_shift_y = 0;
        static int m_shift_target_y = 0;

        public static int MyShiftScaleX(double ai_what)
        {
            return (int) ((ai_what + m_shift_x) * m_scaleX);
        }
        public static int MyShiftScaleY(double ai_what)
        {
            return m_shift_target_y + (int)((ai_what + m_shift_y) * m_scaleY);
        }

        public static int MyScaleX(double ai_what)
        {
            return (int)(ai_what * m_scaleX);
        }
        public static int MyScaleY(double ai_what)
        {
            return (int)(ai_what * m_scaleY);
        }


        public static DrawObj Convert(DxfEntity a_entity)
        {
            //DrawObj l_obj;
            //System.Diagnostics.Debug.WriteLine("converting " + a_entity.ToString());
            
            if (a_entity is DxfInsert)
            {
                return ConvertInsert(a_entity);
            }
            if (a_entity is DxfMText)
            {
                return ConvertDxfMText(a_entity);
            }
            if (a_entity is DxfText)
            {
                return ConvertDxfText(a_entity);
            }
            if (a_entity is DxfArc)
            {
                return ConvertArc(a_entity);
            }
            if (a_entity is DxfCircle)
            {
                return ConvertCircle(a_entity);
            }
            if (a_entity is DxfHatch)
            {
                return ConvertDxfHatch(a_entity);
            }
            if (a_entity is DxfLine)
            {
                return ConvertDxfLine(a_entity);
            }
            if (a_entity is DxfLwPolyline)
            {
                return ConvertDxfLwPolyline(a_entity);
            }
            if (a_entity is DxfPolyline2D)
            {
                return ConvertDxfPolyline2D(a_entity);
            }

            System.Console.WriteLine("did not convert type {0}", a_entity.ToString());
            return null;

        }

        private static Insert ConvertInsert(DxfEntity a_entity)
        {
            DxfInsert l_dxfInsert = a_entity as DxfInsert;

            /*
            if (l_dxfInsert.Block.Name == "A24")
            {
                int o = 5;
            }
            */

            //turns
            int li_turns = 0;
            if (l_dxfInsert.Rotation != 0)
            {
                li_turns = (int)Math.Round(4d * l_dxfInsert.Rotation / Math.PI);
                //Console.WriteLine("l_dxfInsert.Rotation {0}", l_dxfInsert.Rotation);
            }
            bool lb_flipX = (l_dxfInsert.ScaleFactor.X == -1d);
            bool lb_flipY = (l_dxfInsert.ScaleFactor.Y == -1d);

            if (lb_flipX)
            {
                //Console.WriteLine("flip X");
            }
            if (lb_flipY)
            {
                //Console.WriteLine("flip Y");
            }

            string ls_blockName = l_dxfInsert.Block.Name;

            float fl = (float)1.1; //pouze pro zkompilovani
            Insert l_insert = new Insert(Shape.soucastka, MyShiftScaleX(l_dxfInsert.InsertionPoint.X), MyShiftScaleY(l_dxfInsert.InsertionPoint.Y), fl, fl);//99
            l_insert.m_angle = RadiansToAngle(l_dxfInsert.Rotation);
            l_insert.m_scaleX = (float)l_dxfInsert.ScaleFactor.X;
            l_insert.m_scaleY = (float)l_dxfInsert.ScaleFactor.Y;


            //instead of GUID it will be just a name of the block
            l_insert.m_lG = ls_blockName;
            l_insert.m_hor = lb_flipY;
            l_insert.m_ver = lb_flipX;


            foreach (DxfAttribute l_attr in l_dxfInsert.Attributes)
            {
                string ls_text = l_attr.Text;
                string ls_tagString = l_attr.TagString;
                int li_x = MyShiftScaleX(l_attr.AlignmentPoint1.X);
                int li_y = MyShiftScaleY(l_attr.AlignmentPoint1.Y);
                
                int li_turns_attr = 0;//TODO
                if (l_attr.Rotation != 0)
                {
                    li_turns_attr = (int)Math.Round(4d * l_attr.Rotation / Math.PI);
                }

                /*
                DxfExtendedDataCollection coll = l_attr.ExtendedDataCollection;
                Console.WriteLine("attributes {0} ~ {1} ~ {2} ~ {3}", ls_blockName, ls_text, ls_tagString, coll.Count);
                int d = 5;
                */
                Insert.Satelite l_sat = new Insert.Satelite(ls_tagString, ls_text, li_x, li_y, true, li_turns_attr);
                l_insert.m_satelites.Add(l_sat);

            }

            return l_insert;
        }

        private static DrawObj ConvertDxfHatch(DxfEntity a_entity)
        {
            DxfHatch l_hatch = a_entity as DxfHatch;
            if (l_hatch.Pattern == null)
            {
                return null;
                //System.Console.WriteLine("Solid DxfHatch");
            }
            else
            {
                System.Console.WriteLine("Not Solid DxfHatch");
            }
            return null;
        }

        private static DrawObj ConvertDxfPolyline2D(DxfEntity a_entity)
        {
            DxfPolyline2D l_dxfLine = a_entity as DxfPolyline2D;
            DrawRect l_circle = TryMakeCircle(l_dxfLine);
            if (l_circle != null)
            {
                return l_circle;
            }

            DrawPoly l_drawPoly = new DrawPoly(Shape.polyline);

            foreach (DxfVertex2D l_vertex in l_dxfLine.Vertices)
            {
                l_drawPoly.AddPoint(MyShiftScaleX(l_vertex.X), MyShiftScaleY(l_vertex.Y));
            }



            if (l_dxfLine.Closed)
            {
                DxfVertex2D l_vertex = l_dxfLine.Vertices[0];
                l_drawPoly.AddPoint(MyShiftScaleX(l_vertex.X), MyShiftScaleY(l_vertex.Y));
            }


            l_drawPoly.RecalcPosition();
            return l_drawPoly;

        }

        private static DrawObj ConvertDxfLwPolyline(DxfEntity a_entity)
        {
            DxfLwPolyline l_lw_polyline = a_entity as DxfLwPolyline;
            DrawPoly l_drawPoly = new DrawPoly(Shape.polyline);

            foreach (DxfLwPolyline.Vertex l_vertex in l_lw_polyline.Vertices)
            {
                l_drawPoly.AddPoint(MyShiftScaleX(l_vertex.X), MyShiftScaleY(l_vertex.Y));
            }


            if (l_lw_polyline.Closed)
            {
                DxfLwPolyline.Vertex l_vertex = l_lw_polyline.Vertices[0];
                l_drawPoly.AddPoint(MyShiftScaleX(l_vertex.X), MyShiftScaleY(l_vertex.Y));
            }


            l_drawPoly.RecalcPosition();
            return l_drawPoly;
        }

        private static DrawObj ConvertDxfLine(DxfEntity a_entity)
        {
            DxfLine l_line = a_entity as DxfLine;
            DrawPoly l_drawPoly = new DrawPoly(Shape.polyline);
            l_drawPoly.AddPoint(MyShiftScaleX(l_line.Start.X), MyShiftScaleY(l_line.Start.Y));
            l_drawPoly.AddPoint(MyShiftScaleX(l_line.End.X), MyShiftScaleY(l_line.End.Y));
            l_drawPoly.RecalcPosition();
            return l_drawPoly;
        }

        private static DrawObj ConvertCircle(DxfEntity a_entity)
        {
            DxfCircle l_dxfCircle = a_entity as DxfCircle;

            int li_radius = Math.Abs(MyScaleX(l_dxfCircle.Radius));
            int li_center_x = MyShiftScaleX(l_dxfCircle.Center.X);
            int li_center_y = MyShiftScaleY(l_dxfCircle.Center.Y);

            int li_left = li_center_x - li_radius;
            int li_top = li_center_y - li_radius;
            int li_width = 2 * li_radius;

            Rectangle l_boundingRect = new Rectangle(li_left, li_top, li_width, li_width);


            DrawRect l_circle = new DrawRect(Shape.ellipse, l_boundingRect);
            return l_circle;
        }

        private static DrawObj ConvertArc(DxfEntity a_entity)
        {
            DxfArc l_dxfArc = a_entity as DxfArc;
            int li_radius = Math.Abs(MyScaleX(l_dxfArc.Radius));
            int li_center_x = MyShiftScaleX(l_dxfArc.Center.X);
            int li_center_y = MyShiftScaleY(l_dxfArc.Center.Y);

            int li_left = li_center_x - li_radius;
            int li_top = li_center_y - li_radius;
            int li_width = 2 * li_radius;

            Rectangle l_boundingRect = new Rectangle(li_left, li_top, li_width, li_width);

            DrawRect l_arc = new DrawRect(Shape.arc, l_boundingRect);

            l_arc.m_arcBegin = Angle2Size(l_dxfArc.StartAngle);
            l_arc.m_arcEnd = Angle2Size(l_dxfArc.EndAngle);

            return l_arc;
        }

        private static FreeText ConvertDxfText(DxfEntity a_entity)
        {
            DxfText l_dxgMText = a_entity as DxfText;
            if (l_dxgMText.Text.Trim().Length == 0)
            {
                //System.Console.WriteLine("skipping empty text");
                return null;
            }


            EFont l_efont = new EFont();
            l_efont.m_size = (int)(l_efont.m_size * l_dxgMText.Height * 0.06);
            l_efont.m_size = MyScaleX(l_efont.m_size);

            Rectangle l_rect = new Rectangle(MyShiftScaleX(l_dxgMText.AlignmentPoint1.X), MyShiftScaleY(l_dxgMText.AlignmentPoint1.Y), 0, 0);

            FreeText l_text = new FreeText(l_dxgMText.Text, l_efont, l_rect, 0);

            l_text.m_alignment = ConvertAlignment(l_dxgMText);

            return l_text;
        }

        private static FreeText ConvertDxfMText(DxfEntity a_entity)
        {
            DxfMText l_dxfMText = a_entity as DxfMText;
            EFont l_efont = new EFont();
            l_efont.m_size = (int)(l_efont.m_size * l_dxfMText.Height * 0.06);
            l_efont.m_size = MyScaleX(l_efont.m_size);

            Rectangle l_rect = new Rectangle(MyShiftScaleX(l_dxfMText.InsertionPoint.X), MyShiftScaleY(l_dxfMText.InsertionPoint.Y), 0, 0);

            string ls_text = l_dxfMText.SimplifiedText;

            double l_angle = 0;
            double l_axis_x = l_dxfMText.XAxis.X;
            double l_axis_y = l_dxfMText.XAxis.Y;

            if ((l_axis_x == 1) && (l_axis_y == 0))
            {
                l_angle = 0;
            }
            else
            {
                l_angle = Math.Atan2(l_axis_y, l_axis_x);
            }

            int li_angle = 10 * RadiansToAngle(l_angle);

            FreeText l_text = new FreeText(ls_text, l_efont, l_rect, li_angle);
            

            return l_text;
        }

        private static string ParseText(string as_input)
        {
            if (string.IsNullOrEmpty(as_input))
            {
                return as_input;
            }

            if ((as_input[0] == '{') && (as_input[as_input.Length - 1] == '}'))
            {
                int li_index = as_input.IndexOf(';');
                if (-1 == li_index)
                {
                    as_input = as_input.Substring(1, as_input.Length - 2);
                }
                else
                {
                    as_input = as_input.Substring(li_index + 1, as_input.Length - li_index - 2);
                }
            }

            return as_input;

        }

        private static QTextAlignment ConvertAlignment(DxfText l_dxgMText)
        {
            if (l_dxgMText.HorizontalAlignment == TextHorizontalAlignment.Left)
            {
                return QTextAlignment.AL_LM;
            }
            if (l_dxgMText.HorizontalAlignment == TextHorizontalAlignment.Right)
            {
                return QTextAlignment.AL_RM;
            }

            return QTextAlignment.AL_MM;
            
        }

        //some circles are made as polylines with Bulges
        private static DrawRect TryMakeCircle(DxfPolyline2D a_entity)
        {
            if (a_entity.Vertices.Count != 4)
            {
                return null;
            }
            if (a_entity.Vertices[0].Bulge == 0)
            {
                return null;
            }
            double ld_y = a_entity.Vertices[0].Y;
            if ((ld_y != a_entity.Vertices[1].Y) || (ld_y != a_entity.Vertices[1].Y) || (ld_y != a_entity.Vertices[1].Y))
            {
                return null;
            }

            int li_firstX = MyShiftScaleX(a_entity.Vertices[0].X);
            int li_thirdX = MyShiftScaleX(a_entity.Vertices[2].X);

            int li_left = Math.Min(li_firstX, li_thirdX);
            int li_right = Math.Max(li_firstX, li_thirdX);


            int li_width = li_right - li_left;
            int li_height = li_width;

            System.Diagnostics.Debug.Assert(li_width > 0);
            System.Diagnostics.Debug.Assert(li_height > 0);


            int li_my_Y = MyShiftScaleY(ld_y);
            int li_top = li_my_Y - (li_width / 2);

            Rectangle l_boundingRect = new Rectangle(li_left, li_top, li_width, li_width);
            DrawRect l_circle = new DrawRect(Shape.ellipse, l_boundingRect);
            return l_circle;

        }

        //covert angle in radians to "size" = distance of the handle from the center
        private static Size Angle2Size(double a_angle)
        {

            double ld_x =  10000 * Math.Cos(a_angle);
            double ld_y = -10000 * Math.Sin(a_angle);

            int li_x = (int)Math.Round(ld_x);
            int li_y = (int)Math.Round(ld_y);
            
            Size l_size = new Size(li_x, li_y);
            return l_size;
        }

        public static void SetScale(double ai_scale)
        {

            m_scaleX = ai_scale;
            m_scaleY = -m_scaleX;
        }

        public static void SetShift(int ai_shift_x, int ai_shift_y, int ai_shift_target_y )
        {
            m_shift_x = ai_shift_x;
            m_shift_y = ai_shift_y;
            m_shift_target_y = ai_shift_target_y;
        }

        private static int RadiansToAngle(double ad_radians)
        {
            return (int)(-1800 * ad_radians / Math.PI);
        }

    }
}
