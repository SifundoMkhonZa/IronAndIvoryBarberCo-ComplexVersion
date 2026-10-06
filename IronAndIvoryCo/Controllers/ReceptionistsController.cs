using IronAndIvoryCo.Data;
using IronAndIvoryCo.Models;
using IronAndIvoryCo.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IronAndIvoryCo.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReceptionistsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ReceptionistsController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Index() =>
            View(await _context.Receptionists.Include(r => r.Branch).OrderByDescending(r => r.CreatedDate).ToListAsync());

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var r = await _context.Receptionists.Include(x => x.Branch).FirstOrDefaultAsync(m => m.Id == id);
            return r == null ? NotFound() : View(r);
        }

        public IActionResult Create()
        {
            ViewData["BranchId"] = new SelectList(_context.Branches, "BranchId", "BranchName");
            ViewData["AdminId"] = new SelectList(_context.Admins, "AdminId", "Name");
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Phone,Email,BranchId,Shift,DeskNumber,AdminId")] Receptionist receptionist)
        {
            if (ModelState.IsValid)
            {
                receptionist.Role = StaffRole.Receptionist;
                receptionist.CreatedDate = DateTime.Now;
                _context.Add(receptionist);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Receptionist {receptionist.Name} added.";
                return RedirectToAction(nameof(Index));
            }
            ViewData["BranchId"] = new SelectList(_context.Branches, "BranchId", "BranchName", receptionist.BranchId);
            ViewData["AdminId"] = new SelectList(_context.Admins, "AdminId", "Name", receptionist.AdminId);
            return View(receptionist);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var receptionist = await _context.Receptionists.FindAsync(id);
            if (receptionist == null) return NotFound();
            ViewData["BranchId"] = new SelectList(_context.Branches, "BranchId", "BranchName", receptionist.BranchId);
            return View(receptionist);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Phone,Email,BranchId,Shift,DeskNumber,AdminId")] Receptionist receptionist)
        {
            if (id != receptionist.Id) return NotFound();
            if (!ModelState.IsValid)
            {
                ViewData["BranchId"] = new SelectList(_context.Branches, "BranchId", "BranchName", receptionist.BranchId);
                return View(receptionist);
            }
            try
            {
                var existing = await _context.Receptionists.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
                if (existing == null) return NotFound();
                receptionist.Role = StaffRole.Receptionist;
                receptionist.CreatedDate = existing.CreatedDate;
                _context.Update(receptionist);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Receptionists.Any(e => e.Id == id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var r = await _context.Receptionists.Include(x => x.Branch).FirstOrDefaultAsync(m => m.Id == id);
            return r == null ? NotFound() : View(r);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var r = await _context.Receptionists.FindAsync(id);
            if (r != null) _context.Receptionists.Remove(r);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}

