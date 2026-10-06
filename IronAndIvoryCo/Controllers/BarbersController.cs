using IronAndIvoryCo.Data;
using IronAndIvoryCo.Models;
using IronAndIvoryCo.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IronAndIvoryCo.Controllers
{
    public class BarbersController : Controller
    {
        private readonly ApplicationDbContext _context;
        public BarbersController(ApplicationDbContext context) => _context = context;

        // Anyone can view barbers
        [AllowAnonymous]
        public async Task<IActionResult> Index() =>
            View(await _context.Barbers.Include(b => b.Branch).ToListAsync());

        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var barber = await _context.Barbers
             .Include(b => b.Branch)
             .Include(b => b.Reviews).ThenInclude(r => r.Customer)
             .Include(b => b.Schedules)
             .Include(b => b.Appointments)
             .FirstOrDefaultAsync(m => m.Id == id);

            if (barber == null) return NotFound();

            // FIXED: was c.CustomerId -> c.Id
            ViewBag.FirstCustomerId = await _context.Customers.Select(c => c.Id).FirstOrDefaultAsync();

            return View(barber);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["BranchId"] = new SelectList(_context.Branches, "BranchId", "BranchName");
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Phone,Email,BranchId,Speciality")] Barber barber)
        {
            ModelState.Remove("Branch");
            ModelState.Remove("Role");
            if (ModelState.IsValid)
            {
                barber.Role = StaffRole.Barber;
                barber.CreatedDate = DateTime.Now;
                _context.Add(barber);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BranchId"] = new SelectList(_context.Branches, "BranchId", "BranchName", barber.BranchId);
            return View(barber);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var barber = await _context.Barbers.FindAsync(id);
            if (barber == null) return NotFound();
            ViewData["BranchId"] = new SelectList(_context.Branches, "BranchId", "BranchName", barber.BranchId);
            return View(barber);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Phone,Email,BranchId,Speciality")] Barber barber)
        {
            if (id != barber.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.Barbers.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
                    barber.Role = StaffRole.Barber;
                    barber.CreatedDate = existing!.CreatedDate;
                    _context.Update(barber);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Barbers.Any(e => e.Id == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["BranchId"] = new SelectList(_context.Branches, "BranchId", "BranchName", barber.BranchId);
            return View(barber);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var barber = await _context.Barbers.Include(b => b.Branch).FirstOrDefaultAsync(m => m.Id == id);
            return barber == null ? NotFound() : View(barber);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var barber = await _context.Barbers.FindAsync(id);
            if (barber != null) _context.Barbers.Remove(barber);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
