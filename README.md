# Expense Tracker (Dockerized)

Arabic expense tracker built with ASP.NET Core 8, EF Core, SQLite, and Docker Compose.

## Features
- Arabic RTL UI
- EGP currency formatting
- Add expenses with title, amount, category, date, and notes
- Expense list with category filter
- Summary totals and category breakdown
- Delete expense
- Persistent database via Docker volume

## Run with Docker
```bash
docker compose up --build -d
```

Open:
- App: http://localhost:8088
- Swagger API docs: http://localhost:8088/swagger

Stop:
```bash
docker compose down
```

## Project Structure
- `ExpenseTracker.Api/` ASP.NET Core API + static frontend
- `ExpenseTracker.Tests/` integration tests
- `docker-compose.yml` container orchestration
- `Dockerfile` production image build

## Notes
- Default runtime DB is SQLite stored in Docker volume `expense_tracker_data`.
- App can also use SQL Server later by setting `ConnectionStrings__DefaultConnection` to a SQL Server connection string.
