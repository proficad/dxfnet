using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace DxfNet
{
    public class EFont
    {
        public int m_size;
        public bool m_bold;
        public bool m_ital;
        public bool m_under;
        public string m_faceName;
        public Color m_color;

        public EFont()
        {
            m_size = 65; m_faceName = "Arial"; m_color = Color.Black;
        }

        public static EFont StringToEfont(string as_input)
        {
            EFont l_efont = new EFont();

            if (string.IsNullOrWhiteSpace(as_input))
            {
                return l_efont;
            }

            string[] l_array    = as_input.Split(new char[] { ',' });
            l_efont.m_size      = int.Parse(l_array[0]);
            l_efont.m_bold      = l_array[1].Equals("1");
            l_efont.m_ital      = l_array[2].Equals("1");
            l_efont.m_under     = l_array[3].Equals("1");
            l_efont.m_faceName  = l_array[4];
            string ls_fontColor = l_array[5];
            l_efont.m_color     =  ColorTranslator.FromHtml("#" + ls_fontColor);
            

            return l_efont;
        }

        public override string ToString()
        {
            return string.Format("{0},{1},{2},{3},{4},{5}",
                m_size,
                m_bold ? 1 : 0,
                m_ital ? 1 : 0,
                m_under ? 1 : 0,
                m_faceName,
                Helper.Color2String(m_color)
            );
        }
    }

    
}
