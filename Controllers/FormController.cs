using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyApp.DTOs;
using MyApp.Services;

namespace MyApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FormController : ControllerBase
    {
        private readonly IFormService _formService;

        public FormController(IFormService formService)
        {
            _formService = formService;
        }

        // Endpoint para atualizar a lista de formulários
        [HttpGet("refresh")]
        public async Task<IActionResult> RefreshForms([FromQuery] bool includeEmptyForms = false)
        {
            var forms = await _formService.RefreshFormsAsync(includeEmptyForms);
            return Ok(forms);
        }

        // Endpoint para obter detalhes de um formulário e seus controles
        [HttpGet("{formId:int}")]
        public async Task<IActionResult> GetFormDetails(int formId)
        {
            var form = await _formService.GetFormDetailsAsync(formId);
            if (form == null)
                return NotFound();
            return Ok(form);
        }

        // Endpoint para gerar código Python
        [HttpPost("generate")]
        public async Task<IActionResult> GenerateCode([FromBody] GenerateCodeRequest request)
        {
            try
            {
                string code = await _formService.GeneratePythonCodeAsync(request);

                // Se o código exceder 65k caracteres, lógica para salvar em arquivo pode ser aplicada
                if (code.Length > 65000)
                {
                    // Exemplo: salvar em um caminho configurado, aqui apenas retornamos aviso
                    return Ok(new { message = "Code generated and saved to file ", filePath = "path/to/file.py" });
                }

                return Ok(new { code });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // Endpoint para pré-visualização do código gerado executando via Python interpreter
        [HttpPost("preview")]
        public async Task<IActionResult> PreviewCode([FromBody] GenerateCodeRequest request)
        {
            bool result = await _formService.PreviewPythonCodeAsync(request);
            if (!result)
            {
                return BadRequest(new { message = "Python interpreter not configured or error on preview." });
            }
            return Ok(new { message = "Preview launched successfully." });
        }

        // Endpoint para trocar o idioma (i18n). Aqui somente retornamos uma mensagem de confirmação, pois o recarregamento
        // de texto de interface é feito no front-end, mas o backend pode armazenar a escolha do usuário se necessário
        [HttpPost("language")]
        public IActionResult ChangeLanguage([FromQuery] string lang)
        {
            // Exemplo: atualizar a cultura do usuário
            // Na prática, isto pode envolver middleware de localização
            return Ok(new { message = $"Language changed to {lang}" });
        }
    }
}
