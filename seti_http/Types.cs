using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seti_http
{
    class Types
    {
        private string _type;
        private long _size;
        public string type
        {
            get { return _type; }
            set { _type = value; }
        }
        public long size
        {
            get { return _size; }
            set { _size = value; }
        }
        public Types(string type, long size)
        {
            _type = type;
            _size = size;
        }
    }
}
