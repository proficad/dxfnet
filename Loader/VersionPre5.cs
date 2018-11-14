using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Loader
{
    [Serializable]
    public class VersionPre5 : Exception
    {
        public VersionPre5(string as_msg) : base(as_msg)
        {
        }
    }
}
