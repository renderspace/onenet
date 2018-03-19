
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
            <select size="5"  tabindex="1" class="form-control" v-if="hasArticle">
              <option v-bind:key="r.Id" v-bind:value="r.Id" v-for="r in articles[0].Regulars" >{{ r.Title }}</option>
            </select>
          </div>
        </div>
    </div>
    

    <div class="Full" v-if="hasArticle">
      <div class="form-group" >
        <label for="TextBoxDate" class="col-sm-3 control-label">Display date</label>
        <div class="col-sm-9">
          <b-input-group>
            <b-form-input id="TextBoxDate" type="text" placeholder="" v-model="articles[0].DisplayDate"></b-form-input>
              <span class="input-group-addon"><span class="glyphicon glyphicon-hourglass"></span></span>
          </b-input-group>
        </div>
      </div>
      <div class="form-group">
        <label for="TextBoxHumanReadableUrl" class="col-sm-3 control-label">Human readable url</label>
        <div class="col-sm-9">
          <b-form-input v-model="articles[0].HumanReadableUrl" id="TextBoxHumanReadableUrl" class="human-readable-url-input">
        </div>
      </div>
  	  <div class="form-group" v-bind:key="a.LanguageId" v-for="a in articles">
        <label class="col-sm-3 control-label">Title  [{{ a.Language }}]</label>
        <div class="col-sm-9">
          <b-form-input v-model="a.Title" maxlength="4000">
        </div>
      </div>
      <div class="form-group" v-bind:key="a.LanguageId" v-for="a in articles">
        <label class="col-sm-3 control-label">Subtitle  [{{ a.Language }}]</label>
        <div class="col-sm-9">
          <b-form-input v-model="a.SubTitle" maxlength="255">
        </div>
      </div>
      <div class="form-group" v-bind:key="a.LanguageId" v-for="a in articles">
        <label class="col-sm-3 control-label">Teaser  [{{ a.Language }}]</label>
        <div class="col-sm-9">
            <b-form-textarea v-model="a.Teaser" :rows="5"></b-form-textarea>
        </div>
      </div>
      
    </div>

    <div class="form-group">
      
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

import lcid from 'lcid'

export default {
  name: 'articlesSingle',
  props: [ 'articleId'],
  data () {
    return {
      articleId: null,
      article: null,
      regulars: [],
      articles: []
    }
  },
  mounted() {
    this.languages = [1033, 1060] /* fixed for debug.. oneNetLanguages */
    this.loadArticle()
    this.loadRegulars()
  },
  computed: {
    hasArticle() {
      return this.articles.length > 0
    }
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

        this.articles = r.filter(a => { return a !== undefined }).map(response => {
          let article = response.data
          console.log(response)
          article.Language = lcid.from(response.data.LanguageId)
          article.DisplayDate = new Date(parseInt(response.data.DisplayDate.substr(6)))  
          return article
        })
        console.log(this.articles)
        console.log('Got ' + this.articles.length + ' language versions of the article.')
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
      .catch(e => { 
        if (e && e.response && e.response.status === 404) {
          return { data: { Id: this.articleId, LanguageId: langId, DisplayDate: '' }}
        }
      })
    },
    cancel() {
      this.$emit('cancel')
    }
  }
}
</script>
