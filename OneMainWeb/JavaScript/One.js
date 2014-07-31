function trace(msg) {
    if (typeof tracing == 'undefined' || !tracing) return;
    try { console.log(msg); } catch (ex) { }
}

function logError(XMLHttpRequest, textStatus, errorThrown) {
    var errorToLog = "textStatus: " + textStatus + " errorThrown: " + errorThrown;
    trace(errorToLog);
    _gaq.push(['_trackEvent', 'JS logError', errorToLog]);
}


if (window.CKEDITOR) {
    (function () {
        var showCompatibilityMsg = function () {
            var env = CKEDITOR.env;

            var html = '<p><strong>Your browser is not compatible with CKEditor.</strong>';

            var browsers =
    {
        gecko: 'Firefox 2.0',
        ie: 'Internet Explorer 6.0',
        opera: 'Opera 9.5',
        webkit: 'Safari 3.0'
    };

            var alsoBrowsers = '';

            for (var key in env) {
                if (browsers[key]) {
                    if (env[key])
                        html += ' CKEditor is compatible with ' + browsers[key] + ' or higher.';
                    else
                        alsoBrowsers += browsers[key] + '+, ';
                }
            }

            alsoBrowsers = alsoBrowsers.replace(/\+,([^,]+), $/, '+ and $1');

            html += ' It is also compatible with ' + alsoBrowsers + '.';

            html += '</p><p>With non compatible browsers, you should still be able to see and edit the contents (HTML) in a plain text field.</p>';

            var alertsEl = document.getElementById('alerts');
            alertsEl && (alertsEl.innerHTML = html);
        };

        var onload = function () {
            // Show a friendly compatibility message as soon as the page is loaded,
            // for those browsers that are not compatible with CKEditor.
            if (!CKEDITOR.env.isCompatible)
                showCompatibilityMsg();
        };

        // Register the onload listener.
        if (window.addEventListener)
            window.addEventListener('load', onload, false);
        else if (window.attachEvent)
            window.attachEvent('onload', onload);
    })();
}


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


function toggle_visibility(id) {
    var e = document.getElementById(id);
    if (e.style.display == 'block')
        e.style.display = 'none';
    else
        e.style.display = 'block';

    return false;
}

function SelectAllCheckboxes(parentCheckBox) {
    var children = parentCheckBox.children;
    var theBox = (parentCheckBox.type == "checkbox") ? parentCheckBox : parentCheckBox.children.item[0];
    var checkboxes = theBox.form.elements;
    for (i = 0; i < checkboxes.length; i++) {
        if (checkboxes[i].type == "checkbox" && checkboxes[i].id != "gvProducts_ctl01_chkAll") {
            if (checkboxes[i].checked) {
                checkboxes[i].checked = false;
            }
            else {
                checkboxes[i].checked = true;
            }
        }
    }
    theBox.checked = !theBox.checked;
}

$('details').details();

$('.scaffold-edit-button a').addClass("btn btn-info btn-xs");

$('#audit-history').on('show.bs.modal', function (e) {
    //var selectedItemId =  $('#audit-history').data('selected-item-id');
    var selectedItemId = $(this).data('content-id');
    if (selectedItemId > 0) {
        $.ajax({
            url: "/AdminService/GetContentHistory?contentId=" + selectedItemId,
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            type: "GET",
            success: function (data) {
                trace("GetContentHistory success");
                $('#audit-history-table tbody').empty();
                $.map(data, function (item) {
                    $('#audit-history-table tbody').append('<tr><td>' + item.DisplayLastChanged + '</td><td>' + item.Title + '</td></tr>');
                    trace(item);
                });
            },
            error: logError
        });
    }


});