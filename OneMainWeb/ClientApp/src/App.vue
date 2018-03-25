<template>
  <div id="app">

      <b-alert variant="danger" dismissible :show="errors.length > 0" @dismissed="clearErrors">
        <ul>
          <li v-for="error in errors" v-html="error"></li>
        </ul>
      </b-alert>

      <b-alert :show="dismissCountDown"
            dismissible
            variant="success"
            @dismiss-count-down="countDownChanged"
            @dismissed="clearSuccess">
            <h3>Success!</h3>
            <ul>
              <li v-for="success in successes" v-html="success"></li>
            </ul>
      </b-alert>

      <articleSingle @cancel="canceled" v-if="articleId" :articleId="articleId" @error="handleError" @success="handleSuccess" />
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
      articleId: 0,
      errors: [],
      successes: [],
      dismissCountDown: 0
    }
  },
  methods: {
    articleSelected(article) {
      this.articleId = article.Id
    },
    canceled() {
      this.articleId = 0
    },
    clearErrors() {
      this.errors = []
    },
    clearSuccess() {
      this.dismissCountDown = 0
    },
    handleError(e) {
      this.$scrollTo("#app")
      this.errors.push(e)
    },
    handleSuccess(e) {
      this.$scrollTo("#app")
      this.successes.push(e)
      this.dismissCountDown = 5
    },
    countDownChanged (dismissCountDown) {
      this.dismissCountDown = dismissCountDown
      if(dismissCountDown === 0) {
        this.successes = []
      }
    }
  }
}
</script>
