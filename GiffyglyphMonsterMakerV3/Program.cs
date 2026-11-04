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

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
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
