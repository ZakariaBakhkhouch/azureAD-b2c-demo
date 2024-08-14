using AzureAD_B2C_Demo.Data;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using System;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = new PathString("/Authentication/Login");
    options.AccessDeniedPath = new PathString("/Account/AccessDenied");
    options.SlidingExpiration = true;
    options.Cookie.Name = "CimarPortailClient.Cookie";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});


builder.Services.AddMvc(option =>
{
    var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    option.Filters.Add(new AuthorizeFilter(policy));
})
    .AddMicrosoftIdentityUI();

builder.Services.AddAuthorization();

// add Microsoft Identity Web App authentication

builder.Services.AddAuthentication()
    .AddMicrosoftIdentityWebApp(options =>
    {
        var azureAD = builder.Configuration.GetSection("AzureAD");

        options.Instance = azureAD["Instance"]!;
        options.TenantId = azureAD["TenantId"];
        options.ClientId = azureAD["ClientId"];
        options.CallbackPath = azureAD["CallbackUrl"];
        options.SignedOutCallbackPath = azureAD["SigoutcallbackUrl"];
        options.SignInScheme = IdentityConstants.ExternalScheme;

    });

// Configur the OpenIdConnect options

builder.Services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.Events = new OpenIdConnectEvents
    {
        OnTokenValidated = async context =>
        {
            //
        },
        OnRedirectToIdentityProviderForSignOut = context =>
        {
            //
            return Task.CompletedTask;
        }
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Authentication}/{action=Login}/{id?}");

app.Run();
