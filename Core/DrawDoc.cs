using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Xml;

namespace DxfNet
{
    public class DrawDoc
    {

        public List<DrawObj> m_objects = new List<DrawObj>();
  


        public void Add(DrawObj a_drawObj, Layer a_layer)
        {
            a_drawObj.m_layer = a_layer;
            m_objects.Add(a_drawObj);
        }

        internal void WriteElements(XmlWriter a_xmlWriter)
        {
            a_xmlWriter.WriteStartElement("layers");
            a_xmlWriter.WriteStartElement("layer");
            a_xmlWriter.WriteAttributeString("name", "0");

            foreach (DrawObj a_obj in m_objects)
            {
                a_obj.Write2Xml(a_xmlWriter);
            }

            a_xmlWriter.WriteEndElement();
            a_xmlWriter.WriteEndElement();
        }

        public Rectangle GetUsedRect()
        { 
	        Rectangle l_usedrect = new Rectangle();
            Rectangle l_position = new Rectangle();
	        bool	  lb_first = true;

	        foreach(DrawObj l_obj in m_objects)
	        {
                l_obj.RecalcPosition();//2012-11-05
                l_position = l_obj.m_position;
		        //l_position.NormalizeRect();

		        if(lb_first){
			        l_usedrect = l_position;
			        lb_first = false;
		        }
		        else{
			        l_usedrect = Rectangle.Union(l_usedrect, l_position);
			        //l_usedrect.NormalizeRect();
		        }
	        }

	        if (lb_first)
		        return new Rectangle();
	        else
	        {
		        return l_usedrect;
	        }
        }

    }
}
