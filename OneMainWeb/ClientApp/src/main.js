import Vue from 'vue'
import App from './App.vue'

import axios from 'axios'

import BootstrapVue from 'bootstrap-vue'

Vue.use(BootstrapVue)
Vue.prototype.$axios = axios.create()

new Vue({
  el: '#app',
  render: h => h(App)
})
