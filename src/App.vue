<template>
  <div class="app-container">
    <header>
      <h1>Geração de Interfaces em Python - VB6 Migration</h1>
      <div class="header-actions">
        <button @click="refreshForms">Refresh Forms</button>
        <label>
          <input type="checkbox" v-model="includeEmptyForms" />
          Incluir formulários vazios
        </label>
        <select v-model="selectedLanguage" @change="changeLanguage">
          <option value="en">English</option>
          <option value="pt">Português</option>
        </select>
      </div>
    </header>
    <main>
      <div class="content">
        <FormList 
          :forms="forms"
          @formSelected="handleFormSelected"
        />
        <div v-if="selectedForm" class="details-container">
          <FormDetails 
            :form="selectedForm"
            @updateForm="updateSelectedForm"
          />
          <CodeOutput 
            :form="selectedForm"
          />
        </div>
      </div>
    </main>
  </div>
</template>

<script>
import FormList from './components/FormList.vue';
import FormDetails from './components/FormDetails.vue';
import CodeOutput from './components/CodeOutput.vue';
import FormService from './services/FormService';

export default {
  name: 'App',
  components: {
    FormList,
    FormDetails,
    CodeOutput
  },
  data() {
    return {
      forms: [],
      selectedForm: null,
      includeEmptyForms: false,
      selectedLanguage: 'en'
    };
  },
  methods: {
    async refreshForms() {
      try {
        const response = await FormService.refreshForms(this.includeEmptyForms);
        this.forms = response.data;
        this.selectedForm = null;
      } catch (error) {
        alert('Erro ao buscar formulários.');
      }
    },
    handleFormSelected(form) {
      this.selectedForm = form;
    },
    updateSelectedForm(updatedForm) {
      this.selectedForm = updatedForm;
    },
    async changeLanguage() {
      try {
        await FormService.changeLanguage(this.selectedLanguage);
        // Lógica para recarregar traduções, se aplicável
        alert(`Idioma alterado para ${this.selectedLanguage}`);
      } catch (error) {
        alert('Erro ao alterar idioma.');
      }
    }
  },
  mounted() {
    this.refreshForms();
  }
};
</script>

<style src="./App.css" scoped></style>
