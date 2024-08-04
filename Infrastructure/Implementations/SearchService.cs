using Elasticsearch.Net;
using Microsoft.Extensions.Logging;
using Nest;
using TMPApplication.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TMPInfrastructure.Implementations
{
    public class SearchService<TEntity> : ISearchService<TEntity> where TEntity : class
    {
        private readonly IElasticClient _elasticClient;
        private readonly ILogger<SearchService<TEntity>> _logger;

        public SearchService(IElasticClient elasticClient, ILogger<SearchService<TEntity>> logger)
        {
            _elasticClient = elasticClient;
            _logger = logger;
        }

        public async Task IndexDocumentAsync(TEntity document, string indexName)
        {
            _logger.LogInformation("Indexing document in index: {IndexName}", indexName);

            var response = await _elasticClient.IndexAsync(document, idx => idx
                .Index(indexName)
                .Id((document as dynamic)?.Id.ToString())
                .Refresh(Refresh.WaitFor)
            );

            if (!response.IsValid)
            {
                _logger.LogError("Failed to index document in index: {IndexName}. Reason: {Reason}", indexName, response.ServerError?.Error?.Reason);
                throw new Exception($"Failed to index document: {response.ServerError?.Error?.Reason}");
            }

            _logger.LogInformation("Document indexed successfully in index: {IndexName}", indexName);
        }

        public async Task<List<TEntity>> SearchDocumentAsync(string searchTerm, string indexName)
        {
            _logger.LogInformation("Searching documents in index: {IndexName} with search term: {SearchTerm}", indexName, searchTerm);

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
                _logger.LogError("Failed to search documents in index: {IndexName}. Reason: {Reason}", indexName, searchResponse.ServerError?.Error?.Reason);
                throw new Exception($"Failed to search documents: {searchResponse.ServerError?.Error?.Reason}");
            }

            _logger.LogInformation("Search completed successfully in index: {IndexName}. Found {Count} documents", indexName, searchResponse.Documents.Count);
            return searchResponse.Documents.ToList();
        }

        public async Task DeleteDocumentAsync(string id, string indexName)
        {
            _logger.LogInformation("Deleting document with ID: {DocumentId} from index: {IndexName}", id, indexName);

            var response = await _elasticClient.DeleteAsync<TEntity>(id, d => d
                .Index(indexName)
            );

            if (!response.IsValid)
            {
                _logger.LogError("Failed to delete document with ID: {DocumentId} from index: {IndexName}. Reason: {Reason}", id, indexName, response.ServerError?.Error?.Reason);
                throw new Exception($"Failed to delete document: {response.ServerError?.Error?.Reason}");
            }

            _logger.LogInformation("Document with ID: {DocumentId} deleted successfully from index: {IndexName}", id, indexName);
        }
    }
}
