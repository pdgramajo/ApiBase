using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Linq;
using System.Reflection;
using Microsoft.OpenApi.Any;

namespace ApiBase.Helpers
{
    public class FileOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (!(operation?.RequestBody?.Content?.Any(x => x.Key.ToLower() == "multipart/form-data") ?? false)) return;

            var uploadFiles = context.MethodInfo.DeclaringType.GetCustomAttributes(true)
            .Union(context.MethodInfo.GetCustomAttributes(true))
            .OfType<SwaggerUploadFile>();
            if (uploadFiles.Count() == 0) return;
            var uploadFile = uploadFiles.First();
            operation.RequestBody.Content["multipart/form-data"].Schema.Properties =
            new Dictionary<string, OpenApiSchema>
            {
                [uploadFile.Parameter] = new OpenApiSchema
                {
                    Type = "string",
                    Format = "binary",
                    Description = uploadFile.Description
                }
            };
            if (!String.IsNullOrEmpty(uploadFile.Example))
            {
                operation.RequestBody.Content["multipart/form-data"].Schema.Example = new OpenApiString(uploadFile.Example);
                operation.RequestBody.Content["multipart/form-data"].Schema.Description = uploadFile.Example;
            }
        }
    }
    public class SwaggerUploadFile
    {
        public string Parameter { get; set; }
        public string Description { get; set; }
        public string Example { get; set; }
    }
}