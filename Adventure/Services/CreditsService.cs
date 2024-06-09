using Engine;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Adventure.Services;

record Credit(string Title, string Author, string Source, string License);

interface ICreditsService
{
    IEnumerable<Credit> GetCredits();
}

class CreditsService(VirtualFileSystem virtualFileSystem) : ICreditsService
{
    private readonly CreditServiceSourceGenerationContext sourceGenContext = new CreditServiceSourceGenerationContext(new JsonSerializerOptions()
    {
        PropertyNameCaseInsensitive = true,
    });

    public IEnumerable<Credit> GetCredits()
    {
        foreach (var file in virtualFileSystem.listFiles("/", "credits.json", true))
        {
            using var stream = virtualFileSystem.openStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
            var jsonDocument = JsonDocument.Parse(stream);
            switch (jsonDocument.RootElement.ValueKind)
            {
                case JsonValueKind.Object:
                    yield return jsonDocument.RootElement.Deserialize(sourceGenContext.Credit);
                    break;
                case JsonValueKind.Array:
                    foreach (var item in jsonDocument.RootElement.EnumerateArray())
                    {
                        yield return item.Deserialize(sourceGenContext.Credit);
                    }
                    break;
            }
        }
    }
}

[JsonSerializable(typeof(Credit))]
internal partial class CreditServiceSourceGenerationContext : JsonSerializerContext
{
}
