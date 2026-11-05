-- ========================================================
-- Лабораторная работа №4: Скрипт триггеров аудита Supabase / PostgreSQL
-- Выполнить в Supabase -> SQL Editor
-- ========================================================

-- 1. Функция аудита изменений: INSERT / UPDATE / DELETE
CREATE OR REPLACE FUNCTION process_audit()
RETURNS TRIGGER AS $$
DECLARE
    current_user_login TEXT;
BEGIN
    -- Читаем логин, который приложение установило в текущей сессии
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

-- 2. Вешаем триггер на таблицу Computers
DROP TRIGGER IF EXISTS audit_computers ON "Computers";
CREATE TRIGGER audit_computers
AFTER INSERT OR UPDATE OR DELETE ON "Computers"
FOR EACH ROW EXECUTE FUNCTION process_audit();

-- 3. Вешаем триггер на таблицу Sessions
DROP TRIGGER IF EXISTS audit_sessions ON "Sessions";
CREATE TRIGGER audit_sessions
AFTER INSERT OR UPDATE OR DELETE ON "Sessions"
FOR EACH ROW EXECUTE FUNCTION process_audit();

-- 4. Защита журнала аудита от изменений и удаления
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

