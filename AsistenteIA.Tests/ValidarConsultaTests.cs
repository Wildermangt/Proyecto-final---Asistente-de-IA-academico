using AsistenteIA.Services;

namespace AsistenteIA.Tests;

// Caso P02 del plan de pruebas: la consulta vacía o corta no se envía al servicio.
public class ValidarConsultaTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("    ")]
    [InlineData("\n\t")]
    public void ConsultaVacia_PideEscribirUnaConsulta(string? consulta)
    {
        Assert.Equal("Escriba una consulta antes de enviar.", AiService.ValidatePrompt(consulta));
    }

    [Theory]
    [InlineData("API")]
    [InlineData("REST")]
    [InlineData("  API  ")] // Los espacios de los extremos no cuentan.
    public void ConsultaCorta_IndicaElMinimoDeCaracteres(string consulta)
    {
        Assert.Equal("La consulta es demasiado corta (mínimo 5 caracteres).", AiService.ValidatePrompt(consulta));
    }

    [Theory]
    [InlineData("12345")] // Límite inferior exacto.
    [InlineData("¿Qué es una API REST?")]
    public void ConsultaValida_NoDevuelveError(string consulta)
    {
        Assert.Null(AiService.ValidatePrompt(consulta));
    }

    [Fact]
    public void ConsultaDe2000Caracteres_EsValida()
    {
        Assert.Null(AiService.ValidatePrompt(new string('a', AiService.MaxPromptLength)));
    }

    [Fact]
    public void ConsultaDeMasDe2000Caracteres_SeRechaza()
    {
        string consulta = new('a', AiService.MaxPromptLength + 1);

        Assert.Equal("La consulta supera 2000 caracteres.", AiService.ValidatePrompt(consulta));
    }
}
