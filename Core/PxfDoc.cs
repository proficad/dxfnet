using DxfNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Drawing;


namespace Core
{
    /// <summary>
    /// Format similar to DXF that is insertable into ProfiCAD 
    /// </summary>
    public class PxfDoc : DrawDoc
    {
        public override void Save(string as_path)
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
            a_xmlWriter.WriteStartElement("layers");

            foreach (Layer l_layer in m_layers)
            {
                l_layer.SaveToXml(a_xmlWriter);
            }

            a_xmlWriter.WriteEndElement();
        }

        private void WriteIntroElement(XmlWriter a_xmlWriter)
        {
            a_xmlWriter.WriteStartElement("document");
            a_xmlWriter.WriteAttributeString("type", "ProfiCAD pxf");
            a_xmlWriter.WriteAttributeString("version", "12.0");
        }

        public Repo GetRepo()
        {
            return m_repo;
        }


        public override void SetSize(Size a_size)
        {

        }



    }
}
