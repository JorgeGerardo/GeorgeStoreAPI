using EasyData.Services;
using GeorgeStore.Extensions;
using GeorgeStore.Features.Auth;
using GeorgeStore.Infrastructure.Email.Brevo;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddCors(opts =>
{
    opts.AddPolicy("Develop", opts => opts.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());
});
builder.AddJWT();
var app = builder.Build();
app.UseStaticFiles();
app.UseCors("Develop");
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();
app.MapEasyData(opts => opts.UseDbContext<AdminContext>());
app.MapControllers();
app.MapRazorPages();
app.Run();
