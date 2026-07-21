using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotionMcpServer;

var builder = Host.CreateApplicationBuilder(args);

// Em um servidor MCP via stdio, o stdout é o canal de comunicação com o modelo.
// Por isso TODO log precisa ir para o stderr, senão corrompe o protocolo.
builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

// Configuração via variáveis de ambiente (nunca comite o token no repositório).
var token = Environment.GetEnvironmentVariable("NOTION_TOKEN")
    ?? throw new InvalidOperationException(
        "A variável de ambiente NOTION_TOKEN não está definida. " +
        "Crie uma integração em https://www.notion.so/my-integrations e exporte o token.");

var notionVersion = Environment.GetEnvironmentVariable("NOTION_VERSION") ?? "2022-06-28";

// HttpClient tipado e injetado no NotionClient via DI.
builder.Services.AddHttpClient<NotionClient>(client =>
{
    client.BaseAddress = new Uri("https://api.notion.com/v1/");
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    client.DefaultRequestHeaders.Add("Notion-Version", notionVersion);
});

// Registra o servidor MCP com transporte stdio e descobre os tools por reflexão.
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();
