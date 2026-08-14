# Jira Lite — Project Documentation

## Table of Contents
1. [Project Overview](#1-project-overview)
2. [Environment Setup](#2-environment-setup)
3. [Backend — .NET Core 8 Web API](#3-backend--net-core-8-web-api)
4. [Database — SQL Server 2019](#4-database--sql-server-2019)
5. [EF Core Configuration](#5-ef-core-configuration)
6. [Data Models](#6-data-models)
7. [Database Schema](#7-database-schema)
8. [Migrations](#8-migrations)
9. [Seed Data](#9-seed-data)
10. [Frontend — Angular 17](#10-frontend--angular-17)
11. [Git & GitHub](#11-git--github)
12. [Pending Tasks](#12-pending-tasks)

---

## 1. Project Overview

**Project Name:** Jira Lite  
**Purpose:** Learning project — a lightweight Jira-like Project & Task Management API  
**Core Concept:** Project tracking tool with sprints, task assignments, time logging, and dashboards

### Key Requirements (from task brief)
- Hierarchical data: Projects → Epics → Stories → Tasks → Subtasks
- Workflow engine: configurable task statuses with allowed transitions
- Real-time notifications using SignalR
- Time tracking with utilization reports
- Role-based access: Project Admin, Lead, Member, Viewer
- SQL: Recursive CTEs for hierarchy traversal, burndown chart data queries
- Full-text search across tasks and comments
- Activity feed (audit log with user-friendly display)

### Tech Stack

| Layer | Technology |
|---|---|
| Backend | .NET Core 8 Web API |
| ORM | Entity Framework Core 8 |
| Database | SQL Server 2019 Developer Edition |
| Frontend | Angular 17 |
| Version Control | Git + GitHub |

---

## 2. Environment Setup

### SQL Server 2019

- **Edition:** Developer (free for learning/dev)
- **Instance Name:** `LAPTOP-UF34A54U\YOURSERVER`
- **Authentication:** Mixed Mode (Windows + SQL Server Auth)
- **SA Password:** `123456`
- **Port:** Default (1433)
- **SSMS Version:** 22

#### SSL Certificate Fix
SSMS 22 requires encrypted connections by default. A self-signed certificate was created and added to the Windows Trusted Root CA store to resolve the "certificate chain not trusted" error:

```powershell
# Certificate created with Server Authentication EKU
New-SelfSignedCertificate `
    -Subject "CN=LAPTOP-UF34A54U" `
    -DnsName "LAPTOP-UF34A54U","localhost" `
    -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.1") `
    -NotAfter (Get-Date).AddYears(10)
```

Certificate was then:
1. Added to `Cert:\LocalMachine\Root` (Trusted Root CA) via `certutil -addstore`
2. Private key permissions granted to `NT SERVICE\MSSQL$YOURSERVER`
3. Assigned to SQL Server via registry: `HKLM:\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL15.YOURSERVER\MSSQLServer\SuperSocketNetLib`

### Angular CLI
```
Angular CLI : 18.1.4 (global)
Node.js     : 20.19.4
npm         : 10.8.2
```

---

## 3. Backend — .NET Core 8 Web API

### Project Location
```
D:\HCL\jira-lite\jira-lite\
```

### Project Creation
Created using .NET 8 SDK with Web API template + Swagger enabled.

### Folder Structure
```
jira-lite/
├── Controllers/
│   └── WeatherForecastController.cs   (default, to be replaced)
├── Data/
│   └── AppDbContext.cs                (EF Core DbContext)
├── Migrations/
│   ├── 20260814093329_InitialSchema.cs
│   └── 20260814094313_SeedData.cs
├── Models/
│   ├── User.cs
│   ├── Role.cs
│   ├── UserProjectRole.cs
│   ├── WorkflowStatus.cs
│   ├── WorkflowTransition.cs
│   ├── Project.cs
│   ├── Epic.cs
│   ├── Story.cs
│   ├── Task.cs
│   └── Subtask.cs
├── jira-lite-ui/                      (Angular frontend)
├── appsettings.json
├── appsettings.Development.json
├── Program.cs
└── jira-lite.csproj
```

### NuGet Packages Installed

| Package | Version | Purpose |
|---|---|---|
| `Swashbuckle.AspNetCore` | 6.4.0 | Swagger/OpenAPI docs |
| `Microsoft.EntityFrameworkCore.SqlServer` | 8.0.0 | EF Core SQL Server provider |
| `Microsoft.EntityFrameworkCore.Tools` | 8.0.0 | CLI migration commands |
| `Microsoft.EntityFrameworkCore.Design` | 8.0.0 | Design-time EF Core support |

### Program.cs
```csharp
using jira_lite.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

---

## 4. Database — SQL Server 2019

### Database Name
```
JiraLiteDb
```

### Connection String (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=LAPTOP-UF34A54U\\YOURSERVER;Database=JiraLiteDb;User Id=sa;Password=123456;TrustServerCertificate=True;"
  }
}
```

### SSMS Connection Details
| Field | Value |
|---|---|
| Server name | `LAPTOP-UF34A54U\YOURSERVER` |
| Authentication | SQL Server Authentication |
| Login | `sa` |
| Password | `123456` |
| Encryption | Mandatory |

---

## 5. EF Core Configuration

### AppDbContext (`Data/AppDbContext.cs`)

Registers all DbSets and configures relationships using **Fluent API** inside `OnModelCreating`.

#### DbSets
```csharp
DbSet<User>               Users
DbSet<Role>               Roles
DbSet<UserProjectRole>    UserProjectRoles
DbSet<WorkflowStatus>     WorkflowStatuses
DbSet<WorkflowTransition> WorkflowTransitions
DbSet<Project>            Projects
DbSet<Epic>               Epics
DbSet<Story>              Stories
DbSet<Task>               Tasks
DbSet<Subtask>            Subtasks
```

#### Key Fluent API Configurations
- `User.Email` → unique index
- `Project.Key` → unique index
- `UserProjectRole` → composite primary key `(UserId, ProjectId, RoleId)`
- `WorkflowTransition` → `OnDelete: Restrict` on both FK sides (prevents cascade conflicts)
- All hierarchy levels (Epic → Story → Task → Subtask) → `OnDelete: Cascade` downward
- All `AssigneeId` fields → `OnDelete: SetNull` (task stays if user is removed)
- All `CreatedById` fields → `OnDelete: Restrict` (cannot delete user who created items)

---

## 6. Data Models

### User
```
Id           int (PK)
FullName     nvarchar(200)
Email        nvarchar(200) UNIQUE
PasswordHash nvarchar(max)
CreatedAt    datetime2
```

### Role
```
Id          int (PK)
Name        nvarchar(50)
Description nvarchar(max)
```
**Seed values:** Project Admin, Lead, Member, Viewer

### UserProjectRole *(junction table)*
```
UserId      int (PK, FK → Users)
ProjectId   int (PK, FK → Projects)
RoleId      int (PK, FK → Roles)
AssignedAt  datetime2
```

### WorkflowStatus
```
Id    int (PK)
Name  nvarchar(100)
Color nvarchar(20)
Order int
```
**Seed values:** Todo, In Progress, In Review, Done

### WorkflowTransition
```
Id           int (PK)
FromStatusId int (FK → WorkflowStatuses)
ToStatusId   int (FK → WorkflowStatuses)
EntityType   nvarchar(50)   -- "Epic" | "Story" | "Task" | "Subtask"
```

### Project
```
Id          int (PK)
Name        nvarchar(200)
Key         nvarchar(10) UNIQUE   -- e.g. "JL"
Description nvarchar(max)
StartDate   datetime2 (nullable)
EndDate     datetime2 (nullable)
CreatedAt   datetime2
UpdatedAt   datetime2
CreatedById int (FK → Users)
```

### Epic
```
Id          int (PK)
Title       nvarchar(500)
Description nvarchar(max)
Priority    nvarchar(20)   -- Low | Medium | High | Critical
DueDate     datetime2 (nullable)
CreatedAt   datetime2
UpdatedAt   datetime2
ProjectId   int (FK → Projects, CASCADE)
StatusId    int (FK → WorkflowStatuses)
CreatedById int (FK → Users)
AssigneeId  int (FK → Users, nullable, SET NULL)
```

### Story
```
Id          int (PK)
Title       nvarchar(500)
Description nvarchar(max)
Priority    nvarchar(20)
StoryPoints int (nullable)
CreatedAt   datetime2
UpdatedAt   datetime2
EpicId      int (FK → Epics, CASCADE)
StatusId    int (FK → WorkflowStatuses)
CreatedById int (FK → Users)
AssigneeId  int (FK → Users, nullable, SET NULL)
```

### Task
```
Id             int (PK)
Title          nvarchar(500)
Description    nvarchar(max)
Priority       nvarchar(20)
EstimatedHours decimal(8,2) (nullable)
LoggedHours    decimal(8,2) (nullable)
CreatedAt      datetime2
UpdatedAt      datetime2
StoryId        int (FK → Stories, CASCADE)
StatusId       int (FK → WorkflowStatuses)
CreatedById    int (FK → Users)
AssigneeId     int (FK → Users, nullable, SET NULL)
```

### Subtask
```
Id          int (PK)
Title       nvarchar(500)
Description nvarchar(max)
Priority    nvarchar(20)
CreatedAt   datetime2
UpdatedAt   datetime2
TaskId      int (FK → Tasks, CASCADE)
StatusId    int (FK → WorkflowStatuses)
CreatedById int (FK → Users)
AssigneeId  int (FK → Users, nullable, SET NULL)
```

---

## 7. Database Schema

### Entity Relationship Diagram

```
Users ─────────────────────────────────────────────────────┐
  │                                                         │
  │ (CreatedBy)                                             │
  ▼                                                         │
Projects ◄──── UserProjectRoles ────► Roles                 │
  │                                                         │
  ▼                                                         │
Epics ──── StatusId ──► WorkflowStatuses ◄── WorkflowTransitions
  │
  ▼
Stories ──── StatusId ──► WorkflowStatuses
  │
  ▼
Tasks ──── StatusId ──► WorkflowStatuses
  │
  ▼
Subtasks ──── StatusId ──► WorkflowStatuses
```

### Hierarchy Cascade Behaviour
```
Delete Project  → deletes all Epics, Stories, Tasks, Subtasks
Delete Epic     → deletes all Stories, Tasks, Subtasks
Delete Story    → deletes all Tasks, Subtasks
Delete Task     → deletes all Subtasks
Delete User     → blocks if they created any item (Restrict)
                  sets AssigneeId to NULL on assigned items (SetNull)
```

---

## 8. Migrations

Two migrations were applied to `JiraLiteDb`:

| Migration | Description |
|---|---|
| `20260814093329_InitialSchema` | Creates all 10 tables with indexes and FK constraints |
| `20260814094313_SeedData` | Inserts default Roles, WorkflowStatuses, Transitions, User, Project |

### Useful EF Core Commands
```bash
# Create a new migration
dotnet ef migrations add <MigrationName>

# Apply pending migrations to DB
dotnet ef database update

# Revert last migration (code only)
dotnet ef migrations remove

# Revert DB to a specific migration
dotnet ef database update <MigrationName>

# View applied migrations
dotnet ef migrations list
```

---

## 9. Seed Data

Seed data is defined via `HasData()` in `AppDbContext.SeedData()` and applied via the `SeedData` migration.

### Roles (4 rows)
| Id | Name | Description |
|---|---|---|
| 1 | Project Admin | Full access to the project |
| 2 | Lead | Can manage epics and stories |
| 3 | Member | Can work on assigned tasks |
| 4 | Viewer | Read-only access |

### WorkflowStatuses (4 rows)
| Id | Name | Color | Order |
|---|---|---|---|
| 1 | Todo | #e2e8f0 | 1 |
| 2 | In Progress | #3b82f6 | 2 |
| 3 | In Review | #f59e0b | 3 |
| 4 | Done | #22c55e | 4 |

### WorkflowTransitions (20 rows)
5 transitions defined for each of Epic, Story, Task, Subtask:

| From | To | Meaning |
|---|---|---|
| Todo | In Progress | Start work |
| In Progress | In Review | Submit for review |
| In Review | Done | Approve and close |
| In Review | In Progress | Send back for rework |
| In Progress | Todo | Put on hold |

### Admin User
| Field | Value |
|---|---|
| Email | admin@jiralite.com |
| Password | Admin@123 *(stored as BCrypt hash)* |
| Role | Project Admin on "Jira Lite" project |

### Sample Project
| Field | Value |
|---|---|
| Name | Jira Lite |
| Key | JL |
| Created By | admin@jiralite.com |

---

## 10. Frontend — Angular 17

### Project Location
```
D:\HCL\jira-lite\jira-lite-ui\
```

### Created With
```bash
npx @angular/cli@17 new jira-lite-ui --routing=true --style=scss --skip-git=true
```

### Versions
| Tool | Version |
|---|---|
| Angular | 17.3.12 |
| Angular CLI | 17.3.17 |
| Node.js | 20.19.4 |

### Project Structure
```
jira-lite-ui/
├── src/
│   ├── app/
│   │   ├── app.component.ts
│   │   ├── app.component.html
│   │   ├── app.component.scss
│   │   ├── app.config.ts
│   │   └── app.routes.ts
│   ├── index.html
│   ├── main.ts
│   └── styles.scss
├── angular.json
├── package.json
└── tsconfig.json
```

### Key Configurations
- **Routing:** Enabled (standalone routing with `app.routes.ts`)
- **Styles:** SCSS
- **Architecture:** Standalone components (Angular 17 default)

---

## 11. Git & GitHub

### Repository
**URL:** https://github.com/samsanofficial/jira-lite

### Branch Structure
| Branch | Contents |
|---|---|
| `master` | Backend (.NET API) + `jira-lite-ui/` folder (Angular) |
| `frontend` | Standalone Angular app (separate root) |

### Commit History
```
860401d  Add EF Core with SQL Server - JiraLiteDb setup
debcdb8  Add Angular 17 UI project under jira-lite-ui/
2d7c4d8  Merge: keep remote .gitignore
5582834  Add project files.
8228cd2  Add .gitattributes and .gitignore.
aaa445a  Initial commit: ASP.NET Core backend for jira-lite
```

### Local Git Setup
```bash
# Backend repo root
D:\HCL\jira-lite\jira-lite\

# Remote
origin → https://github.com/samsanofficial/jira-lite.git
```

> **Note:** Models, SeedData migration, and AppDbContext updates from the last session
> are **not yet pushed**. Awaiting manual review before commit.

---

## 12. Pending Tasks

### Immediate
- [ ] Review and commit latest changes (Models, Migrations, Seed Data)

### Backend APIs (not started)
- [ ] ProjectsController — CRUD
- [ ] EpicsController — CRUD
- [ ] StoriesController — CRUD
- [ ] TasksController — CRUD + time logging
- [ ] SubtasksController — CRUD
- [ ] AuthController — register, login, JWT token
- [ ] WorkflowController — status transition validation

### Advanced Backend Features
- [ ] JWT Authentication + Role-based Authorization middleware
- [ ] SignalR hub for real-time notifications
- [ ] Recursive CTEs for full hierarchy queries
- [ ] Burndown chart data endpoint
- [ ] Workload distribution queries
- [ ] Full-text search across tasks and comments
- [ ] Activity feed / audit log

### Frontend (Angular 17)
- [ ] HTTP service setup (HttpClient + interceptors)
- [ ] Auth module (login page, JWT storage, guards)
- [ ] Projects list and detail pages
- [ ] Kanban board component (drag & drop)
- [ ] Epic / Story / Task / Subtask forms
- [ ] User management page
- [ ] Dashboard with charts
