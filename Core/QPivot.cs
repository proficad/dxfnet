using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace DxfNet
{
    public class QPivot
    {
        public PointF m_pivot;//axis of the object
        public int m_angle;
        public bool m_horizontal;
        public bool m_vertical;

        public double ScaleX { get; set; }
        public double ScaleY { get; set; }

        public QPivot(PointF a_pivot, int a_angle, bool ab_horizontal, bool ab_vertical)
        {
            m_pivot = a_pivot;
            m_angle = a_angle;
            m_horizontal = ab_horizontal;
            m_vertical = ab_vertical;
        }

        public QPivot(PositionAspect a_aspect)
        {
            m_pivot = a_aspect.m_pivot;
            m_angle = a_aspect.m_angle;
            m_horizontal = a_aspect.m_horizontal;
            m_vertical = a_aspect.m_vertical;
            ScaleX = a_aspect.ScaleX;
            ScaleY = a_aspect.ScaleY;

        }

        public PointF PrevodBodu (int x, int y)
        {
	        return PrevodBodu(new PointF(x,y));
        }

        public PointF PrevodBodu (PointF vstup)
        {
	        PointF	vysledek = vstup;

            if ((m_angle == 0) && (m_vertical == false) && (m_horizontal == false) && (ScaleX == 1) && (ScaleY == 1))
            {
		        return vstup;
	        }

	        //zohlednit polohu bodu vůči středu součástky...
	        double X = m_pivot.X; 
	        double Y = m_pivot.Y; 
	        double x = vstup.X - X; 
	        double y = vstup.Y - Y;


            x = x * ScaleX;
            y = y * ScaleY;


	        // a natočení
	        if (m_vertical) x = -x;
	        if (m_horizontal) y = -y;//máme MM_TEXT

	        //////
	        switch(m_angle)
            {
	        case 0: 
		        vysledek.X = (float)(X + x);
		        vysledek.Y = (float)(Y + y);
		        break;
	        case 900: 
		        vysledek.X = (float)(X + y);
		        vysledek.Y = (float)(Y - x);
		        break;
	        case 1800: 
		        vysledek.X = (float)(X - x);
		        vysledek.Y = (float)(Y - y);
		        break;
	        case -900: 
		        vysledek.X = (float)(X - y);
		        vysledek.Y = (float)(Y + x);
		        break;
	        default: {
		        double pi;
		        pi = 3.1415927;
		        y = -y;
		        double fi = Math.Atan2((double)y, (double)x);
		        double z = (double)((y*y)+(x*x)); z = Math.Sqrt(z);
		        //
		        x = (int) (z * Math.Cos(fi + (pi * 0.25 * m_angle / 450)));
		        y = -(int)(z * Math.Sin(fi + (pi * 0.25 * m_angle / 450)));
		        // update the size
		        //
		        x += X; y += Y ;
		        vysledek.X = (float)x;
		        vysledek.Y = (float)y;
		        }//defa
                break;
	        }//switch


	        return vysledek;	
        }
    }
}
