using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using DxfNet;

namespace Core
{
    public class QDimStyle
    {
     
        const string ATTR_LINE_THICK = "op-lw";
        const string ATTR_LINE_COLOR = "op-lc";

        public int m_arrow_index; //0..normal, 1..floor plan
        public EFont m_label_font;

        public bool m_align_text_with_dim_line;
        public string m_name;

        public enum Text_Position { text_position_above, text_position_over, text_position_below };
        public Text_Position m_text_position;// 0..above, 1..over, 2..below

        public struct ColorThickness { public Color m_color; public int m_thickness; }
        public ColorThickness m_line_ext, m_line_dim;

        public enum Unit { unit_none = -1, unit_mm, unit_cm, unit_m, unit_km };
        public Unit m_unit;

    }
}
