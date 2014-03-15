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
            m_logpen.m_color = Color.Black;
            m_logpen.m_style = 0;
            m_logpen.m_width = 2;

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
        //	BOOL		m_bHatch;
        public int m_hatchspacing;
        public int m_hatchpensize;
        public Size m_hatchoffset;
        public bool m_bBrush;

        public MFC_Types.MyLogPen m_logpen;
        public MFC_Types.MyLogBrush m_logbrush;
        public QLin m_lin;


        public void SaveToXml(System.Xml.XmlWriter a_xmlWriter)
        { 
        }

        public void SaveLineToXml(System.Xml.XmlWriter a_xmlWriter)
        { 
        }

    }
}
