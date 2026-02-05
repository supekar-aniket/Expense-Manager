using ExpenseManager.Areas.Identity.Data;
using ExpenseManager.Helper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//  Database connection
var connectionString = builder.Configuration.GetConnectionString("ApplicationDBContextConnection")
    ?? throw new InvalidOperationException("Connection string 'ApplicationDBContextConnection' not found.");

builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseSqlServer(connectionString));

//  Identity with roles
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
        options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDBContext>();

builder.Services.AddControllersWithViews();

// Register EmailSender for IEmailSender
builder.Services.AddTransient<EmailHelper>();
builder.Services.AddTransient<IEmailSender, EmailSender>();

// UrlHelper for sendEmail
builder.Services.AddScoped<UrlHelper>();

// set time for token
builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromMinutes(10); // Token expires in 10 minutes
});


var app = builder.Build();

// Seed roles & default admin
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // Ensure roles exist
    string[] roles = { "Admin", "User" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // Read admin details from appsettings.json
    var adminConfig = builder.Configuration.GetSection("AdminUser");
    string adminEmail = adminConfig["Email"];
    string adminPassword = adminConfig["Password"];
    string firstName = adminConfig["FirstName"];
    string lastName = adminConfig["LastName"];

    // Find existing admin
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        // Create new Admin
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = firstName,
            LastName = lastName,
            EmailConfirmed = true,
            DateAndTime = DateTime.Now
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
    else
    {
        // Update details if changed
        bool updateNeeded = false;

        if (adminUser.FirstName != firstName) { adminUser.FirstName = firstName; updateNeeded = true; }
        if (adminUser.LastName != lastName) { adminUser.LastName = lastName; updateNeeded = true; }

        if (updateNeeded)
        {
            await userManager.UpdateAsync(adminUser);
        }

        // Update password if different
        var passwordValid = await userManager.CheckPasswordAsync(adminUser, adminPassword);
        if (!passwordValid)
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(adminUser);
            await userManager.ResetPasswordAsync(adminUser, token, adminPassword);
        }

        // Ensure Admin role
        if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseStatusCodePagesWithReExecute("/Home/HandleStatusCode", "?code={0}");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

//  Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
