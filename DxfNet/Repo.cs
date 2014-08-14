using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DxfNet
{
    public class Repo
    {
        public List<PpdDoc>     m_listPpd       = new List<PpdDoc>();
        public List<QImageDesc> m_listImgDesc   = new List<QImageDesc>();
        public List<PtbDoc>     m_listPtb       = new List<PtbDoc>();




        internal PpdDoc FindPpdDocInRepo(string ls_lastGuid)
        {
            foreach(PpdDoc l_doc in m_listPpd)
            {
                if(l_doc.m_lG == ls_lastGuid)
                {
                    return l_doc;
                }
            }

            return null;
            
        }

        public PtbDoc GetPtb(string as_name)
        {
            foreach (PtbDoc l_doc in m_listPtb)
            {
                if (l_doc.Path == as_name)
                {
                    return l_doc;
                }
            }

            return null;
        }

        public void AddPpd(PpdDoc a_ppdDoc)
        {
            m_listPpd.Add(a_ppdDoc);
        }

        public void AddImgDesc(QImageDesc a_imgDesc)
        {
            m_listImgDesc.Add(a_imgDesc);
        }

        public void AddTb(PtbDoc a_ptbDoc)
        {
            m_listPtb.Add(a_ptbDoc);
        }


    }
}
