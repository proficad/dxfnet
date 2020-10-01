using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;



using WW.Cad.Model.Entities;

using DxfNet;
using WW.Cad.Model.Tables;
using System.Globalization;

namespace Dxf2ProfiCAD
{
    public static class Converter
    {
        static double m_scaleX = 1;
        static double m_scaleY = -m_scaleX;

        static int m_shift_x;
        static int m_shift_y;
        static int m_shift_target_y;

        public static int MyShiftScaleX(double ai_what)
        {
            return (int)Math.Round((ai_what + m_shift_x) * m_scaleX);
        }
        public static int MyShiftScaleY(double ai_what)
        {
            return m_shift_target_y + (int)Math.Round((ai_what + m_shift_y) * m_scaleY);
        }

        public static int MyScaleX(double ai_what)
        {
            return (int)Math.Round(ai_what * m_scaleX);
        }



        public static DrawObj Convert(DxfEntity a_entity)
        {
            //DrawObj l_obj;
            //System.Diagnostics.Debug.WriteLine("converting " + a_entity.ToString());
            
            switch (a_entity)
            {
                case DxfInsert l_insert:
                    return ConvertInsert(l_insert);
                case DxfMText l_dxf_mtext:
                    return ConvertDxfMText(l_dxf_mtext);
                case DxfText l_dxf_text:
                    return ConvertDxfText(l_dxf_text);
                case DxfArc  l_arc:
                    return ConvertArc(l_arc);
                case DxfCircle l_circle:
                    return ConvertCircle(l_circle);
                case DxfHatch l_hatch:
                    return ConvertDxfHatch(l_hatch);
                case DxfLine l_dxf_line:
                    return ConvertDxfLine(l_dxf_line);
                case DxfLwPolyline l_dxf_lw_line:
                    return ConvertDxfLwPolyline(l_dxf_lw_line);
                case DxfPolyline2D l_dxf_line_2D:
                    return ConvertDxfPolyline2D(l_dxf_line_2D);
                case DxfDimension.Linear l_dim_linear:
                    return ConvertDxfDimension_Linear(l_dim_linear);
                case DxfDimension.Aligned l_dim_align:
                    return ConvertDxfDimension_Aligned(l_dim_align);
                default:
                    System.Console.WriteLine("did not convert type {0}", a_entity.ToString());
                    return null;
            }
        }

        private static Insert ConvertInsert(DxfInsert a_dxfInsert)
        {

            /*
            if (l_dxfInsert.Block.Name == "A24")
            {
                int o = 5;
            }
            */

    
            bool lb_flipX = a_dxfInsert.ScaleFactor.X == -1d;
            bool lb_flipY = a_dxfInsert.ScaleFactor.Y == -1d;

            if (lb_flipX)
            {
                //Console.WriteLine("flip X");
            }
            if (lb_flipY)
            {
                //Console.WriteLine("flip Y");
            }

            string ls_blockName = a_dxfInsert.Block.Name;

            const float fl = (float)1.1; //pouze pro zkompilovani
            Insert l_insert = new Insert(Shape.soucastka, MyShiftScaleX(a_dxfInsert.InsertionPoint.X), MyShiftScaleY(a_dxfInsert.InsertionPoint.Y), fl, fl);//99
            l_insert.m_angle = RadiansToAngle(a_dxfInsert.Rotation);
            l_insert.m_scaleX = (float)a_dxfInsert.ScaleFactor.X;
            l_insert.m_scaleY = (float)a_dxfInsert.ScaleFactor.Y;


            Console.WriteLine(ls_blockName);

            //instead of GUID it will be just a name of the block
            l_insert.m_lG = ls_blockName;
            l_insert.m_hor = lb_flipY;
            l_insert.m_ver = lb_flipX;


            foreach (DxfAttribute l_attr in a_dxfInsert.Attributes)
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

        private static DrawObj ConvertDxfHatch(DxfHatch a_hatch)
        {
            if (a_hatch.Pattern == null)
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

        private static DrawObj ConvertDxfPolyline2D(DxfPolyline2D a_dxfLine)
        {
            DrawRect l_circle = TryMakeCircle(a_dxfLine);
            if (l_circle != null)
            {
                return l_circle;
            }

            DrawPoly l_drawPoly = new DrawPoly(Shape.polyline);

            foreach (DxfVertex2D l_vertex in a_dxfLine.Vertices)
            {
                l_drawPoly.AddPoint(MyShiftScaleX(l_vertex.X), MyShiftScaleY(l_vertex.Y));
            }



            if (a_dxfLine.Closed)
            {
                DxfVertex2D l_vertex = a_dxfLine.Vertices[0];
                l_drawPoly.AddPoint(MyShiftScaleX(l_vertex.X), MyShiftScaleY(l_vertex.Y));
            }


            l_drawPoly.RecalcPosition();
            l_drawPoly.m_objProps.m_logpen.m_color = Helper.DxfEntityColor2Color(a_dxfLine);
            l_drawPoly.m_objProps.m_lin = Helper.DxfLineType_2_QLin(a_dxfLine.LineType, m_scaleX, a_dxfLine.LineTypeScale);

            return l_drawPoly;

        }

        private static DrawObj ConvertDxfLwPolyline(DxfLwPolyline a_lw_polyline)
        {
            DrawPoly l_drawPoly = new DrawPoly(Shape.polyline);

            foreach (DxfLwPolyline.Vertex l_vertex in a_lw_polyline.Vertices)
            {
                l_drawPoly.AddPoint(MyShiftScaleX(l_vertex.X), MyShiftScaleY(l_vertex.Y));
            }


            if (a_lw_polyline.Closed)
            {
                DxfLwPolyline.Vertex l_vertex = a_lw_polyline.Vertices[0];
                l_drawPoly.AddPoint(MyShiftScaleX(l_vertex.X), MyShiftScaleY(l_vertex.Y));
            }


            l_drawPoly.RecalcPosition();

            l_drawPoly.m_objProps.m_logpen.m_color = Helper.DxfEntityColor2Color(a_lw_polyline);
            l_drawPoly.m_objProps.m_lin = Helper.DxfLineType_2_QLin(a_lw_polyline.LineType, m_scaleX, a_lw_polyline.LineTypeScale);


            return l_drawPoly;
        }

        private static DrawObj ConvertDxfLine(DxfLine a_line)
        {
            //Console.WriteLine("line type: " + a_line.LineType.Name);

            //if(l_line.LineType.Name == DxfLineType.LineTypeNameByLayer)

            DrawPoly l_drawPoly = new DrawPoly(Shape.polyline);
            l_drawPoly.AddPoint(MyShiftScaleX(a_line.Start.X), MyShiftScaleY(a_line.Start.Y));
            l_drawPoly.AddPoint(MyShiftScaleX(a_line.End.X), MyShiftScaleY(a_line.End.Y));
            l_drawPoly.RecalcPosition();

            l_drawPoly.m_objProps.m_logpen.m_color = Helper.DxfEntityColor2Color(a_line);
     
            l_drawPoly.m_objProps.m_lin = Helper.DxfLineType_2_QLin(a_line.LineType, m_scaleX, a_line.LineTypeScale);

            return l_drawPoly;
        }

        private static DrawObj ConvertDxfDimension_Linear(DxfDimension.Linear a_dxf_dim)
        {
            QDimLine.DimDirection l_dir = Helper.IsSame(Math.PI / 2, a_dxf_dim.Rotation)
                ?
                QDimLine.DimDirection.dimdir_ver
                :
                QDimLine.DimDirection.dimdir_hor
                ;

            QDimLine l_dim = new QDimLine(
                Point3D_To_Point(a_dxf_dim.ExtensionLine1StartPoint),
                Point3D_To_Point(a_dxf_dim.ExtensionLine2StartPoint),
                Point3D_To_Point(a_dxf_dim.DimensionLineLocation),
                l_dir
            );

            l_dim.Label.Text = a_dxf_dim.Text ?? a_dxf_dim.Measurement.ToString(CultureInfo.InvariantCulture);
            // we might call GetActualMeasurement() but it will probably better to calculate it in ProfiCAD 

            l_dim.Label.Center = Point3D_To_Point(a_dxf_dim.TextMiddlePoint);

//                l_dim.m_objProps.m_logpen.m_color = Helper.DxfEntityColor2Color(l_dxf_dim);

            return l_dim;
      
         
        }
        private static DrawObj ConvertDxfDimension_Aligned(DxfDimension.Aligned a_dxf_dim)
        {
            QDimLine l_dim = new QDimLine(
                Point3D_To_Point(a_dxf_dim.ExtensionLine1StartPoint),
                Point3D_To_Point(a_dxf_dim.ExtensionLine2StartPoint),
                Point3D_To_Point(a_dxf_dim.DimensionLineLocation), 
                QDimLine.DimDirection.dimdir_aligned
            );

            l_dim.Label.Text = a_dxf_dim.Text ?? a_dxf_dim.Measurement.ToString(CultureInfo.InvariantCulture);
            // we might call GetActualMeasurement() but it will probably better to calculate it in ProfiCAD 

            return l_dim;
        }

        private static DrawObj ConvertCircle(DxfCircle a_dxfCircle)
        {
            int li_radius = Math.Abs(MyScaleX(a_dxfCircle.Radius));
            int li_center_x = MyShiftScaleX(a_dxfCircle.Center.X);
            int li_center_y = MyShiftScaleY(a_dxfCircle.Center.Y);


            Point l_center = new Point(li_center_x, li_center_y);
            QCircle l_circle = new QCircle(l_center, li_radius);

            l_circle.m_objProps.m_logpen.m_color = Helper.DxfEntityColor2Color(a_dxfCircle);


            /*
             * old way (before QCircle existed)
            Rectangle l_boundingRect = new Rectangle(li_left, li_top, li_width, li_width);
            DrawRect l_circle = new DrawRect(Shape.ellipse, l_boundingRect);
            */
            return l_circle;
        }

        private static DrawObj ConvertArc(DxfArc a_dxf_arc)
        {

            int li_radius = Math.Abs(MyScaleX(a_dxf_arc.Radius));
            int li_center_x = MyShiftScaleX(a_dxf_arc.Center.X);
            int li_center_y = MyShiftScaleY(a_dxf_arc.Center.Y);

            int li_left = li_center_x - li_radius;
            int li_top = li_center_y - li_radius;
            int li_width = 2 * li_radius;

            Rectangle l_boundingRect = new Rectangle(li_left, li_top, li_width, li_width);

            DrawRect l_arc = new DrawRect(Shape.arc, l_boundingRect);

            l_arc.m_arcBegin = Angle2Size(a_dxf_arc.StartAngle);
            l_arc.m_arcEnd = Angle2Size(a_dxf_arc.EndAngle);

            l_arc.m_objProps.m_logpen.m_color = Helper.DxfEntityColor2Color(a_dxf_arc);


            return l_arc;
        }

        private static FreeText ConvertDxfText(DxfText a_dxfMText)
        {
            if (a_dxfMText.Text.Trim().Length == 0)
            {
                //System.Console.WriteLine("skipping empty text");
                return null;
            }



            EFont l_efont = new EFont();
            l_efont.m_size = (int)(l_efont.m_size * a_dxfMText.Height * 0.06);
            l_efont.m_size = MyScaleX(l_efont.m_size);


            

            double ld_x = a_dxfMText.AlignmentPoint1.X;
            double ld_y = a_dxfMText.AlignmentPoint1.Y;
            if(a_dxfMText.HorizontalAlignment == TextHorizontalAlignment.Center)
            {
                if (a_dxfMText.AlignmentPoint2.HasValue)
                {
                    ld_x = a_dxfMText.AlignmentPoint2.Value.X;
                    ld_y = a_dxfMText.AlignmentPoint2.Value.Y;
                }
            }
            else if(a_dxfMText.HorizontalAlignment == TextHorizontalAlignment.Left)
            {
                if(a_dxfMText.VerticalAlignment == TextVerticalAlignment.Baseline)
                {
                    //rotate to compensate for the fact the ProfiCAD rotates around middle point of the left edge
                    double ld_half_height = (a_dxfMText.Height / 2);
                    double ld_fi = a_dxfMText.Rotation - (Math.PI / 2);
                    double ld_shift_x = Math.Cos(ld_fi) * ld_half_height;
                    double ld_shift_y = Math.Sin(ld_fi) * ld_half_height;

                    ld_x -= ld_shift_x;
                    ld_y -= ld_shift_y;

                    //double ld_shift = m_scaleY * (l_dxfMText.Height / 2);
                    //ld_y += (l_dxfMText.Height / 2);//we should move it up, but the DXF coordinate system is positive up
                }

            }

            
            
            Rectangle l_rect = new Rectangle(MyShiftScaleX(ld_x), MyShiftScaleY(ld_y), 0, 0);

            FreeText l_text = new FreeText(a_dxfMText.Text, l_efont, l_rect, 0);

            l_text.m_alignment = ConvertAlignment(a_dxfMText);

            l_text.m_efont.m_color = Helper.DxfEntityColor2Color(a_dxfMText);

            l_text.m_angle = RadiansToAnglePositive(a_dxfMText.Rotation);

            return l_text;
        }

        private static FreeText ConvertDxfMText(DxfMText a_dxfMText)
        {
            EFont l_efont = new EFont();
            l_efont.m_size = (int)(l_efont.m_size * a_dxfMText.Height * 0.06);
            l_efont.m_size = MyScaleX(l_efont.m_size);

            Rectangle l_rect = new Rectangle(MyShiftScaleX(a_dxfMText.InsertionPoint.X), MyShiftScaleY(a_dxfMText.InsertionPoint.Y), 0, 0);

            string ls_text = a_dxfMText.SimplifiedText;

            double l_angle;
            double l_axis_x = a_dxfMText.XAxis.X;
            double l_axis_y = a_dxfMText.XAxis.Y;

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
        
        private static int RadiansToAnglePositive(double ad_radians)
        {
            return (int)(1800 * ad_radians / Math.PI);
        }


        private static System.Drawing.Point Point3D_To_Point(WW.Math.Point3D a_point)
        {
            int li_x = (int)Math.Round(a_point.X);
            int li_y = (int)Math.Round(a_point.Y);
            return new System.Drawing.Point(
                MyShiftScaleX(li_x),
                MyShiftScaleY(li_y)
                );
        }
    }
}
