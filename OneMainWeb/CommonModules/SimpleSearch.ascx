<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SimpleSearch.ascx.cs" Inherits="OneMainWeb.CommonModules.SimpleSearch" %>
<%@ Import Namespace="System.Threading" %>

<div class="searchForm">
	<div class="input searchInput">
		<input name="searchText" id="searchText" type="text" class="searchText" />
		<a id="searchButton" class="searchButton"><%=Translate("do_search") %></a>
	</div>
</div>

<div class="content-search">
    <div class="loading" style="display: none;">...</div>
</div>

<script type="text/javascript">
    var noTitleLabel = "<%=Translate("no_page_title") %>";
    var languageId = <%=Thread.CurrentThread.CurrentCulture.LCID%>;

    document.addEventListener('DOMContentLoaded', function () {
        $(function () {
            if (getUrlParameter("q")) {
                var keyword = getUrlParameter("q");
                searchPageContentDelegate(keyword);
            }

            $("#searchText").on("keyup", function (event) {
                event.preventDefault();
                if (event.keyCode == 13) {
                    $(this).click();
                }
            });

            $('#searchButton').on('click', function (e) {
                var keyword = $('#searchText').val();
                _redirectSearch(keyword);
                event.preventDefault();
            });

            function _redirectSearch(keyword) {
                if (keyword != '')
                    window.location.href = '<%=SearchResultsUri%>' + "?q=" + keyword;
                return false;
            }

            function getUrlParameter(name) {
                name = name.replace(/[\[]/, '\\[').replace(/[\]]/, '\\]');
                var regex = new RegExp('[\\?&]' + name + '=([^&#]*)');
                var results = regex.exec(location.search);
                return results === null ? '' : decodeURIComponent(results[1].replace(/\+/g, ' '));
            };

            function searchPageContentDelegate(keyword) {

                $('.content-search .loading').show();
                $('.pageContentSearchResults').remove();
                var html = '<ul class="pageContentSearchResults"></ul>';
                $('.content-search').append(html);
                $pageContentSearchResults = $('.pageContentSearchResults');

                $.ajax({
                    url: "/AdminService/SearchPageContent?keyword=" + keyword + "&languageId=" + languageId,
                    contentType: 'application/json; charset=utf-8',
                    dataType: 'json',
                    type: "GET",
                    success: function (data) {
                        $('.content-search .loading').hide();
                        console.log("SearchPageContent success");
                        $.each(data, function (index, item) {
                            $pageContentSearchResults.append('<li><a href="' + item.Url + '">' + (item.Title.length > 0 ? item.Title : noTitleLabel) + '</a></li>');
                        });
                    },
                    error: function (err) {
                        $('.content-search .loading').hide();
                        handleAjaxError(err)
                    }
                });
            }
        }(jQuery));
    });
</script>