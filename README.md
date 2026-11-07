# GamingHub

GamingHub is a reservation system for a computer gaming club.  
The club has PC and console rooms, every room has stations, and people can book a station for a time slot.
Admins manage rooms, stations and games, members book stations, guests can just look around.

| Part | Technology | Folder |
|---|---|---|
| API | ASP.NET Core 8, Entity Framework Core, MySQL 8.4 | `GamingHub.Api/` |
| WEB | React *(coming soon)* | `GamingHub.Web/` |

---

# Features

### Rooms and stations

- Two room types: PC and Console, each with its own hourly rate
- PC stations have specs (CPU, GPU, RAM, screen), console stations have platform and seats
- Stations can be turned off for maintenance and can't be booked then

### Games

- Game catalog with genre and search filter
- Games can be installed on stations
- Station details page shows the room, installed games and booked time slots in one response

### Reservations

- Book a station for 30 minutes to 12 hours, start must be in the future
- Two reservations on the same station can't overlap
- Price is calculated by the server (room hourly rate × hours)
- Status flow: `Upcoming` → `Started` → `Finished`, or `Upcoming` → `Cancelled` / `NoShow`

### Accounts and security

- Register and log in with username and password (ASP.NET Core Identity, passwords are hashed)
- JWT access tokens, refresh token in an HttpOnly cookie
- Refresh tokens rotate on every use, reusing an old one kills the session
- Logout per device, and a user can be forced to log in again on all devices (`ForceRelogin` flag)

### Other

- Pagination on every list, info in the `Pagination` response header
- Errors returned as Problem Details
- Runs with one Docker command

---

# Roles

| Who | Can do |
|---|---|
| Guest (not logged in) | Browse rooms, stations, station details, games. Register and log in |
| Member | Everything a guest can, plus book stations, see and change own reservations, cancel own reservations |
| Admin | Everything. Manage rooms, stations, games, install games, see all reservations, change any status, delete reservations |

Every new account is a Member. An admin account is created on first start (see Configuration).

---

# How login works

1. `POST /api/auth/login` with username and password
2. You get an `accessToken` in the response body and a `RefreshToken` cookie (HttpOnly, only sent to `/api/auth`)
3. Send the access token with every request: `Authorization: Bearer <accessToken>`
4. Access token lives 10 minutes. When it expires, call `POST /api/auth/refresh` and you get a new one (the cookie is sent automatically)
5. `POST /api/auth/logout` ends the session on that device

Without a token you get `401`. Logged in but not allowed (for example a member creating a room) gives `403`.

---

# Installation for development

1. Download and install  
[Docker Desktop](https://www.docker.com/products/docker-desktop)  
[.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (only needed if you run the API from Visual Studio)  
[Visual Studio 2022](https://visualstudio.microsoft.com) (optional)  
[Postman](https://www.postman.com/downloads) (for testing)

2. Clone
```bash
git clone https://github.com/hammerheartt/GamingHub.git
cd GamingHub
```

3. Run, pick one

Everything in Docker:
```bash
docker compose up --build
```

Or only the database in Docker and the API from Visual Studio:
```bash
docker compose up -d mysql
```
then open `GamingHub.sln` and press **F5**.

Both ways the API runs at `http://localhost:5177`. Don't run both at the same time, they use the same port.  
Tables, roles and the admin account are created automatically on start.

4. Check  
Open `http://localhost:5177/api/rooms`, should return `[]`.

---

# Testing with Postman

1. Import `postman/GamingHub.postman_collection.json` and `postman/GamingHub.postman_environment.json`
2. Select the environment and set `baseUrl` to `http://localhost:5177`
3. Run the whole collection, in order

The collection logs in by itself (first folder registers test users and saves the tokens).
Most requests run as admin, the Authorization folder tests guests, members and ownership, the Sessions folder tests refresh and logout.

---

# Configuration

Local settings are in `GamingHub.Api/appsettings.Development.json`:
- `ConnectionStrings:GamingHub` — database connection
- `Jwt` — issuer, audiences, signing secret, token lifetimes
- `Admin` — the admin account created on first start (dev login: `admin` / `Admin123!`)

These are dev-only values. On a real server set them as environment variables instead, with strong values:
- `ConnectionStrings__GamingHub`
- `Jwt__Secret`
- `Admin__UserName`, `Admin__Email`, `Admin__Password`

In Docker the connection string is already overridden in `docker-compose.yml` (`Server=mysql`).

---

# API

Base URL: `http://localhost:5177`. Request and response bodies are JSON.

**Pagination** — lists take `?pageNumber=1&pageSize=10` (default 10, max 50). Paging info comes back in the `Pagination` header:
```json
{"totalCount":23,"pageSize":10,"currentPage":2,"totalPages":3,"previousPageLink":"…","nextPageLink":"…"}
```

**Errors** — `400` bad input, `401` not logged in, `403` not allowed, `404` not found, `409` conflicts with current data (overlap, room still has stations, etc.).

### Auth

| Method | URL | Who | What |
|---|---|---|---|
| POST | `/api/auth/register` | Anyone | Create a member account → `201`, `400`, `409` (username taken) |
| POST | `/api/auth/login` | Anyone | Log in → `200` + access token + cookie, `401` |
| POST | `/api/auth/refresh` | Anyone with the cookie | New access token → `200`, `401` |
| POST | `/api/auth/logout` | Anyone | Ends the session, deletes cookie → `204` |

```json
// register
{ "userName": "jonas", "email": "jonas@example.com", "password": "Jonas123!" }
// login
{ "userName": "jonas", "password": "Jonas123!" }
// login / refresh response
{ "accessToken": "eyJhbGciOi..." }
```
Password needs at least 6 characters, an uppercase letter, a lowercase letter, a digit and a symbol.

### Rooms

| Method | URL | Who | What |
|---|---|---|---|
| GET | `/api/rooms` | Anyone | List rooms |
| GET | `/api/rooms/{roomId}` | Anyone | One room |
| POST | `/api/rooms` | Admin | Create room |
| PUT | `/api/rooms/{roomId}` | Admin | Update room (`409` if you change type while it has stations) |
| DELETE | `/api/rooms/{roomId}` | Admin | Delete room (`409` if it still has stations) |

```json
{ "name": "PC hall", "type": "PC", "description": "Main hall", "hourlyRate": 3.5 }
```
`type` is `PC` or `Console`.

### Stations

| Method | URL | Who | What |
|---|---|---|---|
| GET | `/api/rooms/{roomId}/stations` | Anyone | List stations of a room |
| GET | `/api/rooms/{roomId}/stations/{stationId}` | Anyone | One station |
| GET | `/api/rooms/{roomId}/stations/{stationId}/details` | Anyone | Station + room + installed games + booked time slots |
| POST | `/api/rooms/{roomId}/stations` | Admin | Create station |
| PUT | `/api/rooms/{roomId}/stations/{stationId}` | Admin | Update station |
| DELETE | `/api/rooms/{roomId}/stations/{stationId}` | Admin | Delete station (`409` if it has upcoming or started reservations) |

```json
// PC room
{ "name": "PC-01", "cpu": "Ryzen 5 7600", "gpu": "RTX 4060", "ramGb": 16, "screen": "24\" 165Hz", "isActive": true }
// Console room
{ "name": "PS5-01", "platform": "PlayStation", "seats": 3, "screen": "65\" TV", "isActive": true }
```
PC rooms need `cpu`, `gpu`, `ramGb`. Console rooms need `platform` (`PlayStation` or `Xbox`) and `seats`.

### Reservations

All reservation endpoints need login. Members only see and touch their own reservations, someone else's gives `404`.

| Method | URL | Who | What |
|---|---|---|---|
| GET | `/api/reservations` | Member, Admin | List reservations. Filters: `date`, `status`, `roomId`, `userId` (members always get only their own) |
| GET | `/api/rooms/{roomId}/stations/{stationId}/reservations` | Member, Admin | Reservations of a station |
| GET | `/api/rooms/{roomId}/stations/{stationId}/reservations/{reservationId}` | Member, Admin | One reservation |
| POST | `/api/rooms/{roomId}/stations/{stationId}/reservations` | Member, Admin | Book (`409` on overlap or inactive station) |
| PUT | `/api/rooms/{roomId}/stations/{stationId}/reservations/{reservationId}` | Member, Admin | Change time (only `Upcoming` ones) |
| PATCH | `/api/rooms/{roomId}/stations/{stationId}/reservations/{reservationId}` | Member, Admin | Change status. Members can only cancel (`403` otherwise) |
| DELETE | `/api/rooms/{roomId}/stations/{stationId}/reservations/{reservationId}` | Admin | Delete |

```json
// POST / PUT
{ "startTime": "2026-11-20T14:00:00+02:00", "endTime": "2026-11-20T16:00:00+02:00" }
// PATCH
{ "status": "Cancelled" }
```
Who booked it comes from the token, not from the body. `?date=YYYY-MM-DD` uses the club's local day (Europe/Vilnius).

### Games

| Method | URL | Who | What |
|---|---|---|---|
| GET | `/api/games` | Anyone | List games. Filters: `genre`, `search` (title) |
| GET | `/api/games/{gameId}` | Anyone | One game |
| POST | `/api/games` | Admin | Create game (`409` if title exists) |
| PUT | `/api/games/{gameId}` | Admin | Update game (`409` if title exists) |
| DELETE | `/api/games/{gameId}` | Admin | Delete game (also removes it from stations) |
| GET | `/api/rooms/{roomId}/stations/{stationId}/games` | Anyone | Games installed on a station |
| PUT | `/api/rooms/{roomId}/stations/{stationId}/games/{gameId}` | Admin | Install game (doing it twice is fine) → `204` |
| DELETE | `/api/rooms/{roomId}/stations/{stationId}/games/{gameId}` | Admin | Uninstall game → `204` |

```json
{ "title": "Counter-Strike 2", "genre": "FPS", "publisher": "Valve", "releaseDate": "2023-09-27" }
```

---

# Useful commands

```bash
docker compose up --build    # start everything
docker compose up -d mysql   # start only the database
docker compose down          # stop
docker compose down -v       # stop and delete all data
docker logs gaminghub-api    # API logs
```
