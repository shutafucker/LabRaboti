# Лабораторная работа №4: Логирование действий админа (Аудит)

## Описание
Реализация надежного журнала аудита на уровне базы данных PostgreSQL (Supabase) через триггеры:
- Таблица `AuditLogs`: `Id`, `TableName`, `Action`, `OldData` (JSON), `NewData` (JSON), `ChangedBy`, `ChangedAt`.
- Передача пользователя через переменную сессии PostgreSQL: `SET app.current_user = '...'`.
- Триггеры `audit_computers` и `audit_sessions` логируют все операции INSERT/UPDATE/DELETE с фиксацией JSON-снапшотов строк.
- Триггер `audit_immutable` запрещает любые операции UPDATE и DELETE над таблицей `AuditLogs`.
- Окно `AuditWindow` в WPF с фильтрацией по логину сотрудника и названию таблицы, сортировкой по времени и лимитом в 200 последних записей.

## Миграция EF Core:
```shell
Add-Migration AddAuditLogs
Update-Database
```
Или:
```shell
dotnet ef migrations add AddAuditLogs
dotnet ef database update
```

## Установка триггеров в базе:
Выполните содержимое файла `database_triggers.sql` в панели **Supabase -> SQL Editor**.

