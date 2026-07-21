using System.ComponentModel;
using ModelContextProtocol.Server;
using NotionMcpServer;

namespace NotionMcpServer.Tools;

/// <summary>
/// Conjunto de ferramentas (tools) MCP que expõem a API do Notion a um LLM.
/// Cada método público marcado com [McpServerTool] vira uma ferramenta que o
/// modelo pode chamar. As descrições são o que o modelo lê para decidir quando usar.
/// </summary>
[McpServerToolType]
public static class NotionTools
{
    [McpServerTool(Name = "notion_search")]
    [Description("Busca páginas no Notion pelo título. Use quando o usuário quiser encontrar uma página existente.")]
    public static Task<string> Search(
        NotionClient notion,
        [Description("Termo a buscar no título das páginas.")] string query,
        [Description("Quantidade máxima de resultados (1 a 50). Padrão 10.")] int pageSize = 10,
        CancellationToken ct = default)
        => notion.SearchPagesAsync(query, pageSize, ct);

    [McpServerTool(Name = "notion_read_page")]
    [Description("Lê o conteúdo textual de uma página do Notion a partir do seu id.")]
    public static Task<string> ReadPage(
        NotionClient notion,
        [Description("O id da página do Notion (ex: obtido via notion_search).")] string pageId,
        CancellationToken ct = default)
        => notion.GetPageContentAsync(pageId, ct);

    [McpServerTool(Name = "notion_query_database")]
    [Description("Lista as linhas de um banco de dados do Notion a partir do id do banco.")]
    public static Task<string> QueryDatabase(
        NotionClient notion,
        [Description("O id do banco de dados (database) do Notion.")] string databaseId,
        [Description("Quantidade máxima de linhas (1 a 50). Padrão 25.")] int pageSize = 25,
        CancellationToken ct = default)
        => notion.QueryDatabaseAsync(databaseId, pageSize, ct);

    [McpServerTool(Name = "notion_create_page")]
    [Description("Cria uma nova página dentro de uma página existente (informada pelo id do pai).")]
    public static Task<string> CreatePage(
        NotionClient notion,
        [Description("Id da página que será a mãe da nova página.")] string parentPageId,
        [Description("Título da nova página.")] string title,
        [Description("Conteúdo opcional. Cada linha vira um parágrafo.")] string? content = null,
        CancellationToken ct = default)
        => notion.CreatePageAsync(parentPageId, title, content, ct);

    [McpServerTool(Name = "notion_append_text")]
    [Description("Adiciona parágrafos de texto ao final de uma página existente do Notion.")]
    public static Task<string> AppendText(
        NotionClient notion,
        [Description("Id da página onde o texto será acrescentado.")] string pageId,
        [Description("Texto a adicionar. Cada linha vira um parágrafo.")] string text,
        CancellationToken ct = default)
        => notion.AppendTextAsync(pageId, text, ct);
}
