
<template>
  <div class="adminSection form-horizontal validationGroup">
    <div class="form-group" v-if="articles.length">
        <div class="col-sm-9 col-sm-offset-3 jumbotron jumbo-less-padding">
          <div	class="col-sm-5">
              <label>Choose from possible categories:</label>
              <select size="5" class="form-control" v-model="selectedRegularAssign">
                  <option v-bind:key="r.Id" v-bind:value="r.Id" v-for="r in regulars" >{{ r.Title }}</option>
              </select>
          </div>
          <div	class="col-sm-1">
            <b-button variant="info" @click="assign"><span class="glyphicon glyphicon-arrow-right"></span></b-button>
            <b-button variant="info" @click="remove"><span class="glyphicon glyphicon-arrow-left"></span></b-button>
          </div>
          <div	class="col-sm-6" v-bind:class="{ 'is-invalid': $v.article.Regulars.$invalid }">
            <label>Categories assigned to current article:</label>
            <select size="5" class="form-control" v-if="article" v-model="selectedRegularRemove">
              <option v-bind:key="r.Id" v-bind:value="r.Id" v-for="r in article.Regulars" >{{ r.Title }}</option>
            </select>
          </div>
        </div>
    </div>
    <div class="Full text-center" v-if="!articles.length">
      <spinner>
    </div>
    <div class="Full" v-if="articles.length">
      <div class="form-group" >
        <label for="TextBoxDate" class="col-sm-3 control-label">Display date</label>
        <div class="col-sm-9">
          <b-input-group v-bind:class="{ 'is-invalid': $v.article.DisplayDate.$invalid }">
            <datepicker v-model="article.DisplayDate" language="sl-si" input-class="form-control"></datepicker>
            <span class="input-group-addon"><span class="glyphicon glyphicon-hourglass"></span></span>
          </b-input-group>
        </div>
      </div>
      <div class="form-group">
        <label for="TextBoxHumanReadableUrl" class="col-sm-3 control-label">Human readable url</label>
        <div class="col-sm-9">
            <b-form-input v-model="article.HumanReadableUrl" id="TextBoxHumanReadableUrl" class="human-readable-url-input" :state="!$v.article.HumanReadableUrl.$invalid">
        </div>

      </div>
  	  <div class="form-group" v-bind:key="a.LanguageId" v-for="a in articles" v-bind:class="{ 'is-invalid': !completeArticle }">
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
      <div class="form-group" v-bind:key="a.LanguageId" v-for="a in articles">
        <label class="col-sm-1 control-label">Html  [{{ a.Language }}]</label>
        <div class="col-sm-11">
          <ckeditor v-model="a.Html" :config="config" ></ckeditor>
        </div>
      </div>
    </div>
    <div class="row">
      <div class="col-sm-3">
        <div class="lastChange" v-if="article">{{ article.DisplayLastChanged }}</div>
      </div>
      <div class="col-sm-9">
        <span>Id: </span><span class="articleId" v-if="article">{{ article.Id }}</span>
        <b-button variant="success" @click="save" :disabled="$v.article.$invalid">Save</b-button>
        <b-button variant="success" @click="saveAndClose" :disabled="$v.article.$invalid">Save &amp; Close</b-button>
        <b-button @click="cancel">Cancel</b-button>
      </div>
    </div>
  </div>
</template>
<script>

import { required } from 'vuelidate/lib/validators'
import lcid from 'lcid'
import Ckeditor from 'vue-ckeditor2'
import Datepicker from 'vuejs-datepicker'
import { oneNetCkConfig } from '../ckconfig.js'
import Spinner from './Spinner.vue'

export default {
  name: 'articlesSingle',
  props: [ 'articleId'],
  components: { Ckeditor, Datepicker, Spinner },
  data () {
    return {
      selectedRegularAssign: null,
      selectedRegularRemove: null,
      articleId: null,
      article: null,
      regulars: [],
      articles: [],
      config: oneNetCkConfig
    }
  },
  mounted() {
    this.languages = oneNetLanguages
    this.loadRegulars()
    if (this.articleId === -1) {
      this.newArticle()
    } else {
      this.loadArticle()
    }
  },
  computed: {
    completeArticle: function() {
      var articlesWithTitle = this.articles.filter(a => { return a && a.Title && a.Title.length })
      if (articlesWithTitle && articlesWithTitle.length > 0) {
        return articlesWithTitle[0]
      } else {
        return null
      }
    }
  },
  methods: {
    handleError(e) {
      if (e.response) {
        this.$emit('error', { status: e.response.status, message: e.response.data })
      } else {
        this.$emit('error', 'unhandeled error!')
        console.log(e)
      }
    },
    loadRegulars() {
      this.$axios.get(`/AdminService/regulars?languageId=${languageId}`)
      .then(response => { this.regulars = response.data })
      .catch(e => { this.handleError(e) })
    },
    newArticle() {
      this.article = {
        Id: this.articleId,
        Regulars: []
      }
      this.articles = this.languages.map(langId => {
        return { Id: this.articleId, LanguageId: langId, DisplayDate: '', HasTranslationInCurrentLanguage: false, Language: lcid.from(langId) }
      });
    },
    loadArticle() {
      let promises = this.languages.map(id => {
        return this.loadArticleByLang(id)
      });
      Promise.all(promises)
      .then(r => {
        this.articles = r.map(response => {
          let article = response.data
          article.DisplayDate = new Date(parseInt(response.data.DisplayDate.substr(6)))
          if (article.HasTranslationInCurrentLanguage === true) {
            this.article = article
          }
          return article
        })
      })
      .catch(e => { this.handleError(e) })
    },
    loadArticleByLang(langId) {
      return this.$axios.get(`/AdminService/articles/${this.articleId}?languageId=${langId}`)
      .catch(e => {
        if (e && e.response && e.response.status === 404) {
          return { data: { Id: this.articleId, LanguageId: langId, DisplayDate: '', HasTranslationInCurrentLanguage: false, Language: lcid.from(langId) }}
        } else {
          this.handleError(e)
          return null // to make it blow up upper
        }
      })
    },
    decodeArticleSaveError(e) {
      switch(e) {
        case -1:
          return 'article was null'
        case -2:
          return '403: not authenticted'
        case -3:
          return 'empty article'
        case -4:
          return 'empty title'
        case -5:
          return 'no translation tag'
        case -6:
          return 'no regulars'
        case -7:
          return 'no HumanReadableUrl'
        case -8:
          return 'no Id after save'
        default:
          return e
      }
    },
    prepareArticleForSave(a) {
      let articleForSave = Object.assign({}, a)
      let displayDate = this.article.DisplayDate.getTime()
      articleForSave.Id = this.article.Id
      articleForSave.DisplayDate = `\/Date(${displayDate}-0100)\/`
      articleForSave.HumanReadableUrl = this.article.HumanReadableUrl
      articleForSave.Regulars = this.article.Regulars
      if(!a.Teaser) {
        articleForSave.Teaser = ''
      }
      if(!a.Html) {
        articleForSave.Html = ''
      }
      return articleForSave
    },
    async save(close) {
      if (!this.article || !this.articles.length) {
        console.log('empty article..')
        return
      }
      if (this.article.Id < 1 && !this.completeArticle) {
        this.$emit('error', 'Trying to save a new article, but no title was provided')
        return
      } else if (this.article.Id < 1) {
        await this.$axios.post(`/AdminService/articles`, this.prepareArticleForSave(this.completeArticle))
          .then((r) => {
            if (r && r.data > 0) {
              this.article.Id = r.data
            } else {
              if (r) { console.log('Article save not successful: ' + this.decodeArticleSaveError(r.data)) }
              this.handleError('Save new article not successful')
            }
          })
          .catch(e => { this.handleError(e) })
      }
      if (this.article.Id < 1){
        // still no article ID?
        return
      }
      console.warn('next ' + this.article.Id)
      let axiosPromises = []
      this.articles.forEach(a => {
        axiosPromises.push(
          this.$axios.post(`/AdminService/articles`, this.prepareArticleForSave(a))
          .catch(e => { this.handleError(e) })
        )
      })
      Promise.all(axiosPromises)
      .then(results => {
        let successes = 0
        results.forEach(r => {
          if (r && r.data > 0) {
            successes++
          } else if (r) {
            console.log('Article save not successful: ' + this.decodeArticleSaveError(r.data))
          }
        })
        if (successes > 0) {
          if (close) {
            this.$emit('cancel')
          }
          this.$emit('success', 'Saved <strong>' + successes + '</strong> language variants.')
        } else {
          this.$emit('error', 'Article NOT saved!')
        }
      })
      .catch(e => { this.handleError(e) })
    },
    assign() {
      if (this.selectedRegularAssign && !this.article.Regulars.filter(r => r.Id === this.selectedRegularAssign).length) {
        let regularToAssign = this.regulars.filter(r => r.Id === this.selectedRegularAssign)
        if (regularToAssign.length) {
          this.article.Regulars.push(regularToAssign[0])
        }
      }
    },
    remove() {
      this.article.Regulars = this.article.Regulars.filter(r => r.Id !== this.selectedRegularRemove)
    },
    saveAndClose() {
      this.save(true)
    },
    cancel() {
      this.$emit('cancel')
    }
  },
  validations: {
    article: {
      Regulars: {
        required
      },
      DisplayDate: {
        required
      },
      HumanReadableUrl: {
        required,
        exists: function (hru) {
          return this.$axios.get(`/AdminService/articles/exists/${hru}?excludeArticleId=${this.article.Id}`)
          .then(r => { return r && r.data === false })
          .catch(e => { this.handleError(e) })
        }
      }
    }
  }
}
</script>
<style>
.is-invalid input, .is-invalid .vdp-datepicker input, .is-invalid select, input.is-invalid { border: 2px solid red !important}
</style>
