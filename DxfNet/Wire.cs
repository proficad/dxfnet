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
        //-------------------------
    }
}
