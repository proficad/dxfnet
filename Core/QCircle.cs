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

        public Point m_center;
        public Point m_tangent;

        public EFont m_efont;
        public int m_text_angle;


        internal override void Write2Xml(XmlWriter a_xmlWriter)
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
