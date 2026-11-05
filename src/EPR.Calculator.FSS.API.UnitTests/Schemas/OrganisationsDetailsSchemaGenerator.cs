using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization.Metadata;
using EPR.Calculator.FSS.API.Models;

namespace EPR.Calculator.FSS.API.UnitTests.Schemas;

/// <summary>
/// Generates the JSON schema for the organisations-details response from the response models,
/// using the same serializer defaults ASP.NET Core uses for controller responses.
/// </summary>
public static class OrganisationsDetailsSchemaGenerator
{
    public const string FileName = "organisations-details.schema.json";

    /// <summary>
    /// Serializer options matching the API's response serialization (ASP.NET Core web defaults).
    /// <see cref="JsonSerializerOptions.RespectNullableAnnotations"/> is set so the schema
    /// distinguishes nullable from non-nullable reference types.
    /// </summary>
    public static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        RespectNullableAnnotations = true,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
    };

    public static string Generate()
    {
        var exporterOptions = new JsonSchemaExporterOptions
        {
            TreatNullObliviousAsNonNullable = true,
            TransformSchemaNode = (context, schema) =>
            {
                // The serializer writes every property (nulls included), so all are always present
                // in the response - not just those declared with the C# 'required' modifier.
                if (schema is not JsonObject obj)
                {
                    return schema;
                }

                if (obj["properties"] is JsonObject properties)
                {
                    obj["required"] = new JsonArray([.. properties.Select(p => (JsonNode)p.Key)]);
                }

                if (context.PropertyInfo?.AttributeProvider is ICustomAttributeProvider provider
                    && provider.GetCustomAttributes(typeof(RegularExpressionAttribute), inherit: true)
                        .OfType<RegularExpressionAttribute>()
                        .FirstOrDefault() is { } regex)
                {
                    obj["pattern"] = regex.Pattern;
                }

                return schema;
            },
        };

        var schema = SerializerOptions.GetJsonSchemaAsNode(typeof(OrganisationsDetailsResponse), exporterOptions);

        var root = new JsonObject
        {
            ["$schema"] = "https://json-schema.org/draft/2020-12/schema",
            ["title"] = "Organisations details response",
            ["description"] = "Response body of GET /api/v1/organisations-details. Generated from the response models by OrganisationsDetailsSchemaGenerator - do not edit by hand.",
        };

        foreach (var (key, value) in schema.AsObject().ToList())
        {
            root[key] = value?.DeepClone();
        }

        return root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }).ReplaceLineEndings("\n") + "\n";
    }
}
