using EasyData.Services;
using GeorgeStore.Extensions;
using GeorgeStore.Features.Auth;
using GeorgeStore.Infrastructure.Email.Brevo;
using Swashbuckle.AspNetCore.SwaggerUI;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsProduction() || builder.Environment.IsStaging())
    builder.Services.AddApplicationInsightsTelemetry();

builder.Services.AddRateLimit();
builder.Services.AddProblemDetails();
builder.Services.AddOutputCache();
builder.Services.AddDependencies();
builder.Services.AddDBConnection(builder);
builder.Services.AddIdentity();

builder.AddKeyVaultIfProduction();
builder.Services.Configure<JWTConfig>(builder.Configuration.GetSection("JWT"));
builder.Services.Configure<BrevoOptions>(builder.Configuration.GetSection("Brevo"));

builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddExternalHttpClients(builder.Configuration);
builder.Services.AddCors(opts => opts.AddPolicy("Develop", opts => opts.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod()));
builder.AddJWT();
var app = builder.Build();
app.UseExceptionHandler();
app.UseStaticFiles();
app.UseCors("Develop");
app.UseSwagger();
app.UseSwaggerUI(opts => opts.DocExpansion(DocExpansion.None));
app.UseOutputCache();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapEasyData(opts => opts.UseDbContext<AdminContext>());
app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapRazorPages();
app.Run();
