<template>
  <div id="app">
      <articlesList />

  </div><!-- app -->
</template>

<script>

import ArticlesList from './Components/ArticlesList.vue'

export default {
  name: 'app',
  components: { ArticlesList },
  data () {
    return {
     // articlesList
    }
  }
}
</script>
