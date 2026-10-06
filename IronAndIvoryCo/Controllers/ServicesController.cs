using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IronAndIvoryCo.Data;
using IronAndIvoryCo.Models;

namespace IronAndIvoryCo.Controllers
{
    public class ServicesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ServicesController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Services.Include(s => s.Branch);
            return View(await applicationDbContext.ToListAsync());
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var service = await _context.Services.Include(s => s.Branch).FirstOrDefaultAsync(m => m.ServiceId == id);
            if (service == null) return NotFound();
            return View(service);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["BranchId"] = new SelectList(_context.Branches, "BranchId", "Address");
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Service service)
        {
            if (ModelState.IsValid)
            {
                if (service.ImageFile != null)
                {
                    string wwwRootPath = _env.WebRootPath;
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(service.ImageFile.FileName);
                    string path = Path.Combine(wwwRootPath, "images", fileName);
                    Directory.CreateDirectory(Path.Combine(wwwRootPath, "images"));
                    using (var fileStream = new FileStream(path, FileMode.Create))
                        await service.ImageFile.CopyToAsync(fileStream);
                    service.ImageUrl = "/images/" + fileName;
                }
                _context.Add(service);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BranchId"] = new SelectList(_context.Branches, "BranchId", "Address", service.BranchId);
            return View(service);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var service = await _context.Services.FindAsync(id);
            if (service == null) return NotFound();
            ViewData["BranchId"] = new SelectList(_context.Branches, "BranchId", "Address", service.BranchId);
            return View(service);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Service service)
        {
            if (id != service.ServiceId) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.Services.AsNoTracking().FirstOrDefaultAsync(s => s.ServiceId == id);
                    if (service.ImageFile != null)
                    {
                        string wwwRootPath = _env.WebRootPath;
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(service.ImageFile.FileName);
                        string path = Path.Combine(wwwRootPath, "images", fileName);
                        Directory.CreateDirectory(Path.Combine(wwwRootPath, "images"));
                        using (var fileStream = new FileStream(path, FileMode.Create))
                            await service.ImageFile.CopyToAsync(fileStream);
                        service.ImageUrl = "/images/" + fileName;
                    }
                    else service.ImageUrl = existing?.ImageUrl;
                    _context.Update(service);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceExists(service.ServiceId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["BranchId"] = new SelectList(_context.Branches, "BranchId", "Address", service.BranchId);
            return View(service);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var service = await _context.Services.Include(s => s.Branch).FirstOrDefaultAsync(m => m.ServiceId == id);
            if (service == null) return NotFound();
            return View(service);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service != null) _context.Services.Remove(service);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ServiceExists(int id) => _context.Services.Any(e => e.ServiceId == id);
    }
}
