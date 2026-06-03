# ESG Resíduos API

API RESTful para gerenciamento de resíduos sólidos com foco em práticas ESG (Environmental, Social and Governance). Permite o controle de pontos de coleta, tipos de resíduos, coletas realizadas, destinações e alertas automatizados de capacidade.

---

## 📋 Sumário

- [Visão Geral](#visão-geral)
- [Tecnologias](#tecnologias)
- [Arquitetura](#arquitetura)
- [Modelos de Dados](#modelos-de-dados)
- [Endpoints da API](#endpoints-da-api)
- [Autenticação](#autenticação)
- [Configuração](#configuração)
- [Como Executar](#como-executar)
- [Docker](#docker)
- [Testes](#testes)

---

## Visão Geral

O **ESG Resíduos** é uma plataforma de gestão ambiental que possibilita:

- Cadastro e monitoramento de **pontos de coleta** de resíduos
- Registro de **tipos de resíduos** (categorias e descrições)
- Lançamento de **coletas** com volume (kg) e rastreamento de status
- Registro de **destinações** (reciclagem, compostagem, descarte controlado etc.)
- Geração automática de **alertas** quando um ponto de coleta atinge o volume limite configurado
- Controle de acesso via **autenticação JWT**

---

## Tecnologias

| Tecnologia | Versão |
|---|---|
| .NET / ASP.NET Core | 8.0 |
| Entity Framework Core | - |
| SQL Server | - |
| JWT Bearer Authentication | - |
| Swagger / OpenAPI | - |
| xUnit | - |
| Docker | - |

---

## Arquitetura

O projeto segue uma arquitetura em camadas simples dentro de uma única Web API:

```
EsgResiduos.Api/
├── Controllers/        # Camada de entrada HTTP (rotas e respostas)
├── Services/           # Regras de negócio
├── Models/             # Entidades do domínio
├── DTOs/
│   ├── Request/        # Objetos de entrada (payload das requisições)
│   └── Response/       # Objetos de saída (payload das respostas)
├── Data/               # DbContext (Entity Framework Core)
├── Migrations/         # Migrações do banco de dados
├── Exceptions/         # Exceções customizadas e handler global
└── Program.cs          # Configuração e inicialização da aplicação

EsgResiduos.Tests/      # Projeto de testes (xUnit)
```

---

## Modelos de Dados

### `User` — Usuário do sistema
| Campo | Tipo | Descrição |
|---|---|---|
| `Id` | int | Identificador único |
| `Name` | string | Nome completo |
| `Email` | string | E-mail (usado no login) |
| `PasswordHash` | string | Senha em hash |
| `Role` | string | Perfil do usuário (`USER`, `ADMIN`) |
| `CreatedAt` | DateTime | Data de criação |

---

### `CollectionPoint` — Ponto de Coleta
| Campo | Tipo | Descrição |
|---|---|---|
| `Id` | int | Identificador único |
| `Name` | string | Nome do ponto de coleta |
| `CapacityKg` | decimal | Capacidade total em kg |
| `AlertVolumeKg` | decimal | Volume que dispara alerta |
| `OccupiedVolumeKg` | decimal | Volume ocupado atualmente |
| `Status` | string | Status atual (`AVAILABLE`, `FULL`, `ALERT`) |
| `UpdatedAt` | DateTime | Última atualização |

---

### `WasteType` — Tipo de Resíduo
| Campo | Tipo | Descrição |
|---|---|---|
| `Id` | int | Identificador único |
| `WasteCategory` | string | Categoria do resíduo (ex.: Orgânico, Plástico) |
| `Description` | string? | Descrição opcional |

---

### `Collection` — Coleta
| Campo | Tipo | Descrição |
|---|---|---|
| `Id` | int | Identificador único |
| `CollectionPointId` | int | Referência ao ponto de coleta |
| `WasteTypeId` | int | Referência ao tipo de resíduo |
| `CollectedAt` | DateTime | Data/hora da coleta |
| `VolumeKg` | decimal | Volume coletado em kg |
| `Status` | string | Status da coleta (`OPEN`, `DESTINATED`) |
| `DestinatedAt` | DateTime? | Data de destinação (quando aplicável) |
| `DestinationHistory` | string? | Histórico resumido de destinações |

---

### `Destination` — Destinação
| Campo | Tipo | Descrição |
|---|---|---|
| `Id` | int | Identificador único |
| `CollectionId` | int | Referência à coleta |
| `DestinatedAt` | DateTime | Data/hora da destinação |
| `DestinationName` | string | Nome do destino (empresa, local) |
| `ProcessingType` | string | Tipo de processamento (ex.: Reciclagem, Compostagem) |
| `DestinatedVolumeKg` | decimal | Volume destinado em kg |

---

### `CollectionAlert` — Alerta de Coleta
| Campo | Tipo | Descrição |
|---|---|---|
| `Id` | int | Identificador único |
| `CollectionPointId` | int | Referência ao ponto de coleta |
| `CollectionId` | int? | Referência à coleta (opcional) |
| `AlertedAt` | DateTime | Data/hora do alerta |
| `AlertType` | string | Tipo do alerta (ex.: `CAPACITY_WARNING`) |
| `Message` | string | Mensagem descritiva do alerta |

---

## Endpoints da API

### 🔐 Auth — `/api/auth`

| Método | Rota | Descrição | Auth |
|---|---|---|---|
| `POST` | `/api/auth/register` | Cadastra novo usuário | ❌ |
| `POST` | `/api/auth/login` | Autentica e retorna JWT | ❌ |

<details>
<summary>Exemplo: POST /api/auth/register</summary>

**Request body:**
```json
{
  "name": "João Silva",
  "email": "joao@email.com",
  "password": "Senha@123"
}
```

**Response 201:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "name": "João Silva",
  "email": "joao@email.com"
}
```
</details>

<details>
<summary>Exemplo: POST /api/auth/login</summary>

**Request body:**
```json
{
  "email": "joao@email.com",
  "password": "Senha@123"
}
```

**Response 200:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "name": "João Silva",
  "email": "joao@email.com"
}
```
</details>

---

### ♻️ Tipos de Resíduo — `/api/wastetypes`

| Método | Rota | Descrição | Auth |
|---|---|---|---|
| `GET` | `/api/wastetypes` | Lista todos os tipos (paginado) | ✅ |
| `GET` | `/api/wastetypes/{id}` | Busca por ID | ✅ |
| `POST` | `/api/wastetypes` | Cadastra novo tipo | ✅ |
| `PUT` | `/api/wastetypes/{id}` | Atualiza tipo | ✅ |
| `DELETE` | `/api/wastetypes/{id}` | Remove tipo | ✅ |

---

### 📍 Pontos de Coleta — `/api/collectionpoints`

| Método | Rota | Descrição | Auth |
|---|---|---|---|
| `GET` | `/api/collectionpoints` | Lista todos (paginado) | ✅ |
| `GET` | `/api/collectionpoints/{id}` | Busca por ID | ✅ |
| `POST` | `/api/collectionpoints` | Cadastra novo ponto | ✅ |
| `PUT` | `/api/collectionpoints/{id}` | Atualiza ponto | ✅ |
| `DELETE` | `/api/collectionpoints/{id}` | Remove ponto | ✅ |

---

### 🗑️ Coletas — `/api/collections`

| Método | Rota | Descrição | Auth |
|---|---|---|---|
| `GET` | `/api/collections` | Lista todas (paginado) | ✅ |
| `GET` | `/api/collections/{id}` | Busca por ID | ✅ |
| `POST` | `/api/collections` | Registra nova coleta | ✅ |
| `DELETE` | `/api/collections/{id}` | Remove coleta | ✅ |

---

### 🏭 Destinações — `/api/destinations`

| Método | Rota | Descrição | Auth |
|---|---|---|---|
| `GET` | `/api/destinations` | Lista todas (paginado) | ✅ |
| `GET` | `/api/destinations/{id}` | Busca por ID | ✅ |
| `POST` | `/api/destinations` | Registra nova destinação | ✅ |
| `DELETE` | `/api/destinations/{id}` | Remove destinação | ✅ |

---

### 🚨 Alertas — `/api/collectionalerts`

| Método | Rota | Descrição | Auth |
|---|---|---|---|
| `GET` | `/api/collectionalerts` | Lista todos (paginado) | ✅ |
| `GET` | `/api/collectionalerts/{id}` | Busca por ID | ✅ |

> Os alertas são gerados automaticamente pelo sistema quando o volume de um ponto de coleta atinge o `AlertVolumeKg` configurado.

---

### 📄 Paginação

Todos os endpoints de listagem suportam paginação via query string:

```
GET /api/collections?page=1&pageSize=10
```

**Resposta paginada:**
```json
{
  "data": [...],
  "page": 1,
  "pageSize": 10,
  "totalCount": 42,
  "totalPages": 5
}
```

---

## Autenticação

A API utiliza **JWT Bearer Token**. Para acessar endpoints protegidos:

1. Realize o login em `POST /api/auth/login`
2. Copie o `token` retornado
3. Envie o header em todas as requisições protegidas:

```
Authorization: Bearer {seu_token_aqui}
```

O token expira em **8 horas** (configurável via `appsettings.json`).

---

## Configuração

### `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=EsgResiduosDb;User Id=sa;Password=sua_senha;"
  },
  "Jwt": {
    "Key": "sua-chave-secreta-jwt-minimo-32-caracteres",
    "Issuer": "EsgResiduos.Api",
    "Audience": "EsgResiduos.Client",
    "ExpirationHours": 8
  }
}
```

> ⚠️ **Nunca suba a `Jwt:Key` real para o repositório.** Utilize variáveis de ambiente ou `appsettings.Development.json` (já no `.gitignore`) em ambientes sensíveis.

### Variáveis de ambiente (alternativa ao appsettings)

```bash
ConnectionStrings__DefaultConnection="Server=...;Database=...;"
Jwt__Key="sua-chave-secreta"
```

---

## Como Executar

### Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (local ou remoto) **ou** Docker

### Passo a passo

```bash
# 1. Clone o repositório
git clone https://github.com/lenonmerlo/esg-residuos-dotnet.git
cd esg-residuos-dotnet

# 2. Configure a connection string no appsettings.json (ou via variável de ambiente)

# 3. Aplique as migrações do banco de dados
cd EsgResiduos.Api
dotnet ef database update

# 4. Execute a aplicação
dotnet run
```

A API estará disponível em:
- `http://localhost:5000`
- `https://localhost:5001`

### Swagger UI

Acesse a documentação interativa em:
```
https://localhost:5001/swagger
```

---

## Docker

### Build da imagem

```bash
cd EsgResiduos.Api
docker build -t esg-residuos-api .
```

### Executar o container

```bash
docker run -d \
  -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Server=host.docker.internal;Database=EsgResiduosDb;User Id=sa;Password=sua_senha;TrustServerCertificate=True;" \
  -e Jwt__Key="sua-chave-secreta-jwt-minimo-32-caracteres" \
  --name esg-residuos-api \
  esg-residuos-api
```

### Docker Compose (exemplo)

```yaml
version: '3.8'
services:
  api:
    build: ./EsgResiduos.Api
    ports:
      - "8080:8080"
    environment:
      - ConnectionStrings__DefaultConnection=Server=db;Database=EsgResiduosDb;User Id=sa;Password=Senha@123;TrustServerCertificate=True;
      - Jwt__Key=sua-chave-secreta-jwt-minimo-32-caracteres
    depends_on:
      - db

  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      SA_PASSWORD: "Senha@123"
      ACCEPT_EULA: "Y"
    ports:
      - "1433:1433"
```

---

## Testes

O projeto `EsgResiduos.Tests` utiliza **xUnit** e banco de dados **InMemory** para testes de integração dos controllers.

```bash
# Executar todos os testes
dotnet test

# Executar com relatório de cobertura (requer coverlet)
dotnet test --collect:"XPlat Code Coverage"
```

### Cobertura de testes atual

| Controller | Teste |
|---|---|
| `CollectionController` | ✅ |
| `CollectionPointController` | ✅ |
| `CollectionAlertController` | ✅ |
| `DestinationController` | ✅ |
| `WasteTypeController` | ✅ |

---

## Tratamento de Erros

A API utiliza um **GlobalExceptionHandler** que padroniza as respostas de erro no formato [RFC 7807 (Problem Details)](https://datatracker.ietf.org/doc/html/rfc7807):

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "Coleta com ID 99 não encontrada."
}
```

| Exceção | Status HTTP |
|---|---|
| `NotFoundException` | 404 Not Found |
| `AppException` | 400 Bad Request / 409 Conflict |
| Não tratada | 500 Internal Server Error |

---

## Licença

Este projeto está sob a licença MIT. Consulte o arquivo [LICENSE](LICENSE) para mais detalhes.
