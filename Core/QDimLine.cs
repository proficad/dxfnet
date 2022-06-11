using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Xml;

namespace DxfNet
{
    public class QDimLine : DrawObj
    {
        public PointF A, B, C;
        public QLabel Label = new QLabel();

        public enum DimDirection { dimdir_none, dimdir_hor, dimdir_ver, dimdir_aligned };
        public DimDirection m_dir;

        public QDimLine(PointF a_a, PointF a_b, PointF a_c, DimDirection a_dir) 
            : base(Shape.dim_line, new RectangleF(a_a.X, a_a.Y, 1, 1))
        {
            A = a_a;
            B = a_b;
            C = a_c;
            m_dir = a_dir;
        }

        internal override void Write2Xml(System.Xml.XmlWriter a_xmlWriter)
        {
            a_xmlWriter.WriteStartElement("dim_line");

            Helper.Point2Attrib(a_xmlWriter, "a", A);
            Helper.Point2Attrib(a_xmlWriter, "b", B);
            Helper.Point2Attrib(a_xmlWriter, "c", C);

            a_xmlWriter.WriteAttributeString("dir", ((int)m_dir).ToString());

            //m_objProps.SaveToXml(a_xmlWriter);
            Label.Write2Xml(a_xmlWriter);

            a_xmlWriter.WriteEndElement();

        }

        public override bool IsValid(int ai_size_x, int ai_size_y)
        {
            if (Math.Abs(m_position.X) > ai_size_x)
            {
                return false;
            }
            if (Math.Abs(m_position.Y) > ai_size_y)
            {
                return false;
            }

            return true;

        }

        internal override void RecalcBounds(ref MyRect l_bounds)
        {
            
        }

        internal override void MoveBy(SizeF a_offset)
        {
            A += a_offset;
            B += a_offset;
            C += a_offset;

        }

        public override void RecalcPosition()
        {
         
        }

        internal override void Recalc_Size(float af_x, float af_y)
        {
            throw new NotImplementedException();
        }
    }
}
