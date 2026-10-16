using System.Globalization;
using System.Text.RegularExpressions;
using AsistenteIA.Services;

namespace AsistenteIA.Converters;

// Muestra la respuesta del modelo con formato básico: títulos (#), negritas (**texto**),
// viñetas (- / *) y código (`texto`). No interpreta HTML ni enlaces: solo arma Spans de texto.
public partial class MarkdownToFormattedStringConverter : IValueConverter
{
    [GeneratedRegex(@"(\*\*[^*\n]+\*\*|__[^_\n]+__|`[^`\n]+`)")]
    private static partial Regex InlineRegex();

    [GeneratedRegex(@"(?<!\w)\*(?=\S)([^*\n]+?)(?<=\S)\*(?!\w)")]
    private static partial Regex ItalicRegex();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var formatted = new FormattedString();
        if (value is not string text || string.IsNullOrWhiteSpace(text))
            return formatted;

        string[] lines = text.Replace("\r\n", "\n").Trim().Split('\n');
        bool previousBlank = false;
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].TrimEnd();
            string trimmed = line.TrimStart();

            if (trimmed.Length == 0 || trimmed is "---" or "***" or "___")
            {
                // Una sola línea en blanco entre párrafos.
                if (!previousBlank && formatted.Spans.Count > 0)
                    formatted.Spans.Add(new Span { Text = "\n" });
                previousBlank = true;
                continue;
            }
            previousBlank = false;

            if (trimmed.StartsWith('#'))
            {
                formatted.Spans.Add(new Span
                {
                    Text = trimmed.TrimStart('#').Trim().Replace("**", string.Empty),
                    FontFamily = "InterBold",
                    FontSize = 16
                });
            }
            else
            {
                string indent = line[..(line.Length - trimmed.Length)];
                if (trimmed.StartsWith("- ") || trimmed.StartsWith("* "))
                    trimmed = indent + "•  " + trimmed[2..];
                else
                    trimmed = indent + trimmed;
                AddInline(formatted, ItalicRegex().Replace(trimmed, "$1"));
            }

            if (i < lines.Length - 1)
                formatted.Spans.Add(new Span { Text = "\n" });
        }
        return formatted;
    }

    private static string MonospaceFont => DeviceInfo.Platform == DevicePlatform.Android ? "monospace" : "Consolas";

    private static void AddInline(FormattedString formatted, string line)
    {
        foreach (string part in InlineRegex().Split(line))
        {
            if (part.Length == 0)
                continue;
            if ((part.StartsWith("**") && part.EndsWith("**")) || (part.StartsWith("__") && part.EndsWith("__")))
                formatted.Spans.Add(new Span { Text = part[2..^2], FontFamily = "InterSemiBold" });
            else if (part.StartsWith('`') && part.EndsWith('`'))
                formatted.Spans.Add(new Span { Text = part[1..^1], FontFamily = MonospaceFont });
            else
                formatted.Spans.Add(new Span { Text = part });
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

// Texto sin símbolos Markdown para vistas previas de una o dos líneas (historial).
public class MarkdownToPlainTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is string text ? AiService.PlainText(text) : string.Empty;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
