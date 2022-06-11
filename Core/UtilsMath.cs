using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace DxfNet
{
    public static class UtilsMath
    {
        public enum cardinal_directions { cd_north, cd_east, cd_south, cd_west, cd_none };

        public static cardinal_directions GetDirection(PointF a_p1, PointF a_p2)
        {
            if (a_p1.Y == a_p2.Y)
            {
                if (a_p1.X > a_p2.X)
                {
                    return cardinal_directions.cd_west;
                }
                else
                {
                    return cardinal_directions.cd_east;
                }
            }


            if (a_p1.X == a_p2.X)
            {
                if (a_p1.Y > a_p2.Y)
                {
                    return cardinal_directions.cd_north;
                }
                else
                {
                    return cardinal_directions.cd_south;
                }
            }

            return cardinal_directions.cd_none;
        }



    }
}
