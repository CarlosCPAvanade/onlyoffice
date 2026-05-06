using Microsoft.AspNetCore.Mvc;
using OnlyOfficeDemo.Api.Models;
using System.Text.Json;

namespace OnlyOfficeDemo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OnlyOfficeController : ControllerBase
{
    private readonly IWebHostEnvironment _env;

    public OnlyOfficeController(IWebHostEnvironment env)
    {
        _env = env;
    }

    // ✅ 1. Servir documento
    [HttpGet("file/{id}")]
    public IActionResult GetFile(string id)
    {
        var filePath = Path.Combine(_env.ContentRootPath, "Storage", "sample.docx");

        if (!System.IO.File.Exists(filePath))
            return NotFound();

        return PhysicalFile(
            filePath,
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "sample.docx"
        );
    }

    // ✅ 2. Config para Angular
    [HttpGet("config/{id}")]
    public IActionResult GetConfig(string id)
    {
        // ⚠️ IMPORTANTE: si usas Docker cambia localhost por:
        // http://host.docker.internal:5000

        var baseUrl = "http://localhost:5000";

        var config = new
        {
            document = new
            {
                fileType = "docx",
                key = $"{id}-{DateTime.Now.Ticks}", // siempre única
                title = "sample.docx",
                url = $"{baseUrl}/api/onlyoffice/file/{id}"
            },
            documentType = "word",
            editorConfig = new
            {
                callbackUrl = $"{baseUrl}/api/onlyoffice/callback/{id}"
            }
        };

        return Ok(config);
    }

    // ✅ 3. Callback (guardar documento)
    [HttpPost("callback/{id}")]
    public async Task<IActionResult> Callback(string id)
    {
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();

        var data = JsonSerializer.Deserialize<OnlyOfficeCallback>(body);

        if (!string.IsNullOrEmpty(data?.url))
        {
            var filePath = Path.Combine(_env.ContentRootPath, "Storage", "sample.docx");

            using var httpClient = new HttpClient();
            var stream = await httpClient.GetStreamAsync(data.url);

            using var fs = new FileStream(filePath, FileMode.Create);
            await stream.CopyToAsync(fs);
        }

        // requerido por OnlyOffice
        return Ok(new { error = 0 });
    }
}