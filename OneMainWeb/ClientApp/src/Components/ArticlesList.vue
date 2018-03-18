<template>
  <div>
    <div class="adminSection">
      <div class="col-md-2">
        <a><span class="glyphicon glyphicon-plus"></span> Add</a>
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
          <tr v-bind:key="a.id" v-for="a in articles">
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
        <li class="active"><a>1</a></li>
        <li><a href="javascript:__doPostBack('ctl00$MainContent$TwoPostbackPager1','2')">2</a></li>
        <li><a href="javascript:__doPostBack('ctl00$MainContent$TwoPostbackPager1','3')">3</a></li>
        <li><a href="javascript:__doPostBack('ctl00$MainContent$TwoPostbackPager1','4')">4</a></li>
        <li><a href="javascript:__doPostBack('ctl00$MainContent$TwoPostbackPager1','5')">5</a></li>
        <li><a href="javascript:__doPostBack('ctl00$MainContent$TwoPostbackPager1','6')">6</a></li>
        <li class="disabled"><a>...</a></li>
        <li><a href="javascript:__doPostBack('ctl00$MainContent$TwoPostbackPager1','105')">{{ lastPage }}</a></li>
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
      lastPage: null
    }
  },
  mounted() {
    this.loadArticles()
  },
  methods: {
    loadArticles() {
      this.$axios.get(`/AdminService/articles?languageId=1060`)
      .then(response => {
        console.log(response)
        let allRecords = response.headers["x-onenet-allrecords"]
        let recordsPerPage = response.headers["x-onenet-recordsperpage"]
        this.lastPage = Math.floor(allRecords / recordsPerPage)
        this.articles = response.data.map( a => {
          a.DisplayDate = new Date(parseInt(a.DisplayDate.substr(6)))
          return a
        })
      })
      .catch(e => { console.log(e) })
    }
  }
}

</script>
