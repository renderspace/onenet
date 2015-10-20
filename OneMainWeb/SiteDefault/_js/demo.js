

$(document).ready(function () {
	$('.magnificgallery').magnificPopup({
		delegate: 'ul li figure a',
		type: 'image',
		gallery: { enabled: true },
		image: {
			titleSrc: function (item) {
				return item.el.parents('li').find('figcaption').html();
			}
		}
	});
	$(".expandCollapseHtml .entry-title").click(function (event) {
			event.preventDefault();
			console.log($(this).parent());
			console.log($(this).parent().parent().find(".entry-content"));
			
			$(this).parent().parent().find(".entry-content").slideToggle("slow", function () {
		});
	});
});


	/*
$('.magnificgallery  img').magnificPopup({
  type: 'image',
  gallery:{
    enabled:true
  }
});*/