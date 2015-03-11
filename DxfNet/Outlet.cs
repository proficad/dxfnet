using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace DxfNet
{
    public class Outlet : DrawObj
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Outlet(int ai_x, int ai_y) : base(Shape.outlet, new Rectangle(ai_x, ai_y, 1, 1))
        {
            X = ai_x;
            Y = ai_y;
        }


        internal override void RecalcBounds(ref MyRect l_bounds)
        { }

        internal override void MoveBy(Size l_offset)
        {
            m_position.X += l_offset.Width;
            m_position.Y += l_offset.Height;
        }

        public override void RecalcPosition()
        { }

        internal override void Write2Xml(System.Xml.XmlWriter a_xmlWriter)
        { }

    }
}
