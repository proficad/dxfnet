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
        public Point A, B, C;
        public QLabel Label = new QLabel();

        public enum DimDirection { dimdir_none, dimdir_hor, dimdir_ver, dimdir_aligned };
        public DimDirection m_dir;

        public QDimLine(Point a_a, Point a_b, Point a_c, DimDirection a_dir) : base(Shape.dim_line, new Rectangle(a_a.X, a_a.Y, 1, 1))
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

        internal override void MoveBy(Size a_offset)
        {
            A.Offset(a_offset.Width, a_offset.Height);
            B.Offset(a_offset.Width, a_offset.Height);
            C.Offset(a_offset.Width, a_offset.Height);

        }

        public override void RecalcPosition()
        {
         
        }
    }
}
