using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;


namespace Core
{
    public class QPaperSize
    {
        public QPaperSize(short ai_paper_size_enum, string as_form_name, Size a_size)
        {
            PaperSizeEnum = ai_paper_size_enum;
            FormName = as_form_name;
            SheetSize = a_size;
        }

        public short PaperSizeEnum;
        public string FormName;
        public Size SheetSize;// mm
    }
}
