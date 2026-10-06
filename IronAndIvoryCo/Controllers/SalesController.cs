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
    public class SalesController : Controller
    {
        private readonly ApplicationDbContext _context;
        public SalesController(ApplicationDbContext context) => _context = context;

        // ============ CART - Customer/Admin only ============
        [Authorize(Roles = "Admin,Customer")]
        public async Task<IActionResult> Cart()
        {
            var email = User.Identity?.Name;
            var cust = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email) ?? await _context.Customers.FirstOrDefaultAsync();
            if (cust == null) return View(new Sale { ProductSales = new List<ProductSale>(), TotalAmount = 0 });

            var cart = await _context.Sales
             .Include(s => s.ProductSales).ThenInclude(ps => ps.Product)
             .Include(s => s.Payments)
             .FirstOrDefaultAsync(s => s.CustomerId == cust.Id && !s.Payments.Any());

            if (cart == null) return View(new Sale { ProductSales = new List<ProductSale>(), TotalAmount = 0 });
            return View(cart);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Customer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var email = User.Identity?.Name;
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
            if (customer == null) return Unauthorized();

            var product = await _context.Products.FindAsync(productId);
            if (product == null) return NotFound();

            var cart = await _context.Sales
              .Include(s => s.ProductSales)
              .Include(s => s.Payments)
              .FirstOrDefaultAsync(s => s.CustomerId == customer.Id && !s.Payments.Any());

            if (cart == null)
            {
                cart = new Sale
                {
                    CustomerId = customer.Id,
                    SaleDate = DateTime.Now,
                    BranchId = (await _context.Branches.FirstOrDefaultAsync())?.BranchId ?? 1,
                    Status = SaleStatus.Pending,
                    TotalAmount = 0,
                    ProductSales = new List<ProductSale>()
                };
                _context.Sales.Add(cart);
                await _context.SaveChangesAsync();
            }

            var line = await _context.ProductSales.FirstOrDefaultAsync(ps => ps.SaleId == cart.SaleId && ps.ProductId == productId);
            if (line != null)
            {
                line.Quantity += quantity;
            }
            else
            {
                line = new ProductSale
                {
                    SaleId = cart.SaleId,
                    ProductId = productId,
                    Quantity = quantity,
                    UnitPrice = product.Price
                };
                _context.ProductSales.Add(line);
            }
            await _context.SaveChangesAsync();

            cart.TotalAmount = await _context.ProductSales.Where(ps => ps.SaleId == cart.SaleId).SumAsync(ps => ps.Quantity * ps.UnitPrice);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Cart));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Customer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int productSaleId, int quantity)
        {
            var line = await _context.ProductSales.Include(ps => ps.Sale).FirstOrDefaultAsync(ps => ps.ProductSaleId == productSaleId);
            if (line == null) return NotFound();
            if (quantity <= 0) _context.ProductSales.Remove(line);
            else line.Quantity = quantity;
            await _context.SaveChangesAsync();
            line.Sale.TotalAmount = await _context.ProductSales.Where(ps => ps.SaleId == line.SaleId).SumAsync(ps => ps.Quantity * ps.UnitPrice);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Cart));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Customer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int productSaleId)
        {
            var line = await _context.ProductSales.Include(ps => ps.Sale).FirstOrDefaultAsync(ps => ps.ProductSaleId == productSaleId);
            if (line != null)
            {
                _context.ProductSales.Remove(line);
                await _context.SaveChangesAsync();
                line.Sale.TotalAmount = await _context.ProductSales.Where(ps => ps.SaleId == line.SaleId).SumAsync(ps => ps.Quantity * ps.UnitPrice);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Cart));
        }

        [Authorize(Roles = "Admin,Customer")]
        public async Task<IActionResult> Checkout()
        {
            var email = User.Identity?.Name;
            var cust = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email) ?? await _context.Customers.FirstOrDefaultAsync();
            if (cust == null) return RedirectToAction(nameof(Cart));

            var cart = await _context.Sales
             .Include(s => s.ProductSales).ThenInclude(ps => ps.Product)
             .Include(s => s.Payments)
             .FirstOrDefaultAsync(s => s.CustomerId == cust.Id && !s.Payments.Any());

            if (cart == null || !cart.ProductSales.Any()) return RedirectToAction(nameof(Cart));
            return View(cart);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Customer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckoutConfirmed(paymentMethod paymentMethod)
        {
            var email = User.Identity?.Name;
            var cust = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email) ?? await _context.Customers.FirstOrDefaultAsync();
            if (cust == null) return RedirectToAction(nameof(Cart));

            var cart = await _context.Sales
             .Include(s => s.ProductSales)
             .Include(s => s.Payments)
             .FirstOrDefaultAsync(s => s.CustomerId == cust.Id && !s.Payments.Any());

            if (cart == null || !cart.ProductSales.Any()) return RedirectToAction(nameof(Cart));

            var payment = new Payment
            {
                PaymentDate = DateTime.Now,
                Amount = cart.TotalAmount,
                paymentMethod = paymentMethod,
                PaymentStatus = PaymentStatus.Paid,
                SaleId = cart.SaleId
            };
            _context.Payments.Add(payment);
            cart.Status = SaleStatus.Completed;
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Order #{cart.SaleId} paid with {paymentMethod}! R {cart.TotalAmount:F2}";
            return RedirectToAction(nameof(Index), "Products");
        }

        // ============ SALES RECORDS - Admin/Receptionist only ============
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Index()
        {
            var sales = _context.Sales.Include(s => s.Branch).Include(s => s.Customer).Include(s => s.Payments);
            return View(await sales.ToListAsync());
        }

        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var sale = await _context.Sales
             .Include(s => s.Branch).Include(s => s.Customer)
             .Include(s => s.ProductSales).ThenInclude(ps => ps.Product)
             .Include(s => s.Payments)
             .FirstOrDefaultAsync(m => m.SaleId == id);
            if (sale == null) return NotFound();
            return View(sale);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["BranchId"] = new SelectList(_context.Branches, "BranchId", "Address");
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Email");
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SaleId,SaleDate,TotalAmount,CustomerId,BranchId")] Sale sale)
        {
            if (ModelState.IsValid) { _context.Add(sale); await _context.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
            ViewData["BranchId"] = new SelectList(_context.Branches, "BranchId", "Address", sale.BranchId);
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Email", sale.CustomerId);
            return View(sale);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var sale = await _context.Sales.FindAsync(id);
            if (sale == null) return NotFound();
            ViewData["BranchId"] = new SelectList(_context.Branches, "BranchId", "Address", sale.BranchId);
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Email", sale.CustomerId);
            return View(sale);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SaleId,SaleDate,TotalAmount,CustomerId,BranchId")] Sale sale)
        {
            if (id != sale.SaleId) return NotFound();
            if (ModelState.IsValid)
            {
                try { _context.Update(sale); await _context.SaveChangesAsync(); }
                catch (DbUpdateConcurrencyException) { if (!_context.Sales.Any(e => e.SaleId == sale.SaleId)) return NotFound(); else throw; }
                return RedirectToAction(nameof(Index));
            }
            ViewData["BranchId"] = new SelectList(_context.Branches, "BranchId", "Address", sale.BranchId);
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Email", sale.CustomerId);
            return View(sale);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var sale = await _context.Sales
             .Include(s => s.Branch).Include(s => s.Customer)
             .Include(s => s.Payments).Include(s => s.ProductSales)
             .FirstOrDefaultAsync(m => m.SaleId == id);
            if (sale == null) return NotFound();
            return View(sale);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sale = await _context.Sales.Include(s => s.ProductSales).Include(s => s.Payments).FirstOrDefaultAsync(s => s.SaleId == id);
            if (sale != null)
            {
                if (sale.Payments.Any()) _context.Payments.RemoveRange(sale.Payments);
                if (sale.ProductSales.Any()) _context.ProductSales.RemoveRange(sale.ProductSales);
                _context.Sales.Remove(sale);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}