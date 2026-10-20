# SkillSwap – Student Time Bank

A web app where students trade skills instead of money. Teach someone for 1 hour and earn 1 credit, then spend it on an hour of anything another student can teach.

Mini project for **Web Technologies with .NET (IT5069)**.

## Features

- **Accounts**: register, log in, log out, edit profile (passwords hashed with ASP.NET Core Identity's `PasswordHasher`)
- **Skills (CRUD)**: offer, browse, search, filter by category, edit, pause, delete
- **Sessions**: request → accept / decline → mark as done, or cancel
- **Credit wallet**: 3 free credits on sign-up, credits held on request, refunded on decline/cancel, paid to the teacher on completion, full transaction history
- **Reviews and leaderboard**: 1–5 star ratings, teachers ranked by hours taught

## Syllabus requirements covered

| Requirement | Where |
|---|---|
| MVC architecture | `Models/`, `Views/`, `Controllers/` |
| EF Core + MySQL | `Data/AppDbContext.cs`, `Migrations/`, Pomelo provider in `Program.cs` |
| CRUD | `SkillsController` (Create/Read/Update/Delete), bookings, reviews, profile |
| Form handling | Tag helpers, `[HttpPost]` actions, anti-forgery tokens |
| Model validation | Data annotations, `IValidatableObject` on `BookingCreateViewModel`, server checks (duplicate email, credits) |
| State management | **Session** (logged-in user), **Cookie** (remember email), **TempData** (flash messages) |
| Responsive UI | Bootstrap 5 + `wwwroot/css/site.css` |

## Tech stack

ASP.NET Core MVC (.NET 8) · Entity Framework Core 8 · MySQL · Pomelo.EntityFrameworkCore.MySql · Bootstrap 5

## Run it

1. Install the .NET 8 SDK and MySQL, and start MySQL.
2. Set your MySQL connection in `appsettings.json` (`ConnectionStrings:DefaultConnection`).
3. Run:

   ```bash
   dotnet run
   ```

   The database and tables are created automatically on first run, along with demo data.

4. Open http://localhost:5254

### Demo accounts

All use the password `Demo@123`:

- `aarav@skillswap.test`
- `isha@skillswap.test`
- `kabir@skillswap.test`
- `meera@skillswap.test`

## Database

| Table | Purpose |
|---|---|
| Users | Members and their credit balance |
| Categories | Skill categories (seeded) |
| SkillOffers | Skills members teach |
| Bookings | Session requests and their status |
| Reviews | Ratings for completed sessions |
| CreditTransactions | Ledger of every credit movement |
