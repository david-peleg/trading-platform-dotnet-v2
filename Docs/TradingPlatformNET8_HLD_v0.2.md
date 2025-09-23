# TradingPlatformNET8 — HLD v0.2 (High‑Level Design, קצר)

**מטרה:** ניבוי מגמות/כיוון מחירים ל‑1–2 חודשים עבור S&P500 ו‑TA‑125.  
**עקרונות:** DB‑First · **SP‑Only** · .NET 8 · Dapper · Carter · Serilog · OTel · תשובות קצרות.

## 1) Scope
- Backfill שנה + עדכון יומי: Prices, News, Filings, Analyst Reports.
- Attribution: קישור ידיעה/אירוע ↔ שינוי מחיר (Direction ±/0, Confidence 0–1, Horizon 7/30/60).
- Features/Signals לשימוש מודלים.

## 2) Logical Architecture
- **Api** (Carter Minimal APIs) – טריגרים/שליפה.
- **Application** – UseCases + Jobs.
- **Domain** – Entities/Records + Interfaces.
- **Infrastructure** – Dapper Repos + HTTP/Parsers (SP‑Only!).
- **Tests** – xUnit.

## 3) Core Data Model (Summary)
- `dbo.Symbols` — Reference (Symbol PK, Name, Exchange, Country, Sector, IsActive).
- `dbo.Prices` — O/H/L/C/Volume (IX: Symbol, Dt).
- `dbo.RawNews` — Source, Url, Headline, BodyHash, PublishedAt, Lang.
- `dbo.Filings` — Symbol, Type, Period, Url, PublishedAt.
- `dbo.AnalystReports` — Symbol, Firm, Rating/Target, Url, PublishedAt.
- `dbo.Attributions` — ItemType, ItemId, Symbol, Direction, Confidence, HorizonD.
- `dbo.Features` — Symbol, Dt, FeatureName, Value, Window.
- `dbo.Signals` — Symbol, Dt, SignalType, Score, Horizon, ModelVersion.

## 4) Flows
1) **Ingestion** → Normalize → Validate → Upsert (SP).  
2) **Attribution** → Symbol match + Direction/Confidence/Horizon.  
3) **Features** → Price/News/NLP.  
4) **Signals** → מודלים קלים, כתיבה ל‑DB.

## 5) Non‑Goals v1
Execution trading; UI מלא; מודלי DL כבדים.

## 6) Ops
Polly (Retry/Timeout), Serilog + OTel, Health, Backfill שנה לכל דומיין.
