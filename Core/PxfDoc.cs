using DxfNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;


namespace Core
{
    /// <summary>
    /// Format similar to DXF that is insertable into ProfiCAD 
    /// </summary>
    class PxfDoc : DrawDoc
    {
        public void Save(string as_path)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.Indent = true;

            XmlWriter l_xmlWriter = XmlWriter.Create(as_path, settings);

            l_xmlWriter.WriteStartDocument();

            WriteIntroElement(l_xmlWriter);
            WriteRepo(l_xmlWriter);


            WriteElements(l_xmlWriter);


            //close the Intro element
            l_xmlWriter.WriteEndElement();



            l_xmlWriter.WriteEndDocument();
            l_xmlWriter.Close();
        }

        internal new void WriteElements(XmlWriter a_xmlWriter)
        {
            a_xmlWriter.WriteStartElement("pages");
            a_xmlWriter.WriteStartElement("page");
            a_xmlWriter.WriteAttributeString("name", "1");



            a_xmlWriter.WriteStartElement("layers");

            foreach (Layer l_layer in m_layers)
            {
                l_layer.SaveToXml(a_xmlWriter);
            }

            a_xmlWriter.WriteEndElement();
            a_xmlWriter.WriteEndElement();
            a_xmlWriter.WriteEndElement();
        }

        private void WriteIntroElement(XmlWriter a_xmlWriter)
        {
            a_xmlWriter.WriteStartElement("document");
            a_xmlWriter.WriteAttributeString("type", "ProfiCAD pxf");
            a_xmlWriter.WriteAttributeString("version", "10.0");
        }

        private void WriteRepo(XmlWriter a_xmlWriter)
        {
            a_xmlWriter.WriteStartElement("repo");
            foreach (PpdDoc l_ppdDoc in GetRepo().m_listPpd)
            {
                l_ppdDoc.SaveToXml(a_xmlWriter);
            }
            a_xmlWriter.WriteEndElement();
        }


    }
}
