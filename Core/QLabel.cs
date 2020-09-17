using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace DxfNet
{
    public class QLabel
    {
        public Point Center;
        public string Text;
        public int AngleTenths;


        internal void Write2Xml(System.Xml.XmlWriter a_xmlWriter)
        {
            a_xmlWriter.WriteStartElement("label");

            Helper.Point2Attrib(a_xmlWriter, "pos_", Center);

            a_xmlWriter.WriteAttributeString("text", Text);


            a_xmlWriter.WriteEndElement();
        }

    }
}
