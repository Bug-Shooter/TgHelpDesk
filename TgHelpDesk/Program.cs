using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Radzen;
using Telegram.Bot;
using TgHelpDesk.Controllers;
using TgHelpDesk.Data;
using TgHelpDesk.Models.Core;
using TgHelpDesk.Models.Statics;
using TgHelpDesk.Models.Users;
using TgHelpDesk.Repositories;
using TgHelpDesk.Repositories.Interface;
using TgHelpDesk.Services.Bot;
using TgHelpDesk.Services.Notification;
using TgHelpDesk.Services.Notification.Interfaces;
using TgHelpDesk.Services.TgUsers;

var builder = WebApplication.CreateBuilder(args);

#region DataProtection (ProdOnly)
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"/www/TgHelpDest_DataProtectionKeys")); //TODO: Вынести в JSON
#endregion

#region SQL
// Add services to the container.
builder.Services.AddDbContext<TgHelpDeskDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
#endregion

#region BotConfiguration
// Setup Bot configuration
var botConfigurationSection = builder.Configuration.GetSection(BotConfiguration.Configuration);
builder.Services.Configure<BotConfiguration>(botConfigurationSection);
var botConfiguration = botConfigurationSection.Get<BotConfiguration>();

builder.Services.AddHttpClient("telegram_bot_client")
                .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
                {
                    BotConfiguration? botConfig = sp.GetConfiguration<BotConfiguration>();
                    TelegramBotClientOptions options = new(botConfig.BotToken);
                    return new TelegramBotClient(options, httpClient);
                });

//Add Bot Hosted Sefvice Web Hook Initializer
builder.Services.AddScoped<UpdateHandlers>();
builder.Services.AddHostedService<ConfigureWebhook>();
#endregion   

#region Auth
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = AuthOptions.ISSUER,
            ValidateAudience = true,
            ValidAudience = AuthOptions.AUDIENCE,
            ValidateLifetime = true,
            IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
            ValidateIssuerSigningKey = true
        };
    });
#endregion

#region Cookies
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = "TgHelpDesk.Session";
    options.IdleTimeout = TimeSpan.FromMinutes(5);
    options.Cookie.IsEssential = true;
});
#endregion

#region OtherServices
// Add services to the container.
builder.Services.AddTransient<TgUsersService>();
builder.Services.AddTransient<IRepository<HelpRequest>,HelpRequestRepository>();
builder.Services.AddTransient<IRepository<TelegramUser>,TelegramUserRepository>();
builder.Services.AddScoped<BotMethods>();
builder.Services.AddScoped<INotificationService<HelpRequest>, MainNotificationService>();
builder.Services.AddRadzenComponents();
#endregion

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStaticFiles();
app.UseSession();

app.UseRouting();

app.UseAuthorization();

app.MapBotWebhookRoute<BotController>(route: botConfiguration.Route);
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapBlazorHub();

//app.MapWhen(ctx => ctx.Request.Path.Value.StartsWith("/Panel"), x =>
//{
//    x.MapFallbackToPage("/_Host")
//})

app.MapFallbackToPage("/Panel", "/_Host");

app.Run();
