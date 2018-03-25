<template>
  <div>
    <div class="adminSection">
      <div class="col-md-2">
        <a class="btn btn-success" @click="add"><span class="glyphicon glyphicon-plus"></span> Add</a>
      </div>
    </div>

    <div class="cc-gv">
      <table cellpadding="0" cellspacing="0" summary="" class="table table-hover table-clickable-row">
        <thead>
          <tr>
            <th scope="col"><input type="checkbox" /></th>
            <th scope="col">Id</th>
            <th scope="col">Status</th>
            <th scope="col">Title</th>
            <th scope="col">HumanReadableUrl</th>
            <th scope="col">Display date</th>
            <th scope="col">Categories</th>
            <th scope="col">&nbsp;</th>
          </tr>
        </thead>
        <tbody>
          <tr v-bind:key="a.id" v-for="a in articles" @click="articleSelected(a)">
            <td><input type="checkbox"  /></td>
            <td>{{ a.Id }}</td>
            <td v-html="a.Status"></td>
            <td>{{ a.Title }}</td>
            <td>{{ a.HumanReadableUrl }}</td>
            <td>{{ a.DisplayDate.toLocaleDateString() }}</td>
            <td>{{ a.Categories }}</td>
            <td><a class="btn btn-info btn-xs  " ><span class="glyphicon glyphicon-pencil"></span> Edit</a></td>
          </tr>
        </tbody>
      </table>
    </div>
    <div class="text-center">
      <ul class="pagination">
         <b-pagination size="md" :total-rows="totalRows" v-model="currentPage" :per-page="perPage" v-on:change="loadArticles" hide-ellipsis limit="10"></b-pagination>
      </ul>
    </div>
  </div>
</template>
<script>

export default {
  name: 'articlesList',
  data () {
    return {
      articles: [],
      perPage: 15,
      currentPage: 1,
      totalRows: null,
    }
  },
  mounted() {
    this.loadArticles()
  },
  methods: {
    articleSelected(a) {
      this.$emit('select', a)
    },
    loadArticles(p) {
      if (p) {
        this.currentPage = p
      }
      this.$axios.get(`/AdminService/articles?languageId=${languageId}&page=${this.currentPage}`)
      .then(response => {
        this.totalRows = response.headers["x-onenet-allrecords"]
        this.articles = response.data.map( a => {
          a.DisplayDate = new Date(parseInt(a.DisplayDate.substr(6)))
          return a
        })
      })
      .catch(e => { console.log(e) })
    },
    add() {
      this.$emit('select', { Id: -1 })
    }
  }
}

</script>
