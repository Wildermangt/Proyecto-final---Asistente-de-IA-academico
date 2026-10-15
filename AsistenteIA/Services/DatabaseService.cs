using AsistenteIA.Models;
using SQLite;

namespace AsistenteIA.Services;

// Persistencia local del historial y de las plantillas con SQLite.
public class DatabaseService
{
    public const string DatabaseFilename = "asistenteia.db3";

    private const SQLiteOpenFlags Flags =
        SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache;

    private readonly SemaphoreSlim _initLock = new(1, 1);
    private SQLiteAsyncConnection? _db;

    public static string DatabasePath => Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);

    private async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        if (_db is not null)
            return _db;

        await _initLock.WaitAsync();
        try
        {
            if (_db is null)
            {
                var db = new SQLiteAsyncConnection(DatabasePath, Flags);
                await db.CreateTableAsync<ConversationRecord>();
                await db.CreateTableAsync<PromptTemplate>();
                if (await db.Table<PromptTemplate>().CountAsync() == 0)
                    await db.InsertAllAsync(DefaultTemplates());
                _db = db;
            }
            return _db;
        }
        finally
        {
            _initLock.Release();
        }
    }

    // ---- Historial (CRUD) ----

    public async Task<int> SaveAsync(ConversationRecord record)
    {
        var db = await GetConnectionAsync();
        return record.Id == 0 ? await db.InsertAsync(record) : await db.UpdateAsync(record);
    }

    public async Task<List<ConversationRecord>> GetConversationsAsync(string? search = null, string? category = null)
    {
        var db = await GetConnectionAsync();
        var query = db.Table<ConversationRecord>();
        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(r => r.Category == category);
        if (!string.IsNullOrWhiteSpace(search))
        {
            string s = search.Trim();
            query = query.Where(r => r.Prompt.Contains(s) || r.Answer.Contains(s));
        }
        return await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
    }

    public async Task<ConversationRecord?> GetConversationAsync(int id)
    {
        var db = await GetConnectionAsync();
        return await db.Table<ConversationRecord>().Where(r => r.Id == id).FirstOrDefaultAsync();
    }

    public async Task<int> DeleteAsync(ConversationRecord record)
    {
        var db = await GetConnectionAsync();
        return await db.DeleteAsync(record);
    }

    // ---- Plantillas (CRUD) ----

    public async Task<List<PromptTemplate>> GetTemplatesAsync()
    {
        var db = await GetConnectionAsync();
        return await db.Table<PromptTemplate>().OrderBy(t => t.Name).ToListAsync();
    }

    public async Task<int> SaveTemplateAsync(PromptTemplate template)
    {
        var db = await GetConnectionAsync();
        return template.Id == 0 ? await db.InsertAsync(template) : await db.UpdateAsync(template);
    }

    public async Task<int> DeleteTemplateAsync(PromptTemplate template)
    {
        var db = await GetConnectionAsync();
        return await db.DeleteAsync(template);
    }

    private static IEnumerable<PromptTemplate> DefaultTemplates() =>
    [
        new() { Name = "Consulta libre", Category = "General", Text = PromptTemplate.Placeholder },
        new() { Name = "Explicar concepto", Category = "Estudio",
            Text = "Explica el siguiente concepto para un estudiante universitario, con una definición, un ejemplo y un error común: {consulta}" },
        new() { Name = "Resumir texto", Category = "Redacción",
            Text = "Resume en máximo cinco viñetas las ideas principales del siguiente texto: {consulta}" },
        new() { Name = "Preguntas de estudio", Category = "Estudio",
            Text = "Genera cinco preguntas de repaso con su respuesta breve sobre el tema: {consulta}" },
        new() { Name = "Explicar código C#", Category = "Programación",
            Text = "Explica paso a paso qué hace el siguiente código C# y sugiere una mejora: {consulta}" },
        new() { Name = "Revisar redacción", Category = "Redacción",
            Text = "Revisa la ortografía y la claridad del siguiente párrafo y explica los cambios: {consulta}" }
    ];
}
