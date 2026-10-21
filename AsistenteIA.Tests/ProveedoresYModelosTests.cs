using AsistenteIA.Models;
using AsistenteIA.Services;

namespace AsistenteIA.Tests;

// Proveedor seleccionable y lista de modelos (fallo F5, caso S05 y riesgo R4 del plan de pruebas).
public class ProveedoresYModelosTests
{
    [Theory]
    [InlineData("pollinations", "Pollinations")]
    [InlineData("gemini", "Google Gemini")]
    public void FromId_DevuelveElProveedorGuardado(string id, string nombre)
    {
        Assert.Equal(nombre, AiProvider.FromId(id).Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("proveedor-inexistente")]
    public void FromId_ConValorDesconocido_UsaPollinations(string? id)
    {
        Assert.Same(AiProvider.Pollinations, AiProvider.FromId(id));
    }

    // S05: todos los servicios se consumen por HTTPS.
    [Fact]
    public void TodosLosProveedores_UsanHttpsYTerminanEnBarra()
    {
        foreach (AiProvider proveedor in AiProvider.All)
        {
            Assert.StartsWith("https://", proveedor.BaseUrl);
            Assert.EndsWith("/", proveedor.BaseUrl); // Necesario para que Uri combine "chat/completions".
        }
    }

    [Fact]
    public void TodosLosProveedores_TienenModeloPorDefectoEnSuListaDeRespaldo()
    {
        foreach (AiProvider proveedor in AiProvider.All)
            Assert.Contains(proveedor.DefaultModel, proveedor.FallbackModels);
    }

    [Theory]
    [InlineData("text", false, true)]
    [InlineData("text", true, false)]   // Solo de pago.
    [InlineData("image", false, false)]
    [InlineData(null, false, false)]
    public void Pollinations_SoloMuestraModelosDeTextoGratuitos(string? categoria, bool soloPago, bool esperado)
    {
        var modelo = new ModelInfo { Id = "openai/gpt-5.4-nano", Category = categoria, PaidOnly = soloPago };

        Assert.Equal(esperado, AiService.IsTextModel(AiProvider.Pollinations, modelo));
    }

    [Theory]
    [InlineData("models/gemini-3.8-flash", true)]
    [InlineData("models/gemini-2.5-pro", true)]
    [InlineData("models/gemini-embedding-001", false)]
    [InlineData("models/gemini-2.5-flash-preview-tts", false)]
    [InlineData("models/gemini-2.5-flash-image", false)]
    [InlineData("models/imagen-4.0-generate", false)]
    [InlineData("models/veo-3.0-generate", false)]
    [InlineData("models/text-bison", false)]
    public void Gemini_SoloMuestraModelosDeTexto(string id, bool esperado)
    {
        Assert.Equal(esperado, AiService.IsTextModel(AiProvider.Gemini, new ModelInfo { Id = id }));
    }

    [Theory]
    [InlineData("models/gemini-3.8-flash", "gemini-3.8-flash")]
    [InlineData("gemini-3.8-flash", "gemini-3.8-flash")]
    [InlineData("openai/gpt-5.4-nano", "openai/gpt-5.4-nano")]
    public void NormalizeModelId_QuitaElPrefijoModels(string id, string esperado)
    {
        Assert.Equal(esperado, AiService.NormalizeModelId(id));
    }
}
