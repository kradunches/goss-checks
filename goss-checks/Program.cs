using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.OpenSsl;

var builder = WebApplication.CreateBuilder(args);

var AppAllowSpecificOrigins = "_appAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: AppAllowSpecificOrigins,
        policy =>
        {
            // policy.WithOrigins("https://kodaktor.ru/")
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

var app = builder.Build();

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Author"] = "d5e5c122-0957-4501-971a-e81248c8522c";
    context.Response.Headers["Access-Control-Allow-Origin"] = "*";
    context.Response.Headers["Content-Type"] = "text/plain; charset=UTF-8";
    
    context.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, max-age=0";
    context.Response.Headers["Pragma"] = "no-cache";
    context.Response.Headers["Expires"] = "0";
    await next();
});

app.UseCors(AppAllowSpecificOrigins);

app.MapGet("/", () =>
{
    return "d5e5c122-0957-4501-971a-e81248c8522c";
});

app.MapGet("/login", () =>
{
    return "d5e5c122-0957-4501-971a-e81248c8522c";
});

app.MapGet("/sample", () =>
{
    return @"function task(x) {
  return x * this ** 2;
}";
});

app.Run();