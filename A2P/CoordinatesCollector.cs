using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WW.Cad.Drawing;
using WW.Cad.IO;
using WW.Cad.Model;
using WW.Cad.Model.Entities;
using WW.Drawing;
using WW.Math;
using WW.Math.Geometry;


namespace Dxf2ProfiCAD
{
    public class CoordinatesCollector : BaseWireframeGraphicsFactory
    {

        public List<Point3D> m_list = new List<Point3D>(2000);


        public override void CreateDot(
            DxfEntity entity,
            DrawContext.Wireframe drawContext,
            ArgbColor color,
            bool forText,
            Vector4D position
        )
        {
            Point3D point = (Point3D)position;
            Console.WriteLine("Dot: {0}", point.ToString());
        }

        public override void CreateLine(
            DxfEntity entity,
            DrawContext.Wireframe drawContext,
            ArgbColor color,
            bool forText,
            Vector4D start,
            Vector4D end
        )
        {
            Point3D point1 = (Point3D)start;
            Point3D point2 = (Point3D)end;
            Console.WriteLine("Line, start: {0}, end: {1}", start.ToString(), end.ToString());
        }

        public override void CreatePath(
            DxfEntity entity,
            DrawContext.Wireframe drawContext,
            ArgbColor color,
            bool forText,
            IList<Polyline4D> polylines,
            bool fill,
            bool correctForBackgroundColor
        )
        {
            WritePolylines(polylines);
        }

        public override void CreatePathAsOne(
            DxfEntity entity,
            DrawContext.Wireframe drawContext,
            ArgbColor color,
            bool forText,
            IList<Polyline4D> polylines,
            bool fill,
            bool correctForBackgroundColor
        )
        {
            WritePolylines(polylines);
        }

        public override void CreateShape(
            DxfEntity entity,
            DrawContext.Wireframe drawContext,
            ArgbColor color,
            bool forText,
            IShape4D shape
        )
        {
            WritePolylines(shape.ToPolylines4D(ShapeTool.DefaultEpsilon));
        }

        private void WritePolylines(IList<Polyline4D> polylines)
        {
            foreach (Polyline4D polyline in polylines)
            {
                foreach (Vector4D vector in polyline)
                {
                    Point3D point = (Point3D)vector;
                    m_list.Add(point);
                }

            }
        }
    }

}
