<template>
  <div class="code-output">
    <h2>Opções de Geração de Código</h2>
    <div class="options">
      <label>
        <input type="checkbox" v-model="options.UseTtk" /> Use TTK
      </label>
      <label>
        <input type="checkbox" v-model="options.RelPos" /> Posicionamento Relativo
      </label>
      <label>
        <input type="checkbox" v-model="options.I18n" /> I18n
      </label>
      <label>
        <input type="checkbox" v-model="options.V2andV3Code" /> Suporta Python 2 e 3
      </label>
      <label>
        <input type="checkbox" v-model="options.UnicodePrefix" /> Prefixo Unicode
      </label>
    </div>
    <button @click="generateCode">Generate Code</button>
    <div class="output-section" v-if="generatedCode">
      <h3>Resultado do Código Gerado</h3>
      <textarea readonly rows="15">{{ generatedCode }}</textarea>
      <div class="actions">
        <button @click="copyCode">Copy to Clipboard</button>
        <button @click="saveCode">Save Code</button>
        <button @click="previewCode">Preview</button>
      </div>
    </div>
  </div>
</template>

<script>
import FormService from '../services/FormService';

export default {
  name: 'CodeOutput',
  props: {
    form: {
      type: Object,
      required: true
    }
  },
  data() {
    return {
      options: {
        UseTtk: false,
        RelPos: false,
        I18n: false,
        V2andV3Code: false,
        UnicodePrefix: false
      },
      generatedCode: null
    };
  },
  methods: {
    async generateCode() {
      // Monta o payload para a requisição
      const request = {
        FormId: this.form.Id,
        UseTtk: this.options.UseTtk,
        RelPos: this.options.RelPos,
        I18n: this.options.I18n,
        V2andV3Code: this.options.V2andV3Code,
        UnicodePrefix: this.options.UnicodePrefix,
        Controls: this.form.Controls
      };
      try {
        const response = await FormService.generateCode(request);
        if (response.data.code) {
          this.generatedCode = response.data.code;
        } else if (response.data.message) {
          this.generatedCode = response.data.message;
        }
      } catch (error) {
        alert('Erro ao gerar código.');
      }
    },
    copyCode() {
      navigator.clipboard.writeText(this.generatedCode)
        .then(() => {
          alert('Código copiado para a área de transferência.');
        })
        .catch(() => {
          alert('Erro ao copiar código.');
        });
    },
    saveCode() {
      // Simulação de salvamento, em produção poderia acionar download ou API
      alert('Código salvo em arquivo.');
    },
    async previewCode() {
      const request = {
        FormId: this.form.Id,
        UseTtk: this.options.UseTtk,
        RelPos: this.options.RelPos,
        I18n: this.options.I18n,
        V2andV3Code: this.options.V2andV3Code,
        UnicodePrefix: this.options.UnicodePrefix,
        Controls: this.form.Controls
      };
      try {
        const response = await FormService.previewCode(request);
        alert(response.data.message);
      } catch (error) {
        alert('Erro ao pré-visualizar código.');
      }
    }
  }
};
</script>

<style src="./CodeOutput.css" scoped></style>
