using GeorgeStore.Extensions;
using GeorgeStore.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDependencies();
builder.Services.AddDBConnection(builder);
builder.Services.AddIdentity();
builder.Services.Configure<JWTConfig>(builder.Configuration.GetSection("JWT"));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(opts =>
{
    opts.AddPolicy("Develop", opts =>
    {
        opts.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod();
    });
});
builder.AddJWT();
var app = builder.Build();
app.UseCors("Develop");
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
