using Append.Blazor.Printing;
using Azure.Identity;
using Blazored.LocalStorage;
using Blazored.Toast;
using GiffyglyphMonsterMakerV3.Areas.Identity;
using GiffyglyphMonsterMakerV3.Data;
using GiffyglyphMonsterMakerV3.Utility;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
/*
var clientID = Environment.GetEnvironmentVariable("AZURE_ClientID");
var credentialOptions = new DefaultAzureCredentialOptions()
{
ManagedIdentityClientId = clientID
};
var credential = new DefaultAzureCredential(credentialOptions);*/

//var connectionString = config["ConnectionStrings__Azure"];
//var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Azure");
var connectionString = config.GetConnectionString("Azure") ?? config["ConnectionStrings__Azure"];
// Add services to the container.
builder.Services.AddDbContextFactory<ApplicationDbContext>(item => item.UseSqlServer(connectionString, conf =>
{
    conf.UseHierarchyId();
}));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = !builder.Environment.IsDevelopment())
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddBlazoredLocalStorage(config =>
{
    config.JsonSerializerOptions.RespectNullableAnnotations = true;
});
/* maybe later
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        IConfigurationSection googleAuthNSection =
            config.GetSection("Authentication:Google");
        options.ClientId = googleAuthNSection["ClientId"];
        options.ClientSecret = googleAuthNSection["ClientSecret"];
    })
    .AddTwitter(twitterOptions =>
    {
        twitterOptions.ConsumerKey = config["Authentication:Twitter:ConsumerAPIKey"];
        twitterOptions.ConsumerSecret = config["Authentication:Twitter:ConsumerSecret"];
        twitterOptions.RetrieveUserDetails = true;
    });
*/
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddBlazoredToast();

builder.Services.AddScoped<IPrintingService, PrintingService>();
builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<ApplicationUser>>();
builder.Services.AddScoped<MonsterService>();
builder.Services.AddScoped<FeatureService>();
builder.Services.AddScoped<CreatureTemplateService>();
builder.Services.AddScoped<FolderService>();
builder.Services.AddTransient<IEmailSender, MailService>();


var app = builder.Build();

// In local Development, create the LocalDB schema on first launch and apply the
// additive custom-branch upgrade script. Production continues to use its configured database.
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
    await using var db = await dbFactory.CreateDbContextAsync();
    await db.Database.EnsureCreatedAsync();

    var upgradePath = Path.Combine(app.Environment.ContentRootPath, "Database", "upgrade-free-text-frequency-and-feature-order.sql");
    if (File.Exists(upgradePath))
    {
        var upgradeSql = await File.ReadAllTextAsync(upgradePath);
        var batches = System.Text.RegularExpressions.Regex.Split(
            upgradeSql, @"^\s*GO\s*(?:--.*)?$", System.Text.RegularExpressions.RegexOptions.Multiline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        foreach (var batch in batches.Where(batch => !string.IsNullOrWhiteSpace(batch)))
            await db.Database.ExecuteSqlRawAsync(batch);
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
