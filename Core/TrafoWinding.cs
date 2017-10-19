using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DxfNet
{
    public class TrafoWinding
    {
        public int m_w1, m_w2, m_w3, m_w4, m_w5, m_w6;
        public int WindingHeight;
        public int Stair;
        public string m_name;

        private int ZjistiPocetVinuti()
        {
            if (m_w1 == 0) return 0;
            if (m_w2 == 0) return 1;
            if (m_w3 == 0) return 2;
            if (m_w4 == 0) return 3;
            if (m_w5 == 0) return 4;
            if (m_w6 == 0) return 5;
            return 6;
        }

        public void CalculateWindingHeight()
        {
            int li_numberOfWindings = ZjistiPocetVinuti();
            if (li_numberOfWindings > 0)
            {
                WindingHeight = (li_numberOfWindings - 1) * Trafo.VYSKA_OBLOUKU;//počet mezer mezi vinutími

                if(li_numberOfWindings >= 6) WindingHeight += (m_w6 * Trafo.VYSKA_OBLOUKU);
                if(li_numberOfWindings >= 5) WindingHeight += (m_w5 * Trafo.VYSKA_OBLOUKU);
                if(li_numberOfWindings >= 4) WindingHeight += (m_w4 * Trafo.VYSKA_OBLOUKU);
                if(li_numberOfWindings >= 3) WindingHeight += (m_w3 * Trafo.VYSKA_OBLOUKU);
                if(li_numberOfWindings >= 2) WindingHeight += (m_w2 * Trafo.VYSKA_OBLOUKU);
                if(li_numberOfWindings >= 1) WindingHeight += (m_w1 * Trafo.VYSKA_OBLOUKU);
            }
        }

        public int CalculateStair()
        {
            // zjisti pocet oblouku celkem
            int li_oblouku = 0;
            //primár
            if (m_w1 != 0)
            {
                li_oblouku += m_w1;
                if (m_w2 != 0)
                {
                    li_oblouku += 1;
                    li_oblouku += m_w2;
                    if (m_w3 != 0)
                    {
                        li_oblouku += 1;
                        li_oblouku += m_w3;
                        if (m_w4 != 0)
                        {
                            li_oblouku += 1;
                            li_oblouku += m_w4;
                            if (m_w5 != 0)
                            {
                                li_oblouku += 1;
                                li_oblouku += m_w5;
                                if (m_w6 != 0)
                                {
                                    li_oblouku += 1;
                                    li_oblouku += m_w6;
                                }
                            }
                        }
                    }
                }
            }
            if ((li_oblouku % 2) != 0)
                Stair = Trafo.VYSKA_OBLOUKU / 2;
            else
                Stair = 0;


            return li_oblouku;
        }

        //-------------------------
    }
}
