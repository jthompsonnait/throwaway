using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using ProjectWebApp.Components;
using ProjectWebApp.Components.Account;
using ProjectWebApp.Data;
using PurchasingSystem;
using ReceivingSystem;
using SalesReturnsSystem;
using ServicingSystem;



var builder = WebApplication.CreateBuilder(args);

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

//Secrets
builder.Configuration.AddUserSecrets<Program>();
// Main connection string
var connectionStringEBike = builder.Configuration.GetConnectionString("eBikeDB");
// Auth Connection String
var connectionStringAuth = builder.Configuration.GetConnectionString("AuthDB");

// Purchasing
builder.Services.PurchasingDependancies(options =>
    options.UseSqlServer(connectionStringEBike));

//Receiving
builder.Services.ReceivingDependencies(options =>
    options.UseSqlServer(connectionStringEBike));

// SalesReturns
builder.Services.SalesReturnDependencies(options =>
    options.UseSqlServer(connectionStringEBike));

//Servicing
builder.Services.ServicingDependencies(options =>
    options.UseSqlServer(connectionStringEBike));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionStringAuth));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    //Adding the role manager must come before the AddEntityFrameworkStores or this will fail
    //Modified for DMIT2018
    .AddRoles<IdentityRole>()
    .AddRoleManager<RoleManager<IdentityRole>>()
    .AddRoleStore<RoleStore<IdentityRole, ApplicationDbContext>>()
    .AddClaimsPrincipalFactory<eBikeClaimsPrincipalFactory>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
