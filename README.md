# Notion MCP Server (.NET / C#)

> **Português** · [English below ↓](#-english)

Um servidor **MCP (Model Context Protocol)** em **C# / .NET 8** que conecta qualquer cliente
compatível com MCP à sua conta do **Notion**, permitindo **buscar, ler, criar e editar
páginas** por linguagem natural.

<p>
  <img alt=".NET 8" src="https://img.shields.io/badge/.NET-8.0-512BD4">
  <img alt="C#" src="https://img.shields.io/badge/C%23-language-239120">
  <img alt="MCP" src="https://img.shields.io/badge/MCP-Model_Context_Protocol-000000">
  <img alt="License MIT" src="https://img.shields.io/badge/license-MIT-green">
</p>

---

## 🇧🇷 Português

### Por que este projeto importa
Servidores MCP em **Python** existem aos milhares. Em **.NET** são raros. Este projeto
demonstra o protocolo que está definindo a arquitetura de agentes de IA em 2026 rodando
sobre o stack corporativo mais usado do mercado.

### O que ele faz
Expõe 5 ferramentas que o modelo pode chamar:

| Ferramenta | O que faz |
|------------|-----------|
| `notion_search` | Busca páginas pelo título |
| `notion_read_page` | Lê o conteúdo textual de uma página |
| `notion_query_database` | Lista as linhas de um banco de dados |
| `notion_create_page` | Cria uma nova página dentro de outra |
| `notion_append_text` | Acrescenta texto ao final de uma página |

Exemplos de comando depois de conectado:
- *"Procure minha página de anotações e me resuma."*
- *"Crie uma página 'Ideias' dentro da página X com estes 3 tópicos."*
- *"Liste as tarefas abertas do meu banco de dados de projetos."*

### Arquitetura
```
Cliente MCP (LLM)  ──MCP (stdio)──►  Notion MCP Server (.NET)  ──REST API──►  Notion
```
```
Program.cs             → configura o host, o HttpClient e o transporte MCP (stdio)
NotionClient.cs        → wrapper HTTP sobre a REST API do Notion
Tools/NotionTools.cs   → as 5 ferramentas MCP que o modelo enxerga
```

### Como rodar
**Pré-requisitos:** [.NET 8 SDK](https://dotnet.microsoft.com/download) e uma conta no Notion.

1. **Crie a integração:** acesse <https://www.notion.so/my-integrations> → **New integration** → copie o **Internal Integration Token**.
2. **Conecte a página:** abra a página/banco no Notion → **•••** → **Connections** → conecte sua integração. *(Sem isso, a integração não enxerga a página.)*
3. **Configure o token e rode:**
   ```bash
   export NOTION_TOKEN=secret_xxxxxxxxxxxxxxxxxxxx
   dotnet run
   ```

### Testar com o MCP Inspector
O jeito mais rápido de testar, sem depender de nenhum cliente específico:
```bash
npx @modelcontextprotocol/inspector dotnet run
```
Abra a URL que aparece no terminal, vá em **Tools**, e chame as ferramentas manualmente
(ex.: `notion_search` com um termo do seu Notion).

### Conectar a um cliente MCP
A maioria dos clientes MCP (Cursor, Windsurf, etc.) usa o mesmo formato de configuração.
Veja `mcp-client-config.example.json`:
```json
{
  "mcpServers": {
    "notion": {
      "command": "dotnet",
      "args": ["run", "--project", "/CAMINHO/ABSOLUTO/PARA/notion-mcp-server"],
      "env": { "NOTION_TOKEN": "secret_xxxxxxxxxxxx" }
    }
  }
}
```

### Decisões técnicas
- **Token só por variável de ambiente** — nenhum segredo no código ou no repositório.
- **Logs no stderr** — em servidores stdio o `stdout` é o canal do protocolo; log no stdout corromperia a comunicação.
- **`NOTION_VERSION` configurável** — o header `Notion-Version` fica em variável de ambiente (padrão `2022-06-28`).
- **Saída já resumida** — cada ferramenta devolve texto limpo em vez de JSON cru, economizando tokens.

---

## 🇬🇧 English

A **Model Context Protocol (MCP)** server written in **C# / .NET 8** that connects any
MCP-compatible client to your **Notion** account, letting the model **search, read, create
and edit pages** through natural language.

### Why this project matters
MCP servers in **Python** exist by the thousands. In **.NET** they are rare. This project
shows the protocol that is shaping AI-agent architecture in 2026 running on the most widely
used enterprise stack.

### What it does
It exposes 5 tools the model can call:

| Tool | What it does |
|------|--------------|
| `notion_search` | Search pages by title |
| `notion_read_page` | Read a page's text content |
| `notion_query_database` | List rows of a database |
| `notion_create_page` | Create a new page inside another |
| `notion_append_text` | Append text to an existing page |

Example commands once connected:
- *"Find my meeting notes page and summarize it."*
- *"Create an 'Ideas' page inside page X with these 3 bullets."*
- *"List the open tasks in my projects database."*

### Architecture
```
MCP client (LLM)  ──MCP (stdio)──►  Notion MCP Server (.NET)  ──REST API──►  Notion
```
```
Program.cs             → host, Notion HttpClient and the MCP (stdio) transport
NotionClient.cs        → thin HTTP wrapper over the Notion REST API
Tools/NotionTools.cs   → the 5 MCP tools the model sees
```

### Getting started
**Prerequisites:** [.NET 8 SDK](https://dotnet.microsoft.com/download) and a Notion account.

1. **Create an integration:** go to <https://www.notion.so/my-integrations> → **New integration** → copy the **Internal Integration Token**.
2. **Connect a page:** open the page/database in Notion → **•••** → **Connections** → connect your integration. *(Without this, the integration can't see the page.)*
3. **Set the token and run:**
   ```bash
   export NOTION_TOKEN=secret_xxxxxxxxxxxxxxxxxxxx
   dotnet run
   ```

### Test with the MCP Inspector
The fastest way to test, without depending on any specific client:
```bash
npx @modelcontextprotocol/inspector dotnet run
```
Open the URL printed in the terminal, go to **Tools**, and call the tools manually
(e.g. `notion_search` with a term from your Notion).

### Connect an MCP client
Most MCP clients (Cursor, Windsurf, etc.) use the same config format. See
`mcp-client-config.example.json`:
```json
{
  "mcpServers": {
    "notion": {
      "command": "dotnet",
      "args": ["run", "--project", "/ABSOLUTE/PATH/TO/notion-mcp-server"],
      "env": { "NOTION_TOKEN": "secret_xxxxxxxxxxxx" }
    }
  }
}
```

### Technical decisions
- **Token via environment variable only** — no secrets in code or repo.
- **Logs to stderr** — for stdio servers, `stdout` is the protocol channel; logging there would corrupt it.
- **Configurable `NOTION_VERSION`** — the `Notion-Version` header lives in an env var (default `2022-06-28`).
- **Pre-summarized output** — each tool returns clean text instead of raw JSON, saving tokens.

---

## Roadmap
- [ ] HTTP transport (`ModelContextProtocol.AspNetCore`) to host it as a service
- [ ] Support for Notion API 2025-09-03 data sources
- [ ] Tool to update database row properties
- [ ] Integration tests against a sandbox workspace

## License
MIT — see [LICENSE](LICENSE).

Built by **Gabriel Oliveira** — .NET developer & AI agents / MCP specialist ·
[LinkedIn](https://www.linkedin.com/in/gabrielandrade-oliveira)
