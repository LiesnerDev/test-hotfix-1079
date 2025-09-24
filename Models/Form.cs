using System.Collections.Generic;

namespace MyApp.Models
{
    public class Form
    {
        public int Id { get; set; }
        public string Name { get; set; }
        // Ex: VBForm
        public string FormType { get; set; }
        
        // Navigation property
        public virtual ICollection<Control> Controls { get; set; } = new List<Control>();
    }
}
