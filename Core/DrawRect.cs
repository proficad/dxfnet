using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace DxfNet
{
    public class DrawRect : DrawObj
    {
        public DrawRect(Shape a_shape, Rectangle a_rect) : base(a_shape, a_rect)
        {
            m_arrow_flipped = false;
        }
        
        public EFont m_efont;
        public int m_rX, m_rY;
        public Size m_arcBegin;	// starting point of arc, pie and chord
        public Size m_arcEnd;	// ending point of arc, pie and chord

        public bool m_arrow_flipped;
        public int m_text_angle;
        internal override void Write2Xml(System.Xml.XmlWriter a_xmlWriter)
        {

            switch (m_nShape)
            {
                case Shape.ellipse:
                    a_xmlWriter.WriteStartElement("ellipse");
                    break;
                case Shape.rectangle:
                    a_xmlWriter.WriteStartElement("rect");
                    break;
                case Shape.arc:
                    a_xmlWriter.WriteStartElement("arc");
                    break;
                case Shape.pie:
                    a_xmlWriter.WriteStartElement("pie");
                    break;
                default:
                    throw new Exception("invalid shape");
            }

            a_xmlWriter.WriteAttributeString("left", m_position.Left.ToString());
            a_xmlWriter.WriteAttributeString("top", m_position.Top.ToString());
            a_xmlWriter.WriteAttributeString("right", m_position.Right.ToString());
            a_xmlWriter.WriteAttributeString("bottom", m_position.Bottom.ToString());

            if (m_nShape == Shape.arc)
            {
                a_xmlWriter.WriteAttributeString("bX", m_arcBegin.Width.ToString());
                a_xmlWriter.WriteAttributeString("bY", m_arcBegin.Height.ToString());
                a_xmlWriter.WriteAttributeString("eX", m_arcEnd.Width.ToString());
                a_xmlWriter.WriteAttributeString("eY", m_arcEnd.Height.ToString());

            }

            a_xmlWriter.WriteEndElement();
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


    }
}
