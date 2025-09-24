<template>
  <div class="form-details">
    <h2>Detalhes do Formulário: {{ form.Name }}</h2>
    <table>
      <thead>
        <tr>
          <th>Nome do Controle</th>
          <th>Tipo</th>
          <th>Valor</th>
          <th>Incluir</th>
          <th v-if="isAnyTextBox">MultiLine</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="(control, index) in form.Controls" :key="control.Id">
          <td>{{ control.Name }}</td>
          <td>{{ control.ControlType }}</td>
          <td>
            <input 
              type="text" 
              v-model="control.customValue" 
              :disabled="!isEditable(control)"
            />
          </td>
          <td>
            <input 
              type="checkbox" 
              v-model="control.IncludeInGeneration"
            />
          </td>
          <td v-if="control.ControlType === 'TextBox'">
            <input 
              type="checkbox" 
              v-model="control.MultiLine"
            />
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script>
export default {
  name: 'FormDetails',
  props: {
    form: {
      type: Object,
      required: true
    }
  },
  computed: {
    isAnyTextBox() {
      return this.form.Controls.some(c => c.ControlType === 'TextBox');
    }
  },
  methods: {
    isEditable(control) {
      // Lógica para definir se o controle pode ter seu valor editado
      return true;
    }
  },
  created() {
    // Inicializa propriedade customValue para cada controle, se não existir
    this.form.Controls.forEach(control => {
      if (typeof control.customValue === 'undefined') {
        control.customValue = '';
      }
    });
  }
};
</script>

<style src="./FormDetails.css" scoped></style>
