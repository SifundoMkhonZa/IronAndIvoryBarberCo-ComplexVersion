using IronAndIvoryCo.Data;
using IronAndIvoryCo.Models;
using IronAndIvoryCo.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IronAndIvoryCo.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Products.Include(p => p.Branch);
            return View(await applicationDbContext.ToListAsync());
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var product = await _context.Products.Include(p => p.Branch)
            .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null) return NotFound();
            return View(product);
        }

        [Authorize(Roles = "Admin,Customer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return NotFound();

            var customer = await _context.Customers.FirstOrDefaultAsync();
            if (customer == null)
            {
                customer = new Customer
                {
                    Name = "Walk-in Customer",
                    Phone = "0123456789",
                    Email = "walkin@ironandivory.co.za",
                    DateOfBirth = new DateTime(2000, 1, 1),
                    Gender = Gender.Male,
                    LoyaltyPoints = 0
                };
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
            }

            // FIXED: Payment -> Payments.Any()
            var openSale = await _context.Sales
               .Include(s => s.Payments)
               .Include(s => s.ProductSales)
               .FirstOrDefaultAsync(s => s.CustomerId == customer.Id && !s.Payments.Any());

            if (openSale == null)
            {
                openSale = new Sale
                {
                    CustomerId = customer.Id,
                    SaleDate = DateTime.Now,
                    TotalAmount = 0,
                    BranchId = product.BranchId,
                    Status = SaleStatus.Pending,
                    Payments = new List<Payment>(),
                    ProductSales = new List<ProductSale>()
                };
                _context.Sales.Add(openSale);
                await _context.SaveChangesAsync();
            }

            var line = await _context.ProductSales.FirstOrDefaultAsync(ps => ps.SaleId == openSale.SaleId && ps.ProductId == productId);

            if (line != null) line.Quantity += quantity;
            else
                _context.ProductSales.Add(new ProductSale
                {
                    SaleId = openSale.SaleId,
                    ProductId = productId,
                    Quantity = quantity,
                    UnitPrice = product.Price
                });

            await _context.SaveChangesAsync();

            openSale.TotalAmount = await _context.ProductSales
            .Where(ps => ps.SaleId == openSale.SaleId)
            .SumAsync(ps => ps.Quantity * ps.UnitPrice);

            await _context.SaveChangesAsync();
            return RedirectToAction("Cart", "Sales");
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
        public async Task<IActionResult> Create(Product product)
        {
            ModelState.Remove("Branch");
            if (ModelState.IsValid)
            {
                if (product.ImageFile != null)
                {
                    string wwwRootPath = _env.WebRootPath;
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(product.ImageFile.FileName);
                    string path = Path.Combine(wwwRootPath, "images", fileName);
                    Directory.CreateDirectory(Path.Combine(wwwRootPath, "images"));
                    using (var fileStream = new FileStream(path, FileMode.Create))
                    {
                        await product.ImageFile.CopyToAsync(fileStream);
                    }
                    product.ImageUrl = "/images/" + fileName;
                }
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BranchId"] = new SelectList(_context.Branches, "BranchId", "Address", product.BranchId);
            return View(product);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            ViewData["BranchId"] = new SelectList(_context.Branches, "BranchId", "Address", product.BranchId);
            return View(product);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            if (id != product.ProductId) return NotFound();
            ModelState.Remove("Branch");
            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == id);
                    if (product.ImageFile != null)
                    {
                        string wwwRootPath = _env.WebRootPath;
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(product.ImageFile.FileName);
                        string path = Path.Combine(wwwRootPath, "images", fileName);
                        Directory.CreateDirectory(Path.Combine(wwwRootPath, "images"));
                        using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            await product.ImageFile.CopyToAsync(fileStream);
                        }
                        product.ImageUrl = "/images/" + fileName;
                    }
                    else product.ImageUrl = existing.ImageUrl;
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["BranchId"] = new SelectList(_context.Branches, "BranchId", "Address", product.BranchId);
            return View(product);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var product = await _context.Products.Include(p => p.Branch).FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.Include(p => p.ProductSales).FirstOrDefaultAsync(p => p.ProductId == id);
            if (product != null)
            {
                if (product.ProductSales.Any()) _context.ProductSales.RemoveRange(product.ProductSales);
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id) => _context.Products.Any(e => e.ProductId == id);
    }
}
