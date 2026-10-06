using IronAndIvoryCo.Data;
using IronAndIvoryCo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IronAndIvoryCo.Controllers
{
    public class SchedulesController : Controller
    {
        private readonly ApplicationDbContext _context;
        public SchedulesController(ApplicationDbContext context) => _context = context;

        [Authorize(Roles = "Admin,Receptionist,Barber")]
        public async Task<IActionResult> Index()
        {
            var email = User.Identity?.Name;
            var query = _context.Schedules.Include(s => s.Barber).AsQueryable();

            if (User.IsInRole("Barber"))
            {
                var barber = await _context.Barbers.FirstOrDefaultAsync(b => b.Email == email);
                if (barber != null) query = query.Where(s => s.BarberId == barber.Id);
                else query = query.Where(s => false);
            }
            return View(await query.ToListAsync());
        }

        [Authorize(Roles = "Admin,Receptionist,Barber")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var schedule = await _context.Schedules.Include(s => s.Barber).FirstOrDefaultAsync(m => m.ScheduleId == id);
            if (schedule == null) return NotFound();

            // SECURITY: Barber can only view own schedule
            if (User.IsInRole("Barber"))
            {
                var email = User.Identity?.Name;
                var barber = await _context.Barbers.FirstOrDefaultAsync(b => b.Email == email);
                if (barber == null || schedule.BarberId != barber.Id)
                {
                    return Forbid();
                }
            }

            return View(schedule);
        }

        [Authorize(Roles = "Admin,Receptionist")]
        public IActionResult Create()
        {
            ViewData["BarberId"] = new SelectList(_context.Barbers, "Id", "Name");
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Receptionist")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ScheduleId,DayOfWeek,StartTime,EndTime,IsAvailable,BarberId")] Schedule schedule)
        {
            if (ModelState.IsValid)
            {
                _context.Add(schedule);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BarberId"] = new SelectList(_context.Barbers, "Id", "Name", schedule.BarberId);
            return View(schedule);
        }

        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var schedule = await _context.Schedules.FindAsync(id);
            if (schedule == null) return NotFound();
            ViewData["BarberId"] = new SelectList(_context.Barbers, "Id", "Name", schedule.BarberId);
            return View(schedule);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Receptionist")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ScheduleId,DayOfWeek,StartTime,EndTime,IsAvailable,BarberId")] Schedule schedule)
        {
            if (id != schedule.ScheduleId) return NotFound();
            if (ModelState.IsValid)
            {
                try { _context.Update(schedule); await _context.SaveChangesAsync(); }
                catch (DbUpdateConcurrencyException) { if (!ScheduleExists(schedule.ScheduleId)) return NotFound(); else throw; }
                return RedirectToAction(nameof(Index));
            }
            ViewData["BarberId"] = new SelectList(_context.Barbers, "Id", "Name", schedule.BarberId);
            return View(schedule);
        }

        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var schedule = await _context.Schedules.Include(s => s.Barber).FirstOrDefaultAsync(m => m.ScheduleId == id);
            if (schedule == null) return NotFound();
            return View(schedule);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin,Receptionist")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var schedule = await _context.Schedules.FindAsync(id);
            if (schedule != null) _context.Schedules.Remove(schedule);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ScheduleExists(int id) => _context.Schedules.Any(e => e.ScheduleId == id);
    }
}