
function swapIt(name_open, name_close) 
{ 
    document.getElementById(name_open).style.display = 'block';
    document.getElementById(name_close).style.display = 'none'; 
}

function GetUrlParam( paramName )
{
	var oRegex = new RegExp( '[\?&]' + paramName + '=([^&]+)', 'i' ) ;
	var oMatch = oRegex.exec( window.top.location.search ) ;
 
	if ( oMatch && oMatch.length > 1 )
		return decodeURIComponent( oMatch[1] ) ;
	else
		return '' ;
}

function OpenFile(fileUrl) {
	funcNum = GetUrlParam('CKEditorFuncNum');
	if (window.top.opener.SetUrl)
		window.top.opener.SetUrl(fileUrl);
    window.top.opener.CKEDITOR.tools.callFunction(funcNum, fileUrl);
    window.top.close();
}

