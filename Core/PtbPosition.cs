using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core;

namespace DxfNet
{
    public class PtbPosition
    {
        public int m_horDist;
        public int m_verDist;
        public bool m_turn;
        public bool m_useTb;
        public PtbDoc m_pPtb;

        public string Path { get; set; }
    }
}
