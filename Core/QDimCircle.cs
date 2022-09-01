using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace DxfNet
{
    public class QDimCircle : DrawObj
    {
        public PointF A, B, C;
        public bool m_has_2_arrows;
        public PointF m_pos_label;

        public QLabel Label;


        public QDimCircle(PointF a_a, PointF a_b, bool ab_has_2_arr, PointF a_pos_label) : base(Shape.dim_circle, new RectangleF(a_a.X, a_a.Y, 1, 1))
        {
            A = a_a;
            B = a_b;
            m_has_2_arrows = ab_has_2_arr;
            m_pos_label = a_pos_label;
        }

        internal override void Write2Xml(System.Xml.XmlWriter a_xmlWriter)
        {

        }

        public override bool IsValid(int ai_size_x, int ai_size_y)
        {
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
            throw new NotImplementedException();
        }
    }
}
