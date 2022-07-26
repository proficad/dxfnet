using System;
using System.Drawing;

namespace DxfNet.MFC_Types
{
	/// <summary>
	/// replaces MFC LOGPEN
	/// </summary>
	public struct MyLogPen
	{
		public int m_style;
		public double m_width;
		public Color m_color;
	}
}
