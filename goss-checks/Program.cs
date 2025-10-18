using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.OpenSsl;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.MapGet("/login", () => { return Results.Text("cheryaev", "text/plain"); });

app.MapPost("/decypher", async (HttpRequest req) =>
{
    var form = await req.ReadFormAsync();
    var keyFile = form.Files["key"];
    var secretFile = form.Files["secret"];

    AsymmetricKeyParameter privateKey;
    using (var ks = keyFile.OpenReadStream())
    using (var sr = new StreamReader(ks, Encoding.UTF8, true))
    {
        var pem = new PemReader(sr);
        var obj = pem.ReadObject();

        if (obj is AsymmetricCipherKeyPair pair)
            privateKey = pair.Private;
        else if (obj is AsymmetricKeyParameter akp)
            privateKey = akp;
        else
            return Results.BadRequest("Unsupported key format");
    }

    byte[] cipherBytes;
    using (var ss = secretFile.OpenReadStream())
    using (var ms = new MemoryStream())
    {
        await ss.CopyToAsync(ms);
        cipherBytes = ms.ToArray();
    }

    byte[] plain;
    try
    {
        var oaep = new OaepEncoding(new RsaEngine(), new Sha1Digest());
        oaep.Init(false, privateKey);
        plain = oaep.ProcessBlock(cipherBytes, 0, cipherBytes.Length);
    }
    catch
    {
        try
        {
            var pkcs1 = new Pkcs1Encoding(new RsaEngine());
            pkcs1.Init(false, privateKey);
            plain = pkcs1.ProcessBlock(cipherBytes, 0, cipherBytes.Length);
        }
        catch (Exception e)
        {
            return Results.BadRequest($"Decryption failed: {e.Message}");
        }
    }

    var result = Encoding.UTF8.GetString(plain).Trim('\r', '\n');
    return Results.Text(result, "text/plain");
});

app.Run();