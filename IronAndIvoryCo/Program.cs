AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
using IronAndIvoryCo.Data;
using IronAndIvoryCo.Models;
using IronAndIvoryCo.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/Home/AccessDenied";
    options.LoginPath = "/Identity/Account/Login";
});

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        
        context.Database.EnsureCreated();

        // 1. BRANCH
        Branch? mainBranch = null;
        if (!context.Branches.Any())
        {
            try
            {
                mainBranch = new Branch
                {
                    BranchName = "Newcastle CBD",
                    Address = "Scott Street, Newcastle",
                };
                var props = typeof(Branch).GetProperties();
                if (props.Any(p => p.Name == "City")) mainBranch.GetType().GetProperty("City")?.SetValue(mainBranch, "Newcastle");
                if (props.Any(p => p.Name == "PhoneNumber")) mainBranch.GetType().GetProperty("PhoneNumber")?.SetValue(mainBranch, "0343123456");
                if (props.Any(p => p.Name == "Email")) mainBranch.GetType().GetProperty("Email")?.SetValue(mainBranch, "cbd@ironandivory.co.za");
                if (props.Any(p => p.Name == "IsActive")) mainBranch.GetType().GetProperty("IsActive")?.SetValue(mainBranch, true);

                context.Branches.Add(mainBranch);
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine("BRANCH SEED FAILED: " + ex.ToString());
                context.ChangeTracker.Clear();
            }
        }
        mainBranch = context.Branches.FirstOrDefault(b => b.BranchName == "Newcastle CBD") ?? context.Branches.FirstOrDefault();

        // 2. SERVICES
        if (mainBranch != null && !context.Services.Any())
        {
            var branchId = mainBranch.BranchId;
            var allServices = new List<Service>
            {
                new Service { Name = "Low Fade Crop", ServiceCategory = ServiceCategory.Haircut, Price = 180, DurationInMinutes = 30, Description = "Modern textured crop with low skin fade.", BranchId = branchId, ImageUrl = "/images/services/low-fade-crop.jpg" },
                new Service { Name = "High Fade Pompadour", ServiceCategory = ServiceCategory.Haircut, Price = 250, DurationInMinutes = 45, Description = "High skin fade with voluminous pompadour.", BranchId = branchId, ImageUrl = "/images/services/high-fade-pompadour.jpg" },
                new Service { Name = "Buzz Cut Skin Fade", ServiceCategory = ServiceCategory.Haircut, Price = 120, DurationInMinutes = 20, Description = "Clean uniform buzz with skin fade sides.", BranchId = branchId, ImageUrl = "/images/services/buzz-cut.jpg" },
                new Service { Name = "French Crop", ServiceCategory = ServiceCategory.Haircut, Price = 200, DurationInMinutes = 35, Description = "Short fringe with textured top and faded sides.", BranchId = branchId, ImageUrl = "/images/services/french-crop.jpg" },
                new Service { Name = "Slick Back Undercut", ServiceCategory = ServiceCategory.Haircut, Price = 220, DurationInMinutes = 40, Description = "Classic slick back with disconnected undercut.", BranchId = branchId, ImageUrl = "/images/services/slick-back.jpg" },
                new Service { Name = "Curly Top Taper", ServiceCategory = ServiceCategory.Haircut, Price = 200, DurationInMinutes = 35, Description = "Keep natural curls on top with clean taper.", BranchId = branchId, ImageUrl = "/images/services/curly-top-taper.jpg" },
                new Service { Name = "Full Beard Trim & Shape", ServiceCategory = ServiceCategory.Beard, Price = 120, DurationInMinutes = 25, Description = "Full beard trimmed, shaped and lined.", BranchId = branchId, ImageUrl = "/images/services/full-beard-trim.jpg" },
                new Service { Name = "Goatee Sculpt", ServiceCategory = ServiceCategory.Beard, Price = 100, DurationInMinutes = 20, Description = "Clean goatee definition and shaping.", BranchId = branchId, ImageUrl = "/images/services/goatee-sculpt.jpg" },
                new Service { Name = "Stubble Trim", ServiceCategory = ServiceCategory.Beard, Price = 80, DurationInMinutes = 15, Description = "Perfect 3mm stubble maintained evenly.", BranchId = branchId, ImageUrl = "/images/services/stubble-definition.jpg" },
                new Service { Name = "Beard Line Up", ServiceCategory = ServiceCategory.Beard, Price = 70, DurationInMinutes = 15, Description = "Sharp lines on cheeks and neckline.", BranchId = branchId, ImageUrl = "/images/services/beard-lineup.jpg" },
                new Service { Name = "Scalp Detox", ServiceCategory = ServiceCategory.Treatment, Price = 200, DurationInMinutes = 30, Description = "Deep scalp exfoliation and nourishment.", BranchId = branchId, ImageUrl = "/images/services/scalp-detox.jpg" },
                new Service { Name = "Dandruff Treatment", ServiceCategory = ServiceCategory.Treatment, Price = 180, DurationInMinutes = 30, Description = "Anti-dandruff treatment with deep cleanse.", BranchId = branchId, ImageUrl = "/images/services/dandruff-treatment.jpg" },
                new Service { Name = "Hair Growth Treatment", ServiceCategory = ServiceCategory.Treatment, Price = 250, DurationInMinutes = 40, Description = "Stimulate growth with oils and massage.", BranchId = branchId, ImageUrl = "/images/services/hair-growth-treatment.jpg" },
            };
            context.Services.AddRange(allServices);
            context.SaveChanges();
        }

        // 3. ROLES
        string[] roles = { "Admin", "Customer", "Barber", "Receptionist" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // 4. USERS
        async Task SeedUser(string email, string password, string role)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(user, role);
                else
                    Console.WriteLine($"FAILED to create {email}: {string.Join(",", result.Errors.Select(e => e.Description))}");
            }
            else if (!await userManager.IsInRoleAsync(user, role))
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }

        await SeedUser("admin@ironandivory.co.za", "Admin@123", "Admin");
        await SeedUser("barber@ironandivory.co.za", "Barber@123", "Barber");
        await SeedUser("reception@ironandivory.co.za", "Recep@123", "Receptionist");
        await SeedUser("customer@ironandivory.co.za", "Customer@123", "Customer");

        // 5. BARBER + SCHEDULE SEED
        if (mainBranch != null)
        {
            if (!context.Barbers.Any())
            {
                var barberPerson = new Barber
                {
                    Name = "Main Barber",
                    Email = "barber@ironandivory.co.za",
                    Phone = "0343123456",
                    BranchId = mainBranch.BranchId,
                    Role = StaffRole.Barber,
                    Speciality = Speciality.Fade,
                };
                context.Barbers.Add(barberPerson);
                context.SaveChanges();
                Console.WriteLine("BARBER SEEDED");
            }

            if (!context.Schedules.Any())
            {
                var barbers = context.Barbers.ToList();
                foreach (var barber in barbers)
                {
                    var days = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
                    foreach (var dayName in days)
                    {
                        context.Schedules.Add(new Schedule
                        {
                            BarberId = barber.Id,
                            DayOfWeek = dayName,
                            StartTime = new TimeOnly(8, 0),
                            EndTime = dayName == "Saturday" ? new TimeOnly(14, 0) : new TimeOnly(17, 0),
                            IsAvailable = true
                        });
                    }
                }
                context.SaveChanges();
                Console.WriteLine("SCHEDULES SEEDED");
            }
        }

        Console.WriteLine("SEED COMPLETE");
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.ToString());
        throw;
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();
app.Run();
