﻿using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

//TODO: Move to Shared folder or Common project?

namespace TMP.Service
{
    /// <summary>
    /// A filter that converts all API paths in the Swagger document to lowercase.
    /// </summary>
    public class LowercaseDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var paths = swaggerDoc.Paths;

            var newPaths = new Dictionary<string, OpenApiPathItem>();

            var removeKeys = new List<string>();

            foreach (var path in paths)
            {
                var newKey = path.Key.ToLower();

                if (newKey != path.Key)
                {
                    removeKeys.Add(path.Key);

                    newPaths.Add(newKey, path.Value);
                }
            }

            foreach (var path in newPaths)
                swaggerDoc.Paths.Add(path.Key, path.Value);

            foreach (var key in removeKeys)
                swaggerDoc.Paths.Remove(key);
        }
    }
}