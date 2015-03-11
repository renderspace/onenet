function trace(msg, style) {
    if (typeof (tracing) === 'undefined' || (tracing !== true)) {
        return;
    }
    try {
        if (typeof (style) === 'undefined') {
            console.log(msg);
        } else {
            console.log(msg, style);
        }
    } catch (ex) { }
}

function logError(XMLHttpRequest, textStatus, errorThrown) {
    var errorToLog = "textStatus: " + textStatus + " errorThrown: " + errorThrown;
    trace(errorToLog);
    //    _gaq.push(['_trackEvent', 'JS logError', errorToLog]);
}

$('.ckeditor4').each(function (index) {
    trace("CKEDITOR " + index + ": " + this.id);
    CKEDITOR.replace(this.id, {
        customConfig: '',
        entities_greek: false,
        forcePasteAsPlainText: true,
        entities: false,
        entities_latin: false,
        toolbar: [

    ['Maximize', 'ShowBlocks', 'About', '-', 'Cut', 'Copy', 'Paste', '-', 'Bold', 'Italic', 'NumberedList', 'BulletedList', '-', 'Link', 'Unlink', 'Anchor', '-', 'Image', 'Table', 'HorizontalRule'],
    ['Templates', 'CreateDiv', 'Blockquote', '-', 'JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock', 'Styles', 'Format', 'RemoveFormat', 'Source'],
        ],

        filebrowserBrowseUrl: '/ckfinder/ckfinder.html',
        filebrowserWindowWidth: '830',
        filebrowserWindowHeight: '600',

        filebrowserImageBrowseLinkUrl: '/ckfinder/ckfinder.html?type:Images',
        filebrowserImageWindowWidth: '830',
        filebrowserImageWindowHeight: '600',

        filebrowserFlashBrowseUrl: '/ckfinder/ckfinder.html?type:Flash',
        filebrowserFlashWindowWidth: '830',
        filebrowserFlashWindowHeight: '600',

        disableObjectResizing: true,
        //customConfig : '/_js/custom_ckeditor.js',
        stylesCombo_stylesSet: 'one_default_styles',
        templates: 'one_default_templates',
        // contentsCss : '/Utils/default_editor.css',
		height:500,
        disableObjectResizing: true,
        resize_enabled: false,
        allowedContent: true, //,extraPlugins: 'youtube'
        skin: 'bootstrapck'
    });
});

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
				//trace(item);
                $('#files-table tbody').append('<tr><td><input type="checkbox" name="fileIdToDelete" value="' + item.Id + '"  /></td><td>' +
                    item.Id + '</td><td>' + item.Icon + '</td><td>' + item.Size + 'kB</td><td>' + item.Name +
                    '</td><td><a href="#" data-toggle="modal" data-target="#text-content-modal" data-file-id="' + item.Id +
                    '"  class="btn btn-info btn-xs"><span class="glyphicon glyphicon-pencil"></span> Edit</a></td></tr>');
            });
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

function getContent(contentId, languageId, enableHtml, enableCk) {
    $.ajax({
        url: "/AdminService/GetContent?id=" + contentId + "&languageId=" + languageId,
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        type: "GET",
        success: function (content) {
            trace(content);
            $("#content-title").val(content.Title);
            $("#content-subtitle").val(content.Subtitle);
            $("#content-teaser").val(content.Teaser);
            if (enableHtml === true) {
                $("#content-html").html(content.Html);
                CKEDITOR.instances["content-html"].setData(content.Html);
                $(".ckbox").show();
            }
            else {
                $(".ckbox").hide();
            }
            $(".j_control_content_id").val(contentId);
            $(".modal-body .col-sm-9").show();
            $(".modal-footer .btn-success").show();
        },
        error: logError
    });
}

function buildFormControl(fieldLabel, fieldId, fieldType) {
    var controlHtml = '<div class="form-group"><label class="col-sm-3 control-label">' + fieldLabel + '</label><div class="col-sm-9">';
    controlHtml += '<span class="">';
    if (fieldType == 'html')
        controlHtml += '<textarea  class="form-control ckeditor4" id="' + fieldId + '" name="' + fieldId + '"></textarea>';
    else if (fieldType == 'singleline')
        controlHtml += '<input type="text" maxlength="255" class="form-control" id="' + fieldId + '" name="' + fieldId + '" />';
    else
        controlHtml += '<input type="text" maxlength="255" class="form-control" id="' + fieldId + '" name="' + fieldId + '" />';
    controlHtml += '</span>';
    controlHtml += '</div>';
    controlHtml += '</div>';
    return controlHtml;
}

function getContentTemplate(instanceId, templateId) {

    $(".j_control_content_template_instance_id").val(instanceId);
    $(".j_control_template_id").val(templateId);

    $.ajax({
        url: "/AdminService/GetTemplate?templateId=" + templateId,
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        type: "GET",
        success: function (template) {
            trace(template);

            if (typeof (template) != "undefined" && typeof (template.ContentFields) != "undefined" && template.ContentFields.length > 0) {

                if ($('.content-fields').html().length > 0) {
                    for (name in CKEDITOR.instances) {
                        CKEDITOR.instances[name].destroy()
                    }
                    $('.content-fields').html('');
                }

                $.each(template.ContentFields, function (index, field) {
                    // do your stuff here
                    $('#' + get_field_id(field.Key)).val(field.Value);
                    var controlHtml = buildFormControl(field.Key, get_field_id(field.Key), field.Value);
                    $('.content-fields').append(controlHtml);
                    if (field.Value == 'html') {
                        var editor = CKEDITOR.instances[get_field_id(field.Key)];
                        if (!editor) {
                            CKEDITOR.replace(get_field_id(field.Key));
                        }
                    }                        
                });

            }
        },
        error: logError
    });

    $.ajax({
        url: "/AdminService/GetContentTemplate?instanceId=" + instanceId,
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        type: "GET",
        success: function (contentTemplate) {
            trace(contentTemplate);

            $(".modal-body .col-sm-9").show();

            if (typeof(contentTemplate.DateCreated) != "undefined" && contentTemplate.DateCreated.length > 0) {
                $(".j_control_date_created").parent().parent().show();
                $(".j_control_date_created").html(contentTemplate.DateCreated);
            } else {
                $(".j_control_date_created").parent().parent().hide();
            }
            if (typeof (contentTemplate.DateModified) != "undefined" && contentTemplate.DateModified.length > 0) {
                $(".j_control_date_modified").parent().parent().show();
                $(".j_control_date_modified").html(contentTemplate.DateModified);
            } else {
                $(".j_control_date_modified").parent().parent().hide();
            }
            if (typeof (contentTemplate.PrincipalCreated) != "undefined" && contentTemplate.PrincipalCreated.length > 0) {
                $(".j_control_principal_created").parent().parent().show();
                $(".j_control_principal_created").html(contentTemplate.PrincipalCreated);
            } else {
                $(".j_control_principal_created").parent().parent().hide();
            }
            if (typeof (contentTemplate.PrincipalModified) != "undefined" && contentTemplate.PrincipalModified.length > 0) {
                $(".j_control_principal_modified").parent().parent().show();
                $(".j_control_principal_modified").html(contentTemplate.PrincipalModified);
            } else {
                $(".j_control_principal_modified").parent().parent().hide();
            }

            if (typeof (contentTemplate.ContentFields) != "undefined" && contentTemplate.ContentFields.length > 0) {
                $.each(contentTemplate.ContentFields, function (index, field) {
                    // check if this control uses ckeditor
                    var editor = CKEDITOR.instances[get_field_id(field.Key)];
                    if (editor) {
                        CKEDITOR.instances[get_field_id(field.Key)].setData(field.Value);
                    } else {
                        $('#' + get_field_id(field.Key)).val(field.Value);
                    }
                });
            }
            
            $(".modal-footer .btn-success").show();
        },
        error: logError
    });
}

function get_field_id(field_name) {
    return field_name.replace(' ', '_').trim().toLowerCase();
}

$('#content-template-modal').on('shown.bs.modal', function (e) {
    
    var button = e.relatedTarget;
    if (button == null) {
        return false;
    }
    $(".modal-body .col-sm-9").hide();
    $(".modal-footer .btn-success").hide();
    var instanceId = $(button).data('content-template-instance-id');
    var templateId = $(button).data('template-id');
    trace("instanceId:" + instanceId);
    trace("templateId:" + templateId);

    var me = $(this);

    if (instanceId > 0 && templateId > 0) {
        getContentTemplate(instanceId, templateId);
    }

});

$('#text-content-modal').on('show.bs.modal', function (e) {

    var button = e.relatedTarget;
    if (button == null) {
        return false;
    }
    $(".modal-body .col-sm-9").hide();
    $(".modal-footer .btn-success").hide();
    $(".modal-body input").val("");
    $(".modal-body textarea").val("");
    var contentId = $(button).data('content-id');
    trace("contentId:" + contentId);
    var fileId = $(button).data('file-id');
    $(".j_control_file_id").val(fileId);
    var me = $(this);
    var languageId = me.data('language-id');
    trace("languageId:" + languageId);
    $(".j_control_language_id").val(languageId);
    $(".j_control_content_id").val("");
    $(".j_control_language").html(me.data('language'));

    var enableCk = $(button).data('ck');
    trace(enableCk);

    var enableHtml = me.data('html') === true;
    trace("enableHtml:" + enableHtml);

    $('#content-title').focus();

    if (fileId > 0) {
        $.ajax({
            url: "/AdminService/GetFileForEditing?id=" + fileId + "&languageId=" + languageId,
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            type: "GET",
            success: function (data) {
                trace("GetFileForEditing success");
                if (data.ContentId > 0) {
                    getContent(data.ContentId, languageId, false, false);
                } else {
                    $(".modal-body .col-sm-9").show();
                    $(".modal-footer .btn-success").show();
                }
                $(".j_control_file").empty().append(data.Icon);
            },
            error: logError
        });
    } else if (contentId > 0) {
        $(".j_control_file").empty();
        getContent(contentId, languageId, enableCk, enableCk);
    }
});

$('#text-content-modal a.btn-success').on('click', function (e) {
    var content = new Object();
    content['Title'] = $("#content-title").val();
    content['Subtitle'] = $("#content-subtitle").val();
    content['Teaser'] = $("#content-teaser").val();
    content['Html'] = CKEDITOR.instances['content-html'].getData();
    trace("saving:");
    trace(content['Title']);
    trace(content['Html']);
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
                trace("data:" + data);
            }
        }
    });
});


$('#content-template-modal a.btn-success').on('click', function (e) {

    var contentTemplate = new Object();

    trace("saving:");

    contentTemplate['InstanceId'] = $(".j_control_content_template_instance_id").val();
    contentTemplate['TemplateId'] = $(".j_control_template_id").val();
    contentTemplate['PrincipalModified'] = $('.j_control_principal').val();

    var contentFields = [];
    $('.content-fields .form-group span input').each(function () {
        var contentField = new Object();
        contentField['Key'] = $(this).attr('name');
        contentField['Value'] = $('#' + $(this).attr('name')).val();
        contentFields.push(contentField);
    });

    $('.content-fields .form-group span textarea').each(function () {
        var editor = CKEDITOR.instances[$(this).attr('name')];
        if (editor) {
            var contentField = new Object();
            contentField['Key'] = $(this).attr('name');
            contentField['Value'] = editor.getData();
            contentFields.push(contentField);
        }
    });

    contentTemplate['ContentFields'] = contentFields;

    trace(contentTemplate);

    $.ajax({
        url: "/AdminService/ChangeContentTemplate",
        data: JSON.stringify(contentTemplate),
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        type: "POST",
        success: function (data) {
            if (data === true) {
                $('#content-template-modal').modal('hide');
            }
            else {
                trace("data:" + data);
            }
        },
        error: function (xhr, ajaxOptions, thrownError) {
            trace(xhr.status);
            trace(xhr.responseText);
            trace(thrownError);
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
    var folderId = node.id;
    trace("folderId " + folderId);
	trace(node);
	if (folderId > 0) {
	    files_databind(folderId);
	    trace("nodeSelected:" + folderId);
	    $('#HiddenSelectedFolderId').val(folderId);
	    $('#LabelFolderId').text(folderId);
        
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
VALIDATION
*/

$.validator.addMethod("absrelurl", function (value, element) {
    trace("absrelurl");
    return this.optional(element) || /(http:\/)?(\/[\w\.\-]+)+\/?/.test(value);
}, "Please enter valid URL");

$(document).ready(function () {
    $("#form1").validate({
        onsubmit: false,
        highlight: function (element) {
            trace('highlight');
            $(element).closest('.form-group').addClass('has-error');
        },
        unhighlight: function (element) {
            trace('unhighlight');
            $(element).closest('.form-group').removeClass('has-error');
        },
        errorElement: 'span',
        errorClass: 'help-block',
        errorPlacement: function (error, element) {
            if (element.parent('.input-group').length || element.prop('type') === 'checkbox' || element.prop('type') === 'radio') {
                error.insertAfter(element.parent());
            } else {
                error.insertAfter(element);
            }
        }
    });
    $('.validationGroup .causesValidation').on('click', Validate);
    $('.validationGroup :text').on('keydown', function (evt) {
        if (evt.keyCode == 13) {
            // Find and store the next input element that comes after the
            // one in which the enter key was pressed.
            var $nextInput = $(this).nextAll(':input:first');
            var $nextLinkButton = $(this).nextAll('a:first');
            trace($nextLinkButton);
            // If the next input is a submit button, go into validation.
            // Else, focus the next form element as if enter == tab.
            if ($nextLinkButton.is('.causesValidation')) {
                if (Validate(evt)) {
                    eval($nextLinkButton.attr('href'));
                }
            } else if ($nextInput.is(':submit')) {
                Validate(evt);
            }
            else {
                evt.preventDefault();
                $nextInput.focus();
            }
        }
    });
});

function Validate(evt) {
    trace('Validate evt');
    var $group = $(this).parents('.validationGroup');
    var isValid = true;
    $group.find(':input').each(function (i, item) {
        if (!$(item).valid()) {
            isValid = false;
            $(item).closest('.form-group').addClass('has-error');
        }
            
    });
    if (!isValid)
        evt.preventDefault();
    trace(isValid);
    return isValid;
}

if (typeof window.FileReader === 'undefined') {
    $('.imageFileUploadStatus').html('File API MISSING!!');
    trace('FileReader === undefined');
}

$('input.imageFileUploadWithPreview').on('change', function (e) {
    e.preventDefault();
    trace('input.imageFileUploadWithPreview.onchange');
    var myId = $(this).attr('id');
    var currentFileInput = $(this);

    var reader = new FileReader();
    reader.onload = function (event) {
        trace('reader.onload');
        var img = new Image();
        img.src = event.target.result;
        if (img.width > 100) {
            img.width = 100;
        }
        currentFileInput.after(img);
    };
    reader.readAsDataURL(document.getElementById(myId).files[0]);
    return false;
});

$(function () {
    $('[data-toggle="tooltip"]').tooltip();
	//$('[data-toggle="confirmation"]').confirmation( { btnOkLabel: 'Yes', btnOkClass: 'btn btn-sm btn-success' });
	$('.publishAll').confirmation( { 
		btnOkLabel: 'Yes', 
		btnOkClass: 'btn btn-sm btn-success', 
		onConfirm: function() {
		__doPostBack('ctl00$MainContent$LinkButtonPublishAll','');
		} 
	});
})