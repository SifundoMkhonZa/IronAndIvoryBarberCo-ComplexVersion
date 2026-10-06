Here — this is your final README with your real repo link. Copy everything:
# IronAndIvoryBarberCo-ComplexVersion - Barbershop Booking System

Modern barbershop management and appointment booking system for Iron & Ivory Co. Newcastle CBD Branch.

**GitHub Repo:** https://github.com/SifundoMkhonZa/IronAndIvoryBarberCo-ComplexVersion

## 🔐 Test Accounts (Seeded Data)

After running the project, use these to login:

| Role | Email | Password |
| :--- | :--- | :--- |
| **Admin** | admin@ironandivory.co.za | Admin@123 |
| **Barber** | barber@ironandivory.co.za | Barber@123 |
| **Receptionist** | reception@ironandivory.co.za | Recep@123 |
| **Customer** | customer@ironandivory.co.za | Customer@123 |

## ✨ Features

**For Customers:**
- Browse Services (Haircuts, Beard Trims, Treatments) with images & prices
- Real-time barber availability check (Mon-Sat, 08:00-17:00, Sat closes 14:00)
- Book appointment with double-booking prevention
- My Orders / Payment / Review

**For Barbers:**
- View own appointments
- Complete appointments (only if payment = Paid)

**For Receptionist / Admin:**
- Confirm / Complete / Cancel any appointment
- Manage Branches, Services, Staff, Schedules, Payments, Reviews
- Dashboard with total appointments, revenue

**Business Rules Implemented:**
- Cannot book if barber has no Schedule for that day
- Booking time must be inside Schedule StartTime - EndTime
- No double booking same barber + same date + same time
- Cannot complete unpaid appointments

## 🛠 Tech Stack

- ASP.NET Core 8.0 MVC
- Entity Framework Core 8
- ASP.NET Core Identity (Roles: Admin, Barber, Receptionist, Customer)
- SQL Server / LocalDB
- Bootstrap 5

## 📁 Project Structure

Models/
  Person.cs (Base class)
  Barber.cs, Customer.cs, Receptionist.cs
  Branch.cs, Service.cs, Appointment.cs, Schedule.cs, Payment.cs, Review.cs
  ApplicationUser.cs
  Enums/ServiceCategory.cs, StaffRole.cs, Speciality.cs, AppointmentStatus.cs
Data/
  ApplicationDbContext.cs
Controllers/
  AppointmentsController.cs, ServicesController.cs, BranchesController.cs, etc
Views/
  Services/Index.cshtml (with service cards)
  Appointments/Create.cshtml (with availability logic)
Program.cs (Roles & Seeding Logic)

## 🚀 How To Run Locally

**Requirements:** .NET 8 SDK, Visual Studio 2022

1. Clone repo:
```bash
git clone https://github.com/SifundoMkhonZa/IronAndIvoryBarberCo-ComplexVersion.git
2. Open `IronAndIvoryCo.sln` in Visual Studio

3. Check `appsettings.json` connection string:
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=IronAndIvoryCo;Trusted_Connection=True;MultipleActiveResultSets=true"
}
4. In Package Manager Console:
Update-Database
5. Press F5 to run. Database will auto-seed Branch, 13 Services, Roles, Users and Schedules.

## 🗓️ Schedule Model Logic

Barbers work Mon-Sat:
- Mon-Fri: 08:00 - 17:00
- Saturday: 08:00 - 14:00
- Sunday: Closed

Seeded in `Program.cs`.

## 🌍 Deployment (Temporary Free Link for Marking)

Deploy via Railway.app:
1. Push to GitHub (Done)
2. Go to railway.app -> New Project -> Deploy from GitHub -> Select this repo
3. Add MSSQL Database plugin
4. Add Variable `DefaultConnection` from plugin







