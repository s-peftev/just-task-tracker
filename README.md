# JustTaskTracker

This guide describes how to **run the application locally** on your machine for development.

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (matching the solution target framework)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) — must be **running** (SQL Server runs in a container via Aspire)
- **.NET Aspire workload** (install once per machine):

```bash
dotnet workload install aspire
```

Verify:

```bash
dotnet workload list
```

(`aspire` should appear in the installed workloads list.)

## Run locally

Start the **App Host** — it orchestrates SQL Server, migrations, API, and Web UI:

```bash
cd src/aspire/JustTaskTracker.AppHost
dotnet run
```

Or set **JustTaskTracker.AppHost** as the startup project in Visual Studio and press **F5**.

The **Aspire Dashboard** opens in the browser. Use the **Endpoints** column to open services:

| Resource | URL |
|----------|-----|
| **webui** | `https://localhost:7108` |
| **api** | `https://localhost:5001` |

Open **webui** from the dashboard (HTTPS link) so the browser origin matches API CORS.

## What starts

- **sql** — SQL Server in Docker (volume `jtt-sql-data`)
- **migrations** — DbUp (runs once before API)
- **api** — backend
- **webui** — Blazor WASM dev server
