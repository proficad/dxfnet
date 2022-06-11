using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace DxfNet
{
    public class QImage : DrawObj
    {
        public QImage(Shape a_shape, Rectangle a_rect, string as_lastGuid, 
            int a_angle_tenths, bool ab_hor, bool ab_ver) 
            : base(a_shape, a_rect)
        {
            LastGuid = as_lastGuid;
            m_angle_tenths = a_angle_tenths;
            m_hor = ab_hor;
            m_ver = ab_ver;
        }

        internal override void Write2Xml(System.Xml.XmlWriter a_xmlWriter)
        {

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

        internal override void MoveBy(SizeF l_offset)
        {
            m_position.X += l_offset.Width;
            m_position.Y += l_offset.Height;
        }

        public override void RecalcPosition()
        {

        }

        public QImageDesc ImgDesc { get; set; }
        public string LastGuid;

        public float Angle { get; private set; }

        public bool m_hor;
        public bool m_ver;
        public int m_angle_tenths;

        public double GetWidth()
        {
            return m_position.Width;
        }

        public double GetHeight()
        {
            return m_position.Height;
        }

        internal override void Recalc_Size(float af_x, float af_y)
        {
            throw new NotImplementedException();
        }
    }
}
