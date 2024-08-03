using Elasticsearch.Net;
using Nest;
using TMPApplication.Interfaces;

namespace TMPInfrastructure.Implementations
{
    public class SearchService<TEntity> : ISearchService<TEntity> where TEntity : class
    {
        private readonly IElasticClient _elasticClient;

        public SearchService(IElasticClient elasticClient)
        {
            _elasticClient = elasticClient;
        }

        public async Task IndexDocumentAsync(TEntity document, string indexName)
        {
            var response = await _elasticClient.IndexAsync(document, idx => idx
                .Index(indexName)
                .Id((document as dynamic)?.Id.ToString())
                .Refresh(Refresh.WaitFor)
            );

            if (!response.IsValid)
            {
                throw new Exception($"Failed to index document: {response.ServerError?.Error?.Reason}");
            }
        }

        public async Task<List<TEntity>> SearchDocumentAsync(string searchTerm, string indexName)
        {
            var searchResponse = await _elasticClient.SearchAsync<TEntity>(s => s
                .Index(indexName)
                .Query(q => q
                    .Bool(b => b
                        .Should(
                            sh => sh
                                .MultiMatch(mm => mm
                                    .Fields(f => f
                                        .Field("title")
                                        .Field("name")
                                        .Field("description")
                                    )
                                    .Query(searchTerm)
                                    .Fuzziness(Fuzziness.Auto)
                                ),
                            sh => sh
                                .Prefix(p => p
                                    .Field("title")
                                    .Value(searchTerm.ToLower())
                                ),
                            sh => sh
                                .Prefix(p => p
                                    .Field("name")
                                    .Value(searchTerm.ToLower())
                                ),
                            sh => sh
                                .Prefix(p => p
                                    .Field("description")
                                    .Value(searchTerm.ToLower())
                                ),
                            sh => sh
                                .Wildcard(w => w
                                    .Field("title")
                                    .Value($"*{searchTerm.ToLower()}*")
                                ),
                            sh => sh
                                .Wildcard(w => w
                                    .Field("name")
                                    .Value($"*{searchTerm.ToLower()}*")
                                ),
                            sh => sh
                                .Wildcard(w => w
                                    .Field("description")
                                    .Value($"*{searchTerm.ToLower()}*")
                                )
                        )
                    )
                )
                .Size(1000)
            );

            if (!searchResponse.IsValid)
            {
                throw new Exception($"Failed to search documents: {searchResponse.ServerError?.Error?.Reason}");
            }

            return searchResponse.Documents.ToList();
        }

        public async Task DeleteDocumentAsync(string id, string indexName)
        {
            var response = await _elasticClient.DeleteAsync<TEntity>(id, d => d
                .Index(indexName)
            );

            if (!response.IsValid)
            {
                throw new Exception($"Failed to delete document: {response.ServerError?.Error?.Reason}");
            }
        }
    }
}
