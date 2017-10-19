using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DxfNet
{
    public enum GateShapeType { gst_and = 0, gst_nand, gst_or, gst_nor, gst_bud, gst_inv, gst_exor, gst_exnor };

    public class QGate : Insert
    {
        public bool m_ASA;//zda je podle normy ASA, jinak CSN
        public bool m_stesnat;//zda budou stěsnány vývody, jinak rozšířit
        public int m_pocetvstupu;
        public bool m_c1;//invertovat 1 vývod?
        public bool m_c2;//invertovat 1 vývod?
        public bool m_c3;//invertovat 1 vývod?
        public bool m_c4;//invertovat 1 vývod?
        public bool m_c5;//invertovat 1 vývod?
        public bool m_c6;//invertovat 1 vývod?
        public bool m_c7;//invertovat 1 vývod?
        public bool m_c8;//invertovat 1 vývod?
        public GateShapeType m_tvar;

        public QGate(int ai_x, int ai_y, float af_scaleX, float af_scaleY) 
            :base(Shape.soucastka, ai_x, ai_y, af_scaleX, af_scaleY)
        { 

        }
    }
}
