using System.Text;
using System.Text.Json;

namespace NotionMcpServer;

/// <summary>
/// Wrapper fino sobre a REST API do Notion. Cada método faz uma chamada HTTP
/// e devolve um texto já resumido e legível para o modelo de linguagem consumir.
/// </summary>
public sealed class NotionClient(HttpClient http)
{
    private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = false };

    // ---------- Leitura ----------

    /// <summary>Busca páginas cujo título casa com o termo informado.</summary>
    public async Task<string> SearchPagesAsync(string query, int pageSize, CancellationToken ct)
    {
        var body = new
        {
            query,
            filter = new { value = "page", property = "object" },
            page_size = Math.Clamp(pageSize, 1, 50)
        };

        using var resp = await http.PostAsync("search", JsonContent(body), ct);
        var json = await ReadOrThrow(resp, ct);

        using var doc = JsonDocument.Parse(json);
        var sb = new StringBuilder();
        var count = 0;

        foreach (var result in doc.RootElement.GetProperty("results").EnumerateArray())
        {
            var id = result.GetProperty("id").GetString();
            var title = ExtractTitle(result);
            sb.AppendLine($"- {title}  (id: {id})");
            count++;
        }

        return count == 0
            ? $"Nenhuma página encontrada para \"{query}\"."
            : $"{count} página(s) encontrada(s):\n{sb}";
    }

    /// <summary>Lê o conteúdo textual (blocos) de uma página.</summary>
    public async Task<string> GetPageContentAsync(string pageId, CancellationToken ct)
    {
        using var resp = await http.GetAsync($"blocks/{pageId}/children?page_size=100", ct);
        var json = await ReadOrThrow(resp, ct);

        using var doc = JsonDocument.Parse(json);
        var sb = new StringBuilder();

        foreach (var block in doc.RootElement.GetProperty("results").EnumerateArray())
        {
            var type = block.GetProperty("type").GetString();
            if (type is null || !block.TryGetProperty(type, out var payload)) continue;
            if (!payload.TryGetProperty("rich_text", out var richText)) continue;

            var text = ExtractRichText(richText);
            if (!string.IsNullOrWhiteSpace(text))
                sb.AppendLine(text);
        }

        return sb.Length == 0 ? "A página não tem conteúdo textual." : sb.ToString();
    }

    /// <summary>Consulta as linhas de um banco de dados do Notion.</summary>
    public async Task<string> QueryDatabaseAsync(string databaseId, int pageSize, CancellationToken ct)
    {
        var body = new { page_size = Math.Clamp(pageSize, 1, 50) };
        using var resp = await http.PostAsync($"databases/{databaseId}/query", JsonContent(body), ct);
        var json = await ReadOrThrow(resp, ct);

        using var doc = JsonDocument.Parse(json);
        var sb = new StringBuilder();
        var count = 0;

        foreach (var row in doc.RootElement.GetProperty("results").EnumerateArray())
        {
            var id = row.GetProperty("id").GetString();
            var title = ExtractTitle(row);
            sb.AppendLine($"- {title}  (id: {id})");
            count++;
        }

        return count == 0
            ? "O banco de dados não retornou linhas."
            : $"{count} linha(s):\n{sb}";
    }

    // ---------- Escrita ----------

    /// <summary>Cria uma nova página dentro de uma página existente.</summary>
    public async Task<string> CreatePageAsync(string parentPageId, string title, string? content, CancellationToken ct)
    {
        var body = new Dictionary<string, object?>
        {
            ["parent"] = new { page_id = parentPageId },
            ["properties"] = new
            {
                title = new
                {
                    title = new[] { new { text = new { content = title } } }
                }
            }
        };

        if (!string.IsNullOrWhiteSpace(content))
            body["children"] = ToParagraphBlocks(content);

        using var resp = await http.PostAsync("pages", JsonContent(body), ct);
        var json = await ReadOrThrow(resp, ct);

        using var doc = JsonDocument.Parse(json);
        var newId = doc.RootElement.GetProperty("id").GetString();
        var url = doc.RootElement.TryGetProperty("url", out var u) ? u.GetString() : null;
        return $"Página \"{title}\" criada com sucesso.\nid: {newId}" + (url is null ? "" : $"\nurl: {url}");
    }

    /// <summary>Adiciona parágrafos de texto ao final de uma página existente.</summary>
    public async Task<string> AppendTextAsync(string pageId, string text, CancellationToken ct)
    {
        var body = new { children = ToParagraphBlocks(text) };
        using var resp = await http.PatchAsync($"blocks/{pageId}/children", JsonContent(body), ct);
        await ReadOrThrow(resp, ct);
        return "Texto adicionado à página com sucesso.";
    }

    // ---------- Helpers ----------

    private static object[] ToParagraphBlocks(string content)
    {
        return content
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line => (object)new
            {
                @object = "block",
                type = "paragraph",
                paragraph = new
                {
                    rich_text = new[] { new { type = "text", text = new { content = line.Trim() } } }
                }
            })
            .ToArray();
    }

    private static string ExtractTitle(JsonElement pageOrRow)
    {
        if (!pageOrRow.TryGetProperty("properties", out var props))
            return "(sem título)";

        foreach (var prop in props.EnumerateObject())
        {
            if (prop.Value.TryGetProperty("type", out var t) && t.GetString() == "title"
                && prop.Value.TryGetProperty("title", out var titleArr))
            {
                var text = ExtractRichText(titleArr);
                return string.IsNullOrWhiteSpace(text) ? "(sem título)" : text;
            }
        }

        return "(sem título)";
    }

    private static string ExtractRichText(JsonElement richText)
    {
        var sb = new StringBuilder();
        foreach (var span in richText.EnumerateArray())
        {
            if (span.TryGetProperty("plain_text", out var plain))
                sb.Append(plain.GetString());
        }
        return sb.ToString();
    }

    private static StringContent JsonContent(object body) =>
        new(JsonSerializer.Serialize(body, JsonOpts), Encoding.UTF8, "application/json");

    private static async Task<string> ReadOrThrow(HttpResponseMessage resp, CancellationToken ct)
    {
        var json = await resp.Content.ReadAsStringAsync(ct);
        if (!resp.IsSuccessStatusCode)
            throw new InvalidOperationException(
                $"A API do Notion retornou {(int)resp.StatusCode} {resp.StatusCode}: {json}");
        return json;
    }
}
