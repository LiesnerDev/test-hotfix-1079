using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.DTOs;
using MyApp.Models;
using MyApp.Services;
using Xunit;

namespace MyApp.Tests.Services
{
    public class FormServiceTests
    {
        private ApplicationDbContext GetInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            var context = new ApplicationDbContext(options);
            return context;
        }

        [Fact]
        public async Task RefreshFormsAsync_ExcludeEmptyForms_ReturnsOnlyFormsWithControls()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetInMemoryContext(dbName);

            // Form with control
            var formWithControls = new Form
            {
                Id = 1,
                Name = "Form 1",
                FormType = "VBForm",
                Controls = new List<Control>
                {
                    new Control { Id = 1, Name = "TextBox1", ControlType = "TextBox" }
                }
            };

            // Form without controls
            var emptyForm = new Form
            {
                Id = 2,
                Name = "Form 2",
                FormType = "VBForm"
            };

            // Form with different FormType
            var otherForm = new Form
            {
                Id = 3,
                Name = "Form 3",
                FormType = "OtherType",
                Controls = new List<Control>
                {
                    new Control { Id = 3, Name = "Button1", ControlType = "Button" }
                }
            };

            context.Forms.AddRange(formWithControls, emptyForm, otherForm);
            await context.SaveChangesAsync();

            var service = new FormService(context);

            // Act
            var resultExcludeEmpty = await service.RefreshFormsAsync(false);
            var resultIncludeEmpty = await service.RefreshFormsAsync(true);

            // Assert
            // Only formWithControls should be returned when excluding empty forms
            Assert.Single(resultExcludeEmpty);
            Assert.Equal(1, resultExcludeEmpty.First().Id);

            // When including empty forms, both forms with and without controls are returned (but still only VBForm)
            Assert.Equal(2, resultIncludeEmpty.Count());
            Assert.DoesNotContain(resultIncludeEmpty, f => f.Id == otherForm.Id);
        }

        [Fact]
        public async Task GetFormDetailsAsync_FormExists_ReturnsFormDetails()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetInMemoryContext(dbName);

            var form = new Form
            {
                Id = 1,
                Name = "Test Form",
                FormType = "VBForm",
                Controls = new List<Control>
                {
                    new Control { Id = 1, Name = "TextBox1", ControlType = "TextBox" },
                    new Control { Id = 2, Name = "Button1", ControlType = "Button" }
                }
            };

            context.Forms.Add(form);
            await context.SaveChangesAsync();

            var service = new FormService(context);

            // Act
            var result = await service.GetFormDetailsAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(form.Id, result.Id);
            Assert.Equal(form.Name, result.Name);
            Assert.Equal(2, result.Controls.Count);
        }

        [Fact]
        public async Task GetFormDetailsAsync_FormDoesNotExist_ReturnsNull()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetInMemoryContext(dbName);
            var service = new FormService(context);

            // Act
            var result = await service.GetFormDetailsAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GeneratePythonCodeAsync_FormNotFound_ThrowsException()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetInMemoryContext(dbName);
            var service = new FormService(context);

            var request = new GenerateCodeRequest
            {
                FormId = 100,
                Controls = new List<ControlDto>()
            };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () => await service.GeneratePythonCodeAsync(request));
        }

        [Fact]
        public async Task GeneratePythonCodeAsync_TextBoxWithMultiLine_GeneratesCorrectCode()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetInMemoryContext(dbName);

            var form = new Form
            {
                Id = 1,
                Name = "Test Form",
                FormType = "VBForm",
                Controls = new List<Control>
                {
                    new Control { Id = 1, Name = "TextBox1", ControlType = "TextBox", IncludeInGeneration = true }
                }
            };

            context.Forms.Add(form);
            await context.SaveChangesAsync();

            var service = new FormService(context);

            // Update the control to use MultiLine
            var request = new GenerateCodeRequest
            {
                FormId = 1,
                UseTtk = false,
                RelPos = false,
                I18n = false,
                V2andV3Code = true,
                UnicodePrefix = false,
                Controls = new List<ControlDto>
                {
                    new ControlDto { Id = 1, IncludeInGeneration = true, MultiLine = true }
                }
            };

            // Act
            var code = await service.GeneratePythonCodeAsync(request);

            // Assert
            Assert.Contains("#!/usr/bin/env python", code);
            Assert.Contains("import tkinter as tk", code);
            Assert.Contains("def create_interface()", code);
            Assert.Contains("tk.Text(root", code);  // Checking that multi-line Text widget is present
        }

        [Fact]
        public async Task GeneratePythonCodeAsync_ButtonControl_GeneratesButtonCode()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetInMemoryContext(dbName);

            var form = new Form
            {
                Id = 2,
                Name = "Button Form",
                FormType = "VBForm",
                Controls = new List<Control>
                {
                    new Control { Id = 10, Name = "Submit", ControlType = "Button", IncludeInGeneration = true }
                }
            };

            context.Forms.Add(form);
            await context.SaveChangesAsync();
            var service = new FormService(context);

            var request = new GenerateCodeRequest
            {
                FormId = 2,
                Controls = new List<ControlDto>
                {
                    new ControlDto { Id = 10, IncludeInGeneration = true }
                }
            };

            // Act
            var code = await service.GeneratePythonCodeAsync(request);

            // Assert
            Assert.Contains("tk.Button(root, text='Submit')", code);
        }

        [Fact]
        public async Task PreviewPythonCodeAsync_WithoutPythonPath_ReturnsFalse()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetInMemoryContext(dbName);

            // Ensure PYTHON_PATH is not set
            Environment.SetEnvironmentVariable("PYTHON_PATH", null);

            var form = new Form
            {
                Id = 3,
                Name = "Preview Test",
                FormType = "VBForm",
                Controls = new List<Control>
                {
                    new Control { Id = 20, Name = "TextBox1", ControlType = "TextBox", IncludeInGeneration = true }
                }
            };
            context.Forms.Add(form);
            await context.SaveChangesAsync();

            var service = new FormService(context);
            var request = new GenerateCodeRequest
            {
                FormId = 3,
                Controls = new List<ControlDto>
                {
                    new ControlDto { Id = 20, IncludeInGeneration = true }
                }
            };

            // Act
            var result = await service.PreviewPythonCodeAsync(request);

            // Assert
            Assert.False(result);
        }
    }
}
