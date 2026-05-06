using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using OnlyOfficeDemo.Api.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

var app = builder.Build();

// ⚠️ Ajusta esto a TU host público (el Document Server debe poder llegar a estas URLs).
// En dev, evita localhost si Document Server está en Docker en otra red.
var publicBaseUrl = builder.Configuration["PublicBaseUrl"] ?? "http://localhost:5000";

// URL de tu OnlyOffice Document Server (donde vive /web-apps/apps/api/documents/api.js)
var documentServerUrl = builder.Configuration["OnlyOffice:DocumentServerUrl"] ?? "http://localhost:8080";

// Carpeta de almacenamiento simple (demo)
var storagePath = Path.Combine(app.Environment.ContentRootPath, "Storage");
Directory.CreateDirectory(storagePath);

// 1) Endpoint que sirve el fichero para que OnlyOffice lo descargue (config.document.url)
app.MapGet("/onlyoffice/file/{docId}", ([FromRoute] string docId) =>
{
    // demo: docId ignora y siempre sirve sample.docx
    var filePath = Path.Combine(storagePath, "sample.docx");
    if (!File.Exists(filePath))
        return Results.NotFound("File not found.");

    // docx mime type
    return Results.File(filePath,
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        fileDownloadName: "sample.docx");
});

// 2) Endpoint que devuelve la config al frontend (Angular)
app.MapGet("/onlyoffice/config/{docId}", ([FromRoute] string docId) =>
{
    // Key: en una integración real debe ser única por versión/sesión.
    // Aquí hacemos una key simple con timestamp ticks.
    var key = $"{docId}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";

    var config = new
    {
        document = new
        {
            fileType = "docx",
            key = key,
            title = "sample.docx",
            url = $"{publicBaseUrl}/onlyoffice/file/{docId}"
        },
        documentType = "word",
        editorConfig = new
        {
            callbackUrl = $"{publicBaseUrl}/onlyoffice/callback/{docId}"
        }
    };

    return Results.Ok(config);
});

// 3) Callback: OnlyOffice llama por POST con JSON (status, key, url, changesurl, etc.) [4](https://api.onlyoffice.com/docs/docs-api/usage-api/callback-handler/)[5](https://deepwiki.com/ONLYOFFICE/DocumentServer/7.5-callback-handler)
app.MapPost("/onlyoffice/callback/{docId}", async (
    [FromRoute] string docId,
    [FromBody] OnlyOfficeCallback body,
    IHttpClientFactory httpClientFactory) =>
{
    // La doc oficial describe muchos campos y cuándo aparecen (status) [4](https://api.onlyoffice.com/docs/docs-api/usage-api/callback-handler/)[5](https://deepwiki.com/ONLYOFFICE/DocumentServer/7.5-callback-handler)
    // En la práctica: cuando status indica "guardar", suele venir body.url con el doc actualizado.
    // La documentación indica que ciertos enlaces (como changesurl) aparecen en status 2,3,6,7. [4](https://api.onlyoffice.com/docs/docs-api/usage-api/callback-handler/)

    // Guardamos cuando haya URL de archivo actualizado.
    // (status exactos dependen del flujo; aquí actuamos si viene url)
    if (!string.IsNullOrWhiteSpace(body.Url))
    {
        try
        {
            var client = httpClientFactory.CreateClient();
            using var resp = await client.GetAsync(body.Url);
            resp.EnsureSuccessStatusCode();

            var filePath = Path.Combine(storagePath, "sample.docx");
            await using var fs = File.Create(filePath);
            await resp.Content.CopyToAsync(fs);
        }
        catch
        {
            // Si falla la descarga/guardado, devolvemos error != 0
            return Results.Ok(new { error = 1 });
        }
    }

    // Respuesta de OK: { error: 0 }
    return Results.Ok(new { error = 0 });
});

app.Run();