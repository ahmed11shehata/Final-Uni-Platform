using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AYA_UIS.API.Filters
{
    /// <summary>
    /// Schema filter to handle enum types and complex objects in Swagger generation
    /// This prevents 500 errors when generating swagger.json
    /// </summary>
    public class EnumSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type.IsEnum)
            {
                schema.Enum.Clear();
                foreach (var enumValue in Enum.GetValues(context.Type))
                {
                    schema.Enum.Add(new Microsoft.OpenApi.Any.OpenApiString(enumValue.ToString()));
                }
            }
        }
    }
}
