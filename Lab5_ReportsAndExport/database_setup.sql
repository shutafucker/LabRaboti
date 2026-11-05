-- ========================================================
-- Полный скрипт создания структуры базы данных (Supabase PostgreSQL)
-- Включает все сущности лаб №1 - №5:
-- Computers, Sessions, Users, AuditLogs + Триггеры аудита
-- ========================================================

-- 1. Таблица компьютеров
CREATE TABLE IF NOT EXISTS "Computers" (
    "Id" SERIAL PRIMARY KEY,
    "Name" TEXT NOT NULL,
    "IsVip" BOOLEAN NOT NULL DEFAULT FALSE
);

-- 2. Таблица сессий
CREATE TABLE IF NOT EXISTS "Sessions" (
    "Id" SERIAL PRIMARY KEY,
    "GamerName" TEXT NOT NULL,
    "StartTime" TIMESTAMP WITH TIME ZONE NOT NULL,
    "EndTime" TIMESTAMP WITH TIME ZONE NULL,
    "ComputerId" INTEGER NOT NULL REFERENCES "Computers"("Id") ON DELETE CASCADE
);

-- 3. Таблица пользователей
CREATE TABLE IF NOT EXISTS "Users" (
    "Id" SERIAL PRIMARY KEY,
    "Username" VARCHAR(50) NOT NULL UNIQUE,
    "Role" VARCHAR(20) NOT NULL DEFAULT 'Operator'
    "PasswordHash" VARCHAR(100) NOT NULL,
);

-- 4. Таблица журнала аудита
CREATE TABLE IF NOT EXISTS "AuditLogs" (
    "Id" BIGSERIAL PRIMARY KEY,
    "TableName" TEXT NOT NULL,
    "Action" TEXT NOT NULL,
    "OldData" TEXT NULL,
    "NewData" TEXT NUL
    "ChangedBy" TEXT NULL,
    "ChangedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT now()
);

-- 5. Триггерная функция аудита
CREATE OR REPLACE FUNCTION process_audit()
RETURNS TRIGGER AS $$
DECLARE
    current_user_login TEXT;
BEGIN
    current_user_login := current_setting('app.current_user', true);

    IF (TG_OP = 'DELETE') THEN
        INSERT INTO "AuditLogs"("TableName", "Action", "OldData", "NewData", "ChangedBy", "ChangedAt")
        VALUES (TG_TABLE_NAME, 'DELETE', to_jsonb(OLD)::text, NULL, current_user_login, now());
        RETURN OLD;
    ELSIF (TG_OP = 'UPDATE') THEN
        INSERT INTO "AuditLogs"("TableName", "Action", "OldData", "NewData", "ChangedBy", "ChangedAt")
        VALUES (TG_TABLE_NAME, 'UPDATE', to_jsonb(OLD)::text, to_jsonb(NEW)::text, current_user_login, now());
        RETURN NEW;
    ELSIF (TG_OP = 'INSERT') THEN
        INSERT INTO "AuditLogs"("TableName", "Action", "OldData", "NewData", "ChangedBy", "ChangedAt")
        VALUES (TG_TABLE_NAME, 'INSERT', NULL, to_jsonb(NEW)::text, current_user_login, now());
        RETURN NEW;
    END IF;
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

-- 6. Триггеры аудита
DROP TRIGGER IF EXISTS audit_computers ON "Computers";
CREATE TRIGGER audit_computers
AFTER INSERT OR UPDATE OR DELETE ON "Computers"
FOR EACH ROW EXECUTE FUNCTION process_audit();

DROP TRIGGER IF EXISTS audit_sessions ON "Sessions";
CREATE TRIGGER audit_sessions
AFTER INSERT OR UPDATE OR DELETE ON "Sessions"
FOR EACH ROW EXECUTE FUNCTION process_audit();

-- 7. Защита от модификации и удаления записей аудита
CREATE OR REPLACE FUNCTION prevent_audit_modification()
RETURNS TRIGGER AS $$
BEGIN
    RAISE EXCEPTION 'Audit logs cannot be modified or deleted';
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS audit_immutable ON "AuditLogs";
CREATE TRIGGER audit_immutable
BEFORE UPDATE OR DELETE ON "AuditLogs"
FOR EACH ROW EXECUTE FUNCTION prevent_audit_modification();

-- 8. Начальные пользователи (Пароли: admin123 / operator123)
INSERT INTO "Users" ("Username", "PasswordHash", "Role")
VALUES 
    ('admin', '$2b$12$o/V7Px3gK9685PjQPy6SRuOUMttEG1LUziLIpZlUJYSm/NsnUve3.', 'Admin'),
    ('operator', '$2b$12$oHh6v12H2V/09uJjVekPxeQGsmUaHj6k1mK6hQf7Y1p1j8X0P9M4q', 'Operator')
ON CONFLICT ("Username") DO NOTHING;

-- 9. Тестовые ПК
INSERT INTO "Computers" ("Name", "IsVip")
VALUES 
    ('PC-01 (VIP)', TRUE),
    ('PC-02', FALSE),
    ('PC-03', FALSE),
    ('PC-04 (VIP)', TRUE)
ON CONFLICT DO NOTHING;

