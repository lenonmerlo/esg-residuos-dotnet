using System.Text.Json;

using Json.Schema;

namespace EsgResiduos.Tests.Support;

internal static class JsonSchemaContract
{
    public static void AssertValid(string json, string schemaFile)
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Schemas", schemaFile);
        JsonSchema schema = JsonSchema.FromText(File.ReadAllText(path));
        using JsonDocument document = JsonDocument.Parse(json);

        EvaluationResults results = schema.Evaluate(document.RootElement, new EvaluationOptions
        {
            OutputFormat = OutputFormat.List
        });

        Assert.True(results.IsValid, $"Contrato {schemaFile} inválido: {results}");
    }
}
