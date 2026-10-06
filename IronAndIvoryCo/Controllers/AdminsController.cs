using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IronAndIvoryCo.Data;
using IronAndIvoryCo.Models;

namespace IronAndIvoryCo.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AdminsController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Index() =>
            View(await _context.Admins.ToListAsync());

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var admin = await _context.Admins.Include(a => a.StaffMembers).FirstOrDefaultAsync(m => m.Id == id);
            if (admin == null) return NotFound();
            return View(admin);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Phone,Email,AccessCode")] Admin admin)
        {
            if (ModelState.IsValid)
            {
                admin.CreatedDate = DateTime.Now;
                _context.Add(admin);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(admin);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var admin = await _context.Admins.FindAsync(id);
            if (admin == null) return NotFound();
            return View(admin);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Phone,Email,AccessCode")] Admin admin)
        {
            if (id != admin.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.Admins.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
                    admin.CreatedDate = existing!.CreatedDate;
                    _context.Update(admin);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Admins.Any(e => e.Id == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(admin);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var admin = await _context.Admins.FirstOrDefaultAsync(m => m.Id == id);
            if (admin == null) return NotFound();
            return View(admin);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var admin = await _context.Admins.FindAsync(id);
            if (admin != null) _context.Admins.Remove(admin);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
