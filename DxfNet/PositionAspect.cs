using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace DxfNet
{
    public class PositionAspect
    {
        public Point m_pivot;
        public int m_otacek;
        public bool m_horizontal;
        public bool m_vertical;

        public PositionAspect()
        {
            m_pivot = new Point(0,0);
            m_otacek = 0;
            m_horizontal = false;
            m_vertical = false;
        }


        public PositionAspect(Point a_pivot, int a_otacek, bool a_horizontal, bool a_vertical)
        {
            m_pivot = a_pivot;
            m_otacek = a_otacek;
            m_horizontal = a_horizontal;
            m_vertical = a_vertical;
        }
    }
}
