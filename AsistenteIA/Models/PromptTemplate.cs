using SQLite;

namespace AsistenteIA.Models;

// Plantilla de prompt académico editable. {consulta} se reemplaza por el texto del usuario.
public class PromptTemplate
{
    public const string Placeholder = "{consulta}";

    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Category { get; set; } = "General";

    public string Text { get; set; } = Placeholder;

    public string Apply(string userText) =>
        Text.Contains(Placeholder)
            ? Text.Replace(Placeholder, userText.Trim())
            : $"{Text.Trim()}\n\n{userText.Trim()}";

    public override string ToString() => Name;
}
