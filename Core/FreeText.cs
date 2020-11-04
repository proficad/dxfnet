using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Text.RegularExpressions;

namespace DxfNet
{
    public class FreeText : DrawObj
    {

        public EFont m_efont;
        
        public bool m_isInTb;
        bool m_bTurnWithSymbol = false;
        public QTextAlignment m_alignment;
        public int m_angle;

        public FreeText(string as_text, EFont a_efont, Rectangle a_rect, int ai_angle) : base(Shape.text, a_rect)
        {
            m_text = as_text;
            m_efont = a_efont;
            m_angle = ai_angle;
            m_isInTb = false;
            m_alignment = QTextAlignment.AL_MM;
        }

        public void SetTb()
        {
            m_isInTb = true;
        }

        internal override void Write2Xml(System.Xml.XmlWriter a_xmlWriter)
        { 
           	//saving INDEPENDANT texts only!!!!

            //HACK
	        int li_druhpisma = 0;
	

            Point l_pivot2Save = GetPivotRotated();
	        string ls_currentFont = m_efont.ToString();

            SaveTextToXml(a_xmlWriter, li_druhpisma, m_text, l_pivot2Save, m_angle, ls_currentFont, m_bTurnWithSymbol);

        }

        private Point GetPivotRotated()
        {
            //HACK
            return GetCenterPoint();
        }

        private void SaveTextToXml(System.Xml.XmlWriter a_xmlWriter, int li_druhpisma, string as_text, Point l_pivot2Save, int ai_angle, string as_currentFont, bool a_bTurnWithSymbol)
        {
            // https://stackoverflow.com/questions/397250/unicode-regex-invalid-xml-characters/961504#961504

            // filters control characters but allows only properly-formed surrogate sequences
            Regex _invalidXMLChars = new Regex(
                @"(?<![\uD800-\uDBFF])[\uDC00-\uDFFF]|[\uD800-\uDBFF](?![\uDC00-\uDFFF])|[\x00-\x08\x0B\x0C\x0E-\x1F\x7F-\x9F\uFEFF\uFFFE\uFFFF]",
                RegexOptions.Compiled);

            /// <summary>
            /// removes any unusual unicode characters that can't be encoded into XML
            /// </summary>
            string RemoveInvalidXMLChars(string text)
            {
                if (string.IsNullOrEmpty(text)) return "";
                return _invalidXMLChars.Replace(text, "");
            }

            string ls_stringEncoded = Helper.EncodeHtml(as_text);

            ls_stringEncoded = RemoveInvalidXMLChars(ls_stringEncoded);


//            a_xmlWriter.WriteStartElement("text");
//            a_xmlWriter.WriteElementString("text", ls_stringEncoded);
            a_xmlWriter.WriteStartElement("text");
//            
	
	//	const int li_posXtoSave = (AL_LM == GetZarovnani()) ? m_position.left : l_center.x;
	
//	TRACE(_T("saving text \"%s\" with pivot %d %d\n"), as_text, a_pos.x, a_pos.y);

            a_xmlWriter.WriteAttributeString("x", l_pivot2Save.X.ToString());

            a_xmlWriter.WriteAttributeString("y", l_pivot2Save.Y.ToString());

	
	        if (ai_angle != 0)
	        {
                a_xmlWriter.WriteAttributeString("a", ai_angle.ToString());
	        }

            a_xmlWriter.WriteAttributeString("f", as_currentFont);
	
	
	        //if (ai_druhPisma == NAPISY)
	        //{
            a_xmlWriter.WriteAttributeString("l", "1");
	        //}
	
            int li_alignment = (int)m_alignment;
            if(li_alignment != 0)
            {
                a_xmlWriter.WriteAttributeString("al", li_alignment.ToString());
            }

            a_xmlWriter.WriteString(ls_stringEncoded);
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

        internal override void RecalcBounds(ref MyRect l_bounds) 
        { }

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
