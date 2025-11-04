using if_hub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace if_hub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Apenas usuários logados podem fazer upload
    public class UploadController : ControllerBase
    {
        private readonly IFileStorageService _fileStorageService;

        public UploadController(IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Nenhum arquivo enviado.");
            }
            
            var fileUrl = await _fileStorageService.SaveFileAsync(file);

            if (string.IsNullOrEmpty(fileUrl))
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro ao salvar o arquivo.");
            }

            return Ok(new { url = fileUrl, fileName = file.FileName, contentType = file.ContentType });
        }
    }
}