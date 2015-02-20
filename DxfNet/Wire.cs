using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public System.Drawing.Point GetEndingPoint(bool ab_first)
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

        //-------------------------

    }
}
