﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DxfNet
{
    public class SettingsPage
    {
        public int PagesHor { get; set; }
        public int PagesVer { get; set; }
        public bool DrawFrame { get; set; }
        public MyRect PageMargins { get; set; }
        public bool IncludeMargins { get; set; }
    }
}
