using System.Net;
using System.Text;
using AsistenteIA.Services;

namespace AsistenteIA.Tests;

// Traducción de las respuestas de error del proveedor a mensajes comprensibles
// (casos P04, A07, A08 y fallo F6 del plan de pruebas).
public class ErroresHttpTests
{
    private static HttpResponseMessage Respuesta(HttpStatusCode codigo, string cuerpo = "") =>
        new(codigo) { Content = new StringContent(cuerpo, Encoding.UTF8, "application/json") };

    private static Task<AiServiceException> Traducir(HttpStatusCode codigo, string cuerpo = "") =>
        AiService.CreateHttpErrorAsync(Respuesta(codigo, cuerpo), CancellationToken.None);

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, 401)]
    [InlineData(HttpStatusCode.Forbidden, 403)]
    public async Task ClaveInvalida_SeInformaComoNoAutorizado(HttpStatusCode codigo, int numero)
    {
        AiServiceException error = await Traducir(codigo);

        Assert.Equal(AiErrorKind.Unauthorized, error.Kind);
        Assert.Equal(codigo, error.StatusCode);
        Assert.StartsWith($"Error {numero}: la clave no es válida", error.Message);
    }

    // F6: Google responde 400 con un arreglo JSON cuando la clave no es válida.
    [Fact]
    public async Task Error400DeGoogleConClaveInvalida_SeTrataComoClaveInvalida()
    {
        const string cuerpo = """[{"error":{"code":400,"message":"API key not valid. Please pass a valid API key.","status":"INVALID_ARGUMENT"}}]""";

        AiServiceException error = await Traducir(HttpStatusCode.BadRequest, cuerpo);

        Assert.Equal(AiErrorKind.Unauthorized, error.Kind);
        Assert.Contains("la clave no es válida para este proveedor", error.Message);
        Assert.Contains("(Proveedor: API key not valid.", error.Message);
    }

    [Fact]
    public async Task Error400SinRelacionConLaClave_EsSolicitudRechazada()
    {
        VerificarError(await Traducir(HttpStatusCode.BadRequest, """{"error":{"message":"Invalid model parameter"}}"""),
            AiErrorKind.BadResponse, "Error 400: solicitud rechazada por el servicio.");
    }

    [Fact]
    public async Task Error402_IndicaSaldoAgotado()
    {
        VerificarError(await Traducir(HttpStatusCode.PaymentRequired), AiErrorKind.PaymentRequired, "Error 402: el saldo");
    }

    // A08: cuota del plan gratuito agotada.
    [Fact]
    public async Task Error429_IndicaCuotaAgotadaConElMensajeDelProveedor()
    {
        AiServiceException error = await Traducir(HttpStatusCode.TooManyRequests,
            """{"error":{"message":"You exceeded your current quota"}}""");

        VerificarError(error, AiErrorKind.RateLimited, "Error 429: demasiadas solicitudes o cuota agotada.");
        Assert.EndsWith("(Proveedor: You exceeded your current quota)", error.Message);
    }

    [Fact]
    public async Task Error404_IndicaQueElModeloNoExiste()
    {
        VerificarError(await Traducir(HttpStatusCode.NotFound), AiErrorKind.BadResponse, "Error 404: el modelo no existe");
    }

    // A07: proveedor saturado.
    [Theory]
    [InlineData(HttpStatusCode.InternalServerError, 500)]
    [InlineData(HttpStatusCode.BadGateway, 502)]
    [InlineData(HttpStatusCode.ServiceUnavailable, 503)]
    public async Task Errores5xx_IndicanServicioNoDisponible(HttpStatusCode codigo, int numero)
    {
        VerificarError(await Traducir(codigo), AiErrorKind.ServerError, $"Error {numero}: el servicio de IA no está disponible");
    }

    [Fact]
    public async Task SinCuerpo_ElMensajeNoMencionaAlProveedor()
    {
        AiServiceException error = await Traducir(HttpStatusCode.ServiceUnavailable);

        Assert.DoesNotContain("Proveedor:", error.Message);
    }

    [Fact]
    public void MensajeDeErrorEnObjeto_SeExtrae()
    {
        Assert.Equal("Model not found", AiService.ExtractErrorMessage("""{"error":{"message":"Model not found"}}"""));
    }

    [Fact]
    public void MensajeDeErrorEnArreglo_SeExtrae()
    {
        Assert.Equal("Quota exceeded", AiService.ExtractErrorMessage("""[{"error":{"message":"Quota exceeded"}}]"""));
    }

    [Fact]
    public void ErrorComoTexto_SeExtrae()
    {
        Assert.Equal("Unauthorized", AiService.ExtractErrorMessage("""{"error":"Unauthorized"}"""));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("<html>Bad Gateway</html>")]
    [InlineData("""{"detail":"sin campo error"}""")]
    [InlineData("[]")]
    public void CuerpoSinMensajeReconocible_DevuelveNulo(string cuerpo)
    {
        Assert.Null(AiService.ExtractErrorMessage(cuerpo));
    }

    [Fact]
    public void MensajeMuyLargo_SeRecortaA200Caracteres()
    {
        string largo = new('x', 500);

        string? mensaje = AiService.ExtractErrorMessage($$$"""{"error":{"message":"{{{largo}}}"}}""");

        Assert.NotNull(mensaje);
        Assert.Equal(201, mensaje.Length);
        Assert.EndsWith("…", mensaje);
    }

    private static void VerificarError(AiServiceException error, AiErrorKind tipo, string inicioDelMensaje)
    {
        Assert.Equal(tipo, error.Kind);
        Assert.StartsWith(inicioDelMensaje, error.Message);
    }
}
