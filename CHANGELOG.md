# Changelog

## 0.1.0

First release. Pulled together and cleaned up from a couple of years of playground code.

- Paging: page and total in one round trip (`COUNT(*) OVER()`).
- Index hints: `FORCE INDEX` / `USE INDEX` injection.
- Full-text: `MATCH ... AGAINST` query helper.
- Suffix search: ends-with served by an index, via a reversed column.
- Value squash: a type-per-column table collapsed into one indexed column.
- Parallel fan-out: concurrent subqueries with a capped connection pool.
