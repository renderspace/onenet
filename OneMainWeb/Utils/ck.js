 CKEDITOR.stylesSet.add('one_default_styles',
[
	{name:'Veliko',element:'big'},
	{name:'Malo',element:'small'},
	{name:'Nadpisano',element:'sup'},
	{name:'Podpisano',element:'sub'},
	{name:'Neoblikovana tabela',element:'table',attributes:{'class':'no_style'}},
	{name:'Slika levo',element:'img',attributes:{'class':'left_pict'}},
	{name:'Slika desno',element:'img',attributes:{'class':'right_pict'}},
	{name:'Preklici oblivanje',element:'div',attributes:{'class':'clear'}}
]);

CKEDITOR.addTemplates('one_default_templates',
{
imagesPath:'/Res/',
templates:[
	{title:'Slika in tekst',image:'template1.gif',description:'Slika levo, s tekstom ki obliva sliko',html:'<p><img class="left_pict" src="/Res/template1.gif" />Tukaj vnesi besedilo</p>'},
	{title:'Slika in tekst',image:'template2.gif',description:'Slika desno, s tekstom ki obliva sliko',html:'<p><img class="right_pict" src="/Res/template2.gif" />Tukaj vnesi besedilo</p>'},
]});
 
 
 CKEDITOR.editorConfig = function(config) {
			 config.entities_greek = false;
	        config.forcePasteAsPlainText = true;
	        config.entities = false; 
	        config.entities_latin = false;
	        config.toolbar = [

    ['Maximize','ShowBlocks','About','-','Cut','Copy','Paste','-','Bold','Italic','NumberedList','BulletedList','-','Link','Unlink','Anchor','-','Image','Table','HorizontalRule'],
	['Templates','CreateDiv','Blockquote','-','JustifyLeft','JustifyCenter','JustifyRight','JustifyBlock','Styles','Format','RemoveFormat'],
];
/*
	    config.filebrowserBrowseUrl = '/adm/LinkBrowser.aspx';
	    config.filebrowserWindowWidth = '830';
	    config.filebrowserWindowHeight = '600';
    
	    config.filebrowserImageBrowseLinkUrl = '/adm/ImageBrowser.aspx';
	    config.filebrowserImageWindowWidth = '830';
	    config.filebrowserImageWindowHeight = '600';

	    config.filebrowserFlashBrowseUrl = '/adm/FlashBrowser.aspx';
	    config.filebrowserFlashWindowWidth = '830';
	    config.filebrowserFlashWindowHeight = '600'; 
*/		
		config.filebrowserBrowseUrl = '/ckfinder/ckfinder.html';
	    config.filebrowserWindowWidth = '830';
	    config.filebrowserWindowHeight = '600';
    
	    config.filebrowserImageBrowseLinkUrl = '/ckfinder/ckfinder.html?type=Images';
	    config.filebrowserImageWindowWidth = '830';
	    config.filebrowserImageWindowHeight = '600';

	    config.filebrowserFlashBrowseUrl = '/ckfinder/ckfinder.html?type=Flash';
	    config.filebrowserFlashWindowWidth = '830';
	    config.filebrowserFlashWindowHeight = '600'; 
		/*
		config.filebrowserUploadUrl = '/ckfinder/core/connector/php/connector.php?command=QuickUpload&type=Files';
		config.filebrowserImageUploadUrl = '/ckfinder/core/connector/php/connector.php?command=QuickUpload&type=Images';
		config.filebrowserFlashUploadUrl = '/ckfinder/core/connector/php/connector.php?command=QuickUpload&type=Flash';
		*/

		config.disableObjectResizing = true;
		config.resize_enabled = false;
		
		config.customConfig = '/_js/custom_ckeditor.js';
		config.stylesCombo_stylesSet = 'one_default_styles';
		config.templates = 'one_default_templates';
		config.contentsCss = '/Utils/default_editor.css';
		
		config.disableObjectResizing = true;
		config.resize_enabled = false;
	}; 
	
	


