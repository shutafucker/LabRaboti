# Лабораторная работа №1: Создание WPF компьютерного клуба

## Описание проекта
Десктопное приложение на WPF (.NET 8) с Entity Framework Core и подключением к облачной базе данных PostgreSQL (Supabase). Реализована загрузка списка компьютеров и добавление новых записей.

## Структура проекта
- `Models.cs` — сущности `Computer` и `Session` (связь 1-ко-многим).
- `AppDbContext.cs` — контекст базы данных и конфигурация связи через Fluent API.
- `MainWindow.xaml` / `MainWindow.xaml.cs` — окно со списком компьютеров (`DataGrid`), кнопкой асинхронной загрузки и добавления тестового ПК.

## Команды для миграции (в Консоли диспетчера пакетов):
```shell
Add-Migration AddCyberClubTables
Update-Database
```
Или через .NET CLI:
```shell
dotnet ef migrations add AddCyberClubTables
dotnet ef database update
```

