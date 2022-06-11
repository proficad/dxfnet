using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace DxfNet
{
    class Attribute
    {
        string Tag { get; set; }
        string Value { get; set; }
        PointF AlignmentPoint1 { get; set; }
    }
}
