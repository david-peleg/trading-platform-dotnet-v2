# Security Policy (Short)
- Secrets: user-secrets/KeyVault. אין בקוד.
- DB User: EXECUTE-Only על SPs. אין SELECT/INSERT ישיר.
- Logging: No PII/body content; TraceId בלבד; Serilog+OTel.
- Transport: TLS חובה בפרוד. אין TrustServerCertificate=True.
- Reviews: כל PR נבדק לכללי SP-Only.
