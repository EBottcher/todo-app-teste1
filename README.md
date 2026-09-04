# Todo Monorepo (POC)

Monorepo with two apps:

1. **Backend**: ASP.NET Core Web API (`backend/src/TodoApp.Api`)
2. **Frontend**: React SPA (`frontend`)

## Backend architecture (Clean Architecture)

`backend/src/` is split into:

- `TodoApp.Domain`: entities, enums, value objects
- `TodoApp.Application`: DTOs, repository interfaces, use-case services
- `TodoApp.Infrastructure`: in-memory repository implementations and DI wiring
- `TodoApp.Api`: REST controllers and runtime composition

### Implemented required features

- Task CRUD
- Status, priority, labels/tags
- Due dates, reminders, recurring tasks
- Projects/lists
- Smart filters (`Today`, `Overdue`, `Upcoming`)
- Activity log (`who changed what`) via `X-User` header

### REST endpoints

- `GET/POST /api/projects`
- `GET/PUT/DELETE /api/projects/{id}`
- `GET/POST /api/tasks`
- `GET/PUT/DELETE /api/tasks/{id}`
- `GET /api/activity?limit=100`

## Run

### Backend

```bash
dotnet run --project backend/src/TodoApp.Api/TodoApp.Api.csproj
```

Default API URL from launch settings: `http://localhost:5009`.

### Frontend

Requires Node.js + npm.

```bash
cd frontend
npm install
npm run dev
```

Optional env:

- `VITE_API_BASE_URL` (default: `http://localhost:5009/api`)

## Nice-to-have roadmap (optional)

- [ ] **Subtasks and checklists**: add `Subtask` aggregate and checklist endpoints.
- [ ] **Attachments**: add `IAttachmentStorage` abstraction in `Application`; implement local disk provider in `Infrastructure`; keep S3 provider plug-in ready later.
- [ ] **Import/Export (CSV/JSON)**: add bulk import/export endpoints and validate schema contracts.
