/*
Copyright (c) 2003-2010, CKSource - Frederico Knabben. All rights reserved.
For licensing, see LICENSE.html or http://ckeditor.com/license
*/


CKEDITOR.addStylesSet( 'custom_ckeditor', [
	{ name : 'Blue Title Custom', element : 'h3', styles : { 'color' : 'Blue' } },
	{ name : 'Red Title Custom', element : 'h3', styles : { 'color' : 'Red' } },
	{ name:'Image on Left', element:'img', attributes:{class:'left_pict'}},
	{ name:'Image on Right', element:'img', attributes:{class:'right_pict'}}
]);

CKEDITOR.addTemplates( 'one_custom_templates',
{
	// The name of sub folder which hold the shortcut preview images of the templates.
	imagesPath : CKEDITOR.getUrl( CKEDITOR.plugins.getPath( 'templates' ) + 'templates/images/' ),

	// The templates definitions.
	templates :
		[
			{
				title: 'My Template 1',
				image: 'template1.gif',
				description: 'Description of my template 1.',
				html:
					'<h2>Template 1</h2>' +
					'<p><img src="/logo.png" style="float:left" />Type the text here.</p>'
			},
			{
				title: 'My Template 2',
				html:
					'<h3>Template 2</h3>' +
					'<p>Type the text here.</p>'
			}
		]
});



CKEDITOR.editorConfig = function( config )
{
	 // Define changes to default configuration here. For example:
	 //config.language = 'fr';
	 config.uiColor = '#0000FF';
	 config.stylesCombo_stylesSet = 'custom_ckeditor';
	 config.templates = 'one_custom_templates';
	 config.toolbar = [

    ['Cut','Copy','Paste','-','Undo','Redo','-','PasteText','PasteFromWord'],
    ['-','Find','Replace'],

    ['Bold','Italic','Underline','-','Subscript','Superscript'],
    ['NumberedList','BulletedList','-','Outdent','Indent','Blockquote'],

    ['Link','Unlink','Anchor'],
    ['Image','Flash','Table','HorizontalRule','SpecialChar','PageBreak','-','CreateDiv'],
	'/',
    ['Styles','Format'],

    ['Maximize', 'ShowBlocks','-','About','Templates']
];
};










