using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuceneSearchEngine.Indexing.Metadata;
namespace LuceneSearchEngine.Indexing.Entity
{
    public interface IBaseLuceneEntity
    {
        [LuceneField]
        string EntityId { get; set; }
        [LuceneField]
        double Phi { get; set; }
        [LuceneField]
        double Lamda { get; set; }
        double Distance { get; set; }
        ILuceneCustomMapper Mapper { get; set; }
    }
}
