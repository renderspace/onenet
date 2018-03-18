<template>
  <div id="app">

      <articleSingle @cancel="canceled" v-if="articleId" :articleId="articleId"  />
      <articlesList @select="articleSelected" v-else />
  </div><!-- app -->
</template>

<script>

import ArticlesList from './Components/ArticlesList.vue'
import ArticleSingle from './Components/ArticleSingle.vue'

export default {
  name: 'app',
  components: { ArticlesList, ArticleSingle },
  data () {
    return {
      articleId: 0
    }
  },
  methods: {
    articleSelected(article) {
      this.articleId = article.Id
    },
    canceled() {
      this.articleId = 0
    }
  }
}
</script>
