using System.Collections.Generic;
using System.Threading.Tasks;
using MyApp.DTOs;

namespace MyApp.Services
{
    public interface IFormService
    {
        /// <summary>
        /// Atualiza e retorna a lista dos formulários do tipo VBForm com ou sem formulários vazios
        /// </summary>
        /// <param name="includeEmptyForms">Se true, inclui formulários sem controles</param>
        /// <returns></returns>
        Task<IEnumerable<FormDto>> RefreshFormsAsync(bool includeEmptyForms);

        /// <summary>
        /// Retorna os detalhes de um formulário e seus controles
        /// </summary>
        Task<FormDto> GetFormDetailsAsync(int formId);

        /// <summary>
        /// Gera código Python para o formulário com base nas propriedades dos controles
        /// </summary>
        Task<string> GeneratePythonCodeAsync(GenerateCodeRequest request);

        /// <summary>
        /// Cria um arquivo Python temporário e o executa, se possível
        /// </summary>
        Task<bool> PreviewPythonCodeAsync(GenerateCodeRequest request);
    }
}
