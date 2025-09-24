using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.DTOs;
using MyApp.Models;

namespace MyApp.Services
{
    public class FormService : IFormService
    {
        private readonly ApplicationDbContext _context;

        public FormService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<FormDto>> RefreshFormsAsync(bool includeEmptyForms)
        {
            var query = _context.Forms.Include(f => f.Controls)
                .Where(f => f.FormType == "VBForm");

            if (!includeEmptyForms)
            {
                query = query.Where(f => f.Controls.Any());
            }

            var forms = await query.ToListAsync();
            return forms.Select(f => new FormDto
            {
                Id = f.Id,
                Name = f.Name,
                FormType = f.FormType,
                Controls = f.Controls.Select(c => new ControlDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    ControlType = c.ControlType,
                    IncludeInGeneration = c.IncludeInGeneration,
                    MultiLine = c.MultiLine
                }).ToList()
            });
        }

        public async Task<FormDto> GetFormDetailsAsync(int formId)
        {
            var form = await _context.Forms
                .Include(f => f.Controls)
                .FirstOrDefaultAsync(f => f.Id == formId);
            if (form == null)
            {
                return null;
            }

            return new FormDto
            {
                Id = form.Id,
                Name = form.Name,
                FormType = form.FormType,
                Controls = form.Controls.Select(c => new ControlDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    ControlType = c.ControlType,
                    IncludeInGeneration = c.IncludeInGeneration,
                    MultiLine = c.MultiLine
                }).ToList()
            };
        }

        public async Task<string> GeneratePythonCodeAsync(GenerateCodeRequest request)
        {
            // Obtem o formulário e seus controles enviados via request (apenas os que foram habilitados para geração)
            var form = await _context.Forms.Include(f => f.Controls).FirstOrDefaultAsync(f => f.Id == request.FormId);
            if (form == null)
                throw new Exception("Form not found");

            // Se o usuário atualizou as propriedades dos controles na UI, aplica essas configurações
            // Vamos usar um dicionário para mapear as alterações	
            var updatedControls = request.Controls.ToDictionary(c => c.Id);

            StringBuilder codeBuilder = new StringBuilder();

            // Header
            if (request.V2andV3Code)
            {
                codeBuilder.AppendLine("#!/usr/bin/env python");
            }
            codeBuilder.AppendLine("import tkinter as tk");
            if (request.UseTtk)
            {
                codeBuilder.AppendLine("from tkinter import ttk");
            }
            codeBuilder.AppendLine("");

            // Aplicando gettext se i18n estiver habilitado
            if (request.I18n)
            {
                codeBuilder.AppendLine("import gettext");
                codeBuilder.AppendLine("_ = gettext.gettext");
                codeBuilder.AppendLine("");
            }

            // Função para construir a interface
            codeBuilder.AppendLine("def create_interface():");
            codeBuilder.AppendLine("    root = tk.Tk()");
            codeBuilder.AppendFormat("    root.title('{0}')\n", request.I18n ? "_('" + form.Name + "')" : form.Name);
            codeBuilder.AppendLine("");

            // Gerar os controles
            foreach (var control in form.Controls)
            {
                // Se houver atualização para esse controle, aplica a configuração
                if (updatedControls.ContainsKey(control.Id))
                {
                    var updated = updatedControls[control.Id];
                    if (!updated.IncludeInGeneration)
                        continue; // Pula controles que não serão gerados

                    // Exemplo: para TextBox, aplicar propriedade MultiLine
                    if (control.ControlType == "TextBox")
                    {
                        bool multiLine = updated.MultiLine;
                        if (multiLine)
                        {
                            codeBuilder.AppendLine("    text = tk.Text(root, height=5, width=30)");
                            codeBuilder.AppendLine("    text.pack()");
                        }
                        else
                        {
                            codeBuilder.AppendLine("    entry = tk.Entry(root)");
                            codeBuilder.AppendLine("    entry.pack()");
                        }
                    }
                    else if (control.ControlType == "Button")
                    {
                        codeBuilder.AppendLine("    btn = tk.Button(root, text='" + control.Name + "')");
                        codeBuilder.AppendLine("    btn.pack()");
                    }
                    // Outros tipos de controle podem ser adicionados aqui
                }
            }
            
            codeBuilder.AppendLine("");
            codeBuilder.AppendLine("    return root");
            codeBuilder.AppendLine("");
            codeBuilder.AppendLine("if __name__ == '__main__':");
            codeBuilder.AppendLine("    app = create_interface()");
            codeBuilder.AppendLine("    app.mainloop()");

            string code = codeBuilder.ToString();
            return code;
        }

        public async Task<bool> PreviewPythonCodeAsync(GenerateCodeRequest request)
        {
            // Gera o código
            string code = await GeneratePythonCodeAsync(request);
            // Verifica se há um interpretador Python configurado
            // Suponha que a variável de ambiente PYTHON_PATH configure o interpretador
            string pythonPath = Environment.GetEnvironmentVariable("PYTHON_PATH");
            if (string.IsNullOrEmpty(pythonPath))
            {
                return false;
            }

            // Cria um arquivo temporário
            string tempFile = Path.Combine(Path.GetTempPath(), $"preview_{Guid.NewGuid()}.py");
            await File.WriteAllTextAsync(tempFile, code);

            // Executa o arquivo temporário usando Process
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = pythonPath,
                Arguments = tempFile,
                UseShellExecute = false
            };
            try
            {
                Process process = Process.Start(startInfo);
                return true;
            }
            catch (Exception ex)
            {
                // Registrar ex quando necessário
                return false;
            }
        }
    }
}
