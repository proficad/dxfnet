using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WW.Cad.Model;
using WW.Cad.Model.Entities;
using WW.Cad.Model.Tables;
using DxfNet;

namespace Loader
{
    class ExportContext
    {
   

        public static void Setup(DxfModel a_model, PCadDoc a_doc)
        {
            
            Current.m_model = a_model;
            //Current.m_entityCollection = a_model.Entities;
            Current.m_blockCollection = a_model.Blocks;
            Current.m_pcadDoc = a_doc;
        }

        private static ExportContext _current;
        public static ExportContext Current
        {
            get
            {
                if(_current == null)
                {
                    _current = new ExportContext();
                }
                return _current;
            }
        }
   
        private DxfBlockCollection m_blockCollection;
        public DxfBlockCollection BlockCollection
        {
            get 
            {
                return m_blockCollection;
            }
        }

        private DxfModel m_model;
        public DxfModel Model
        {
            get
            {
                return m_model;
            }
        }
        private PCadDoc m_pcadDoc;
        public PCadDoc PCadDocument
        {
            get
            {
                return m_pcadDoc;
            }
        }

        public DxfLayer Layer;

        private bool _black_by_layer;
        public bool BlackByLayer
        {
            get { return _black_by_layer; }
             set
            {
                 Current._black_by_layer = value;
            }
        }
        
        //----------------------------------
    }
}
