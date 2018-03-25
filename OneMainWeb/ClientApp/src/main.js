import Vue from 'vue'
import App from './App.vue'

import axios from 'axios'
import Vuelidate from 'vuelidate'
import BootstrapVue from 'bootstrap-vue'
import VueScrollTo from 'vue-scrollto'

Vue.use(VueScrollTo)
Vue.use(BootstrapVue)
Vue.use(Vuelidate)
Vue.prototype.$axios = axios.create()

new Vue({
  el: '#app',
  render: h => h(App)
})
