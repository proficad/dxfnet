using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace DxfNet
{
    public class PositionAspect
    {
        public PointF m_pivot;
        public int m_angle;
        public bool m_horizontal;
        public bool m_vertical;
        public double ScaleX { get; set; }
        public double ScaleY { get; set; }

        public PositionAspect()
        {
            m_pivot = new Point(0,0);
            m_angle = 0;
            m_horizontal = false;
            m_vertical = false;
        }


        public PositionAspect(PointF a_pivot, int a_angle, bool a_horizontal, bool a_vertical)
        {
            m_pivot = a_pivot;
            m_angle = a_angle;
            m_horizontal = a_horizontal;
            m_vertical = a_vertical;
            ScaleX = 1;
            ScaleY = 1;
        }

        public PositionAspect(PointF a_pivot, int a_angle, bool a_horizontal, bool a_vertical, double ad_scale_x, double ad_scale_y)
        {
            m_pivot = a_pivot;
            m_angle = a_angle;
            m_horizontal = a_horizontal;
            m_vertical = a_vertical;
            ScaleX = ad_scale_x;
            ScaleY = ad_scale_y;
        }


    }
}
