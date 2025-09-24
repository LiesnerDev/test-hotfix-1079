using System.Collections.Generic;

namespace MyApp.DTOs
{
    public class FormDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FormType { get; set; }
        public List<ControlDto> Controls { get; set; } = new List<ControlDto>();
    }

    public class ControlDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ControlType { get; set; }
        public bool IncludeInGeneration { get; set; }
        public bool MultiLine { get; set; }
        // Propriedades adicionais podem ser mapeadas aqui
    }
}
