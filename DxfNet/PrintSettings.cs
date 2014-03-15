using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DxfNet
{
    public class PrintSettings
    {
        public int SheetSizeX { get; set; }
        public int SheetSizeY { get; set; }
        public bool WantsCustom { get; set; }
        public int CustomSizeX { get; set; }
        public int CustomSizeY { get; set; }
    }
}
