using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using DxfNet.MFC_Types;

namespace DxfNet
{


    public enum HatchType 
    { 
        NONE=-1,
        HT_HORIZONTAL=0,
        HT_VERTICAL=1,
        HT_FDIAGONAL=2,
        HT_BDIAGONAL=3,
        HT_CROSS=4,
        HT_DIAGCROSS=5
    }

    public class ObjProps
    {
        public ObjProps()
        {
            const int DEFAULT_LINE_THICKNESS = 0;


            m_logpen.m_color = Color.Black;
            m_logpen.m_style = 0;
            m_logpen.m_width = DEFAULT_LINE_THICKNESS;

            m_logbrush.m_color = Color.FromArgb(255, 255, 128);
            m_logbrush.m_style = 0;
            m_logbrush.m_hatch = ObjPropsHatch.HS_HORIZONTAL;

            m_bPen = true;
            m_hatchtype = HatchType.NONE;
            m_hatchspacing = 20;//2mm
            m_hatchpensize = 2;//0.2 mm
            m_hatchoffset.Width = 0;
            m_hatchoffset.Height = 0;
            m_bBrush = false;
        }
        public bool m_bPen;
        public HatchType m_hatchtype;
        public int m_hatchspacing;
        public int m_hatchpensize;
        public Size m_hatchoffset;
        public bool m_bBrush;

        public MFC_Types.MyLogPen m_logpen;
        public MFC_Types.MyLogBrush m_logbrush;
        public QLin m_lin;
        public struct ColorOnOff {public System.Drawing.Color Color; public bool IsOn;};
        public ColorOnOff m_contour2, m_insulation;

        public void SaveToXml(System.Xml.XmlWriter a_xmlWriter)
        {
            if (m_bPen)
            {
                SaveLineToXml(a_xmlWriter);
            }

            if (m_bBrush)
            {
                a_xmlWriter.WriteAttributeString("op-fd", "1");
                a_xmlWriter.WriteAttributeString("op-fc", Helper.RGB_2_Int(m_logbrush.m_color).ToString());
            }
        }

        public void SaveLineToXml(System.Xml.XmlWriter a_xmlWriter)
        {
            a_xmlWriter.WriteAttributeString("op-lc", Helper.RGB_2_Int(m_logpen.m_color).ToString());

      
            a_xmlWriter.WriteAttributeString("op-lw", m_logpen.m_width.ToString());
            

            if (!string.IsNullOrEmpty(m_lin.m_name)) //if line not solid
            {
                a_xmlWriter.WriteStartElement("op-lt");
                a_xmlWriter.WriteAttributeString("head", m_lin.m_name);
                a_xmlWriter.WriteAttributeString("body", m_lin.m_body);
                a_xmlWriter.WriteEndElement();
            }

        }






    }
}
