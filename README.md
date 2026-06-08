# AgroShield - AlertEngine Api

## Integrantes

| Nome                             | RM       |
| -------------------------------- | -------- |
| Guilherme Santiago da Silva      | RM552321 |
| Gabriel Souza Fiore              | RM553710 |
| Gustavo Gouvea Soares            | RM553842 |
| Pedro Henrique Mello Silva Alves | RM554223 |
| Gabriel Borba                    | RM553187 |

---

## Resumo da Solução

Servico **backend** em C# (.NET 8) para composicao de alertas agricolas e gerenciamento de terrenos.

- **Entrada:** dados do terreno + metricas geo (NDVI, umidade, etc.)
- **Saida:** JSON com `mensagemParaFala` para o servico **Python TTS**
- **Consumidor:** API Spring Boot (Java) — orquestrador
- **Banco de dados:** MySQL com Entity Framework Core

## Relacao com o Tema — Industria Espacial

O modulo processa metricas extraidas de imagens de satelite (NDVI, umidade). Essas metricas vem de satelites de observacao da Terra como Sentinel-2 e Landsat, que identificam a saude da vegetacao via espectro multiespectral. Sem esses dados orbitais, nao seria possivel detectar estresse hidrico.

O C# atua como motor de classificacao: recebe os dados brutos do satelite (via Java), aplica regras agronomicas e devolve um alerta priorizado com a mensagem para o TTS Python falar ao agricultor.

## ODS Relacionado

**ODS 2 — Fome Zero e Agricultura Sustentavel**
Alertas baseados em satelite ajudam a evitar perda de safra e otimizam o uso de agua na irrigacao.

**ODS 13 — Acao Contra a Mudanca Global do Clima**
Monitoramento continuo por satelite permite adaptar cultivos a eventos climaticos extremos com antecedencia.

## Funcionalidades

### 1. Composicao de Alertas Agricolas

- Endpoint: `POST /api/v1/alertas/compor`
- Avalia metricas NDVI, umidade, irrigacao e dias sem imagem satelite
- Classifica o risco em 4 niveis: BAIXA, MEDIA, ALTA, CRITICO
- Salva o alerta no historico do terreno automaticamente

### 2. CRUD de Terrenos

- `GET /api/v1/terrenos` — listar todos
- `GET /api/v1/terrenos/{id}` — buscar por ID
- `POST /api/v1/terrenos` — criar
- `PUT /api/v1/terrenos/{id}` — atualizar
- `DELETE /api/v1/terrenos/{id}` — deletar

### 3. Exportacao de Historico de Alertas

- `GET /api/v1/terrenos/{id}/alertas/exportar`
- Retorna historico completo em JSON

### Funcionalidade Avançada (Bônus)

Classificação automática de risco agrícola baseada em métricas
oriundas de monitoramento por satélite.

Níveis:

- BAIXA
- MEDIA
- ALTA
- CRITICO

A classificação é realizada pelo endpoint:

POST /api/v1/alertas/compor

## Como Rodar

```powershell
cd AgroShield.AlertEngine.Api
dotnet restore
dotnet run
```

- API: `http://localhost:5050`
- Swagger: `http://localhost:5050/swagger`

## Configuracao do Banco MySQL

Edite a connection string em `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=agroshield_csharp;User=root;Password=0000;SslMode=None;AllowPublicKeyRetrieval=True"
  }
}
```


## Pacotes NuGet

| Pacote                                 | Versao |
| -------------------------------------- | ------ |
| `Pomelo.EntityFrameworkCore.MySql`     | 8.0.2  |
| `Microsoft.EntityFrameworkCore`        | 8.0.10 |
| `Microsoft.EntityFrameworkCore.Design` | 8.0.10 |
| `Microsoft.EntityFrameworkCore.Tools`  | 8.0.10 |
| `Swashbuckle.AspNetCore`               | 6.6.2  | 


```powershell
dotnet add package nome_do_pacote
``` 

## Comandos de Migration

```powershell
dotnet ef migrations add InitialCreate
dotnet ef database update
```

A migration `InitialCreate` ja esta na pasta `Migrations/`, so rodar o `database update`.

## Regras implementadas

| Codigo                          | Severidade | Condicao                                          |
| ------------------------------- | ---------- | ------------------------------------------------- |
| `COLAPSO_PRODUTIVO`             | CRITICO    | NDVI < 0,15 + umidade >= 0,90 + irrigacao ativa   |
| `SEM_COBERTURA_SATELITE`        | CRITICO    | Sem imagem > 20 dias                              |
| `RISCO_FUNGO`                   | ALTA       | NDVI norte baixo + umidade alta + irrigacao ativa |
| `NDVI_CRITICO`                  | ALTA       | NDVI medio < 0,3                                  |
| `ESTRESSE_HIDRICO`              | MEDIA      | NDVI norte baixo, sul ok                          |
| `DADOS_SATELITE_DESATUALIZADOS` | MEDIA      | Sem imagem > 10 dias                              |
| `IRRIGACAO_OPCIONAL`            | BAIXA      | Zona sul com NDVI alto                            |
| `AREA_CULTIVO_INATIVA`          | BAIXA      | emCultivo false com area > 0                      |
| `SAUDAVEL`                      | BAIXA      | Nenhuma regra disparada                           |



## Testes

```powershell
dotnet test
```

7 testes unitarios cobrindo todos os cenarios de alerta.

## Estrutura do Projeto

```
AgroShield.AlertEngine.Api/
├── Controllers/
│   ├── AlertasController.cs
│   └── TerrenosController.cs
├── Data/
│   └── AgroShieldDbContext.cs
├── Entities/
│   ├── Terreno.cs
│   └── HistoricoAlerta.cs
├── Migrations/
│   ├── 20260601000000_InitialCreate.cs
│   └── AgroShieldDbContextModelSnapshot.cs
├── Models/
│   ├── AlertaAvaliado.cs
│   ├── AlertaComposicaoResponse.cs
│   ├── ComporAlertaRequest.cs
│   ├── GeoMetricasInput.cs
│   ├── TerrenoRequest.cs
│   └── TerrenoResponse.cs
├── Services/
│   ├── IAlertCompositionService.cs
│   └── AlertCompositionService.cs
├── Program.cs
├── appsettings.json
└── AGROSHIELD_CSHARP.postman_collection.json

AgroShield.AlertEngine.Api.Tests/
└── AlertCompositionServiceTests.cs
```

## Decisoes Tecnicas

Optamos por **Controller → Service → DbContext** direto, sem camada de Repository, porque o escopo da GS nao justifica a complexidade extra. O `AlertCompositionService` acumula candidatos em lista e escolhe o de maior prioridade — isso resolve o caso de multiplas regras verdadeiras ao mesmo tempo sem precisar de if/else aninhado.

Os niveis CRITICO (`COLAPSO_PRODUTIVO` e `SEM_COBERTURA_SATELITE`) foram adicionados porque o PDF da GS pede classificacao em 4 niveis como bonus. Os limiares foram baseados em referencias agronomicas para culturas de soja e milho no cerrado brasileiro.

Escolhemos `Pomelo.EntityFrameworkCore.MySql` por ser o provider MySQL mais ativo para .NET 8. Os indices foram colocados em `Nome`, `TerrenoId`, `Codigo` e `CriadoEm` que sao os campos mais consultados nas queries de filtro e relatorio.

O `AlertasController` valida se o terreno existe antes de gerar o alerta — isso evita historico orfao caso o Java mande um `terrenoId` invalido. O alerta e salvo no banco logo apos a composicao, na mesma requisicao.

---

- limite de caracteres em campos de texto
