using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DxfNet
{
    public class QIC : Insert
    {
        public const int INT_VYV = 40;

        public string m_name;		//1
        public int m_numberOfOutletsVer;	//2
        public int m_numberOfOutletsHor;	//3
        public string m_desc;		//4
        public bool m_ver_left;	//5
        public bool m_ver_right;
        //	bool m_void;
        public string m_pos_hor;	//6
        public string m_out_d;	//7
        public string m_out_n;	//8
        public string m_out_n_inv;//9
        public bool m_mark;

        public QIC(int ai_x, int ai_y, float af_scaleX, float af_scaleY)
            : base(Shape.soucastka, ai_x, ai_y, af_scaleX, af_scaleY)
        { 

        }
        //---------------------------
    }
}
