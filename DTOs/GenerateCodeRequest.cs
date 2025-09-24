using System.Collections.Generic;

namespace MyApp.DTOs
{
    public class GenerateCodeRequest
    {
        public int FormId { get; set; }

        // Options para influenciar a geração de código
        public bool UseTtk { get; set; }
        public bool RelPos { get; set; }
        public bool I18n { get; set; }
        public bool V2andV3Code { get; set; }
        public bool UnicodePrefix { get; set; }

        // Lista de controles com suas propriedades atualizadas, caso haja alterações via UI
        public List<ControlDto> Controls { get; set; } = new List<ControlDto>();
    }
}
