<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AzureSearchForm.ascx.cs" Inherits="OneMainWeb.CommonModules.AzureSearchForm" %>

<script type="text/javascript">
    function _doSearch()
	{
        var str = document.getElementById('_searchText').value;
		if (str != '')
			window.location.href = '<%=SearchResultsUri%>' + "?q=" + str;
		return false;
    }

    document.getElementById("_searchText").addEventListener("keyup", function (event) {
        event.preventDefault();
        if (event.keyCode == 13) {
            document.getElementById("_searchButton").click();
        }
    });
</script>

<div class="searchForm">	
	<div class="input searchInput">
		<input name="_searchText" id="_searchText" type="text" class="searchText" onkeypress="return fnTrapKD(event);" />
		<a id="_searchButton" class="searchButton" onclick="return _doSearch();"><%=Translate("do_search") %></a>
	</div>
</div>