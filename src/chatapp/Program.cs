using System.Reflection;
using System.Security.Claims;
using chatapp.Components;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using System.Linq.Dynamic.Core;
using chatapp;
using Microsoft.Identity.Web.UI;
using shared.Services;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging.ApplicationInsights;

var builder = WebApplication.CreateBuilder(args);

// load appsettings dynamically when changes are made
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

// load environment specific appsettings
var env = builder.Environment;
builder.Configuration.AddJsonFile($"appsettings{env.EnvironmentName}.json", optional: true);

// add user secret so that developers do not have to check-in secrets into source control.
builder.Configuration.AddEnvironmentVariables()
    .AddUserSecrets(Assembly.GetExecutingAssembly(), true);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddMicrosoftIdentityConsentHandler(); // add microsoft identity consent handler via Microsoft.Identity.Web

// add open AI service settings into DI service
builder.Services.Configure<AzureOpenAIServiceOptions>(
    builder.Configuration.GetSection(AzureOpenAIServiceOptions.Position));

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<HttpContextAccessor>();

// add event handlers when a user logins
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(options =>
                {
                    builder.Configuration.Bind("AzureAdB2C", options);
                    options.Events = new OpenIdConnectEvents
                    {
                        OnRedirectToIdentityProvider = async ctxt =>
                        { 
                            await Task.Yield();
                        },
                        OnAuthenticationFailed = async ctxt =>
                        {
                            await Task.Yield();
                        },
                        OnSignedOutCallbackRedirect = async ctxt =>
                        {
                            ctxt.HttpContext.Response.Redirect(ctxt.Options.SignedOutRedirectUri);
                            ctxt.HandleResponse();
                            await Task.Yield();
                        },
                        OnTicketReceived = async ctxt =>
                        {
                            if (ctxt.Principal != null)
                            {
                                if (ctxt.Principal.Identity is ClaimsIdentity identity)
                                {
                                    // Dynamic string-based querying through an opensource LINQ provider: https://dynamic-linq.net/overview
                                    var colClaims = await ctxt.Principal.Claims.ToDynamicListAsync();
                                    var IdentityProvider = colClaims.FirstOrDefault(
                                        c => c.Type == "http://schemas.microsoft.com/identity/claims/identityprovider")?.Value;
                                    var Objectidentifier = colClaims.FirstOrDefault(
                                        c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
                                    var EmailAddress = colClaims.FirstOrDefault(
                                        c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;
                                    var FirstName = colClaims.FirstOrDefault(
                                        c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname")?.Value;
                                    var LastName = colClaims.FirstOrDefault(
                                        c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname")?.Value;
                                    var AzureB2CFlow = colClaims.FirstOrDefault(
                                        c => c.Type == "http://schemas.microsoft.com/claims/authnclassreference")?.Value;
                                    var auth_time = colClaims.FirstOrDefault(
                                        c => c.Type == "auth_time")?.Value;
                                    var DisplayName = colClaims.FirstOrDefault(
                                        c => c.Type == "name")?.Value;
                                    var idp_access_token = colClaims.FirstOrDefault(
                                        c => c.Type == "idp_access_token")?.Value;
                                }
                            }
                            await Task.Yield();
                        },
                    };
                });

builder.Services.AddControllersWithViews()
               .AddMicrosoftIdentityUI(); // add microsoft identity UI as part of the blazor app.


builder.Logging.AddFilter<ApplicationInsightsLoggerProvider>("dramapal", LogLevel.Trace);
builder.Services.AddSingleton<AuditLogger>();
builder.Logging.AddApplicationInsights(
        configureTelemetryConfiguration: (config) =>
            config.ConnectionString = builder.Configuration.GetConnectionString("APPLICATIONINSIGHTS_CONNECTION_STRING"),
            configureApplicationInsightsLoggerOptions: (options) => { }
    );

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseAntiforgery(); // order matters, ref at https://learn.microsoft.com/en-us/aspnet/core/blazor/security/?view=aspnetcore-8.0#antiforgery-support

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// var logger = app.Services.GetRequiredService<ILogger<Program>>();
// using (logger.BeginScope("hello scope"))
// {
//     logger.LogError("Test error message");
// }

app.Run();
