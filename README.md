# GamingHub

Computer gaming club reservation system.

| Part | Technology | Folder |
|---|---|---|
| API | ASP.NET Core 8 · Entity Framework Core · MySQL 8.4 | `GamingHub.Api/` |
| WEB | React *(coming soon)* | `GamingHub.Web/` |
---

## API
## Prerequisites

| Tool | Version | Download |
|---|---|---|
| Git | any | https://git-scm.com |
| .NET SDK | 8.0 | https://dotnet.microsoft.com/download/dotnet/8.0 |
| Docker Desktop | latest | https://www.docker.com/products/docker-desktop |
| Visual Studio 2022 *(optional)* | 17.8+ | https://visualstudio.microsoft.com |
| Postman *(for testing)* | any | https://www.postman.com/downloads |

## Setup

### 1. Clone the repository
```
git clone https://github.com/hammerheartt/GamingHub.git
cd GamingHub
```

### 2. Start the database
Make sure Docker Desktop is running, then:
```
docker compose up -d
```
This starts MySQL on `localhost:3306` with database `gaminghub` (user `root`, password `root`).
Check it is running: `docker ps` → `gaminghub-mysql` should be **Up**.

### 3. Run the API
```
cd GamingHub.Api
dotnet run
```
or open `GamingHub.sln` in Visual Studio and press **F5**.

On startup the API automatically creates all database tables (EF Core migrations).
The console shows the address, e.g. `Now listening on: http://localhost:5123`.
Test it: open `http://localhost:5123/api/rooms` in a browser → `[]`.

### 4. Test with Postman
Import `postman/GamingHub.postman_collection.json` and set the `baseUrl` variable to the address from step 3.

## Configuration

The local connection string is in `GamingHub.Api/appsettings.Development.json` and matches `docker-compose.yml`.
In production it is provided via the environment variable `ConnectionStrings__GamingHub`.

## Useful commands

| Action | Command |
|---|---|
| Start database | `docker compose up -d` |
| Stop database | `docker compose down` |
| Reset database (delete all data) | `docker compose down -v`, then `docker compose up -d` and restart the API |

## Troubleshooting

- **`Unable to connect to any of the specified MySQL hosts`**: the database isn't running. Start Docker Desktop and run `docker compose up -d`. Right after starting, MySQL needs ~10–20 seconds before it accepts connections.
- **Port 3306 already in use**: a local MySQL is already running. Stop it (Windows Services → MySQL80 → Stop), or change the port in `docker-compose.yml` to `"3307:3306"` and in the connection string to `Port=3307`.
- **`docker` not recognized**: restart the terminal or Visual Studio after installing Docker.
