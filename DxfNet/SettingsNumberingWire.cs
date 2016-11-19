using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;


namespace DxfNet
{
    public class SettingsNumberingWire
    {
        const string ElementName = "NumberingWire";
        public const string Attr_Enabled = "Enabled";
        public const string Attr_Prefill = "Prefill";
        public const string Attr_Digits = "Digits";
        public const string Attr_TypeRenumbering = "TypeRenumbering";
        public const string Attr_ShowWireName = "ShowWireName";
        public const string Attr_WireLabelDist_a = "WireLabelDist_a";
        public const string Attr_WireLabelDist_b = "WireLabelDist_b";
        public const string Attr_WireLabelDist_c = "WireLabelDist_c";
        public const string Attr_Vertically = "Vertically";
        public const string Attr_LongWireLength = "LongWireLength";


        public bool Enabled     { get; set; }
        public bool Vertically  { get; set; }

        public enum EnumShowWireNumbers { swn_no, swn_just_free, swn_both };
        public EnumShowWireNumbers ShowWireNumbers { get; set; }

        public int WireLabelDist_A { get; set; }
        public int WireLabelDist_B { get; set; }
        public int WireLabelDist_C { get; set; }

        public int Long_Wire_Len { get; set; }


    }

}
