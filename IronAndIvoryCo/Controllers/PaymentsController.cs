using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IronAndIvoryCo.Data;
using IronAndIvoryCo.Models;
using IronAndIvoryCo.Models.Enums;

namespace IronAndIvoryCo.Controllers
{
    [Authorize]
    public class PaymentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public PaymentsController(ApplicationDbContext context) => _context = context;

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Payments.Include(p => p.Appointment).Include(p => p.Sale);
            return View(await applicationDbContext.ToListAsync());
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var payment = await _context.Payments.Include(p => p.Appointment).Include(p => p.Sale)
              .FirstOrDefaultAsync(m => m.PaymentId == id);
            if (payment == null) return NotFound();
            return View(payment);
        }

        [Authorize(Roles = "Admin,Customer")]
        public async Task<IActionResult> Create(int? appointmentId, int? saleId)
        {
            if (appointmentId != null)
            {
                var appt = await _context.Appointments.Include(a => a.Service).Include(a => a.Customer)
                  .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);
                if (appt != null)
                {
                    ViewBag.AppointmentId = appt.AppointmentId;
                    ViewBag.CustomerName = appt.Customer?.Name;
                    ViewBag.ServiceName = appt.Service?.Name;
                    ViewBag.DefaultAmount = appt.Service?.Price ?? 0;
                    ViewData["AppointmentId"] = new SelectList(_context.Appointments, "AppointmentId", "AppointmentId", appointmentId);
                }
            }
            else
            {
                ViewData["AppointmentId"] = new SelectList(_context.Appointments, "AppointmentId", "AppointmentId");
            }
            ViewData["SaleId"] = new SelectList(_context.Sales, "SaleId", "SaleId", saleId);
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Customer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PaymentId,Amount,paymentMethod,PaymentStatus,PaymentDate,SaleId,AppointmentId")] Payment payment)
        {
            if (ModelState.IsValid)
            {
                payment.PaymentStatus = PaymentStatus.Paid;
                payment.PaymentDate = DateTime.Now;
                _context.Add(payment);
                await _context.SaveChangesAsync();

                if (payment.AppointmentId != null)
                {
                    var appointment = await _context.Appointments.FindAsync(payment.AppointmentId);
                    if (appointment != null)
                    {
                        appointment.IsPaid = true;
                        appointment.AmountPaid = payment.Amount;
                        if (appointment.AppointmentStatus == AppointmentStatus.Pending)
                            appointment.AppointmentStatus = AppointmentStatus.Confirmed;
                        _context.Update(appointment);
                        await _context.SaveChangesAsync();
                    }
                }
                return RedirectToAction("Index", "Appointments");
            }
            ViewData["AppointmentId"] = new SelectList(_context.Appointments, "AppointmentId", "AppointmentId", payment.AppointmentId);
            ViewData["SaleId"] = new SelectList(_context.Sales, "SaleId", "SaleId", payment.SaleId);
            return View(payment);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null) return NotFound();
            ViewData["AppointmentId"] = new SelectList(_context.Appointments, "AppointmentId", "AppointmentId", payment.AppointmentId);
            ViewData["SaleId"] = new SelectList(_context.Sales, "SaleId", "SaleId", payment.SaleId);
            return View(payment);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PaymentId,Amount,paymentMethod,PaymentStatus,PaymentDate,SaleId,AppointmentId")] Payment payment)
        {
            if (id != payment.PaymentId) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(payment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PaymentExists(payment.PaymentId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["AppointmentId"] = new SelectList(_context.Appointments, "AppointmentId", "AppointmentId", payment.AppointmentId);
            ViewData["SaleId"] = new SelectList(_context.Sales, "SaleId", "SaleId", payment.SaleId);
            return View(payment);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var payment = await _context.Payments.Include(p => p.Appointment).Include(p => p.Sale)
              .FirstOrDefaultAsync(m => m.PaymentId == id);
            if (payment == null) return NotFound();
            return View(payment);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment != null) _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PaymentExists(int id) => _context.Payments.Any(e => e.PaymentId == id);
    }
}
