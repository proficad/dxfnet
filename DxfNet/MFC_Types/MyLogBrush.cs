using System;
using System.Drawing;

namespace DxfNet.MFC_Types
{

    public enum ObjPropsHatch
    {
        HS_HORIZONTAL,
        HS_VERTICAL,
        HS_FDIAGONAL,
        HS_BDIAGONAL,
        HS_CROSS,
        HS_DIAGCROSS
    }

	/// <summary>
	/// Summary description for MyLogBrush.
	/// </summary>
	public struct MyLogBrush
	{
		public int m_style;
		public Color m_color;
        public ObjPropsHatch m_hatch;
	}
}
