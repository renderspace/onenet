
<template>
  <div class="adminSection form-horizontal validationGroup">
    <div class="form-group">
        <div class="col-sm-9 col-sm-offset-3 jumbotron jumbo-less-padding">
          <div	class="col-sm-5">
              <label>Choose from possible categories:</label>
              <select size="5"  tabindex="1" class="form-control">
                  <option v-bind:key="r.Id" v-bind:value="r.Id" v-for="r in regulars" >{{ r.Title }}</option>
              </select>
          </div>
          <div	class="col-sm-1">
            <b-button variant="info" @click="assign"><span class="glyphicon glyphicon-arrow-right"></span></b-button>
            <b-button variant="info" @click="remove"><span class="glyphicon glyphicon-arrow-left"></span></b-button>
          </div>
          <div	class="col-sm-6">
            <label>Categories assigned to current article:</label>
            <select size="5"  tabindex="1" class="form-control" v-if="article">
              <option v-bind:key="r.Id" v-bind:value="r.Id" v-for="r in article.Regulars" >{{ r.Title }}</option>
            </select>
          </div>
        </div>
    </div>
    <div class="row">
      <div class="col-sm-3">
        <div class="lastChange" v-if="article">{{ article.DisplayLastChanged }}</div>
      </div>
      <div class="col-sm-9">
        <span>Id: {{ articleId }}</span>
        <b-button @click="cancel">Cancel</b-button>
      </div>
    </div>
  </div>
</template>
<script>

export default {
  name: 'articlesSingle',
  props: [ 'articleId'],
  data () {
    return {
      articleId: null,
      article: null,
      regulars: []
    }
  },
  mounted() {
    this.languages = oneNetLanguages
    this.loadArticle()
    this.loadRegulars()
  },
  methods: {
    loadRegulars() {
      this.$axios.get(`/AdminService/regulars?languageId=${languageId}`)
      .then(response => { this.regulars = response.data })
      .catch(e => { console.log(e) })
    },
    loadArticle() {

      let promises = this.languages.map(id => {
        return this.loadArticleByLang(id)
      });
      Promise.all(promises)
      .then(r => {
        console.log('++++')
        console.log(r)
        console.log('++++')
      } )
      .catch(e => {
        console.log('------------')
        console.log(e)
        console.log('------------')
      })
    },
    loadArticleByLang(langId) {
      console.log(langId)
      return this.$axios.get(`/AdminService/articles/${this.articleId}?languageId=${langId}`)
      /*.then(response => {
        console.log('got one')
        this.article = response.data
        this.article.DisplayDate = new Date(parseInt(response.data.DisplayDate.substr(6)))
      })*/
      .catch(e => { console.log('silent') })
    },
    cancel() {
      this.$emit('cancel')
    }
  }
}
</script>
