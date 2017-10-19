using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DxfNet
{
    public enum trafoCoreType { tct_none = 1, tct_solid, tct_double, tct_dash };
    
    public class Trafo : Insert
    {
        public const int VYSKA_OBLOUKU = 20; 
        public trafoCoreType m_jadro;
        public TrafoWinding m_pri = new TrafoWinding();
        public TrafoWinding m_sec = new TrafoWinding();
        public int m_stairjadro;
        public int m_vyskajadra;


        public Trafo(int ai_x, int ai_y, float af_scaleX, float af_scaleY)
            : base(Shape.soucastka, ai_x, ai_y, af_scaleX, af_scaleY)
        {
            m_position.X = ai_x;
            m_position.Y = ai_y;
        }

        public void CalculatePosition()
        {
            m_pri.CalculateWindingHeight();
            m_sec.CalculateWindingHeight();


            //JADRO
            m_vyskajadra = Math.Max((m_pri.WindingHeight), (m_sec.WindingHeight));


            int li_obloukuPrimar      = m_pri.CalculateStair();
            int li_obloukuSecundar    = m_sec.CalculateStair();



            if (li_obloukuPrimar > li_obloukuSecundar)
                m_stairjadro = m_pri.Stair;
            else
                m_stairjadro = m_sec.Stair;

        }
        //--------------------------------
    }
}
