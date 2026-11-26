# Cinema Lives Inside (CIL)

An **ASP.NET Core MVC** web application for browsing movies, checking showtimes, selecting seats on an interactive hall map, booking tickets, and generating **AES-encrypted QR-coded PDF tickets**.

---

## Table of Contents

- [Features](#features)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Database Schema](#database-schema)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
  - [1. Clone the Repository](#1-clone-the-repository)
  - [2. Configure Application Settings](#2-configure-application-settings)
  - [3. Apply Database Migrations](#3-apply-database-migrations)
  - [4. Run the Application](#4-run-the-application)
- [Key Routes & Controllers](#key-routes--controllers)
- [PDF & QR Ticket Generation](#pdf--qr-ticket-generation)
- [Authentication](#authentication)
- [Security Notes](#security-notes)

---

## Features

- **Now Showing Catalog**: Displays currently active movie rentals (`RentalEnd > DateTime.Now`) with posters, age ratings, runtimes, genres, and rental periods.
- **Movie Details & Showtimes**: Shows full movie info, embedded trailer playback (when configured), and screenings grouped by date in an accordion view with hall numbers and start times.
- **Interactive Seat Selection**: Dynamically loads a hall's row-and-seat grid, highlights **VIP** seats, disables already reserved seats, and lets authenticated users book multiple seats in a single order.
- **Encrypted QR-Coded PDF Tickets**: Generates A4 PDF tickets in memory on the fly with **QuestPDF** and **QRCoder**. Standard tickets use a light/blue theme, while VIP tickets use a dark/gold theme. Each ticket embeds a QR code containing an AES-encrypted JSON payload.
- **Viewer Ticket History & Filtering**: Authenticated users can view all their booked tickets, preview PDFs inline, download them, and filter by **VIP** status (`?vip=true`) or **Active** upcoming screenings (`?active=true`).
- **Google OAuth 2.0 + People API**: Supports sign-in with Google and automatically retrieves the user's phone number and birthday via the Google People API.
- **Automatic Database Seeding**: On startup (outside EF design-time), `DbSeeder` populates sample age limits, genres, localisations, seat types, cinema contact info, two halls (with standard and VIP rows), movies, rentals, and screenings.

---

## Tech Stack

| Layer | Technology |
| --- | --- |
| Framework | .NET 10.0 (`net10.0`), ASP.NET Core MVC |
| ORM & Database | Entity Framework Core 10 (`Npgsql.EntityFrameworkCore.PostgreSQL`, `Microsoft.EntityFrameworkCore.Proxies` lazy loading) + PostgreSQL |
| Authentication | ASP.NET Core Cookie Authentication & Google OAuth 2.0 (`Microsoft.AspNetCore.Authentication.Google`) |
| PDF Generation | `QuestPDF` (Community License) |
| QR Code Generation | `QRCoder` |
| Frontend | Razor Views (`.cshtml`), Bootstrap 5 (dark theme), jQuery, jQuery Validation |

---

## Project Structure

```text
Cinema/
├── Controllers/
│   ├── AccountController.cs      # Google OAuth, Cookie auth, Login/Register/Logout
│   ├── CinemaController.cs       # Main catalog, movie details, partial seat map, privacy, errors
│   └── OrderController.cs        # Ticket booking (CreateOrder) & user ticket history (ViewersTickets)
├── Migrations/                   # EF Core PostgreSQL migrations (InitialPostgres)
├── Models/
│   ├── DatabaseClasses/          # EF Core DbContext (Database.cs), entities, and DbSeeder.cs
│   ├── DTOs/                     # TicketDto
│   ├── Responses/                # CreateOrderResponse
│   ├── ValidationModels/         # LoginModel, RegisterModel
│   └── ErrorViewModel.cs
├── Services/
│   └── TicketService.cs          # AES encryption, QR code generation, QuestPDF ticket rendering
├── Views/
│   ├── Account/                  # Register view
│   ├── Cinema/                   # Main, Details, PartialCheckScreeningData, Privacy
│   ├── Order/                    # CreateOrder, ViewersTickets
│   └── Shared/                   # _Layout, Error, _ValidationScriptsPartial
├── wwwroot/                      # Static assets (CSS, JS, Bootstrap/jQuery libs, movie posters)
├── Properties/
│   └── launchSettings.json       # Local HTTP/HTTPS run profiles
├── appsettings.json              # Connection string, Google OAuth, and Ticket secret key config
├── Cinema.csproj                 # Project dependencies & target framework
├── dotnet-tools.json             # Local dotnet-ef tool manifest (v10.0.15)
└── Program.cs                    # App startup, DI, auth, seeding, and middleware pipeline
```

---

## Database Schema

The EF Core `Database` context (`Models/DatabaseClasses/Database.cs`) manages the following entities in PostgreSQL:

- **Cinema & Venue**: `Cinema`, `ContactInformation`, `Hall`, `Row`, `Place`, `SeatType`
- **Movies & Metadata**: `Movie`, `Genre` (many-to-many via `MovieGenres`), `Localisation`, `AgeLimit`
- **Scheduling**: `MovieRental`, `MovieScreening`
- **Users & Bookings**: `Viewer`, `Reserved`, `Ticket`

---

## Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL](https://www.postgresql.org/download/) (default config expects `localhost:5432`, database `cinema`)
- A **Google Cloud OAuth 2.0** Client ID and Client Secret (if using Google Sign-In)

---

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/junpr0g/Cinema.git
cd Cinema
```

### 2. Configure Application Settings

Update `appsettings.json` (or preferably use .NET User Secrets / environment variables) with your PostgreSQL connection string, Google OAuth credentials, and a 32-character AES ticket encryption key:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=cinema;Username=postgres;Password=YOUR_PASSWORD"
  },
  "Authentication": {
    "Google": {
      "ClientId": "YOUR_GOOGLE_CLIENT_ID",
      "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET"
    }
  },
  "Ticket": {
    "SecretKey": "YOUR_32_BYTE_AES_SECRET_KEY_HERE"
  }
}
```

> **Note:** `TicketService` also checks the `TICKET_SECRET` environment variable as a fallback. The AES key must match the required AES key length (e.g., 32 ASCII characters for AES-256).

### 3. Apply Database Migrations

Restore the local `dotnet-ef` tool and apply the PostgreSQL migrations:

```bash
dotnet tool restore
dotnet dotnet-ef database update
```

### 4. Run the Application

```bash
dotnet run
```

By default (from `Properties/launchSettings.json`), the application runs on:
- **HTTP**: `http://localhost:5233`
- **HTTPS**: `https://localhost:7013` (with `http://localhost:5233`)

On first run, `DbSeeder.Seed(db)` automatically seeds the database with sample halls, movies, rentals, and screenings.

---

## Key Routes & Controllers

| Controller | Action | Route / Method | Auth Required | Description |
| --- | --- | --- | --- | --- |
| `Cinema` | `Main` | `GET /` or `/Cinema/Main` | No | Displays active movie rentals |
| `Cinema` | `Details` | `GET /Cinema/Details/{id}` | No | Movie info, trailer, and screenings grouped by date |
| `Cinema` | `PartialCheckScreeningData` | `GET /Cinema/PartialCheckScreeningData/{id}` | No | Partial view rendering hall seats & availability |
| `Cinema` | `Privacy` | `GET /Cinema/Privacy` | Yes | Privacy policy page |
| `Order` | `CreateOrder` | `GET /Order/CreateOrder?screeningId=...&placeIds=...` | Yes | Reserves seats, creates tickets, renders PDFs |
| `Order` | `ViewersTickets` | `GET /Order/ViewersTickets?vip=...&active=...` | Yes | Lists user's tickets with optional VIP/active filters |
| `Account` | `Login` | `GET /Account/Login` | Yes | Triggers Google OAuth challenge / redirects to `Main` |
| `Account` | `GoogleLogin` | `POST /Account/GoogleLogin` | No | Initiates Google OAuth flow with People API scopes |
| `Account` | `GoogleResponse` | `GET /Account/GoogleResponse` | No | Handles Google callback, creates/updates `Viewer` |
| `Account` | `Register` | `GET` / `POST /Account/Register` | No | Local viewer registration endpoint |
| `Account` | `Logout` | `GET /Account/Logout` | No | Signs out of cookie authentication |

---

## PDF & QR Ticket Generation

`Services/TicketService.cs` builds each ticket in memory:
1. Serializes `TicketData` (`Id`, `Movie`, `Place`, `Row`, `Date`, `IsVip`, `ViewerName`) to JSON.
2. Encrypts the JSON string using AES (`Ticket:SecretKey`).
3. Encodes the encrypted Base64 string into a PNG QR code (`QRCoder`, ECC Level `M`).
4. Renders an A4 PDF (`QuestPDF`) styled according to `IsVip` (dark background with gold accents for VIP, light background with blue accents for standard seats) and returns the PDF as a `byte[]` (converted to Base64 for inline browser preview and download).

---

## Authentication

- Uses **Cookie Authentication** as the default scheme and **Google OAuth** (`/signin-google` callback) as the default challenge scheme.
- Requests the following Google scopes:
  - `https://www.googleapis.com/auth/userinfo.profile`
  - `https://www.googleapis.com/auth/user.phonenumbers.read`
  - `https://www.googleapis.com/auth/user.birthday.read`
- `Options.SaveTokens = true` is enabled so `AccountController.GoogleResponse` can query `https://people.googleapis.com/v1/people/me` for the user's phone number and birthday when creating or updating a `Viewer` record.

---

## Security Notes

- **Secrets Management**: Do not commit real database passwords, Google OAuth `ClientSecret` values, or production `Ticket:SecretKey` values in `appsettings.json`. Use `dotnet user-secrets` for local development and environment variables or a secret manager in production.
- **AES IV**: `TicketService.EncryptString` currently uses a zeroed 16-byte IV (`new byte[16]`). For production use, generate a random IV per ticket and prepend it to the ciphertext.
- **Password Storage**: Local registration/login in `AccountController` compares plain-text passwords on `Viewer`. If local email/password login is used in production, add password hashing (e.g., ASP.NET Core Identity `PasswordHasher<T>`).
