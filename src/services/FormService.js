import axios from 'axios';

const API_BASE = '/api/form';

export default {
  refreshForms(includeEmptyForms) {
    return axios.get(`${API_BASE}/refresh`, {
      params: {
        includeEmptyForms: includeEmptyForms
      }
    });
  },
  getFormDetails(formId) {
    return axios.get(`${API_BASE}/${formId}`);
  },
  generateCode(request) {
    return axios.post(`${API_BASE}/generate`, request);
  },
  previewCode(request) {
    return axios.post(`${API_BASE}/preview`, request);
  },
  changeLanguage(lang) {
    return axios.post(`${API_BASE}/language`, null, {
      params: {
        lang: lang
      }
    });
  }
};
