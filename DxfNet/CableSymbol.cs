﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace DxfNet
{
    public class CableSymbol : DrawObj
    {
        private int m_min;
        private int m_common;
        private int m_max;
        private bool m_hor;

        public bool Hor { get { return m_hor; } }
        public int Min { get { return m_min; } }
        public int Common { get { return m_common; } }
        public int Max { get { return m_max; } }
        public List<Insert.Satelite> m_satelites = new List<Insert.Satelite>();


        public CableSymbol(int li_min, int li_common, int li_max, bool lb_hor, Rectangle a_rect) : base(Shape.cable, a_rect)
        {
            
            this.m_min = li_min;
            this.m_common = li_common;
            this.m_max = li_max;
            this.m_hor = lb_hor;
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
