# DB Guidelines (SP-Only)
- אין inline SQL בקוד. כל פעולה דרך SP מאושר.
- כל שינוי סכימה/שאילתה = קובץ .sql תחת db/ddl או db/sp + סקירה.
- סוגי TVP סטנדרטיים: dbo.SymbolType, dbo.PriceDailyType, ...
- Naming: <Area>_<Verb> (e.g., Prices_UpsertDaily).
- גרסאות: שדה Extended Property 'Version' לכל SP (אופציונלי).
