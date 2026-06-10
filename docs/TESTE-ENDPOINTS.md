# Guia de teste dos endpoints (ordem recomendada)

Este roteiro valida o fluxo completo da API com autenticação JWT, criação de dados relacionados e limpeza respeitando chaves estrangeiras.

## Pré-requisitos

- API em execução (`dotnet run --project EsgResiduos.Api`)
- Swagger (`/swagger`) ou Postman

## 1) Auth

1. `POST /api/auth/register` — cria usuário e já retorna token
2. Clique em **Authorize** e informe: `Bearer {token}`
3. `POST /api/auth/login` — valida login com as mesmas credenciais

## 2) WasteTypes

1. `POST /api/wastetypes` — cria tipo de resíduo
2. `GET /api/wastetypes` — lista paginada
3. `GET /api/wastetypes/{wasteTypeId}` — busca por ID
4. `PUT /api/wastetypes/{wasteTypeId}` — atualiza

## 3) CollectionPoints

1. `POST /api/collectionpoints` — cria ponto de coleta
2. `GET /api/collectionpoints` — lista paginada
3. `GET /api/collectionpoints/{collectionPointId}` — busca por ID
4. `PUT /api/collectionpoints/{collectionPointId}` — atualiza

## 4) Collections

1. `POST /api/collections` — cria coleta (usando `wasteTypeId` e `collectionPointId`)
2. `GET /api/collections` — lista paginada
3. `GET /api/collections/{collectionId}` — busca por ID

## 5) CollectionAlerts

1. `GET /api/collectionalerts` — verifica alertas automáticos
2. `GET /api/collectionalerts/{alertId}` — busca por ID

## 6) Destinations

1. `POST /api/destinations` — destina a coleta (`collectionId`)
2. `GET /api/destinations` — lista paginada
3. `GET /api/destinations/{destinationId}` — busca por ID

> Dica: após criar destination, consulte `GET /api/collectionalerts` novamente para ver `DESTINATION_NOTIFICATION`.

## 7) Deletes (ordem obrigatória)

1. `DELETE /api/collections/{collectionId}`
2. `DELETE /api/collectionpoints/{collectionPointId}`
3. `DELETE /api/wastetypes/{wasteTypeId}`

⚠️ Sempre excluir nessa ordem para evitar conflito de FK.

---

## Checklist rápido de sucesso

- Login retorna token válido
- CRUD principal responde com 2xx
- Alertas aparecem após coleta/destinação
- Deletes funcionam na ordem correta
- Sem erro 500 por FK nos cenários esperados
