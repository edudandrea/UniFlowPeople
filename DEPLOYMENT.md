# Deploy do UniFlow People

Este projeto pode rodar como um unico app ASP.NET Core servindo a API e o Angular.

## Variaveis de ambiente

Configure estas variaveis no ambiente de producao:

```text
ASPNETCORE_ENVIRONMENT=Production
Jwt__Key=<chave-com-pelo-menos-32-caracteres>
```

Banco de dados, use uma destas opcoes:

```text
DATABASE_URL=<url-postgres>
```

ou:

```text
ConnectionStrings__DefaultConnection=Host=...;Port=5432;Database=...;Username=...;Password=...
```

Opcionalmente, para aplicar migrations ao iniciar:

```text
ApplyMigrationsOnStartup=true
```

Se o frontend ficar em outro dominio, configure CORS:

```text
Cors__AllowedOrigins__0=https://seu-front.com
```

## Railway

1. Suba o repositorio para o GitHub.
2. No Railway, crie um projeto e adicione um servico PostgreSQL.
3. Crie o servico da aplicacao a partir do repositorio.
4. O Railway detecta o `Dockerfile` e usa a porta da variavel `PORT`.
5. Configure `Jwt__Key` e, se necessario, `ApplyMigrationsOnStartup=true`.
6. Gere um dominio publico em Settings > Networking.

O healthcheck configurado em `railway.json` usa `/health`.

## IIS

No servidor Windows:

1. Instale o IIS e o ASP.NET Core Hosting Bundle da versao do runtime usada pelo projeto.
2. Configure um banco PostgreSQL acessivel pelo servidor.
3. Publique a aplicacao:

```powershell
dotnet publish .\back\UniFlowPeople.Api\UniFlowPeople.Api.csproj -c Release -o .\publish\UniFlowPeople
```

O publish executa o build do Angular e copia os arquivos para `wwwroot`.

No IIS:

1. Crie um site ou application apontando para a pasta `publish\UniFlowPeople`.
2. No Application Pool, use `.NET CLR Version` como `No Managed Code`.
3. Configure as variaveis `ASPNETCORE_ENVIRONMENT`, `Jwt__Key` e a connection string no ambiente do site, no servidor, ou no `web.config` publicado.
4. Garanta permissao de escrita para a pasta `wwwroot\uploads` se o sistema for receber anexos.
