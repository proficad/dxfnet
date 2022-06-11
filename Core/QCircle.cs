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
        public QCircle(PointF a_center, PointF a_tangent) : base(Shape.circle, new Rectangle())
        {
            m_center = a_center;
            m_tangent = a_tangent;
        }

        public QCircle(PointF a_center, int ai_radius) : base(Shape.circle)
        {
            m_center = a_center;
            m_tangent.X = m_center.X + ai_radius;
            m_tangent.Y = m_center.Y;
        }


        public PointF m_center;
        public PointF m_tangent;

        public EFont m_efont;
        public int m_text_angle;


        internal override void Write2Xml(XmlWriter a_xmlWriter)
        {
            a_xmlWriter.WriteStartElement("circle");

            Helper.Point2Attrib(a_xmlWriter, "center", m_center);
            Helper.Point2Attrib(a_xmlWriter, "tangent", m_tangent);

            m_objProps.SaveToXml(a_xmlWriter);

            a_xmlWriter.WriteEndElement();
        }


        public override bool IsValid(int ai_size_x, int ai_size_y)
        {
            if (Math.Abs(m_center.X) > ai_size_x)
            {
                return false;
            }
            if (Math.Abs(m_center.Y) > ai_size_y)
            {
                return false;
            }

            return true;
        }

        internal override void RecalcBounds(ref MyRect l_bounds)
        {
            throw new NotImplementedException();
        }

        internal override void MoveBy(SizeF l_offset)
        {
            throw new NotImplementedException();
        }

        public override void RecalcPosition()
        {
            throw new NotImplementedException();
        }

        internal override void Recalc_Size(float af_x, float af_y)
        {
            m_center.X = (int)Math.Round(m_center.X * af_x);
            m_center.Y = (int)Math.Round(m_center.Y * af_y);

            m_tangent.X = (int)Math.Round(m_tangent.X * af_x);
            m_tangent.Y = (int)Math.Round(m_tangent.Y * af_y);
         
        }
    }
}
