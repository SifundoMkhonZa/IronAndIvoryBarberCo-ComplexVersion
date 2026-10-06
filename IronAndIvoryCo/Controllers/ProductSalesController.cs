using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IronAndIvoryCo.Data;
using IronAndIvoryCo.Models;

namespace IronAndIvoryCo.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductSalesController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ProductSalesController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.ProductSales.Include(p => p.Product).Include(p => p.Sale);
            return View(await applicationDbContext.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var productSale = await _context.ProductSales.Include(p => p.Product).Include(p => p.Sale)
              .FirstOrDefaultAsync(m => m.ProductSaleId == id);
            if (productSale == null) return NotFound();
            return View(productSale);
        }

        public IActionResult Create()
        {
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "Brand");
            ViewData["SaleId"] = new SelectList(_context.Sales, "SaleId", "SaleId");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductSaleId,Quantity,UnitPrice,ProductId,SaleId")] ProductSale productSale)
        {
            if (ModelState.IsValid)
            {
                _context.Add(productSale);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "Brand", productSale.ProductId);
            ViewData["SaleId"] = new SelectList(_context.Sales, "SaleId", "SaleId", productSale.SaleId);
            return View(productSale);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var productSale = await _context.ProductSales.FindAsync(id);
            if (productSale == null) return NotFound();
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "Brand", productSale.ProductId);
            ViewData["SaleId"] = new SelectList(_context.Sales, "SaleId", "SaleId", productSale.SaleId);
            return View(productSale);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductSaleId,Quantity,UnitPrice,ProductId,SaleId")] ProductSale productSale)
        {
            if (id != productSale.ProductSaleId) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(productSale);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductSaleExists(productSale.ProductSaleId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "Brand", productSale.ProductId);
            ViewData["SaleId"] = new SelectList(_context.Sales, "SaleId", "SaleId", productSale.SaleId);
            return View(productSale);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var productSale = await _context.ProductSales.Include(p => p.Product).Include(p => p.Sale)
              .FirstOrDefaultAsync(m => m.ProductSaleId == id);
            if (productSale == null) return NotFound();
            return View(productSale);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var productSale = await _context.ProductSales.FindAsync(id);
            if (productSale != null) _context.ProductSales.Remove(productSale);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductSaleExists(int id) => _context.ProductSales.Any(e => e.ProductSaleId == id);
    }
}
