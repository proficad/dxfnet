using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace DxfNet
{
    public class Wire : DrawPoly
    {
        private bool m_drop1;
        private bool m_drop2;

        bool m_is_connected_first;
        bool m_is_connected_last;

        string m_name;

        public string GetName()
        {
            return m_name;
        }

        public void SetName(string as_name)
        {
            m_name = as_name;
        }

        public bool Is_connected_first
        {
            get { return m_is_connected_first; }
            set { m_is_connected_first = value; }
        }

        public bool Is_connected_last
        {
            get { return m_is_connected_last; }
            set { m_is_connected_last = value; }
        }

       public DxfNet.WireLabelPos m_label_beg = null, m_label_mid = null, m_label_end = null;


        public Wire()
            : base(Shape.spoj)
        { }

        public void SetDrop1(bool ab_set)
        {
            m_drop1 = ab_set;
        }
        public void SetDrop2(bool ab_set)
        {
            m_drop2 = ab_set;
        }
        public bool GetDrop1()
        {
            return m_drop1;
        }
        public bool GetDrop2()
        {
            return m_drop2;
        }

        public System.Drawing.PointF GetEndingPoint(bool ab_first)
        {
            return ab_first ? m_points[0] : m_points.Last();
        }

        public void SetupWireStatusConnectedKapky()
        {
            if (m_drop1)
            {
                Is_connected_first = true;
            }
            if (m_drop2)
            {
                Is_connected_last = true;
            }
        }

        public PointF GetLastButOne()
        {
            return m_points[m_points.Count - 2];
        }

        public bool IsWireShort(int ai_long_wire_len)
        {
            if (m_points.Count != 2) // > 2 means it is long, < 2 means it is not valid
            {
                return false;
            }

            double li_dist = Helper.EasyDistance2Points(m_points[0], m_points[1]);
            return li_dist < (10 * ai_long_wire_len);
        }

        public PointF GetLineCenterPoint()
        {
            PointF l_center = new PointF();
            l_center.X = (m_points[0].X + m_points[1].X) / 2;
            l_center.Y = (m_points[0].Y + m_points[1].Y) / 2;

            return l_center;
        }

        public WireLabelPos GetEndingLabelVis(bool ab_first)
        {
            if (ab_first)
            {
                return m_label_beg;
            }
            else
            {
                return m_label_end;
            }
        }
        //-------------------------

    }
}
