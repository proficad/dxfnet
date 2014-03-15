using System;

namespace DxfNet
{
    public enum QTextAlignment
    {
        AL_LT = 1,
        AL_LM = 2,
        AL_LB = 3,
        AL_MT = 8,
        AL_MM = 0, // old wrong center, drawn with DT_LEFT
        AL_MB = 4,
        AL_RT = 7,
        AL_RM = 6,
        AL_RB = 5,
        AL_CENTER = 9 // real center after the fix in 6.6
    }


}