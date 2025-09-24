<template>
  <div class="form-list">
    <h2>Lista de Formulários</h2>
    <ul>
      <li 
        v-for="form in forms" 
        :key="form.Id"
        @click="selectForm(form)"
      >
        {{ form.Name }} - {{ form.FormType }}
      </li>
    </ul>
  </div>
</template>

<script>
export default {
  name: 'FormList',
  props: {
    forms: {
      type: Array,
      required: true
    }
  },
  methods: {
    selectForm(form) {
      this.$emit('formSelected', form);
    }
  }
};
</script>

<style src="./FormList.css" scoped></style>
