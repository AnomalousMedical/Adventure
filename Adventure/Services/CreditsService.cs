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
        yield return new Credit("AMD FidelityFX SDK 1.0", null, "https://github.com/GPUOpen-LibrariesAndSDKs/FidelityFX-SDK", "MIT License");
        yield return new Credit("BepuPhysics2", "Ross Nordby", "https://github.com/bepu/bepuphysics2", "Apache License 2.0");
        yield return new Credit("Diligent Engine", "TheMostDiligent", "https://diligentgraphics.com/", "Apache License 2.0");
        yield return new Credit("FreeImage", null, "https://freeimage.sourceforge.io/", "FreeImage Public License - Version 1.0");
        yield return new Credit("FreeType", null, "https://freetype.org/", "The FreeType Project LICENSE");
        yield return new Credit("MyGUI Font Rendering", "Altren and mynameco", "https://github.com/MyGUI/mygui", "MIT License");
        yield return new Credit("OpenALSoft", "kcat", "https://github.com/kcat/openal-soft", "LGPL Version 2, June 1991");
        yield return new Credit("OggVorbis", null, "https://xiph.org/", "BSD license");
        yield return new Credit("RogueLike", "Andy Stobirski", "https://github.com/AndyStobirski/RogueLike", "Public Domain");
        yield return new Credit("Steamworks.NET", "Riley Labrecque", "https://github.com/rlabrecque/Steamworks.NET", "MIT");

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
