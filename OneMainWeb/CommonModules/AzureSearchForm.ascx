<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AzureSearchForm.ascx.cs" Inherits="OneMainWeb.CommonModules.AzureSearchForm" %>

<script type="text/javascript">
    function doSearch()
	{
		var str = document.getElementById('searchText').value;
		if (str != '')
			window.location.href = <%=SearchResultsUri%> + "?q=" + str;
		return false;
    }

    document.getElementById("searchText").addEventListener("keyup", function (event) {
        event.preventDefault();
        if (event.keyCode == 13) {
            document.getElementById("searchButton").click();
        }
    });
</script>

<div class="searchForm">	
	<div class="input searchInput">
		<input name="searchText" id="searchText" type="text" class="searchText" onkeypress="return fnTrapKD(event);" />
		<a id="searchButton" class="searchButton" onclick="return doSearch();"><%=Translate("do_search") %></a>
	</div>
</div>