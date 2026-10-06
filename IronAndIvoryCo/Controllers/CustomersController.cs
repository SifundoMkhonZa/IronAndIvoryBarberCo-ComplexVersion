using IronAndIvoryCo.Data;
using IronAndIvoryCo.Models;
using IronAndIvoryCo.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IronAndIvoryCo.Controllers
{
    public class CustomersController : Controller
    {
        private readonly ApplicationDbContext _context;
        public CustomersController(ApplicationDbContext context) => _context = context;

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index() => View(await _context.Customers.ToListAsync());

        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var customer = await _context.Customers.FirstOrDefaultAsync(m => m.Id == id);
            if (customer == null) return NotFound();
            return View(customer);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,DateOfBirth,Gender,LoyaltyPoints,Name,Phone,Email,CreatedDate")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                customer.CreatedDate = DateTime.Now;
                _context.Add(customer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DateOfBirth,Gender,LoyaltyPoints,Name,Phone,Email,CreatedDate")] Customer customer)
        {
            if (id != customer.Id) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(customer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var customer = await _context.Customers.FirstOrDefaultAsync(m => m.Id == id);
            if (customer == null) return NotFound();
            return View(customer);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null) _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> MyProfile()
        {
            var email = User.Identity?.Name;
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
            if (customer == null)
            {
                customer = new Customer
                {
                    Name = email!,
                    Email = email!,
                    Phone = "0000000000",
                    Gender = Gender.Other,
                    CreatedDate = DateTime.Now,
                    LoyaltyPoints = 0,
                    DateOfBirth = DateTime.Now.AddYears(-20)
                };
                _context.Add(customer);
                await _context.SaveChangesAsync();
            }
            return View(customer);
        }

        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> EditOwn(int? id)
        {
            if (id == null) return NotFound();
            var email = User.Identity?.Name;
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();
            if (customer.Email != email) return Forbid();
            return View("EditOwn", customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> EditOwn(int id, [Bind("Id,Name,Phone,DateOfBirth,Gender")] Customer edited)
        {
            var email = User.Identity?.Name;
            var existing = await _context.Customers.FindAsync(id);
            if (existing == null) return NotFound();
            if (existing.Email != email) return Forbid();

            existing.Name = edited.Name;
            existing.Phone = edited.Phone;
            existing.DateOfBirth = edited.DateOfBirth;
            existing.Gender = edited.Gender;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction(nameof(MyProfile));
        }

        private bool CustomerExists(int id) => _context.Customers.Any(e => e.Id == id);
    }
}