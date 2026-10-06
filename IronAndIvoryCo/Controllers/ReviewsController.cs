using IronAndIvoryCo.Data;
using IronAndIvoryCo.Models;
using IronAndIvoryCo.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IronAndIvoryCo.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ReviewsController(ApplicationDbContext context) => _context = context;

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var reviews = _context.Reviews.Include(r => r.Barber).Include(r => r.Customer).OrderByDescending(r => r.ReviewDate);
            return View(await reviews.ToListAsync());
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var review = await _context.Reviews.Include(r => r.Barber).Include(r => r.Customer)
           .FirstOrDefaultAsync(m => m.ReviewId == id);
            return review == null ? NotFound() : View(review);
        }

        // Customer can ONLY review their own completed appointment
        [Authorize(Roles = "Admin,Customer")]
        public async Task<IActionResult> Create(int? appointmentId = null)
        {
            var email = User.Identity?.Name;
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
            if (customer == null) return NotFound("Customer profile not found");

            Appointment? appointment = null;

            if (appointmentId.HasValue)
            {
                appointment = await _context.Appointments
                 .Include(a => a.Barber)
                 .Include(a => a.Service)
                 .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId && a.CustomerId == customer.Id);
            }
            else
            {
                appointment = await _context.Appointments
                 .Include(a => a.Barber)
                 .Include(a => a.Service)
                 .Where(a => a.CustomerId == customer.Id && a.AppointmentStatus == AppointmentStatus.Completed)
                 .OrderByDescending(a => a.Date)
                 .FirstOrDefaultAsync();
            }

            if (appointment == null)
            {
                TempData["Error"] = "You need a completed appointment to review";
                return RedirectToAction("Index", "Appointments");
            }

            bool alreadyReviewed = await _context.Reviews.AnyAsync(r => r.AppointmentId == appointment.AppointmentId);
            if (alreadyReviewed)
            {
                TempData["Error"] = "You already reviewed this appointment";
                return RedirectToAction("Index", "Appointments");
            }

            ViewBag.Appointment = appointment;
            ViewBag.BarberName = appointment.Barber.Name;

            var review = new Review
            {
                AppointmentId = appointment.AppointmentId,
                BarberId = appointment.BarberId,
                CustomerId = customer.Id,
                ReviewDate = DateTime.Now
            };

            return View(review);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Customer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Rating,Comment,BarberId,CustomerId,AppointmentId,ReviewDate")] Review review)
        {
            ModelState.Remove("Barber");
            ModelState.Remove("Customer");
            ModelState.Remove("Appointment");

            if (User.IsInRole("Customer"))
            {
                var email = User.Identity?.Name;
                var cust = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
                if (cust != null)
                {
                    review.CustomerId = cust.Id;
                    var appt = await _context.Appointments.FindAsync(review.AppointmentId);
                    if (appt != null && appt.CustomerId == cust.Id)
                    {
                        review.BarberId = appt.BarberId;
                    }
                    else
                    {
                        ModelState.AddModelError("", "Invalid appointment");
                    }
                }
            }

            review.ReviewDate = DateTime.Now;

            if (ModelState.IsValid)
            {
                var exists = await _context.Appointments.AnyAsync(a => a.AppointmentId == review.AppointmentId);
                if (!exists)
                {
                    ModelState.AddModelError("AppointmentId", "Appointment does not exist");
                }
                else
                {
                    _context.Add(review);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Review submitted!";
                    return RedirectToAction("Details", "Barbers", new { id = review.BarberId });
                }
            }

            var appt2 = await _context.Appointments.Include(a => a.Barber).Include(a => a.Service).FirstOrDefaultAsync(a => a.AppointmentId == review.AppointmentId);
            ViewBag.Appointment = appt2;
            ViewBag.BarberName = appt2?.Barber?.Name;
            return View(review);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return NotFound();
            ViewData["BarberId"] = new SelectList(_context.Barbers, "Id", "Name", review.BarberId);
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Email", review.CustomerId);
            return View(review);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ReviewId,Rating,Comment,ReviewDate,CustomerId,BarberId,AppointmentId")] Review review)
        {
            if (id != review.ReviewId) return NotFound();
            ModelState.Remove("Barber");
            ModelState.Remove("Customer");
            ModelState.Remove("Appointment");
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(review);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReviewExists(review.ReviewId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["BarberId"] = new SelectList(_context.Barbers, "Id", "Name", review.BarberId);
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Email", review.CustomerId);
            return View(review);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var review = await _context.Reviews.Include(r => r.Barber).Include(r => r.Customer)
           .FirstOrDefaultAsync(m => m.ReviewId == id);
            return review == null ? NotFound() : View(review);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null) _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReviewExists(int id) => _context.Reviews.Any(e => e.ReviewId == id);
    }
}
