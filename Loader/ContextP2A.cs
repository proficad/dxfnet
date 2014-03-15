using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DxfNet;

namespace Loader
{
    public class ContextP2A
    {
        public static ContextP2A Current = new ContextP2A();

        public Layer CurrentLayer;
    }
}
