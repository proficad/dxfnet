using DxfNet;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Xml;

namespace Core
{
    internal class QArc_3_P : DrawObj
    {
        public QArc_3_P(PointF a_point_1, PointF a_point_2, PointF a_point_3): base(Shape.arc_3_p, new Rectangle())
        {
            m_point_1 = a_point_1;
            m_point_2 = a_point_2;
            m_point_3 = a_point_3;
        }


        public override bool IsValid(int ai_size_x, int ai_size_y)
        {
            throw new NotImplementedException();
        }

        public override void RecalcPosition()
        {
            throw new NotImplementedException();
        }

        internal override void MoveBy(SizeF l_offset)
        {
            throw new NotImplementedException();
        }

        internal override void RecalcBounds(ref MyRect l_bounds)
        {
            throw new NotImplementedException();
        }

        internal override void Recalc_Size(float af_x, float af_y)
        {
            throw new NotImplementedException();
        }

        internal override void Write2Xml(XmlWriter a_xmlWriter)
        {
            a_xmlWriter.WriteStartElement(ATTR_NAME_ARC_3_P);

            Helper.Point2Attrib(a_xmlWriter, ATTR_POINT_1, m_point_1);
            Helper.Point2Attrib(a_xmlWriter, ATTR_POINT_2, m_point_2);
            Helper.Point2Attrib(a_xmlWriter, ATTR_POINT_3, m_point_3);

            m_objProps.SaveToXml(a_xmlWriter);

            a_xmlWriter.WriteEndElement();
        }



        private PointF m_point_1;
        private PointF m_point_2;
        private PointF m_point_3;


        const string ATTR_NAME_ARC_3_P = "arc_3p";

        const string ATTR_POINT_1 = "p_1";
        const string ATTR_POINT_2 = "p_2";
        const string ATTR_POINT_3 = "p_3";

    }
}
