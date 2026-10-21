using SQLite;

namespace AsistenteIA.Models;

// Entidad local del historial (tabla ConversationRecord en SQLite).
public class ConversationRecord
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Prompt { get; set; } = string.Empty;

    public string Answer { get; set; } = string.Empty;

    [Indexed]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Indexed]
    public string Category { get; set; } = "General";

    public bool IsVerified { get; set; }

    public string Model { get; set; } = string.Empty;

    [Ignore]
    public string CreatedAtLocal => CreatedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm");

    [Ignore]
    public string VerificationText => IsVerified ? "Verificada con fuentes" : "Sin verificar";
}
