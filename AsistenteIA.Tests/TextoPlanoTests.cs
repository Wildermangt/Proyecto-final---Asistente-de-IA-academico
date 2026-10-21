using AsistenteIA.Services;

namespace AsistenteIA.Tests;

// AiService.PlainText limpia el Markdown de la respuesta para copiar, compartir y las vistas previas
// del historial (casos C01, C03 y fallos F7, F16 y F19 del plan de pruebas).
public class TextoPlanoTests
{
    [Theory]
    [InlineData("# Título", "Título")]
    [InlineData("### Paso 1", "Paso 1")]
    [InlineData("   ## Con sangría", "Con sangría")]
    public void Titulos_PierdenLosNumerales(string markdown, string esperado)
    {
        Assert.Equal(esperado, AiService.PlainText(markdown));
    }

    [Fact]
    public void Negritas_SeQuitanSinDejarAsteriscos()
    {
        Assert.Equal("API REST es un estilo de arquitectura.",
            AiService.PlainText("**API REST** es un estilo de __arquitectura__."));
    }

    // Regresión de F19: la cursiva se quitaba antes que la negrita y quedaba "*API REST*".
    [Theory]
    [InlineData("**Sin estado (*Stateless*):** cada solicitud es independiente.",
                "Sin estado (Stateless): cada solicitud es independiente.")]
    [InlineData("Un *recurso* se identifica con una **URI**.", "Un recurso se identifica con una URI.")]
    public void NegritaConCursivaAdentro_NoDejaAsteriscos(string markdown, string esperado)
    {
        string texto = AiService.PlainText(markdown);

        Assert.Equal(esperado, texto);
        Assert.DoesNotContain("*", texto);
    }

    [Fact]
    public void Vinetas_SeConviertenEnPuntosConservandoLaSangria()
    {
        string markdown = "Verbos HTTP:\n- GET\n* POST\n  - PUT";

        Assert.Equal("Verbos HTTP:\n• GET\n• POST\n  • PUT", AiService.PlainText(markdown));
    }

    [Fact]
    public void CodigoEnLinea_PierdeLasComillasInvertidas()
    {
        Assert.Equal("Use GET /models para listar.", AiService.PlainText("Use `GET /models` para listar."));
    }

    [Theory]
    [InlineData("Antes\n---\nDespués")]
    [InlineData("Antes\n***\nDespués")]
    [InlineData("Antes\n___\nDespués")]
    public void LineasSeparadoras_SeEliminan(string markdown)
    {
        Assert.Equal("Antes\n\nDespués", AiService.PlainText(markdown));
    }

    [Fact]
    public void SaltosDeLineaDeWindows_SeNormalizan()
    {
        Assert.Equal("Línea 1\nLínea 2", AiService.PlainText("Línea 1\r\nLínea 2\r\n"));
    }

    [Fact]
    public void VariasLineasEnBlanco_QuedanEnUnaSola()
    {
        Assert.Equal("Párrafo 1\n\nPárrafo 2", AiService.PlainText("Párrafo 1\n\n\n\n\nPárrafo 2"));
    }

    [Fact]
    public void AsteriscosDeMultiplicacion_SeConservan()
    {
        Assert.Equal("2 * 3 * 4 = 24", AiService.PlainText("2 * 3 * 4 = 24"));
    }

    [Fact]
    public void TextoSinMarkdown_NoCambia()
    {
        const string texto = "Una API REST usa HTTP, tildes (á, é, ñ) y signos ¿? ¡!";

        Assert.Equal(texto, AiService.PlainText(texto));
    }
}
