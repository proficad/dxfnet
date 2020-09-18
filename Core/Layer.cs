using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace DxfNet
{
    public class Layer : IEnumerable
    {
        public Layer(string as_name)
        {
            Name = as_name;
           
        }


        public IEnumerator GetEnumerator()
        { 
            foreach(DrawObj l_item in m_objects)
            {
                yield return l_item;
            }
        }


        public string Name {get; set;}

        public List<DrawObj> m_objects = new List<DrawObj>();


        internal void GetWires(List<Wire> a_list_of_wires)
        {
            throw new NotImplementedException();
        }

        public void Add(DrawObj a_drawObj)
        {
            m_objects.Add(a_drawObj);
        }

        public void SaveToXml(XmlWriter a_xmlWriter)
        {
            a_xmlWriter.WriteStartElement("layer");
            a_xmlWriter.WriteAttributeString("name", Name);
            a_xmlWriter.WriteAttributeString("v", "1");//visible

            foreach (DrawObj a_obj in m_objects)
            {
                a_obj.Write2Xml(a_xmlWriter);
            }

            a_xmlWriter.WriteEndElement();

        }

    }
}
