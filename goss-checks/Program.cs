using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Security.Cryptography;

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

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Author"] = LOGIN;
    context.Response.Headers["Access-Control-Allow-Origin"] = "*";
    context.Response.Headers["Content-Type"] = "text/plain; charset=UTF-8";
    
    context.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, max-age=0";
    context.Response.Headers["Pragma"] = "no-cache";
    context.Response.Headers["Expires"] = "0";
    await next();
});


app.MapGet("/", () =>
{
    return LOGIN;
});

app.MapGet("/promise", () =>
{
    return @"function task(x) {
    return new Promise((res, rej) => x < 18 ? res('yes') : rej('no'));
}
";
});

app.MapGet("/fetch", () =>
{
    var html = @"<!doctype html>
<html>
<head><meta charset=""utf-8""></head>
<body>
    <input id=""inp"" />
    <button id=""bt"">fetch</button>
    <script>
        document.getElementById('bt').addEventListener('click', async function() {
            var inp = document.getElementById('inp');
            try {
                var res = await fetch(inp.value);
                var text = await res.text();
                inp.value = text;
            } catch (e) {
                inp.value = e.toString();
            }
        });
    </script>
</body>
</html>
";
    return Results.Content(html, "text/html; charset=UTF-8");
});

app.MapGet("/login", () =>
{
    return LOGIN;
});

app.MapGet("/sample", () =>
{
    return @"function task(x) {
  return x * this ** 2;
}";
});

app.Map("/result4", async (HttpContext context) =>
{
    var xTest = context.Request.Headers["x-test"].ToString();
    var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
    return new Dictionary<string, object>
    {
        ["message"] = LOGIN,
        ["x-result"] = xTest,
        ["x-body"] = body
    };
});

app.MapGet("/hour", () =>
{
    return DateTime.Now.Hour;
});

app.MapGet("/code", () =>
{
    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Program.cs");
    var sourceCode = File.ReadAllText(filePath);
    return Results.Content(sourceCode, "text/plain", Encoding.UTF8, 200);
});

app.MapGet("/sha/{input}", (string input) =>
{
    using var sha1 = SHA1.Create();
    var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));

    return BitConverter.ToString(hash).Replace("-", "").ToLower();
});

app.Map("/req", async (HttpContext context) =>
{
    string addr = null;

    if (context.Request.Method is "GET")
        addr = context.Request.Query["addr"];
    else if (context.Request.Method is "POST")
    {
        var form = await context.Request.ReadFormAsync();
        addr = form["addr"];
    }

    try
    {
        using var client = new HttpClient();
        var content = await client.GetStringAsync(addr);

        return Results.Content(content, "text/plain", Encoding.UTF8, 200);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.MapFallback(() => LOGIN);

app.Run();