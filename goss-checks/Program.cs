using System.Text;

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
    return "d5e5c122-0957-4501-971a-e81248c8522c";
});

app.MapGet("/sample", () =>
{
    return @"function task(x) {
  return x * this ** 2;
}";
});

app.MapGet("/result4", async (HttpContext context) =>
{
    var xTest = context.Request.Headers["x-test"].ToString();
    var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
    return new Dictionary<string, object>
    {
        ["message"] = "d5e5c122-0957-4501-971a-e81248c8522c",
        ["x-result"] = xTest,
        ["x-body"] = body
    };
});

app.Run();