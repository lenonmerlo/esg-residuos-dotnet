# ESG Resíduos API

API RESTful para gerenciamento de resíduos sólidos com foco em práticas ESG (Environmental, Social and Governance). Permite o controle de pontos de coleta, tipos de resíduos, coletas realizadas, destinações e alertas automatizados de capacidade.

---

## Sumário

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
- **Notificações** automáticas sobre destinação correta dos resíduos ao registrar uma destinação
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

O projeto segue o padrão **MVVM** (Model-View-ViewModel), com separação clara entre apresentação e regras de negócio:

| Camada MVVM | Pasta no projeto | Responsabilidade |
|---|---|---|
| **Model** | `Models/` + `Data/` | Entidades do domínio e persistência (EF Core) |
| **View** | `Controllers/` + `DTOs/` | Exposição HTTP — rotas, contratos de entrada/saída |
| **ViewModel** | `ViewModels/` | Orquestração da lógica de negócio e transformação Model ↔ View |

```
EsgResiduos.Api/
├── Controllers/        # View — endpoints REST
├── ViewModels/         # ViewModel — regras de negócio
├── Models/             # Model — entidades do domínio
├── DTOs/
│   ├── Request/        # Contratos de entrada (payload das requisições)
│   └── Response/       # Contratos de saída (payload das respostas)
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
| `Status` | string | Status atual (`AVAILABLE`, `NEAR_LIMIT`, `CRITICAL`) |
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
| `AlertType` | string | Tipo do alerta (`LIMIT_REACHED`, `CAPACITY_EXCEEDED`, `DESTINATION_NOTIFICATION`) |
| `Message` | string | Mensagem descritiva do alerta |

---

## Endpoints da API

### Auth — `/api/auth`

| Método | Rota | Descrição | Auth |
|---|---|---|---|
| `POST` | `/api/auth/register` | Cadastra novo usuário | Não |
| `POST` | `/api/auth/login` | Autentica e retorna JWT | Não |

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

### Tipos de Resíduo — `/api/wastetypes`

| Método | Rota | Descrição | Auth |
|---|---|---|---|
| `GET` | `/api/wastetypes` | Lista todos os tipos (paginado) | Sim |
| `GET` | `/api/wastetypes/{id}` | Busca por ID | Sim |
| `POST` | `/api/wastetypes` | Cadastra novo tipo | Sim |
| `PUT` | `/api/wastetypes/{id}` | Atualiza tipo | Sim |
| `DELETE` | `/api/wastetypes/{id}` | Remove tipo | Sim |

---

### Pontos de Coleta — `/api/collectionpoints`

| Método | Rota | Descrição | Auth |
|---|---|---|---|
| `GET` | `/api/collectionpoints` | Lista todos (paginado) | Sim |
| `GET` | `/api/collectionpoints/{id}` | Busca por ID | Sim |
| `POST` | `/api/collectionpoints` | Cadastra novo ponto | Sim |
| `PUT` | `/api/collectionpoints/{id}` | Atualiza ponto | Sim |
| `DELETE` | `/api/collectionpoints/{id}` | Remove ponto | Sim |

---

### Coletas — `/api/collections`

| Método | Rota | Descrição | Auth |
|---|---|---|---|
| `GET` | `/api/collections` | Lista todas (paginado) | Sim |
| `GET` | `/api/collections/{id}` | Busca por ID | Sim |
| `POST` | `/api/collections` | Registra nova coleta | Sim |
| `DELETE` | `/api/collections/{id}` | Remove coleta | Sim |

---

### Destinações — `/api/destinations`

| Método | Rota | Descrição | Auth |
|---|---|---|---|
| `GET` | `/api/destinations` | Lista todas (paginado) | Sim |
| `GET` | `/api/destinations/{id}` | Busca por ID | Sim |
| `POST` | `/api/destinations` | Registra nova destinação | Sim |

---

### Alertas — `/api/collectionalerts`

| Método | Rota | Descrição | Auth |
|---|---|---|---|
| `GET` | `/api/collectionalerts` | Lista todos (paginado) | Sim |
| `GET` | `/api/collectionalerts/{id}` | Busca por ID | Sim |

> Os alertas são gerados automaticamente pelo sistema quando o volume de um ponto de coleta atinge o `AlertVolumeKg` configurado. Ao registrar uma destinação, o sistema também cria uma notificação (`DESTINATION_NOTIFICATION`) orientando sobre o descarte correto do resíduo.

---

### Paginação

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

> Não commite a `Jwt:Key` real. Use variáveis de ambiente ou `appsettings.Development.json`.

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

A API sobe em `http://localhost:5016`. O terminal exibe o link do Swagger ao iniciar.

```
http://localhost:5016/swagger
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

Cada controller possui ao menos um teste de integração validando status HTTP 200.

---

## Tratamento de Erros

A API utiliza um **GlobalExceptionHandler** que padroniza as respostas de erro no formato [RFC 7807 (Problem Details)](https://datatracker.ietf.org/doc/html/rfc7807):

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "title": "Coleta com id 99 não encontrado."
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
