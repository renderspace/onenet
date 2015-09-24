

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
});


	/*
$('.magnificgallery  img').magnificPopup({
  type: 'image',
  gallery:{
    enabled:true
  }
});*/