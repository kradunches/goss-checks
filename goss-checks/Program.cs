using System.Text.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;

const string LOGIN = "d5e5c122-0957-4501-971a-e81248c8522c";

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

app.UseCors(AppAllowSpecificOrigins);

app.Use(async (HttpContext context, RequestDelegate next) =>
{
    context.Response.Headers["X-Author"] = LOGIN;
    context.Response.Headers["Access-Control-Allow-Origin"] = "*";
    // context.Response.Headers["Content-Type"] = "text/plain; charset=UTF-8";

    context.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, max-age=0";
    context.Response.Headers["Pragma"] = "no-cache";
    context.Response.Headers["Expires"] = "0";
    await next(context);
});

app.MapGet("/login", () => { return LOGIN; });

app.MapPost("/size2json", async (HttpContext context) =>
{
    IFormCollection formData = await context.Request.ReadFormAsync();
    var pngFile = formData.Files["image"];

    try
    {
        using var stream = pngFile.OpenReadStream();
        using var image = await Image.LoadAsync(stream);

        var result = new { width = image.Width, height = image.Height };

        return Results.Json(result);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

// app.MapPost("/insert", async (HttpContext context) =>
// {
//     IFormCollection form = await context.Request.ReadFormAsync();
//     var login = form["login"].ToString();
//     var password = form["password"].ToString();
//     var url = form["URL"].ToString();
//
//     var client = new MongoClient(url);
//     var database = client.GetDatabase(("readusers"));
//     var collection = database.GetCollection<BsonDocument>("users");
//
//     var doc = new BsonDocument
//     {
//         { "login", login },
//         { "password", password }
//     };
//
//     await collection.InsertOneAsync(doc);
//
//     return Results.StatusCode(200);
// });

app.MapFallback(() => LOGIN);

app.Run();