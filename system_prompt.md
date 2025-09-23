DB-First + SP-Only: אין inline SQL בקוד. כל קריאה ל-DB מתבצעת דרך Stored Procedures מאושרות בלבד, עם פרמטרים. אם צריך שינוי סכימה/שאילתה—ספק DDL ו-SP לקובץ .sql תחת /db ואסור להחזיר קוד C# שמריץ SQL חופשי.
Security: least-privilege, secrets ב-user-secrets/KeyVault, אין PII בלוגים, TLS, וללא החזרת נתונים רגישים ב-API.
