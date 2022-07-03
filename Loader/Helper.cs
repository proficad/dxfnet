using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;


using WW.Math;
using WW.Cad.IO;
using WW.Cad.Model;
using WW.Cad.Model.Entities;
using WW.Cad.Model.Tables;

using DxfNet;

namespace Loader
{
    internal static class Helper
    {
        public static Point RotatePoint(Point a_axis, int ai_angle, Point a_pointToRotate)
        {
            double li_angle = - Math.PI * ai_angle;
            li_angle /= 180.0;


            double fi_vstupu = Math.Atan2((double)(a_pointToRotate.Y - a_axis.Y), (double)(a_pointToRotate.X - a_axis.X));
            double modul_vstupu = MyHypot(a_pointToRotate.X - a_axis.X, a_pointToRotate.Y - a_axis.Y);
            //

            int li_x = a_axis.X + (int)(0.5 + (modul_vstupu * Math.Cos(fi_vstupu + li_angle)));
            int li_y = a_axis.Y + (int)(0.5 + (modul_vstupu * Math.Sin(fi_vstupu + li_angle)));
            
            return new Point(li_x, li_y);
        }

        public static PointF PrevodBodu(PointF origin, PointF vektor, double ad_vstup_x, double ad_vstup_y)
        {
            PointF l_point_vstup = new PointF((float)ad_vstup_x, (float)ad_vstup_y);
            return PrevodBodu(origin, vektor, l_point_vstup);
        }

        public static PointF PrevodBodu(PointF origin, PointF vektor, PointF vstup)
        {
            PointF vysledek = new PointF();

            //zjistit úhel šipky
            double fi_sipky = Math.Atan2((double)(vektor.Y - origin.Y), (double)(vektor.X - origin.X));
            double fi_vstupu = Math.Atan2((double)(vstup.Y - origin.Y), (double)(vstup.X - origin.X));
            double modul_vstupu = MyHypot((vstup.Y - origin.Y), (vstup.X - origin.X));
            //
            vysledek.X = (float)(origin.X + modul_vstupu * Math.Cos(fi_vstupu + fi_sipky));
            vysledek.Y = (float)(origin.Y + modul_vstupu * Math.Sin(fi_vstupu + fi_sipky));

            return vysledek;
        }

        public static PointF PrevodBodu(PointF a_vstup, PositionAspect a_aspect)
        {
            if (
                (a_aspect.m_angle == 0) 
                && (a_aspect.m_vertical == false) && (a_aspect.m_horizontal == false) 
                && (a_aspect.ScaleX == 1) && (a_aspect.ScaleY == 1)
                )
            {
                return a_vstup;
            }

            QPivot pivot = new QPivot(a_aspect);

            return pivot.PrevodBodu(a_vstup);
        }

        public static PointF PrevodBodu(int ai_x, int ai_y, PositionAspect a_aspect)
        {
            return PrevodBodu(new PointF(ai_x, ai_y), a_aspect);
        }

        public static Vector2D Vector3DTo2D(Vector3D a_v3d)
        {
            Vector2D l_vecResult = new Vector2D();
            l_vecResult.X = a_v3d.X;
            l_vecResult.Y = a_v3d.Y;
            return l_vecResult;
        }

        public static void My_Bezier(DxfEntityCollection a_coll, PositionAspect a_aspect, 
                                    PointF a_begin, PointF a_a1, PointF a_a2, PointF a_end, 
                                    int ai_thickness, System.Drawing.Color a_color, bool ab_block)
        {
            PointF[] points = { Helper.PrevodBodu(a_begin, a_aspect), Helper.PrevodBodu(a_a1, a_aspect), Helper.PrevodBodu(a_a2, a_aspect), Helper.PrevodBodu(a_end, a_aspect) };
            DrawPoly l_bezi = new DrawPoly(Shape.bezier, ai_thickness, a_color, points);
            Exporter.ExportBezier(a_coll, l_bezi, ab_block);
        }

        public static int MyHypot(int a, int b)
        {
            int sumOfSquares = a * a + b * b;
            return (int)Math.Sqrt(sumOfSquares);
        }
        
        public static double MyHypot(double a, double b)
        {
            double sumOfSquares = a * a + b * b;
            return (int)Math.Sqrt(sumOfSquares);
        }

        public static void ExportPolylineAux(DxfEntityCollection coll, DrawPoly drawPoly, bool ab_block)
        {
            int li_arrSize = drawPoly.m_points.Count;
            Point2D[] l_arrPoints = new Point2D[li_arrSize];
            for (int i = 0; i < li_arrSize; i++)
            {
                l_arrPoints[i].X = drawPoly.m_points[i].X;
                l_arrPoints[i].Y = Exporter.REVERSE_Y * drawPoly.m_points[i].Y;
            }

            DxfPolyline2D dxfPolyline = new DxfPolyline2D(l_arrPoints);

            dxfPolyline.DefaultStartWidth = drawPoly.m_objProps.m_logpen.m_width;
            dxfPolyline.DefaultEndWidth = drawPoly.m_objProps.m_logpen.m_width;

            //99 dxfPolyline.ColorSource = AttributeSource.This;
            dxfPolyline.Color = Helper.MakeEntityColorByBlock(drawPoly.m_objProps.m_logpen.m_color, ab_block);

            coll.Add(dxfPolyline);
        }


        public static void ExportRectangleAux(DxfEntityCollection coll, Point a_leftTop, Point a_rightBottom, int ai_lineThickness, System.Drawing.Color a_lineColor, bool ab_block)
        {
            Point2D[] l_arrPoints = new Point2D[4];

            l_arrPoints[0].X = a_leftTop.X;
            l_arrPoints[0].Y = a_leftTop.Y * Exporter.REVERSE_Y;

            l_arrPoints[1].X = a_leftTop.X;
            l_arrPoints[1].Y = a_rightBottom.Y * Exporter.REVERSE_Y;

            l_arrPoints[2].X = a_rightBottom.X;
            l_arrPoints[2].Y = a_rightBottom.Y * Exporter.REVERSE_Y;

            l_arrPoints[3].X = a_rightBottom.X;
            l_arrPoints[3].Y = a_leftTop.Y * Exporter.REVERSE_Y;


            DxfPolyline2D dxfPolyline = new DxfPolyline2D(l_arrPoints);

            dxfPolyline.DefaultStartWidth = ai_lineThickness;
            dxfPolyline.DefaultEndWidth = ai_lineThickness;

            //99 dxfPolyline.ColorSource = AttributeSource.This;
            dxfPolyline.Color = Helper.MakeEntityColorByBlock(a_lineColor, ab_block);

            dxfPolyline.Closed = true;
            coll.Add(dxfPolyline);
        }

        private static GraphicsPath GetCapsule(RectangleF baseRect)
        {
            float diameter;
            RectangleF arc;
            GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
            try
            {
                if (baseRect.Width > baseRect.Height)
                {
                    // return horizontal capsule 
                    diameter = baseRect.Height;
                    SizeF sizeF = new SizeF(diameter, diameter);
                    arc = new RectangleF(baseRect.Location, sizeF);
                    path.AddArc(arc, 90, 180);
                    arc.X = baseRect.Right - diameter;
                    path.AddArc(arc, 270, 180);
                }
                else if (baseRect.Width < baseRect.Height)
                {
                    // return vertical capsule 
                    diameter = baseRect.Width;
                    SizeF sizeF = new SizeF(diameter, diameter);
                    arc = new RectangleF(baseRect.Location, sizeF);
                    path.AddArc(arc, 180, 180);
                    arc.Y = baseRect.Bottom - diameter;
                    path.AddArc(arc, 0, 180);
                }
                else
                {
                    // return circle 
                    path.AddEllipse(baseRect);
                }
            }
            catch (Exception)
            {
                path.AddEllipse(baseRect);
            }
            finally
            {
                path.CloseFigure();
            }
            return path;
        }

        public static GraphicsPath GetRoundedRect(RectangleF baseRect, float radiusX, float radiusY)
        {
           
            FixRectangle(ref baseRect);

            // if corner radius is less than or equal to zero, 
            // return the original rectangle 
            if (radiusX <= 0.0F)
            {
                GraphicsPath mPath = new GraphicsPath();
                mPath.AddRectangle(baseRect);
                mPath.CloseFigure();
                return mPath;
            }

            // if the corner radius is greater than or equal to 
            // half the baseRect.Width, or baseRect.Height (whichever is shorter) 
            // then return a capsule instead of a lozenge 
            if (radiusX >= (Math.Min(baseRect.Width, baseRect.Height)) / 2.0)
                return GetCapsule(baseRect);

            // create the arc for the rectangle sides and declare 
            // a graphics path object for the drawing

            GraphicsPath gp = new GraphicsPath();
            gp.StartFigure();
            gp.AddArc(baseRect.X, baseRect.Y, radiusX, radiusY, 180, 90);
            gp.AddArc(baseRect.X + baseRect.Width - radiusX, baseRect.Y, radiusX, radiusY, 270, 90);
            gp.AddArc(baseRect.X + baseRect.Width - radiusX, baseRect.Y + baseRect.Height - radiusY, radiusX, radiusY, 0, 90);
            gp.AddArc(baseRect.X, baseRect.Y + baseRect.Height - radiusY, radiusX, radiusY, 90, 90);
            gp.CloseFigure();
            return gp;
        }

        public static void FixRectangle(ref RectangleF a_rect)
        {
            if (a_rect.Width < 0)
            {
                a_rect.X = a_rect.X + a_rect.Width;
                a_rect.Width = -a_rect.Width;
            }
            if (a_rect.Height < 0)
            {
                a_rect.Y = a_rect.Y + a_rect.Height;
                a_rect.Height = -a_rect.Height;
            }
        }
        public static void FixRectangle(ref Rectangle a_rect)
        {
            if (a_rect.Width < 0)
            {
                a_rect.X = a_rect.X + a_rect.Width;
                a_rect.Width = -a_rect.Width;
            }
            if (a_rect.Height < 0)
            {
                a_rect.Y = a_rect.Y + a_rect.Height;
                a_rect.Height = -a_rect.Height;
            }
        }

        public static void FixSize(ref Size a_size)
        {
            if (a_size.Width < 0)
            {
                a_size.Width = -a_size.Width;
            }
            if (a_size.Height < 0)
            {
                a_size.Height = -a_size.Height;
            }
        }

        public static float ParseScale(System.Xml.XmlAttribute a_attr)
        {
	        if (a_attr == null)
	        {
                return 1;
	        }
            IFormatProvider fp = System.Globalization.CultureInfo.InvariantCulture;//US format
            return float.Parse(a_attr.Value, fp);
        }

        public static EntityColor MakeEntityColorByBlock(System.Drawing.Color a_color, bool ab_block)
        {
            if(ExportContext.Current.BlackByLayer && ColorsAreSame(a_color, System.Drawing.Color.Black))
            {
                return EntityColor.ByLayer;
            }

            return EntityColor.CreateFrom((WW.Drawing.ArgbColor)a_color);
            /*
            if (ab_block)
            {
                return EntityColor.CreateFromRgb(ColorType.ByBlock, a_color.R, a_color.G, a_color.B);
            }
            else
            {
                return EntityColor.CreateFrom((WW.Drawing.ArgbColor)a_color);
            }
            */
        }

        private static bool ColorsAreSame(System.Drawing.Color a_color_1, System.Drawing.Color a_color_2)
        {
            if(a_color_1.R != a_color_2.R)
            {
                return false;
            }
            if (a_color_1.G != a_color_2.G)
            {
                return false;
            }
            if (a_color_1.B != a_color_2.B)
            {
                return false;
            }

            return true;
        }

        internal static string SanitizeLayerName(string as_nodeName)
        {
            string ls_result = string.Empty;
            foreach(char l_char in as_nodeName)
            {
                if(char.IsLetterOrDigit(l_char) || l_char == '-')
                {
                    ls_result += l_char;
                }
                else
                {
                    ls_result += " ";
                }
            }

            return ls_result.Trim();
        }

        internal static AttachmentPoint FlipAlignment(AttachmentPoint attachmentPoint)
        {
            switch(attachmentPoint)
            {
                case AttachmentPoint.TopLeft:
                    return AttachmentPoint.TopRight;
                case AttachmentPoint.TopRight:
                    return AttachmentPoint.TopLeft;

                case AttachmentPoint.MiddleLeft:
                    return AttachmentPoint.MiddleRight;
                case AttachmentPoint.MiddleRight:
                    return AttachmentPoint.MiddleLeft;

                case AttachmentPoint.BottomLeft:
                    return AttachmentPoint.BottomRight;
                case AttachmentPoint.BottomRight:
                    return AttachmentPoint.BottomLeft;


                default:
                    return attachmentPoint;
            }
        }

        internal static Point3D Point_To_Point3D(Point a_point)
        {
            return new Point3D(a_point.X, a_point.Y, 0);
        }
        internal static Point3D Point_To_Point3D(PointF a_point)
        {
            return new Point3D(a_point.X, a_point.Y, 0);
        }

        //--------------------------------
    }
}
