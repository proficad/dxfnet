using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;


using WW.Math;
using WW.Cad.Model.Entities;
using WW.Cad.Drawing;

using DxfNet;
using WW.Cad.Model.Tables;
using System.Globalization;
using System.Runtime.CompilerServices;
using WW.Cad.Model;
using static WW.Cad.Model.Entities.DxfDimension;
using static WW.Cad.Model.DxfValueFormat;
using static WW.Cad.Model.Objects.Annotations.DxfDimensionObjectContextData;

namespace Dxf2ProfiCAD
{
    public static class Converter
    {
        private static double m_scaleX = 1;
        private static double m_scaleY = -m_scaleX;

        static int m_shift_target_y;

        public static Stack<int> m_shifts_x = new Stack<int>();
        public static Stack<int> m_shifts_y = new Stack<int>();

        public static Size m_size_target;

        public static double MyShiftScaleX(double ai_what)
        {
            return (ai_what + m_shifts_x.Peek()) * m_scaleX;
        }
        public static double MyShiftScaleY(double ai_what)
        {
            double ld_result = m_shift_target_y + (ai_what + m_shifts_y.Peek()) * m_scaleY;
            return ld_result;
        }

        private static int MyScaleX(double ai_what)
        {
            return (int)Math.Round(ai_what * m_scaleX);
        }



        public static DrawObj Convert(DxfEntity a_entity)
        {
            DrawObj l_obj;
            //System.Diagnostics.Debug.WriteLine("converting " + a_entity.ToString());
            
            switch (a_entity)
            {
                case DxfInsert l_insert:
                    l_obj = ConvertInsert(l_insert);
                    break;
                case DxfMText l_dxf_mtext:
                    l_obj = ConvertDxfMText(l_dxf_mtext);
                    break;
                case DxfText l_dxf_text:
                    l_obj =  ConvertDxfText(l_dxf_text);
                    break;
                case DxfArc  l_arc:
                    l_obj =  ConvertArc(l_arc);
                    break;
                case DxfCircle l_circle:
                    l_obj =  ConvertCircle(l_circle);
                    break;
                case DxfEllipse l_ellipse:
                    l_obj =  ConvertDxfEllipse(l_ellipse);
                    break;
                case DxfSolid l_solid:
                    l_obj =  ConvertDxfSolid(l_solid);
                    break;
                case DxfHatch l_hatch:
                    l_obj =  ConvertDxfHatch(l_hatch);
                    break;
                case DxfLine l_dxf_line:
                    l_obj =  ConvertDxfLine(l_dxf_line);
                    break;
                case DxfLwPolyline l_dxf_lw_line:
                    l_obj =  ConvertDxfLwPolyline(l_dxf_lw_line);
                    break;
                case DxfPolyline2D l_dxf_line_2D:
                    l_obj =  ConvertDxfPolyline2D(l_dxf_line_2D);
                    break;
                case DxfPolyline2DSpline l_dxf_line_2D_spline:
                    l_obj =  ConvertDxfPolyline2D_Spline(l_dxf_line_2D_spline);
                    break;
                case DxfSpline l_dxf_spline:
                    l_obj =  ConvertDxfSpline(l_dxf_spline);
                    break;
                case DxfDimension.Linear l_dim_linear:
                    l_obj =  ConvertDxfDimension_Linear(l_dim_linear);
                    break;
                case DxfDimension.Aligned l_dim_align:
                    l_obj =  ConvertDxfDimension_Aligned(l_dim_align);
                    break;
                default:
                    System.Console.WriteLine("did not convert type {0}", a_entity.ToString());
                    return null;
            }

            if (l_obj != null && l_obj.IsValid(Converter.m_size_target.Width, Converter.m_size_target.Height))
            {
                return l_obj;
            }

            return null;
        }



        private static DrawObj ConvertDxfSolid(DxfSolid a_solid)
        {
            if (a_solid.Points.Count != 4)
            {
                return null;
            }

            DrawPoly l_drawPoly = new DrawPoly(Shape.poly);

            l_drawPoly.AddPoint(MyShiftScaleX(a_solid.Points[0].X), MyShiftScaleY(a_solid.Points[0].Y));
            l_drawPoly.AddPoint(MyShiftScaleX(a_solid.Points[1].X), MyShiftScaleY(a_solid.Points[1].Y));

            l_drawPoly.AddPoint(MyShiftScaleX(a_solid.Points[3].X), MyShiftScaleY(a_solid.Points[3].Y));

            //yes, it is correct, point 3 comes before point 2, because of autocad rules
            if (a_solid.Points[2] != a_solid.Points[3])
            {
                l_drawPoly.AddPoint(MyShiftScaleX(a_solid.Points[2].X), MyShiftScaleY(a_solid.Points[2].Y));
            }

            l_drawPoly.RecalcPosition();

            //no border
            //l_drawPoly.m_objProps.m_bPen = false;
            l_drawPoly.m_objProps.m_logpen.m_width = 0;


            l_drawPoly.m_objProps.m_bBrush = true;
            l_drawPoly.m_objProps.m_logbrush.m_color = Helper.DxfEntityColor2Color(a_solid);

            //workaround for now: ProfiCAD cannot draw polygon without border,
            //so let us make the line same color as the filling so it will be invisible
            l_drawPoly.m_objProps.m_logpen.m_color = l_drawPoly.m_objProps.m_logbrush.m_color;

            return l_drawPoly;

        }


        private static DrawObj ConvertDxfEllipse(DxfEllipse a_dxf_ellipse)
        {
            if ((a_dxf_ellipse.MajorAxisEndPoint.X != 0) && (a_dxf_ellipse.MajorAxisEndPoint.Y != 0))
            {
                Console.WriteLine("did not convert rotated ellipse " + a_dxf_ellipse.Center.ToString());
                return null;
            }

            int li_radius_x = 0, li_radius_y = 0;

            if (a_dxf_ellipse.MajorAxisEndPoint.X != 0 && a_dxf_ellipse.MinorAxisEndPoint.Y != 0)
            {
                li_radius_x = MyScaleX(a_dxf_ellipse.MajorAxisEndPoint.X);
                li_radius_y = MyScaleX(a_dxf_ellipse.MinorAxisEndPoint.Y);
            }
            else if (a_dxf_ellipse.MajorAxisEndPoint.Y != 0 && a_dxf_ellipse.MinorAxisEndPoint.X != 0)
            {
                li_radius_x = MyScaleX(a_dxf_ellipse.MinorAxisEndPoint.X);
                li_radius_y = MyScaleX(a_dxf_ellipse.MajorAxisEndPoint.Y);
            }

            if ((li_radius_x == 0) || (li_radius_y == 0))
            {
                Console.WriteLine("did not convert flat ellipse " + a_dxf_ellipse.Center.ToString());
                return null;
            }

            li_radius_x = Math.Abs(li_radius_x);
            li_radius_y = Math.Abs(li_radius_y);

            PointF l_center_point = new PointF(
                (float)MyShiftScaleX(a_dxf_ellipse.Center.X),
                (float)MyShiftScaleY(a_dxf_ellipse.Center.Y)
            );

            RectangleF l_boundingRect = new RectangleF(
                l_center_point.X - li_radius_x, 
                l_center_point.Y - li_radius_y,
                li_radius_x + li_radius_x, 
                li_radius_y  + li_radius_y
            );


            DrawRect l_ellipse = new DrawRect(Shape.ellipse, l_boundingRect);


            l_ellipse.m_objProps.m_logpen.m_color = Helper.DxfEntityColor2Color(a_dxf_ellipse);
            l_ellipse.m_objProps.m_lin = Helper.DxfLineType_2_QLin(a_dxf_ellipse.LineType, m_scaleX, a_dxf_ellipse.LineTypeScale);


            return l_ellipse;

        }


        private static DrawObj ConvertDxfSpline(DxfSpline a_lDxfSpline)
        {
            int li_points_count = a_lDxfSpline.ControlPoints.Count();
            if (li_points_count > 50)
            {
                return null;
            }

            //Console.WriteLine($"spline has {} points");
            

            var iii = new GraphicsConfig();
            //iii.NoOfSplineLineSegments = 20;
            //iii.ShapeFlattenEpsilon = -0.1;

            CoordinatesCollector l_coordinatesCollector = new CoordinatesCollector();
            DrawContext.Wireframe drawContext =
                new DrawContext.Wireframe.ModelSpace(
                    a_lDxfSpline.Model,
                    iii,
                    Matrix4D.Identity
                );


            a_lDxfSpline.Draw(drawContext, l_coordinatesCollector);

            if (l_coordinatesCollector.m_list.Count == 0)
            {
                return null;
            }


            DrawPoly l_drawPoly = new DrawPoly(Shape.polyline);

            /*
            int li_spline_points_count = coordinatesCollector.m_list.Count();
            int li_max_points = 180;
            int li_step = li_spline_points_count / li_max_points;
            if (li_step == 0)
            {
                li_step = 1;
            }
            int li_i = 0;
            while(li_i < (li_spline_points_count - 1))
            {
                Point3D l_point = coordinatesCollector.m_list[li_i];
                l_drawPoly.AddPoint(
                    MyShiftScaleX(l_point.X),
                    MyShiftScaleY(l_point.Y)
                );

                li_i += li_step;
            }
            */


            foreach (WW.Math.Point3D l_point in l_coordinatesCollector.m_list)
            {
                l_drawPoly.AddPoint(
                    MyShiftScaleX(l_point.X),
                    MyShiftScaleY(l_point.Y)
                );
            }

            l_drawPoly.m_objProps.m_logpen.m_color = Helper.DxfEntityColor2Color(a_lDxfSpline);
            l_drawPoly.m_objProps.m_lin = Helper.DxfLineType_2_QLin(a_lDxfSpline.LineType, m_scaleX, a_lDxfSpline.LineTypeScale);


            return l_drawPoly;
        }


        private static Insert ConvertInsert(DxfInsert a_dxfInsert)
        {
            if (a_dxfInsert?.Block == null)
            {
                return null;
            }

            string ls_blockName = a_dxfInsert.Block.Name;

            if (!a_dxfInsert.Model.Blocks.Contains(ls_blockName))
            {
                //this insert does not have a block, we cannot use it
                return null;
            }

        
    
            bool lb_flipX = a_dxfInsert.ScaleFactor.X < 0d;
            bool lb_flipY = a_dxfInsert.ScaleFactor.Y < 0d;


            const float fl = (float)1.1; //pouze pro zkompilovani
            Insert l_insert = new Insert(
                Shape.soucastka, 
                MyShiftScaleX(a_dxfInsert.InsertionPoint.X), 
                MyShiftScaleY(a_dxfInsert.InsertionPoint.Y), 
                fl, 
                fl);//99

            l_insert.m_angle = RadiansToAnglePositive(a_dxfInsert.Rotation);
            l_insert.m_scaleX = Math.Abs((float)a_dxfInsert.ScaleFactor.X);
            l_insert.m_scaleY = Math.Abs((float)a_dxfInsert.ScaleFactor.Y);


            //Console.WriteLine(ls_blockName);

            //instead of GUID it will be just a name of the block
            l_insert.m_lG = ls_blockName;
            l_insert.m_hor = lb_flipY;
            l_insert.m_ver = lb_flipX;


            foreach (DxfAttribute l_attr in a_dxfInsert.Attributes)
            {
                string ls_text = l_attr.Text;
                string ls_tagString = l_attr.TagString;
                double li_x = MyShiftScaleX(l_attr.AlignmentPoint1.X);
                double li_y = MyShiftScaleY(l_attr.AlignmentPoint1.Y);
                
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
                Insert.Satelite l_sat = new Insert.Satelite(ls_tagString, ls_text, (int)li_x, (int)li_y, true, li_turns_attr);
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

            if (a_dxfLine.DefaultStartWidth > 0)
            {
                if (a_dxfLine.DefaultStartWidth == a_dxfLine.DefaultEndWidth)
                {
                    l_drawPoly.m_objProps.m_logpen.m_width = MyScaleX(a_dxfLine.DefaultStartWidth);
                }
            }


            return l_drawPoly;

        }


        private static DrawObj ConvertDxfPolyline2D_Spline(DxfPolyline2DSpline a_dxfLine2DSpline)
        {
            if (a_dxfLine2DSpline.SplineType != SplineType.CubicBSpline)
            {
                return null;
            }

            DrawPoly l_drawPoly = new DrawPoly(Shape.bezier);

            foreach (DxfVertex2D l_vertex in a_dxfLine2DSpline.ControlPoints)
            {
                l_drawPoly.AddPoint(MyShiftScaleX(l_vertex.X), MyShiftScaleY(l_vertex.Y));
            }

            l_drawPoly.m_objProps.m_logpen.m_color = Helper.DxfEntityColor2Color(a_dxfLine2DSpline);
            l_drawPoly.m_objProps.m_lin = Helper.DxfLineType_2_QLin(a_dxfLine2DSpline.LineType, m_scaleX, a_dxfLine2DSpline.LineTypeScale);

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
            double ld_center_x = MyShiftScaleX(a_dxfCircle.Center.X);
            double ld_center_y = MyShiftScaleY(a_dxfCircle.Center.Y);


            PointF l_center = new PointF((float)ld_center_x, (float)ld_center_y);
            QCircle l_circle = new QCircle(l_center, li_radius);

            l_circle.m_objProps.m_logpen.m_color = Helper.DxfEntityColor2Color(a_dxfCircle);
            l_circle.m_objProps.m_lin = Helper.DxfLineType_2_QLin(a_dxfCircle.LineType, m_scaleX, a_dxfCircle.LineTypeScale);

            return l_circle;
        }

        private static DrawObj ConvertArc_To_Lines(DxfArc a_dxf_arc)
        {
            


            var graphics_config = new GraphicsConfig();
//            graphics_config.NoOfSplineLineSegments = 3;
//            graphics_config.NoOfArcLineSegments = 3;
//            graphics_config.ShapeFlattenEpsilon = -0.1;

            CoordinatesCollector l_coordinatesCollector = new CoordinatesCollector();
            DrawContext.Wireframe drawContext =
                new DrawContext.Wireframe.ModelSpace(
                    a_dxf_arc.Model,
                    graphics_config,
                    Matrix4D.Identity
                );


            a_dxf_arc.Draw(drawContext, l_coordinatesCollector);

            if (l_coordinatesCollector.m_list.Count == 0)
            {
                return null;
            }

            DrawPoly l_drawPoly = new DrawPoly(Shape.polyline);

//            Console.WriteLine($"seznam má {CoordinatesCollector.m_list.Count()} bodů");


            foreach (WW.Math.Point3D l_point in l_coordinatesCollector.m_list)
            {
                //                Console.WriteLine("   {0}", l_point.ToString());
                l_drawPoly.AddPoint(
                    MyShiftScaleX(l_point.X),
                    MyShiftScaleY(l_point.Y)
                );
            }


            l_drawPoly.m_objProps.m_logpen.m_color = Helper.DxfEntityColor2Color(a_dxf_arc);
            l_drawPoly.m_objProps.m_lin = Helper.DxfLineType_2_QLin(a_dxf_arc.LineType, m_scaleX, a_dxf_arc.LineTypeScale);

            return l_drawPoly;
        }


        private static DrawObj ConvertArc(DxfArc a_dxf_arc)
        {
            if (Program.m_repo_level == 0)
            {
                return Convert_Arc_3P(a_dxf_arc);
                //return ConvertArc_Native(a_dxf_arc);
            }
            else
            {
                return ConvertArc_To_Lines(a_dxf_arc);
            }
        }


        private static DrawObj Convert_Arc_3P(DxfArc a_dxf_arc)
        {
         
            SizeF l_size_begin   = Angle2Size(a_dxf_arc.StartAngle, a_dxf_arc.Radius);
            SizeF l_size_mid     = Angle2Size((a_dxf_arc.StartAngle + a_dxf_arc.EndAngle) / 2, a_dxf_arc.Radius);
            SizeF l_size_end     = Angle2Size(a_dxf_arc.EndAngle, a_dxf_arc.Radius);

          

            double ld_begin_x   = a_dxf_arc.Center.X + l_size_begin.Width;
            double ld_begin_y   = a_dxf_arc.Center.Y + l_size_begin.Height;
            PointF l_point_begin = new PointF(
                (float) MyShiftScaleX(ld_begin_x),
                (float) MyShiftScaleY(ld_begin_y)
            );

            double ld_mid_x = a_dxf_arc.Center.X + l_size_mid.Width;
            double ld_mid_y = a_dxf_arc.Center.Y + l_size_mid.Height;
            PointF l_point_mid = new PointF(
                (float)MyShiftScaleX(ld_mid_x),
                (float)MyShiftScaleY(ld_mid_y)
            );

            double ld_end_x     = a_dxf_arc.Center.X + l_size_end.Width;
            double ld_end_y     = a_dxf_arc.Center.Y + l_size_end.Height;
            PointF l_point_end  = new PointF(
                (float)MyShiftScaleX(ld_end_x),
                (float)MyShiftScaleY(ld_end_y)
            );



            QArc_3_P l_arc = new QArc_3_P(l_point_begin, l_point_mid, l_point_end);
            l_arc.m_objProps.m_logpen.m_color = Helper.DxfEntityColor2Color(a_dxf_arc);

            return l_arc;
        }


        private static DrawObj ConvertArc_Native(DxfArc a_dxf_arc)
        {

            int li_radius = Math.Abs(MyScaleX(a_dxf_arc.Radius));
            double li_center_x = MyShiftScaleX(a_dxf_arc.Center.X);
            double li_center_y = MyShiftScaleY(a_dxf_arc.Center.Y);

            double li_left = li_center_x - li_radius;
            double li_top = li_center_y - li_radius;
            int li_width = 2 * li_radius;

            RectangleF l_boundingRect = new RectangleF((float)li_left, (float)li_top, li_width, li_width);

            DrawRect l_arc = new DrawRect(Shape.arc_rect, l_boundingRect);

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
            l_efont.m_size = MyScaleX(l_efont.m_size * a_dxfMText.Height * 0.06);
            if (l_efont.m_size == 0)
            {
                l_efont.m_size = 30;
            }



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

            
            
            RectangleF l_rect = new RectangleF(
                (float)MyShiftScaleX(ld_x),
                (float)MyShiftScaleY(ld_y), 
                0, 
                0
            );

            string ls_text = a_dxfMText.Text.Replace("\t", "");

            FreeText l_text = new FreeText(ls_text, l_efont, l_rect, 0);

            l_text.m_alignment = ConvertAlignment(a_dxfMText);

            l_text.m_efont.m_color = Helper.DxfEntityColor2Color(a_dxfMText);

            l_text.m_angle = RadiansToAnglePositive(a_dxfMText.Rotation);

            return l_text;
        }

        private static FreeText ConvertDxfMText(DxfMText a_dxfMText)
        {
            EFont l_efont = new EFont();
            l_efont.m_size = MyScaleX(l_efont.m_size * a_dxfMText.Height * 0.06);

            if (l_efont.m_size == 0)
            {
                l_efont.m_size = 30;
            }


            RectangleF l_rect = new RectangleF(
                (float)MyShiftScaleX(a_dxfMText.InsertionPoint.X),
                (float)MyShiftScaleY(a_dxfMText.InsertionPoint.Y), 
                0, 
                0);

            string ls_text = a_dxfMText.SimplifiedText;
            ls_text = ls_text.Replace("\t", "");

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
            l_text.m_alignment = ConvertAlignment(a_dxfMText);


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

        private static QTextAlignment ConvertAlignment(DxfText l_dxf_text)
        {
            if (l_dxf_text.HorizontalAlignment == TextHorizontalAlignment.Left)
            {
                return QTextAlignment.AL_LM;
            }
            if (l_dxf_text.HorizontalAlignment == TextHorizontalAlignment.Right)
            {
                return QTextAlignment.AL_RM;
            }

            return QTextAlignment.AL_MM;
        }

        private static QTextAlignment ConvertAlignment(DxfMText l_dxf_MText)
        {
            if ((l_dxf_MText.AttachmentPoint == AttachmentPoint.MiddleLeft) 
                ||
                (l_dxf_MText.AttachmentPoint == AttachmentPoint.TopLeft)
                )
            {
                return QTextAlignment.AL_LM;
            }
            if (l_dxf_MText.AttachmentPoint == AttachmentPoint.MiddleRight)
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

            double li_firstX = MyShiftScaleX(a_entity.Vertices[0].X);
            double li_thirdX = MyShiftScaleX(a_entity.Vertices[2].X);

            double li_left     = Math.Min(li_firstX, li_thirdX);
            double li_right    = Math.Max(li_firstX, li_thirdX);


            double li_width = li_right - li_left;
            double li_height = li_width;

            System.Diagnostics.Debug.Assert(li_width > 0);
            System.Diagnostics.Debug.Assert(li_height > 0);


            double li_my_Y = MyShiftScaleY(ld_y);
            double li_top = li_my_Y - (li_width / 2);

            RectangleF l_boundingRect = new RectangleF((float)li_left, (float)li_top, (float)li_width, (float)li_width);
            DrawRect l_circle = new DrawRect(Shape.ellipse, l_boundingRect);
            return l_circle;

        }

        //covert angle in radians to "size" = distance of the handle from the center
        private static Size Angle2Size(double a_angle)
        {

            double ld_x =  1000 * Math.Cos(a_angle);
            double ld_y = -1000 * Math.Sin(a_angle);

            int li_x = (int)Math.Round(ld_x);
            int li_y = (int)Math.Round(ld_y);
            
            Size l_size = new Size(li_x, li_y);
            return l_size;
        }

        private static SizeF Angle2Size(double a_angle, double ad_radius)
        {

            double ld_x = ad_radius * Math.Cos(a_angle);
            double ld_y = ad_radius * Math.Sin(a_angle);


            SizeF l_size = new SizeF((float)ld_x, (float)ld_y);
            return l_size;
        }

        public static void SetScale(double ai_scale)
        {

            m_scaleX = ai_scale;
            m_scaleY = -m_scaleX;

            
        }

        public static void SetShift(int ai_shift_x, int ai_shift_y, int ai_shift_target_y )
        {
            m_shifts_x.Push(ai_shift_x);
            m_shifts_y.Push(ai_shift_y);
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


        private static System.Drawing.PointF Point3D_To_Point(WW.Math.Point3D a_point)
        {
            int li_x = (int)Math.Round(a_point.X);
            int li_y = (int)Math.Round(a_point.Y);
            return new System.Drawing.PointF(
                (float)MyShiftScaleX(li_x),
                (float)MyShiftScaleY(li_y)
                );
        }

        public static void SetSize(Size a_size_target)
        {
            m_size_target = a_size_target;
        }

    }
}
