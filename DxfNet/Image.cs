using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace DxfNet
{
    public class QImage : DrawObj
    {
        public QImage(Shape a_shape, Rectangle a_rect, string as_lastGuid) : base(a_shape, a_rect)
        {
            LastGuid = as_lastGuid;
        }

        internal override void Write2Xml(System.Xml.XmlWriter a_xmlWriter)
        {

        }

        internal override void RecalcBounds(ref MyRect l_bounds)
        {

        }

        internal override void MoveBy(Size l_offset)
        {
            m_position.X += l_offset.Width;
            m_position.Y += l_offset.Height;
        }

        public override void RecalcPosition()
        {

        }

        public QImageDesc ImgDesc { get; set; }
        public string LastGuid;


        public double GetWidth()
        {
            return m_position.Width;
        }

        public double GetHeight()
        {
            return m_position.Height;
        }


    }
}
