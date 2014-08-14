function trace(msg) {
    if (typeof tracing == 'undefined' || !tracing) return;
    try { console.log(msg); } catch (ex) { }
}

function logError(XMLHttpRequest, textStatus, errorThrown) {
    var errorToLog = "textStatus: " + textStatus + " errorThrown: " + errorThrown;
    trace(errorToLog);
    //    _gaq.push(['_trackEvent', 'JS logError', errorToLog]);
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
    var languageId =  $(this).data('language-id');
    if (selectedItemId > 0) {
        $.ajax({
            url: "/AdminService/GetContentHistory?contentId=" + selectedItemId + "&languageId=" + languageId,
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

function getTree(callback) {
    var selectedFolderId = $('#HiddenSelectedFolderId').val();
    if (selectedFolderId < 1)
        selectedFolderId = 0;
    $.ajax({
        url: "/AdminService/GetFolderTree?selectedId=" + selectedFolderId + "&languageId=" + languageId,
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        type: "GET",
        success: callback,
        error: logError
    });
};

function files_databind(selectedFolderId) {
    $.ajax({
        url: "/AdminService/ListFiles?folderId=" + selectedFolderId + "&languageId=" + languageId,
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        type: "GET",
        success: function (data) {
            trace("ListFiles success");
            $('#files-table tbody').empty();
            $.map(data, function (item) {
                $('#files-table tbody').append('<tr><td><input type="checkbox" name="fileIdToDelete" value="' + item.Id + '"  /></td><td>' +
                    item.Id + '</td><td>' + item.Icon + '</td><td>' + item.Size + 'kB</td><td>' + item.Name +
                    '</td><td><a href="#" data-toggle="modal" data-target="#text-content-modal" data-id="' + item.Id +
                    '"  class="btn btn-info btn-xs"><span class="glyphicon glyphicon-pencil"></span> Edit</a></td></tr>');
            });
            trace(data.length);
            if (data.length == 0) {
                $('#files-table thead').hide();
                $('#ButtonDelete').hide();
            } else {
                $('#files-table thead').show();
                $('#ButtonDelete').show();
            }
        },
        error: logError
    });
};


$('#text-content-modal').on('show.bs.modal', function (e) {
    var button = e.relatedTarget;
    if (button == null) {
        return false;
    }
    $(".modal-body .col-sm-9").hide();
    $(".modal-footer .btn-success").hide();
    $(".modal-body input").val("");
    $(".modal-body textarea").val("");
    var fileId = $(button).data('id');
    $(".j_control_file_id").val(fileId);
    var me = $(this);
    var languageId = $(this).data('language-id');
    trace("languageId:" + languageId);
    $(".j_control_language_id").val(languageId);
    $(".j_control_content_id").val("");
    $(".j_control_language").html($(this).data('language'));
    if (fileId > 0) {
        $.ajax({
            url: "/AdminService/GetFileForEditing?id=" + fileId + "&languageId=" + languageId,
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            type: "GET",
            success: function (data) {
                trace("GetFileForEditing success");
                if (data.ContentId > 0) {
                    $.ajax({
                        url: "/AdminService/GetContent?id=" + data.ContentId + "&languageId=" + languageId,
                        contentType: 'application/json; charset=utf-8',
                        dataType: 'json',
                        type: "GET",
                        success: function (content) {
                            $("#content-title").val(content.Title);
                            $("#content-subtitle").val(content.Subtitle);
                            $("#content-teaser").val(content.Teaser);
                            $(".j_control_content_id").val(data.ContentId);
                            $(".modal-body .col-sm-9").show();
                            $(".modal-footer .btn-success").show();
                        },
                        error: logError
                    });
                } else {
                    $(".modal-body .col-sm-9").show();
                    $(".modal-footer .btn-success").show();
                }
                $(".j_control_file").empty().append(data.Icon);

            },
            error: logError
        });
    }
});


$('#text-content-modal a.btn-success').on('click', function (e) {
    console.log("mijav");
    var content = new Object();
    content['Title'] = $("#content-title").val();
    content['Subtitle'] = $("#content-subtitle").val();
    content['Teaser'] = $("#content-teaser").val();
    content['LanguageId'] = $(".j_control_language_id").val();
    content['ContentId'] = $(".j_control_content_id").val();
    content['FileId'] = $(".j_control_file_id").val();
    $.ajax({
        url: "/AdminService/ChangeContent",
        data: JSON.stringify(content),
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        type: "POST",
        success: function (data) {
            if (data === true) {
                $('#text-content-modal').modal('hide');
            }
            else {
                console.log("data:" + data);
            }
        }
    });
});


var folderTree = $('#tree');
if (folderTree.is("div")) {
    getTree(function (d1) {
        var selectedFolderId = $('#HiddenSelectedFolderId').val();
        folderTree.treeview({ data: JSON.parse(d1) });
        if (selectedFolderId > 0) {
            files_databind(selectedFolderId);
        }
    });

    folderTree.removeClass("treeview");
}

$('#tree').on('nodeSelected', function (event, node) {
    if (node.id > 0) {
        files_databind(node.id);
        trace("nodeSelected:" + node.id);
        $('#HiddenSelectedFolderId').val(node.id);
    } else {
        $('#files-table tbody').empty();
    }
});

$('#CheckboxNewDatabase').on("click", function (event) {
    var $this = $(this);
    if ($this.is(':checked')) {
        $(".new_database_fields").show();
    } else {
        $(".new_database_fields").hide();
    }
});

var CheckboxNewDatabase = $('#CheckboxNewDatabase');
if (CheckboxNewDatabase.length > 0) {
    if (CheckboxNewDatabase.is(':checked')) {
        $(".new_database_fields").show();
    } else {
        $(".new_database_fields").hide();
    }
}


/*

*/