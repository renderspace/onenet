
<template>
  <div class="adminSection form-horizontal validationGroup">
    <div class="form-group">

    </div>
    <div class="row">
      <div class="col-sm-3">
        <div class="lastChange">6.6.6666 66:66:66, xxxxx</div>
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
      articleId: null
    }
  },
  methods: {
    cancel() {
      this.$emit('cancel')
    }
  }
}
</script>
