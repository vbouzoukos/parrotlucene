using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuceneSearchEngine.Error
{
    public class NoIndexException:Exception
    {
        public NoIndexException(string indexName,string index):base(string.Format("The Index {0} was not founded at path {1}", indexName, index))
        {
        }
    }
}
