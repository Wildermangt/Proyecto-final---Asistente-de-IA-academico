using System.Globalization;
using AsistenteIA.Services;

namespace AsistenteIA.Converters;

// Texto sin símbolos Markdown para vistas previas de una o dos líneas (historial).
public class MarkdownToPlainTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is string text ? AiService.PlainText(text) : string.Empty;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
