<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SimpleSearch.ascx.cs" Inherits="OneMainWeb.CommonModules.SimpleSearch" %>

<div class="searchForm">	
	<div class="input searchInput">
		<input name="_searchText" id="_searchText" type="text" class="searchText" />
		<a id="_searchButton" class="searchButton" onclick="return _doSearch();"><%=Translate("do_search") %></a>
	</div>
</div>

<script type="text/javascript">
    function _doSearch()
	{
        var str = document.getElementById('_searchText').value;
		if (str != '')
			window.location.href = '<%=SearchResultsUri%>' + "?q=" + str;
		return false;
    }

	st = document.getElementById("_searchText");
	if (st) {
		st.addEventListener("keyup", function (event) {
			event.preventDefault();
			if (event.keyCode == 13) {
				document.getElementById("_searchButton").click();
			}
		});
	}
</script>