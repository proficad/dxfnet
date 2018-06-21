using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace DxfNet
{
    public class QDimCircle : DrawObj
    {
        public Point A, B, C;
        public bool m_has_2_arrows;
        public Point m_pos_label;

        public QDimCircle(Point a_a, Point a_b, bool ab_has_2_arr, Point a_pos_label) : base(Shape.dim_circle, new Rectangle(a_a.X, a_a.Y, 1, 1))
        {
            A = a_a;
            B = a_b;
            m_has_2_arrows = ab_has_2_arr;
            m_pos_label = a_pos_label;
        }

        internal override void Write2Xml(System.Xml.XmlWriter a_xmlWriter)
        {
            throw new NotImplementedException();
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
