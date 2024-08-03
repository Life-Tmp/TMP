namespace TMPApplication.Interfaces
{
    public interface ISearchService<TEntity> where TEntity : class
    {
        Task IndexDocumentAsync(TEntity document, string indexName);
        Task<List<TEntity>> SearchDocumentAsync(string searchTerm, string indexName);
        Task DeleteDocumentAsync(string id, string indexName);
    }
}
