using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DxfNet;

namespace Loader
{
    class Program
    {
        static void Main(string[] args)
        {
            string ls_path = @"C:\temp\sch\vstupní díl přijímače.sxe";
            ls_path = @"C:\temp\sch\trafo.sxe";
            ls_path = @"C:\Documents and Settings\Vasek\Dokumenty\schémata\blokové schema.SXE";
            ls_path = @"C:\Documents and Settings\Vasek\Dokumenty\schémata\pokusy\1.SXE";
            ls_path = @"C:\temp\sch\test.sxe";
            CollPages l_collPages = Loader.Load(ls_path);


            //99 zmenit DrawDocDumper.Dump(l_collPages);

            



            //string ls_pathDxf = @"c:\temp\ProfiCAD1.dxf";
            //Exporter.Export(l_collPages, ls_pathDxf);
        }
    }
}
