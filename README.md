# EfQueryKit

[![ci](https://github.com/dfedoryshchev/efquerykit/actions/workflows/ci.yml/badge.svg)](https://github.com/dfedoryshchev/efquerykit/actions/workflows/ci.yml)

EF Core query extensions for MySQL-backed apps that outgrow naive LINQ.

These came out of taking a heavily-used search feature from tens of seconds down to
sub-second on MySQL, without reaching for a separate search engine. They cover the things
EF Core does not do out of the box but that a big relational search/listing feature ends
up needing.

## What's in it

- **Paging** - the page of rows and the total in one round trip via `COUNT(*) OVER()`.
- **Index hints** - inject `FORCE INDEX` / `USE INDEX` when the planner picks the wrong one.
- **Full-text** - `MATCH ... AGAINST` from a query helper.
- **Suffix search** - "ends with" served by an index, via a reversed column.
- **Value squash** - collapse a type-per-column table into one indexed text column.
- **Parallel fan-out** - run independent subqueries concurrently with a capped pool.

## Status

Built as a personal playground across 2024-25, then cleaned up and released as v0.1.0.
Targets .NET 8 and EF Core 8 (Pomelo MySQL). The samples run on SQLite so they work without
a MySQL server; the MySQL-specific pieces (index hints, full-text) target MySQL.

## Background

The reasoning behind these, and why a ladder of cheaper fixes often beats jumping straight
to a search engine, is written up in a companion article, "Search Before a Search Engine"
(link to follow once it is published).

## License

MIT. See [LICENSE](LICENSE).
