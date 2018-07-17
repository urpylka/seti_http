using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seti_http
{
    class links
    {
        private string _link;
        private bool _visited;
        private string _bUri;
        public string link
        {
            get { return _link; }
            set { _link = value; }
        }
        public bool visited
        {
            get { return _visited; }
            set { _visited = value; }
        }
        public string bUri
        {
            get { return _bUri; }
            set { _bUri = value; }
        }
        public links(string link, string bUri, bool visited)
        {
            _link = link;
            _visited = visited;
            _bUri = bUri;
        }
    }
}
