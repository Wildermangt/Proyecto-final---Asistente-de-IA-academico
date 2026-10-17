using System.Text.RegularExpressions;

namespace AsistenteIA.Controls;

// Muestra la respuesta del modelo con formato básico: títulos (#), negritas (**texto**), cursivas,
// viñetas (- / *), código en línea (`texto`) y bloques de código (```).
//
// Cada párrafo es una etiqueta propia y ningún Span contiene saltos de línea. Una sola etiqueta con
// Spans de "\n" provocaba un bucle infinito en MAUI para Windows (FormattedStringExtensions.
// RecalculateSpanPositions mide un Span de altura 0) que agotaba la memoria y cerraba la app (F18).
public partial class MarkdownView : ContentView
{
    public static readonly BindableProperty MarkdownProperty = BindableProperty.Create(
        nameof(Markdown), typeof(string), typeof(MarkdownView), string.Empty,
        propertyChanged: (b, _, n) => ((MarkdownView)b).Render(n as string));

    public string Markdown
    {
        get => (string)GetValue(MarkdownProperty);
        set => SetValue(MarkdownProperty, value);
    }

    // La negrita admite cursiva adentro: **Sin estado (*Stateless*):**
    [GeneratedRegex(@"(\*\*(?:[^*\n]|\*(?!\*))+?\*\*|__[^_\n]+__|`[^`\n]+`)")]
    private static partial Regex InlineRegex();

    [GeneratedRegex(@"(?<!\w)\*(?=\S)([^*\n]+?)(?<=\S)\*(?!\w)")]
    private static partial Regex ItalicRegex();

    private static string MonospaceFont => DeviceInfo.Platform == DevicePlatform.Android ? "monospace" : "Consolas";

    private readonly VerticalStackLayout _paragraphs = new() { Spacing = 6 };

    public MarkdownView() => Content = _paragraphs;

    private void Render(string? text)
    {
        _paragraphs.Children.Clear();
        if (string.IsNullOrWhiteSpace(text))
            return;

        bool inCode = false;
        foreach (string rawLine in text.Replace("\r\n", "\n").Trim().Split('\n'))
        {
            string line = rawLine.TrimEnd();
            string trimmed = line.TrimStart();

            if (trimmed.StartsWith("```"))
            {
                inCode = !inCode;
                continue;
            }
            if (inCode)
            {
                if (trimmed.Length > 0)
                    _paragraphs.Children.Add(CodeLine(line));
                continue;
            }
            if (trimmed.Length == 0 || trimmed is "---" or "***" or "___")
                continue;

            if (trimmed.StartsWith('#'))
            {
                _paragraphs.Children.Add(Paragraph(trimmed.TrimStart('#').Trim(), heading: true));
                continue;
            }

            int indent = line.Length - trimmed.Length;
            if (trimmed.StartsWith("- ") || trimmed.StartsWith("* "))
            {
                var bullet = Paragraph("•  " + trimmed[2..], heading: false);
                bullet.Margin = new Thickness(6 + indent * 6, 0, 0, 0);
                _paragraphs.Children.Add(bullet);
            }
            else
            {
                _paragraphs.Children.Add(Paragraph(trimmed, heading: false));
            }
        }
    }

    private static Label Paragraph(string text, bool heading)
    {
        var formatted = new FormattedString();
        foreach (string part in InlineRegex().Split(text))
        {
            if (part.Length == 0)
                continue;
            if ((part.StartsWith("**") && part.EndsWith("**") && part.Length > 4) || (part.StartsWith("__") && part.EndsWith("__") && part.Length > 4))
                formatted.Spans.Add(new Span { Text = ItalicRegex().Replace(part[2..^2], "$1"), FontFamily = heading ? "InterBold" : "InterSemiBold" });
            else if (part.StartsWith('`') && part.EndsWith('`') && part.Length > 2)
                formatted.Spans.Add(new Span { Text = part[1..^1], FontFamily = MonospaceFont });
            else
            {
                // La cursiva (*texto*) se quita después de separar las negritas; al revés, "**API**" quedaría "*API*".
                string plain = ItalicRegex().Replace(part, "$1");
                if (plain.Length > 0)
                    formatted.Spans.Add(new Span { Text = plain, FontFamily = heading ? "InterBold" : null });
            }
        }

        var label = new Label { FormattedText = formatted, LineBreakMode = LineBreakMode.WordWrap };
        if (heading)
        {
            label.FontSize = 16;
            label.Margin = new Thickness(0, 6, 0, 0);
            SemanticProperties.SetHeadingLevel(label, SemanticHeadingLevel.Level3);
        }
        return label;
    }

    private static Label CodeLine(string line) => new()
    {
        Text = line,
        FontFamily = MonospaceFont,
        FontSize = 13,
        LineBreakMode = LineBreakMode.WordWrap,
        Margin = new Thickness(10, 0, 0, 0)
    };
}
