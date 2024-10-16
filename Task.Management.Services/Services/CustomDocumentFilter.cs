using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CRM.Equitec.API.Services
{
    public class CustomDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            /*context.SchemaGenerator.GenerateSchema(typeof(AuthResponseError), context.SchemaRepository);*/
        }
    }
}