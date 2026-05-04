using GeorgeStore.Extensions;
using GeorgeStore.Features.Brevo;
using GeorgeStore.Features.Users;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDependencies();
builder.Services.AddDBConnection(builder);
builder.Services.AddIdentity();
builder.Services.Configure<JWTConfig>(builder.Configuration.GetSection("JWT"));
builder.Services.Configure<BrevoOptions>(builder.Configuration.GetSection("Brevo"));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddExternalHttpClients(builder.Configuration);
builder.Services.AddCors(opts =>
{
    opts.AddPolicy("Develop", opts =>
    {
        opts.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod();
    });
});
builder.AddJWT();
var app = builder.Build();
app.UseStaticFiles();
app.UseCors("Develop");
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
