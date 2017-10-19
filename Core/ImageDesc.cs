using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DxfNet
{
    public class QImageDesc
    {
        public string LastGuid { get; set; }
        public int ImgType { get; set; }
        public string ImgEncoded { get; set; }


        public QImageDesc(string as_lastGuid, int ai_type, string as_imgEncoded)
        {
            LastGuid = as_lastGuid;
            ImgType = ai_type;
            ImgEncoded = as_imgEncoded;
        }



    }
}
