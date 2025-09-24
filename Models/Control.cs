namespace MyApp.Models
{
    public class Control
    {
        public int Id { get; set; }
        public int FormId { get; set; }
        public string Name { get; set; }
        // Ex: TextBox, Button e etc
        public string ControlType { get; set; }

        // Exemplo: Armazena propriedades específicas do controle (como MultiLine para TextBox, etc.)
        // Para simplificação, usaremos propriedades básicas
        public bool IncludeInGeneration { get; set; } = true;
        public bool MultiLine { get; set; } = false; // somente aplicável em alguns controles

        // Outras propriedades específicas podem ser adicionadas conforme necessário

        // Navigation property
        public virtual Form Form { get; set; }
    }
}
