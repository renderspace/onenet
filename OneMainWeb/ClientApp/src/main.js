import Vue from 'vue'
import App from './App.vue'

import axios from 'axios'

Vue.prototype.$axios = axios.create()

new Vue({
  el: '#app',
  render: h => h(App)
})
