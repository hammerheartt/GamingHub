# GamingHub

Computer gaming club reservation system.

| Part | Technology | Folder |
|---|---|---|
| API | ASP.NET Core 8 · Entity Framework Core · MySQL 8.4 | `GamingHub.Api/` |
| WEB | React *(coming soon)* | `GamingHub.Web/` |
---

## API

### API endpoints

Base URL (local): `http://localhost:5177`. All request and response bodies are JSON.

**Pagination** — every list endpoint accepts `?pageNumber=1&pageSize=10` (defaults: page 1, size 10, max 50).
Paging info is returned in the `Pagination` response header:
```json
{"totalCount":23,"pageSize":10,"currentPage":2,"totalPages":3,"previousPageLink":"…","nextPageLink":"…"}
```

**Errors** are returned as [Problem Details](https://www.rfc-editor.org/rfc/rfc9457) (`application/problem+json`):
`400` invalid input · `404` resource not found · `409` conflicts with current state.

#### Rooms

| Method | URI | Description | Success | Errors |
|---|---|---|---|---|
| GET | `/api/rooms` | List rooms (paginated) | 200 | – |
| GET | `/api/rooms/{roomId}` | Get one room | 200 | 404 |
| POST | `/api/rooms` | Create a room | 201 | 400 |
| PUT | `/api/rooms/{roomId}` | Update a room | 200 | 400, 404, 409 (type change while room has stations) |
| DELETE | `/api/rooms/{roomId}` | Delete a room | 204 | 404, 409 (room has stations) |

```json
{ "name": "PC hall", "type": "PC", "description": "Main hall", "hourlyRate": 3.5 }
```
`type`: `PC` · `Console`

#### Stations

| Method | URI | Description | Success | Errors |
|---|---|---|---|---|
| GET | `/api/rooms/{roomId}/stations` | List stations of a room (paginated) | 200 | 404 |
| GET | `/api/rooms/{roomId}/stations/{stationId}` | Get one station | 200 | 404 |
| POST | `/api/rooms/{roomId}/stations` | Create a station | 201 | 400, 404 |
| PUT | `/api/rooms/{roomId}/stations/{stationId}` | Update a station | 200 | 400, 404 |
| DELETE | `/api/rooms/{roomId}/stations/{stationId}` | Delete a station | 204 | 404, 409 (has upcoming/started reservations) |

```json
// PC room
{ "name": "PC-01", "cpu": "Ryzen 5 7600", "gpu": "RTX 4060", "ramGb": 16, "screen": "24\" 165Hz", "isActive": true }
// Console room
{ "name": "PS5-01", "platform": "PlayStation", "seats": 3, "screen": "65\" TV", "isActive": true }
```
PC rooms require `cpu`, `gpu`, `ramGb`; console rooms require `platform` (`PlayStation` · `Xbox`) and `seats`.

#### Reservations

| Method | URI | Description | Success | Errors |
|---|---|---|---|---|
| GET | `/api/reservations` | List reservations across all stations (paginated). Filters: `date`, `status`, `roomId`, `customerEmail` | 200 | 400 |
| GET | `/api/rooms/{roomId}/stations/{stationId}/reservations` | List reservations of a station (paginated) | 200 | 404 |
| GET | `/api/rooms/{roomId}/stations/{stationId}/reservations/{reservationId}` | Get one reservation | 200 | 404 |
| POST | `/api/rooms/{roomId}/stations/{stationId}/reservations` | Create a reservation | 201 | 400, 404, 409 (overlap / inactive station) |
| PUT | `/api/rooms/{roomId}/stations/{stationId}/reservations/{reservationId}` | Change time or customer | 200 | 400, 404, 409 (overlap / not upcoming) |
| PATCH | `/api/rooms/{roomId}/stations/{stationId}/reservations/{reservationId}` | Change status | 200 | 400, 404, 409 (invalid transition) |
| DELETE | `/api/rooms/{roomId}/stations/{stationId}/reservations/{reservationId}` | Delete a reservation | 204 | 404 |

```json
// POST / PUT
{ "customerName": "Jonas Jonaitis", "customerEmail": "jonas@example.com",
  "startTime": "2026-11-20T14:00:00+02:00", "endTime": "2026-11-20T16:00:00+02:00" }
// PATCH
{ "status": "Started" }
```

Rules:
- `totalPrice` = room hourly rate × hours, calculated by the server.
- Reservations on the same station cannot overlap (cancelled / no-show ones are ignored).
- Start must be in the future; duration 30 min – 12 h.
- Status transitions: `Upcoming` → `Started` / `Cancelled` / `NoShow`; `Started` → `Finished`.
- `?date=YYYY-MM-DD` uses the club's local day (Europe/Vilnius).

#### Games

| Method | URI | Description | Success | Errors |
|---|---|---|---|---|
| GET | `/api/games` | List games (paginated). Filters: `genre`, `search` (title) | 200 | – |
| GET | `/api/games/{gameId}` | Get one game | 200 | 404 |
| POST | `/api/games` | Create a game | 201 | 400, 409 (title exists) |
| PUT | `/api/games/{gameId}` | Update a game | 200 | 400, 404, 409 (title exists) |
| DELETE | `/api/games/{gameId}` | Delete a game (also removes it from stations) | 204 | 404 |

```json
{ "title": "Counter-Strike 2", "genre": "FPS", "publisher": "Valve", "releaseDate": "2023-09-27" }
```

#### Games on stations

| Method | URI | Description | Success | Errors |
|---|---|---|---|---|
| GET | `/api/rooms/{roomId}/stations/{stationId}/games` | List games installed on a station (paginated) | 200 | 404 |
| PUT | `/api/rooms/{roomId}/stations/{stationId}/games/{gameId}` | Install a game on a station (idempotent) | 204 | 404 |
| DELETE | `/api/rooms/{roomId}/stations/{stationId}/games/{gameId}` | Uninstall a game from a station | 204 | 404 |


## Prerequisites

| Tool | Version | Download |
|---|---|---|
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
