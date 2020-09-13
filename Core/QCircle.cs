using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Xml;

namespace DxfNet
{
    public class QCircle : DrawObj
    {
        public QCircle(Point a_center, Point a_tangent) : base(Shape.circle, new Rectangle())
        {
            m_center = a_center;
            m_tangent = a_tangent;
        }

        public QCircle(Point a_center, int ai_radius) : base(Shape.circle)
        {
            m_center = a_center;
            m_tangent.X = m_center.X + ai_radius;
            m_tangent.Y = m_center.Y;
        }


        public Point m_center;
        public Point m_tangent;

        public EFont m_efont;
        public int m_text_angle;


        internal override void Write2Xml(XmlWriter a_xmlWriter)
        {
            a_xmlWriter.WriteStartElement("circle");

            Helper.Point2Attrib(a_xmlWriter, "center", m_center);
            Helper.Point2Attrib(a_xmlWriter, "tangent", m_tangent);

            a_xmlWriter.WriteEndElement();
        }


        internal override void RecalcBounds(ref MyRect l_bounds)
        {
            throw new NotImplementedException();
        }

        internal override void MoveBy(Size l_offset)
        {
            throw new NotImplementedException();
        }

        public override void RecalcPosition()
        {
            throw new NotImplementedException();
        }
    }
}
