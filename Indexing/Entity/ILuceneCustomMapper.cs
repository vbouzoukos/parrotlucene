namespace LuceneSearchEngine.Indexing.Entity
{
    public interface ILuceneCustomMapper
    {
        string Save(string mainEntityID,object entity);
        object Load(string mainEntityID);
    }
}