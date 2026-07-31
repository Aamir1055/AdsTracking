# Ads Tracking System

Ad traffic tracking and routing system: Meta ads → Landing page → Telegram channel → App download.

## Architecture

- **Backend**: ASP.NET Core 8 Web API
- **Database**: MySQL (XAMPP)
- **Frontend**: Plain HTML/CSS/JS landing page

## Project Structure

```
Ads-Tracking/
├── backend/          # ASP.NET Core API
│   ├── Controllers/  # API endpoints
│   ├── Services/     # Business logic
│   ├── Models/       # Entity models
│   ├── DTOs/         # Request/Response objects
│   ├── Data/         # EF Core DbContext
│   └── Infrastructure/ # Retry queue
├── frontend/         # Landing page (HTML/CSS/JS)
│   ├── index.html
│   ├── styles.css
│   └── tracking.js
└── database/         # SQL schema
    └── schema.sql
```

## Setup

### 1. Database

Start XAMPP MySQL, then either:
- Import `database/schema.sql` via phpMyAdmin
- Or just run the backend — EF Core will create tables automatically via `EnsureCreated()`

### 2. Backend

```bash
cd backend
dotnet run
```

The API starts on `http://localhost:5000` by default.

### 3. Frontend

The frontend is served automatically by the backend from the `frontend/` folder.
Visit `http://localhost:5000` to see the landing page.

## API Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| POST | /api/visits | Record a website visit |
| POST | /api/telegram-clicks | Record a Telegram link click |
| GET | /download | Log download + redirect to app URL |
| GET | /api/reports/visits | Visit counts grouped by UTM |
| GET | /api/reports/visits/detail | Individual visit records |
| GET | /api/reports/telegram-clicks | Telegram click count |
| GET | /api/reports/downloads | Download count |

## Swagger

When running in Development mode: `http://localhost:5000/swagger`

## How the Tracking Flow Works

1. **Meta Ad → Landing Page**: User clicks ad, arrives with UTM params + fbclid in URL
2. **Page Load**: JavaScript extracts params, generates/reads visitor_id cookie, POSTs to `/api/visits`
3. **Telegram Click**: User clicks "Join Telegram" → click event sent via sendBeacon → navigates to Telegram
4. **Download**: Telegram channel has link to `yoursite.com/download` → server logs event → 302 redirects to actual app
