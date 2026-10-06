using IronAndIvoryCo.Data;
using IronAndIvoryCo.Models;
using IronAndIvoryCo.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IronAndIvoryCo.Controllers
{
    [Authorize]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AppointmentsController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            var query = _context.Appointments
           .Include(a => a.Barber).Include(a => a.Branch).Include(a => a.Customer)
           .Include(a => a.Receptionist).Include(a => a.Service).Include(a => a.Payments).Include(a => a.Review)
           .AsQueryable();

            if (User.IsInRole("Customer"))
            {
                var email = User.Identity?.Name;
                query = query.Where(a => a.Customer.Email == email);
            }
            else if (User.IsInRole("Barber"))
            {
                var email = User.Identity?.Name;
                query = query.Where(a => a.Barber.Email == email);
            }
            return View(await query.OrderByDescending(a => a.Date).ThenByDescending(a => a.Time).ToListAsync());
        }

        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> MyOrders()
        {
            var email = User.Identity?.Name;
            var myOrders = await _context.Appointments
           .Include(a => a.Barber).Include(a => a.Branch).Include(a => a.Service)
           .Include(a => a.Payments).Include(a => a.Review).Include(a => a.Customer)
           .Where(a => a.Customer.Email == email)
           .OrderByDescending(a => a.Date).ThenByDescending(a => a.Time)
           .ToListAsync();
            return View("Index", myOrders);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var appointment = await _context.Appointments
           .Include(a => a.Barber).Include(a => a.Branch).Include(a => a.Customer)
           .Include(a => a.Receptionist).Include(a => a.Service).Include(a => a.Payments).Include(a => a.Review)
           .FirstOrDefaultAsync(m => m.AppointmentId == id);
            if (appointment == null) return NotFound();
            return View(appointment);
        }

        [Authorize(Roles = "Customer,Admin,Receptionist")]
        public async Task<IActionResult> Create()
        {
            var scheduledIds = await _context.Schedules
             .Where(s => s.IsAvailable)
             .Select(s => s.BarberId)
             .Distinct().ToListAsync();

            var barbers = scheduledIds.Any()
             ? await _context.Barbers.Where(b => scheduledIds.Contains(b.Id)).ToListAsync()
               : await _context.Barbers.ToListAsync();

            ViewData["BarberId"] = new SelectList(barbers, "Id", "Name");
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Name");
            ViewData["BranchId"] = new SelectList(_context.Branches, "BranchId", "BranchName");
            ViewData["ServiceId"] = new SelectList(_context.Services, "ServiceId", "Name");
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Customer,Admin,Receptionist")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Date,Time,Note,CustomerId,BarberId,ServiceId,ReceptionistId,BranchId")] Appointment appointment)
        {
            if (User.IsInRole("Customer"))
            {
                var email = User.Identity?.Name;
                var cust = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
                if (cust != null) appointment.CustomerId = cust.Id;
            }

            if (appointment.ReceptionistId == 0) appointment.ReceptionistId = null;
            if (appointment.ReceptionistId == null)
            {
                var firstRec = await _context.Receptionists.FirstOrDefaultAsync();
                if (firstRec != null) appointment.ReceptionistId = firstRec.Id;
            }

            // ===== FIXED SCHEDULE VALIDATION FOR YOUR MODEL =====
            if (appointment.BarberId != 0 && appointment.Date != default)
            {
                var dayName = appointment.Date.DayOfWeek.ToString(); // "Monday"

                var schedule = await _context.Schedules
                 .FirstOrDefaultAsync(s => s.BarberId == appointment.BarberId
                                           && s.DayOfWeek == dayName
                                           && s.IsAvailable);

                if (schedule == null)
                {
                    ModelState.AddModelError("BarberId", $"Barber not scheduled on {dayName}. Choose another barber or date.");
                }
                else
                {
                    // Appointment.Time is TimeOnly - compare correctly
                    if (appointment.Time < schedule.StartTime || appointment.Time >= schedule.EndTime)
                    {
                        ModelState.AddModelError("Time", $"Barber works {schedule.StartTime:hh\\:mm} - {schedule.EndTime:hh\\:mm} on {dayName}.");
                    }

                    var conflict = await _context.Appointments.AnyAsync(a =>
                        a.BarberId == appointment.BarberId &&
                        a.Date == appointment.Date &&
                        a.Time == appointment.Time &&
                        a.AppointmentStatus != AppointmentStatus.Cancelled);

                    if (conflict)
                    {
                        ModelState.AddModelError("Time", $"Barber already booked at {appointment.Time:hh\\:mm} on {appointment.Date:dd MMM}. Choose another slot.");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                appointment.IsPaid = false;
                appointment.AmountPaid = 0;
                appointment.AppointmentStatus = AppointmentStatus.Pending;
                _context.Add(appointment);
                await _context.SaveChangesAsync();

                if (User.IsInRole("Customer"))
                    return RedirectToAction(nameof(MyOrders));
                return RedirectToAction(nameof(Index));
            }

            var scheduledIds2 = await _context.Schedules.Where(s => s.IsAvailable).Select(s => s.BarberId).Distinct().ToListAsync();
            var barbers2 = scheduledIds2.Any() ? await _context.Barbers.Where(b => scheduledIds2.Contains(b.Id)).ToListAsync() : await _context.Barbers.ToListAsync();

            ViewData["BarberId"] = new SelectList(barbers2, "Id", "Name", appointment.BarberId);
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Name", appointment.CustomerId);
            ViewData["BranchId"] = new SelectList(_context.Branches, "BranchId", "BranchName", appointment.BranchId);
            ViewData["ServiceId"] = new SelectList(_context.Services, "ServiceId", "Name", appointment.ServiceId);
            return View(appointment);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();
            ViewData["BarberId"] = new SelectList(_context.Barbers, "Id", "Name", appointment.BarberId);
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Name", appointment.CustomerId);
            ViewData["BranchId"] = new SelectList(_context.Branches, "BranchId", "BranchName", appointment.BranchId);
            ViewData["ServiceId"] = new SelectList(_context.Services, "ServiceId", "Name", appointment.ServiceId);
            ViewData["ReceptionistId"] = new SelectList(_context.Receptionists, "Id", "Name", appointment.ReceptionistId);
            return View(appointment);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AppointmentId,Date,Time,AppointmentStatus,Note,CustomerId,BarberId,ServiceId,ReceptionistId,BranchId,IsPaid,AmountPaid")] Appointment appointment)
        {
            if (id != appointment.AppointmentId) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(appointment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BarberId"] = new SelectList(_context.Barbers, "Id", "Name", appointment.BarberId);
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Name", appointment.CustomerId);
            ViewData["BranchId"] = new SelectList(_context.Branches, "BranchId", "BranchName", appointment.BranchId);
            ViewData["ServiceId"] = new SelectList(_context.Services, "ServiceId", "Name", appointment.ServiceId);
            ViewData["ReceptionistId"] = new SelectList(_context.Receptionists, "Id", "Name", appointment.ReceptionistId);
            return View(appointment);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Receptionist")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();
            appointment.AppointmentStatus = AppointmentStatus.Confirmed;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Receptionist,Barber")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.AppointmentId == id);
            if (appointment == null) return NotFound();
            if (!appointment.IsPaid)
            {
                TempData["Error"] = "Cannot complete — payment not received!";
                return RedirectToAction(nameof(Index));
            }
            appointment.AppointmentStatus = AppointmentStatus.Completed;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var appointment = await _context.Appointments
           .Include(a => a.Barber).Include(a => a.Branch).Include(a => a.Customer)
           .Include(a => a.Receptionist).Include(a => a.Service)
           .FirstOrDefaultAsync(m => m.AppointmentId == id);
            if (appointment == null) return NotFound();
            return View(appointment);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null) _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}