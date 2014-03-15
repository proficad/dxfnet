using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using WW.Cad.IO;
using WW.Cad.Model;
using WW.Cad.Model.Entities;
using WW.Math;

using WW.Cad.Base;
using WW.Cad.Model.Tables;


namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            new BezierExample().Test();
        }
    }

    public class InsertExample
    {
        public void Test()
        {
            DxfModel model = new DxfModel(DxfVersion.Dxf14);

            // Create block.
            DxfBlock block = new DxfBlock("TEST_BLOCK");
            model.Blocks.Add(block);
            block.Entities.Add(new DxfCircle(Point3D.Zero, 2d, Color.Blue));
            block.Entities.Add(new DxfLine(Point3D.Zero, new Point3D(-1, 2, 1), Color.Red));
            block.Entities.Add(new DxfLine(Point3D.Zero, new Point3D(2, 0, 1), Color.Green));

            // Insert block at 3 positions.
            model.Entities.Add(new DxfInsert(block, new Point3D(1, 0, 0)));
            model.Entities.Add(new DxfInsert(block, new Point3D(3, 1, 0)));
            model.Entities.Add(new DxfInsert(block, new Point3D(2, -3, 0)));

            DxfWriter.WriteDxf("DxfWriteInsertTest.dxf", model, false);
        }
    }

    public class BezierExample
    {
        public void Test()
        {
            DxfModel model = new DxfModel(DxfVersion.Dxf14);
            int li_arrSize = 4;
            Point2D[] l_arrPoints = new Point2D[li_arrSize];

/* sin
            l_arrPoints[0].X = -1; l_arrPoints[0].Y = 0;
            l_arrPoints[1].X = 0; l_arrPoints[1].Y = 1;
            l_arrPoints[2].X = 0; l_arrPoints[2].Y = -1;
            l_arrPoints[3].X =  1; l_arrPoints[3].Y = 0;
*/
            l_arrPoints[0].X = 0; l_arrPoints[0].Y = 1;
            l_arrPoints[1].X = 4; l_arrPoints[1].Y = 2;
            l_arrPoints[2].X = 4; l_arrPoints[2].Y = -2;
            l_arrPoints[3].X = 0; l_arrPoints[3].Y = -1;

            
            DxfPolyline2DSpline dxfPolyline = new DxfPolyline2DSpline(SplineType.CubicBSpline, l_arrPoints);
            dxfPolyline.Closed = false;

            model.Entities.Add(dxfPolyline);
            DxfWriter.WriteDxf("Bezier.dxf", model, false);
        }
    }

    public class HatchExample
    {
        public void Test()
        {
            DxfModel model = new DxfModel(DxfVersion.Dxf14);

            DxfHatch hatch = new DxfHatch();
            hatch.Color = Color.Green;
            //hatch.ElevationPoint = new Point3D(0, 0, 2);
            //hatch.ZAxis = new Vector3D(-0.707, 0, 0.707);

/*
            // A boundary path bounded by lines.
            DxfHatch.BoundaryPath boundaryPath1 = new DxfHatch.BoundaryPath();
            boundaryPath1.Type = BoundaryPathType.Default;
            hatch.BoundaryPaths.Add(boundaryPath1);
            boundaryPath1.Edges.Add(new DxfHatch.BoundaryPath.LineEdge(new Point2D(0, 0), new Point2D(1, 0)));
            boundaryPath1.Edges.Add(new DxfHatch.BoundaryPath.LineEdge(new Point2D(1, 0), new Point2D(1, 1)));
            boundaryPath1.Edges.Add(new DxfHatch.BoundaryPath.LineEdge(new Point2D(1, 1), new Point2D(0, 1)));
            boundaryPath1.Edges.Add(new DxfHatch.BoundaryPath.LineEdge(new Point2D(0, 1), new Point2D(0, 0)));

            // A boundary path bounded by an ellipse.
            DxfHatch.BoundaryPath boundaryPath2 = new DxfHatch.BoundaryPath();
            boundaryPath2.Type = BoundaryPathType.Default;
            hatch.BoundaryPaths.Add(boundaryPath2);
            DxfHatch.BoundaryPath.EllipseEdge edge = new DxfHatch.BoundaryPath.EllipseEdge();
            edge.CounterClockWise = true;
            edge.Center = new Point2D(1, 1);
            edge.MajorAxisEndPoint = new Vector2D(0.4d, -0.2d);
            edge.MinorToMajorRatio = 0.7;
            edge.StartAngle = 0d;
            edge.EndAngle = Math.PI * 2d / 3d;
            boundaryPath2.Edges.Add(edge);
*/
            // A boundary path bounded by lines and an arc.
            DxfHatch.BoundaryPath boundaryPath3 = new DxfHatch.BoundaryPath();
            boundaryPath3.Type = BoundaryPathType.Outermost;
            hatch.BoundaryPaths.Add(boundaryPath3);
            DxfHatch.BoundaryPath.ArcEdge arcEdge = new DxfHatch.BoundaryPath.ArcEdge();
            arcEdge.Center = new Point2D(0, 0);
            arcEdge.Radius = 1.0d;
            arcEdge.StartAngle = 0;
            arcEdge.EndAngle = 3 * Math.PI / 2d;
            arcEdge.CounterClockWise = true;
            boundaryPath3.Edges.Add(arcEdge);
            boundaryPath3.Edges.Add(new DxfHatch.BoundaryPath.LineEdge(new Point2D(0, -1.0d), new Point2D(0, 0)));
            boundaryPath3.Edges.Add(new DxfHatch.BoundaryPath.LineEdge(new Point2D(0d, 0d), new Point2D(1d, 0d)));
//            boundaryPath3.Edges.Add(new DxfHatch.BoundaryPath.LineEdge(new Point2D(0d, 0.5d), new Point2D(0.5d, 1d)));
/*
            // A boundary path bounded by a polyline.
            DxfHatch.BoundaryPath boundaryPath6 = new DxfHatch.BoundaryPath();
            boundaryPath6.Type = BoundaryPathType.Polyline;
            hatch.BoundaryPaths.Add(boundaryPath6);
            boundaryPath6.PolylineData =
                new DxfHatch.BoundaryPath.Polyline(
                    new DxfHatch.BoundaryPath.Polyline.Vertex[] {
                        new DxfHatch.BoundaryPath.Polyline.Vertex(0.5, -0.5),
                        new DxfHatch.BoundaryPath.Polyline.Vertex(0.5, 0.5),
                        new DxfHatch.BoundaryPath.Polyline.Vertex(1.5, 0.5),
                        new DxfHatch.BoundaryPath.Polyline.Vertex(1.5, 0-.25),
                        new DxfHatch.BoundaryPath.Polyline.Vertex(0.75, -0.25),
                        new DxfHatch.BoundaryPath.Polyline.Vertex(0.75, 0.25),
                        new DxfHatch.BoundaryPath.Polyline.Vertex(1.25, 0.25),
                        new DxfHatch.BoundaryPath.Polyline.Vertex(1.25, -0.5)
                    }
                );
            boundaryPath6.PolylineData.Closed = true;
*/
            // Define the hatch fill pattern.
            // Don't set a pattern for solid fill.
            /*
            hatch.Pattern = new DxfPattern();
            DxfPattern.Line patternLine = new DxfPattern.Line();
            hatch.Pattern.Lines.Add(patternLine);
            patternLine.Angle = Math.PI / 4d;
            patternLine.Offset = new Vector2D(0.02, -0.01d);
            patternLine.DashLengths.Add(0.02d);
            patternLine.DashLengths.Add(-0.01d);
            patternLine.DashLengths.Add(0d);
            patternLine.DashLengths.Add(-0.01d);
            */
            model.Entities.Add(hatch);

            DxfWriter.WriteDxf("DxfWriteHatchTest.dxf", model, false);
        }
    }

    public class MTextExample
    {
        public void Test()
        {
            DxfModel model = new DxfModel(DxfVersion.Dxf15);
            DxfMText mtext = new DxfMText(
                @"Multiline1, Multiline2 in Green",
                new Point3D(-2d, -3d, 1d),
                1d
            );
            model.Entities.Add(mtext);
            DxfWriter.WriteDxf("MText.dxf", model, false);
        }
    }

    public class DxfTextExample
    {
        public void Test()
        {
            DxfModel model = new DxfModel(DxfVersion.Dxf15);

            // Text.
            model.Entities.Add(new DxfText("Wout \n Ware  1", Point3D.Zero, 1d));

            // Extruded text.
            DxfText text2 = new DxfText("Wout Ware 2", new Point3D(0d, 2d, 0d), 1d);
            text2.Thickness = 0.4d;
            model.Entities.Add(text2);

            // DxfText with custom text style.
            DxfTextStyle textStyle = new DxfTextStyle("MYSTYLE", "arial.ttf");
            model.TextStyles.Add(textStyle);
            DxfText text3 = new DxfText("Wout Ware 3", new Point3D(0d, 4d, 0d), 1d);
            text3.Thickness = 0.4d;
            text3.Style = textStyle;
            model.Entities.Add(text3);

            DxfWriter.WriteDxf("DxfWriteTest-R15-ascii.dxf", model, false);
        }
    }


    public class PieExample
    {
        public void Test()
        {
            Rectangle l_rect = new Rectangle(100, 100, 300, 200);
            double startAngle = 0.98;
            double endAngle = -2.55;

            DxfModel model = new DxfModel(DxfVersion.Dxf14);

            //need center, long axis and min/maj ratio
            Point3D l_center = GetRectCenterPoint(l_rect);
            Point2D l_center2D = new Point2D();
            l_center2D.X = l_center.X;
            l_center2D.Y = l_center.Y;

            Vector3D l_longAxis = new Vector3D();
            l_longAxis.X = l_rect.Width / 2;
            l_longAxis.Y = l_longAxis.Z = 0;
            int li_heightHalf = l_rect.Height / 2;
            if (li_heightHalf == 0)
            {
                return;
            }
            double ld_ratio = li_heightHalf / l_longAxis.X;


            DxfHatch hatch = new DxfHatch();
            ExportPieInner(startAngle, endAngle, l_center2D, l_longAxis, ld_ratio, hatch);

            model.Entities.Add(hatch);
            DxfWriter.WriteDxf(@"c:\down\Pie.dxf", model, false);
        }

        private static void ExportPieInner(double startAngle, double endAngle, Point2D l_center2D, Vector3D l_longAxis, double ld_ratio, DxfHatch hatch)
        {
            DxfHatch.BoundaryPath boundaryPath3 = new DxfHatch.BoundaryPath();
            boundaryPath3.Type = BoundaryPathType.Outermost;
            hatch.BoundaryPaths.Add(boundaryPath3);
            DxfHatch.BoundaryPath.EllipseEdge ellipseEdge = new DxfHatch.BoundaryPath.EllipseEdge();
            ellipseEdge.Center = l_center2D;
            ellipseEdge.CounterClockWise = true;
            ellipseEdge.MajorAxisEndPoint = Vector3DTo2D(l_longAxis);
            ellipseEdge.MinorToMajorRatio = ld_ratio;
            ellipseEdge.StartAngle = startAngle;
            ellipseEdge.EndAngle = endAngle;

            //------------------------ B
            Vector2D xaxis = new Vector2D(ellipseEdge.MajorAxisEndPoint.X, ellipseEdge.MajorAxisEndPoint.Y);
            xaxis.Normalize();
            Vector2D yaxis = new Vector2D(-xaxis.Y, xaxis.X);
            yaxis.Normalize();

            double a = ellipseEdge.MajorAxisEndPoint.GetLength();
            double b = ld_ratio * a;

            xaxis *= a;
            yaxis *= b;

            double param1 = PrapareParam(startAngle, ld_ratio);
            double param2 = PrapareParam(endAngle, ld_ratio);

            Point2D startPoint = l_center2D + System.Math.Cos(param1) * xaxis + System.Math.Sin(param1) * yaxis;
            Point2D endPoint = l_center2D + System.Math.Cos(param2) * xaxis + System.Math.Sin(param2) * yaxis;
            //------------------------ E


            boundaryPath3.Edges.Add(ellipseEdge);
            boundaryPath3.Edges.Add(new DxfHatch.BoundaryPath.LineEdge(endPoint, l_center2D));
            boundaryPath3.Edges.Add(new DxfHatch.BoundaryPath.LineEdge(l_center2D, startPoint));
        }


        private static Point3D GetRectCenterPoint(Rectangle a_rect)
        {
            Point3D l_result = new Point3D();
            l_result.X = (a_rect.Left + a_rect.Right) / 2;
            l_result.Y = (a_rect.Top + a_rect.Bottom) / 2;
            return l_result;
        }

        public static Vector2D Vector3DTo2D(Vector3D a_v3d)
        {
            Vector2D l_vecResult = new Vector2D();
            l_vecResult.X = a_v3d.X;
            l_vecResult.Y = a_v3d.Y;
            return l_vecResult;
        }

        public static double PrapareParam(double input, double minorToMajorRatio)
        {
            double cos = System.Math.Cos(input);
            double sin = minorToMajorRatio * System.Math.Sin(input);
            double parameter = System.Math.Atan2(sin, cos);
            return parameter;
        }
    }

    public class SplineExample
    {
        public void Test()
        {
            DxfModel model = new DxfModel(DxfVersion.Dxf13);
            DxfSpline spline = new DxfSpline();
            spline.Degree = 3;
            spline.Closed = true;
            spline.Color = Color.Blue;
            spline.ControlPoints.AddRange(
                new Point3D[] {
                    new Point3D(-1, 0, 1),
                    new Point3D(-2, 2, 1),
                    new Point3D(-0.5, 2, 1),
                    new Point3D(0.5, 2, 1),
                    new Point3D(2, 2, 1),
                    new Point3D(1, 0, 1)
                }
            );
            spline.Knots.AddRange(new double[] { 0, 0, 0, 0, 0.4, 0.6, 1, 1, 1, 1 });
            model.Entities.Add(spline);
            DxfWriter.WriteDxf("DxfWriteSplineTest.dxf", model, false);
        }
    }

    public class Polyline2DSplineExample
    {
        public void Test()
        {
            DxfModel model = new DxfModel(DxfVersion.Dxf14);

            // 2D spline.
            DxfPolyline2DSpline polyline2 = new DxfPolyline2DSpline(SplineType.CubicBSpline);
            polyline2.ControlPoints.AddRange(
                new DxfVertex2D[] {
                    new DxfVertex2D(0, -4),
                    new DxfVertex2D(0, -3),
                    new DxfVertex2D(1, -5),
                    new DxfVertex2D(1, -4)
                }
            );
            model.Entities.Add(polyline2);

            DxfWriter.WriteDxf("WriteDxfPolyline3DTest-R14.dxf", model, false);
        }
    }

    public class RoundedRectTest
    {
        public void Test()
        {
            double li_radX = 1;
            double li_radY = 0.5;

            DxfModel model = new DxfModel(DxfVersion.Dxf14);

            DxfVertex2D[] pole = new DxfVertex2D[] 
            {
                    new DxfVertex2D(3 - li_radX, -2),
                    new DxfVertex2D(-3 + li_radX, -2),
                    new DxfVertex2D(-3, -2 + li_radY),
                    new DxfVertex2D(-3, 2 - li_radY),
                    new DxfVertex2D(-3 + li_radX, 2),
                    new DxfVertex2D(3 - li_radX, 2),
                    new DxfVertex2D(3, 2 - li_radY),
                    new DxfVertex2D(3, -2 + li_radY)
            };

            Point2D center = new Point2D(-3 + li_radX, -2 + li_radY);
            Point2D start = new Point2D(-3 + li_radX, -2);
            Point2D end = new Point2D(-3, -2 + li_radY);

            pole[1].Bulge = GetBulgeFromEndPointsFixed(center, start, end);

            // 2D spline.
            DxfPolyline2D polyline2D = new DxfPolyline2D(pole );
            polyline2D.Closed = true;

            model.Entities.Add(polyline2D);

            DxfWriter.WriteDxf("RoundedRectTest-R14.dxf", model, false);
        }

        /// <summary>
        /// Calculates the <see cref="Bulge"/> from the arc segment's 
        /// center and end points.
        /// </summary>
        /// <param name="center">The arc center.</param>
        /// <param name="arcStart">The arc start point.</param>
        /// <param name="arcEnd">The arc end point.</param>
        public static double GetBulgeFromEndPointsFixed(
            Point2D center,
            Point2D arcStart,
            Point2D arcEnd
        )
        {
            // Take v1 as the x-axis.
            Vector2D v1 = arcStart - center;
            Vector2D yaxis = new Vector2D(-v1.Y, v1.X);

            Vector2D v2 = arcEnd - center;
            // Project on v1.
            double x = Vector2D.DotProduct(v1, v2);
            double y = Vector2D.DotProduct(yaxis, v2);

            double angle = System.Math.Atan2(y, x);

            return System.Math.Tan(angle / 4d);
        }

    }



    public class DiametricDimensionExample
    {
        public void Test()
        {
            DxfModel model = new DxfModel(DxfVersion.Dxf14);

            DxfLayer layer = new DxfLayer("DIMENSIONS");
            model.Layers.Add(layer);

            DxfBlock block = new DxfBlock("DIAMETRIC_DIMENSIONS");
            model.Blocks.Add(block);

            DxfInsert insert = new DxfInsert(block, new Point3D(10, -15, 0));
            insert.Layer = layer;
            model.Entities.Add(insert);

            DxfDimension.Diametric diametricDimension1 = new DxfDimension.Diametric();
            diametricDimension1.ArcLineIntersectionPoint1 = new Point3D(0, 0, 0);
            diametricDimension1.ArcLineIntersectionPoint2 = new Point3D(2, 1, 0);
            block.Entities.Add(diametricDimension1);

            DxfWriter.WriteDxf("Test.dxf", model, false);
        }
    }


}
