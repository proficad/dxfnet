using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Specialized;
using Core;
using DxfNet;

namespace Loader
{
    class DrawDocDumper
    {

        //--------------------------------------------
        public static void Dump(PCadDoc a_doc)
        {
            StringCollection list = new StringCollection();
            list.Add(string.Format("============================\ndumping {0}", a_doc.Parent.m_path));
            list.Add(string.Format("pages hor {0}", a_doc.Parent.m_settingsPage.PagesHor));
            list.Add(string.Format("pages ver {0}", a_doc.Parent.m_settingsPage.PagesVer));

            list.Add(string.Format("\t Repo: \n"));
            foreach (PpdDoc ppdDoc in a_doc.Parent.m_repo.m_listPpd)
            {
                list.Add(string.Format("ppd name={0}\n\t\tlG={1}", ppdDoc.m_name, ppdDoc.m_lG));
                DumpObjects(ppdDoc, list);
                
            }

            list.Add(string.Format("\n\t Elements: \n"));
            DumpObjects(a_doc, list);
            

            list.Add(string.Format("============================\n"));
            foreach (string ls_row in list)
            {
                System.Diagnostics.Trace.WriteLine(ls_row);
            }
        }

        //--------------------------------------------
        public static void DumpObjects(DrawDoc a_doc, StringCollection a_list)
        {
            foreach (DrawObj drawObj in a_doc)
            {
                if (drawObj is Insert)
                {
                    Insert i = (Insert)drawObj;
                    a_list.Add(string.Format("insert lg={0}", i.m_lG));
                }
                else
                {
                    a_list.Add(string.Format("drawObj shape={0}", drawObj.m_nShape));

                }
            }

        }
        //--------------------------------------------
    }
}
