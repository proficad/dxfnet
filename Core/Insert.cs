using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Core;

namespace DxfNet
{
    public class Insert : DrawObj
    {
        public class Satelite 
        {
            public Satelite(string as_name, string as_value, int ai_x, int ai_y, bool ab_visible, int ai_turns)
            {
                m_name = as_name;
                m_value = as_value;
                m_x = ai_x;
                m_y = ai_y;
                m_visible = ab_visible;
                m_turns = ai_turns;
            }
            public string m_name;
            public string m_value;
            public int m_x;
            public int m_y;
            public bool m_visible;
            public int m_turns;
            public QTextAlignment m_alignment;
            


            internal void Write2Xml(System.Xml.XmlWriter a_xmlWriter)
            {
                a_xmlWriter.WriteStartElement("att");
                a_xmlWriter.WriteAttributeString("name", m_name);
                a_xmlWriter.WriteAttributeString("value", m_value);
                a_xmlWriter.WriteAttributeString("x", m_x.ToString());
                a_xmlWriter.WriteAttributeString("y", m_y.ToString());
                a_xmlWriter.WriteAttributeString("v", "1");
                a_xmlWriter.WriteAttributeString("t", m_turns.ToString());
                a_xmlWriter.WriteEndElement();
            }
        }

        private PpdDoc m_ppdDoc;
        public string m_lG;
        public bool m_hor;//horizontal
        public bool m_ver;//vertical
        public int m_angle;
        public Color m_color_border;
        public Color m_color_fill;
        public float m_scaleX;
        public float m_scaleY;
        public List<Satelite> m_satelites = new List<Satelite>();

        public System.Collections.Hashtable m_parameters = new System.Collections.Hashtable(2);


        public Insert(Shape a_shape, double a_x, double a_y, float af_scaleX, float af_scaleY) : base(a_shape)
        {
            m_position.X = (float)a_x;
            m_position.Y = (float)a_y;

            m_scaleX = af_scaleX;
            m_scaleY = af_scaleY;
        }

        public void SetPpdDoc(PpdDoc a_ppdDoc) 
        {
            m_ppdDoc = a_ppdDoc; 
        }


        internal override void Write2Xml(System.Xml.XmlWriter a_xmlWriter)
        {
            //need to offset by the PPD's offset


            a_xmlWriter.WriteStartElement("elem");
            a_xmlWriter.WriteAttributeString("lG", m_lG);
            a_xmlWriter.WriteAttributeString("x", Helper.GetRectCenterPoint(m_position).X.ToString());
            a_xmlWriter.WriteAttributeString("y", Helper.GetRectCenterPoint(m_position).Y.ToString());
            if (m_hor)
            {
                a_xmlWriter.WriteAttributeString("h", "1");
            }
            if (m_ver)
            {
                a_xmlWriter.WriteAttributeString("v", "1");
            }
            if (m_angle != 0)
            {
                a_xmlWriter.WriteAttributeString("a", m_angle.ToString());
            }
            if ((m_scaleX != 1f) && (m_scaleX > 0))
            {
                a_xmlWriter.WriteAttributeString("sX", m_scaleX.ToString());
            }
            if ((m_scaleY != 1f) && (m_scaleY > 0))
            {
                a_xmlWriter.WriteAttributeString("sY", m_scaleY.ToString());
            }

            if (m_satelites.Count > 0)
            {
                WriteAttributes(a_xmlWriter);
            }

            a_xmlWriter.WriteEndElement();
        }

        private void WriteAttributes(System.Xml.XmlWriter a_xmlWriter)
        {
            a_xmlWriter.WriteStartElement("atts");
            foreach(Insert.Satelite l_sat in m_satelites)
            {
                l_sat.Write2Xml(a_xmlWriter);
            }
            a_xmlWriter.WriteEndElement();
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

        internal override void RecalcBounds(ref MyRect l_bounds) { }

        internal override void MoveBy(SizeF l_offset)
        {
            m_position.X += l_offset.Width;
            m_position.Y += l_offset.Height;
        }

        public void Offset(SizeF l_offset)
        {
            m_position.X += l_offset.Width;
            m_position.Y += l_offset.Height;
        }

        public override void RecalcPosition()
        {
            Point l_centerPoint = Helper.GetRectCenterPoint(m_position);
            /*
            if (m_lG == "L51")
            {
                int ddd = 4;
            }
            */
            if (m_ppdDoc != null)
            {

                RectangleF l_ppdRect = m_ppdDoc.GetPosition();
                l_ppdRect.X += l_centerPoint.X;
                l_ppdRect.Y += l_centerPoint.Y;

                m_position = l_ppdRect;
            }
        }


   




        internal PointF RecalculatePoint(PointF a_vyvod)
        {
            
            
            if ((0 == m_angle)&&(false == m_ver)&&(false == m_hor)&&(m_scaleX==1)&&(m_scaleY==1))
	        {
		        return a_vyvod;
	        }


	        PositionAspect l_positionAspect = new PositionAspect();
	        l_positionAspect.m_pivot = Helper.GetRectCenterPoint(m_position);
	        l_positionAspect.m_angle = m_angle;
	        l_positionAspect.m_horizontal = m_hor;
	        l_positionAspect.m_vertical = m_ver;
	        l_positionAspect.ScaleX = m_scaleX;
	        l_positionAspect.ScaleY = m_scaleY;

	        QPivot pivot = new QPivot(l_positionAspect);
	        PointF	vysledek = pivot.PrevodBodu(a_vyvod);

        	return vysledek;	


        }

        internal override void Recalc_Size(float af_x, float af_y)
        {
            m_position.X = (int)Math.Round(m_position.X * af_x);
            m_position.Y = (int)Math.Round(m_position.Y * af_y);

            m_position.Width = (int)Math.Round(m_position.Width * af_x);
            m_position.Height = (int)Math.Round(m_position.Height * af_y);
        }
    }
}
