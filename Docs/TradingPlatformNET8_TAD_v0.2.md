# TradingPlatformNET8 — TAD/LLD v0.2 (Technical/Low‑Level Design, קצר)

**סטנדרטים:** .NET 8 · C#12 · Dapper · **SP‑Only** · Carter · xUnit · Serilog · OTel.

## 1) Solution Layout
`Api` · `Application` · `Domain` · `Infrastructure` · `Tests` · `Docs` · `prompts` · `db/ddl, db/sp`

## 2) DB Schema (תקציר)
- **Symbols**: `Symbol nvarchar(20) PK, Name, Exchange, Country, Sector, IsActive bit`  
- **Prices**: `Symbol, Dt date, Open, High, Low, Close, Volume, Source` (IX(Symbol,Dt))  
- **RawNews**: `Id, Source, Url, Headline, BodyHash binary(32), PublishedAt, Lang`  
- **Filings**: `Id, Symbol, Type, PeriodStart/End, Url, PublishedAt`  
- **AnalystReports**: `Id, Symbol, Firm, Rating/Target, Url, PublishedAt`  
- **Attributions**: `ItemType tinyint, ItemId bigint, Symbol, Direction smallint, Confidence float, HorizonD smallint`  
- **Features / Signals**: כמו ב‑HLD עם אינדקסי זמן.

## 3) Interfaces (עיקריות)
- `ISymbolRegistry` — `UpsertAsync`, `GetAllAsync`, `TryMatchAsync(text)`  
- `IRawNewsRepository` — `UpsertAsync`, `GetLatestAsync`  
- `IPriceRepository` — `BackfillAsync`, `UpsertDailyAsync`, `GetSeries`  
- `IFilingsRepository`, `IAnalystReportsRepository` — `UpsertAsync`  
- `IAttributionService` — `LinkAsync(item) ⇒ [(symbol, direction, confidence, horizonD)]`  
- `IFeatureService` — `ComputeAsync(symbol, dtRange)`  
- `ISignalService` — `GenerateAsync(symbol, horizon)`

> כל המימושים ניגשים ל‑DB דרך **Stored Procedures** בלבד (CommandType.StoredProcedure).

## 4) Jobs
- `SymbolsSeedJob` (חד‑פעמי)  
- `PricesBackfillJob (365d)` + `PricesDailyJob`  
- `NewsBackfillJob (365d)` + `NewsDailyJob`  
- `FilingsBackfillJob` + `Daily`  
- `AnalystBackfillJob` + `Daily`  
- `DailyAggregationJob` (Features/Signals)

## 5) Endpoints (Carter, תקציר)
- `POST /ingestion/symbols/seed`  
- `POST /ingestion/prices/backfill?days=365` · `POST /ingestion/prices/daily/run`  
- `POST /ingestion/news/backfill?days=365` · `POST /ingestion/news/daily/run`  
- `POST /ingestion/filings/backfill?days=365` · `POST /ingestion/analyst/backfill?days=365`  
- `POST /features/recompute?symbol=...&days=365` · `POST /signals/generate?horizonD=30`

## 6) Security (תמצית)
Least‑privilege DB users (EXECUTE on SPs only), secrets ב‑user‑secrets/KeyVault, TLS בפרוד, ללא PII בלוגים.

## 7) Testing
xUnit: Hash/Parser/Repo; Integration: Repo + Endpoints; Contract tests ל‑SPs.
