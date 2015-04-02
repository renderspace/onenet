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
    trace(XMLHttpRequest);
    bootbox.alert({ title: "Error has occured", message: "<h4>If a problem persists, please contact the system administrator.</h4><p>The following information might be of some use to the administrator:</p> <code>errorThrown: " + errorThrown + "</code>" });
    if (typeof (_gaq) !== 'undefined') {
        _gaq.push(['_trackEvent', 'JS logError', errorToLog]);
    }
        
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

$(document).ready(function() {
    $('#CheckBoxShowPath').change(function () {
        var selectedFolderId = $('#HiddenSelectedFolderId').val();
        if (selectedFolderId > 0) {
            files_databind(selectedFolderId);
        }
    });
});

function files_databind(selectedFolderId) {

    var showPath = $("#CheckBoxShowPath").is(':checked');

    trace("showPath:" + showPath);

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
                var r = '<tr><td><input type="checkbox" name="fileIdToDelete" value="' + item.Id + '"  /></td><td>' +
                    item.Id + '</td><td>' + item.Icon + '</td><td>' + item.Size + 'kB</td><td>';
                if (showPath) {
                    r += '<input type="text" name="textbox" value="' + item.Uri + '" onclick="this.select()" class="hun" />';
                }
                else {
                    r += item.Name;
                }

                r += '</td><td><a href="#" data-toggle="modal" data-target="#text-content-modal" data-file-id="' + item.Id +
                    '"  class="btn btn-info btn-xs"><span class="glyphicon glyphicon-pencil"></span> Edit</a></td></tr>';
                $('#files-table tbody').append(r);
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
    var ckEditor = CKEDITOR.instances["content-html"];
    if (ckEditor !== undefined) {
        ckEditor.destroy();
        trace("ckEditor.destroy");
    }
    $(".ckbox").empty();

    $.ajax({
        url: "/AdminService/GetContent?id=" + contentId + "&languageId=" + languageId,
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        type: "GET",
        success: function (content) {
            trace(content);
            $("#form-title").show();
            $("#form-subtitle").show();
            $("#form-teaser").show();

            $("#content-title").val(content.Title);
            $("#content-subtitle").val(content.Subtitle);
            $("#content-teaser").val(content.Teaser);
            $(".ckbox").html('<div class="col-sm-12"><textarea  class="form-control" id="content-html"></textarea></div>');
            var $contenthtml = $("#content-html");
            $contenthtml.val(content.Html);
            $(".ckbox").show();
            if (enableCk === true) {
                replaceCKEditor(document.getElementById('content-html'));
            }
            else {
                var myCodeMirror = CodeMirror.fromTextArea(document.getElementById('content-html'), {
                        lineNumbers: true,
                        mode: "htmlembedded"
                });
                $('#content-html').data('CodeMirrorInstance', myCodeMirror);
                if (enableHtml === true) {
                    $("#form-title").hide();
                    $("#form-subtitle").hide();
                    $("#form-teaser").hide();
                }
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
    else if (fieldType == 'file')
        controlHtml += '<input type="text" maxlength="255" class="form-control absrelurl" id="' + fieldId + '" name="' + fieldId + '" />';
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
                            replaceCKEditor(document.getElementById(get_field_id(field.Key)));
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

            if (typeof (contentTemplate.DateModified) != "undefined" && contentTemplate.DateModified != null && contentTemplate.DateModified.length > 0) {
                $(".j_control_principal").html(contentTemplate.DateModified + ", " + contentTemplate.PrincipalModified);
            } else if (typeof (contentTemplate.DateCreated) != "undefined" && contentTemplate.DateCreated != null && contentTemplate.DateCreated.length > 0) {
                $(".j_control_principal").html(contentTemplate.DateCreated + ', ' + contentTemplate.PrincipalCreated);
            } 

            if (typeof (contentTemplate.ContentFields) != "undefined" && contentTemplate.ContentFields != null && contentTemplate.ContentFields.length > 0) {
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



function replaceCKEditor(id) {
    CKEDITOR.replace(id, {
        customConfig: '',
        entities_greek: false,
        forcePasteAsPlainText: true,
        entities: false,
        entities_latin: false,
        toolbar: [
    { name: 'tools', items: ['Maximize', 'ShowBlocks', '-', 'Styles'] },
	{ name: 'document', groups: ['mode', 'document', 'doctools'], items: ['Source', '-'] },
	{ name: 'clipboard', groups: ['clipboard'], items: ['Cut', 'Copy', 'Paste'] },
	'/',
	{ name: 'basicstyles', groups: ['basicstyles', 'cleanup'], items: ['Bold', 'Italic', 'Underline', 'Strike', 'Subscript', 'Superscript', '-', 'RemoveFormat'] },
	{ name: 'paragraph', groups: ['list', 'indent', 'blocks', 'align'], items: ['NumberedList', 'BulletedList', '-', 'Outdent', 'Indent', '-', 'Blockquote',  '-', 'JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock'] },
	{ name: 'links', items: ['Link', 'Unlink', 'Anchor'] },
	{ name: 'insert', items: ['Image',  'Table', 'HorizontalRule', 'SpecialChar'] }
        ]
            /*[
    ['Maximize', 'ShowBlocks', 'About', '-', 'Cut', 'Copy', 'Paste', '-', 'Bold', 'Italic', 'NumberedList', 'BulletedList', 'Indent', '-', 'Link', 'Unlink', 'Anchor', '-', 'Image', 'Table', 'HorizontalRule'],
    ['Templates', 'CreateDiv', 'Blockquote', '-', 'JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock', 'Styles', 'Format', 'RemoveFormat', 'Source'],
        ]*/,

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
        stylesCombo_stylesSet: 'one_default_styles',
        templates: 'one_default_templates',
        // contentsCss : '/Utils/default_editor.css',
        height: 500,
        disableObjectResizing: true,
        resize_enabled: false,
        allowedContent: true, //,extraPlugins: 'youtube'
        skin: 'bootstrapck'
    });
}

CKEDITOR.dtd.$removeEmpty['i'] = false;



$(document).ready(function () {

    $('.ckeditor4').each(function (index) {
        trace("CKEDITOR " + index + ": " + this.id);
        replaceCKEditor(this.id);
        trace(this.id);
    });

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

    $('#text-content-modal a.btn-success').on('click', function (e) {
        var content = new Object();
        content['Title'] = $("#content-title").val();
        content['Subtitle'] = $("#content-subtitle").val();
        content['Teaser'] = $("#content-teaser").val();
        trace("Saving content from modal window.");

        var ckEditor = CKEDITOR.instances["content-html"];
        var myCodeMirror = $('#content-html').data('CodeMirrorInstance');
        if (ckEditor !== undefined) {
            content['Html'] = ckEditor.getData();
        }
        else if (myCodeMirror !== undefined) {
            content['Html'] = myCodeMirror.getValue();
        }

        
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

        e.preventDefault();

        var divsWithErrors = $(this).closest('.validationGroup').find('.has-error');
        if (divsWithErrors.length == 0) {
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
        }
    });

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
            getContent(contentId, languageId, true, enableCk);
        }
    });
    
    $('#audit-history').on('show.bs.modal', function (e) {
        //var selectedItemId =  $('#audit-history').data('selected-item-id');
        var selectedItemId = $(this).data('content-id');
        var languageId = $(this).data('language-id');
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
});

function Validate(evt) {
    trace('Validate evt');
    var $group = $(this).closest('.validationGroup');
    var isValid = true;
    $group.find(':input').each(function (i, item) {
        if (!$(item).valid()) {
            isValid = fals;e
            $(item).closest('.form-group').addClass('has-error');
        } else {
            $(item).closest('.form-group').removeClass('has-error');
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



/*
*
*    ********** SCAFFOLD *************
*
*/

SUGGEST_ENTRIES_IN_DROPDOWN_LIMIT = 30;

function populateForeignKeyOptions (virtualTableId, v, whenSelected) {
    $.ajax({
        url: "/ScaffoldService/GetForeignKeyOptions",
        data: { virtualTableId: virtualTableId, columnId: v.Id, limit: SUGGEST_ENTRIES_IN_DROPDOWN_LIMIT },
        dataType: "json",
        type: "GET",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            var optionsHtml = "";
            var noOfItems = 0;
            if (data.length == SUGGEST_ENTRIES_IN_DROPDOWN_LIMIT) {
                $(v.InputId + 's').hide();
                $(v.InputId + 'a').removeClass('hide');
                $(v.InputId + 'a').autocomplete({
                    source: '/_ashx/BambooOneToManyData.ashx?virtualTableId=' + virtualTableId + '&columnId=' + v.Id,
                    select: function (event, ui) {
                        trace("autocomplete: " + ui.item.id);
                        return whenSelected(event, ui);
                    }
                });
            } else {
                $.each(data, function (k, v) {
                    optionsHtml += '<option value="' + k + '">' + v.Value + '</option>';
                    noOfItems++;
                });
                $(v.InputId + 's').append(optionsHtml);
                $(v.InputId + 's').change(function (event) {
                    trace("select changed to: " + $(this).val() + "/" + $(this).children("option").filter(":selected").text());
                    var ui = {
                        item: {
                            id: $(this).val(),
                            label: $(this).children("option").filter(":selected").text()
                        }
                    };
                    trace("select changed to: " + ui.item.id + "/" + ui.item.label);
                    return whenSelected(event, ui);
                });
            }
        },
        error: function () { alert("OneToMany"); }
    });
}

function loadAllToManyRelationships() {
    $('.toMany').each(function () {
        var $me = $(this);
        var relationId = $me.data('relation-id');
        var pk = $me.data('pk');
        var virtualTableId = $me.data('virtual-table-id');
        var foreignKeyColumnName = $me.data('foreignKeyColumnName');
        var friendlyName = $me.data('friendly-name');
        trace(relationId);
        trace(pk);
        trace(virtualTableId);
        trace("foreignKeyColumnName;" + foreignKeyColumnName);
        trace($me.data());
        if (relationId > 0) {
            $.ajax({
                url: "/ScaffoldService/ListItemsForRelation",
                data: { relationId: relationId, primaryKey: pk },
                contentType: 'application/json; charset=utf-8',
                dataType: 'json',
                type: "GET",
                success: function (data) {
                    var tbl_body = "<table  class=\"table\">";
                    $.each(data, function (index) {
                        var tbl_row = "";
                        $.each(this, function (k, v) {
                            tbl_row += "<td>" + v + "</td>";
                        })
                        tbl_body += "<tr>" + tbl_row + '<td>';
                        if (index > 0) {
                            tbl_body += '<a data-ck="true" data-relation-id="' + relationId +
                                '" data-pk="' + this.PK +
                                '" data-force-fk-value="' + pk +
                                '" data-force-fk-column="' + foreignKeyColumnName +
                                '" data-virtual-table-id="' + virtualTableId +
                                '" data-friendly-name="' + friendlyName +
                            '" data-target="#to-many-modal" data-toggle="modal" data-backdrop="false" data-keyboard="true" class="btn btn-info">' +
                            '<span class="glyphicon glyphicon-pencil"></span> Edit</a></td></tr>';
                        }
                        else {
                            tbl_body += '</td></tr>';
                        }
                    })
                    $me.html(tbl_body + "</table>");
                    $me.append('<a data-ck="true" data-force-fk-value="' + pk +
                                '" data-force-fk-column="' + foreignKeyColumnName +
                                '" data-virtual-table-id="' + virtualTableId +
                                '" data-friendly-name="' + friendlyName +
                            '" data-target="#to-many-modal" data-toggle="modal" data-backdrop="false" data-keyboard="true" class="btn btn-info">' +
                            '<span class="glyphicon glyphicon-plus"></span> Add</a>');
                },
                error: logError
            });
        }
    });
}

$(function () {
    loadAllToManyRelationships();
});


function endToManyModal(message) {
    trace(message);
    $('#to-many-modal').modal('hide');
    $('#to-many-modal .form-horizontal').html('');
    $('#to-many-modal a').removeData();
    loadAllToManyRelationships();
}


$('#to-many-modal').on('shown.bs.modal', function (e) {
    var button = e.relatedTarget;
    if (button == null) {
        return false;
    }

    var primaryKey = $(button).data('pk');
    var relationId = $(button).data('relation-id');
    var forceFkValue = $(button).data('force-fk-value');
    var forceFkColumn = $(button).data('force-fk-column');
    var virtualTableId = $(button).data('virtual-table-id');
    var friendlyName = $(button).data('friendly-name');

    trace("pk:" + primaryKey);
    trace("relationId:" + relationId);
    trace("forceFkValue:" + forceFkValue);
    trace("forceFkColumn:" + forceFkColumn);
    trace("virtualTableId:" + virtualTableId);

    var me = $(this);

    trace("ItemEditor bind to virtualTableId: " + virtualTableId);

    $.ajax({
        url: "/ScaffoldService/GetItem",
        data: { virtualTableId: virtualTableId, primaryKey: primaryKey },
        dataType: "json",
        type: "GET",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            var idx = 0;
            if (data == null) {
                logError(null, null, "Item doesn't exists or you don't have the rights.");
                return false;
            }
            $(".form-horizontal", me).html('<div class="form-group"><h3 class="col-sm-3 control-label">' + friendlyName + '</h3></div>'); 
            $('#to-many-modal a.btn-success').data('relation-id', relationId);
            $('#to-many-modal a.btn-success').data('virtual-table-id', virtualTableId);
            $('#to-many-modal a.btn-success').data('pk', primaryKey);
            $('#to-many-modal a.btn-success').data('force-fk-value', forceFkValue);
            $('#to-many-modal a.btn-success').data('force-fk-column', forceFkColumn);

            $('#to-many-modal a.btn-danger').data('virtual-table-id', virtualTableId);
            $('#to-many-modal a.btn-danger').data('pk', primaryKey);

            $.each(data.Columns, function (k, v) {
                $(".form-horizontal", me).append('<div class="form-group"><label class="col-sm-3 control-label">' + v.FriendlyName +
                    '</label><div class="col-sm-9" id="' + v.InputId.substring(1) +
                    '" data-backend-type="' + v.BackendType +
                    '" data-fq-name="' + v.FQName +
                    '"></div></div>');
                trace(v.BackendType);
                if (v.BackendType == "Display") {
                    $(v.InputId).append('<input type="text" class="form-control" readonly="true" />');
                    $(v.InputId + " input").val(v.Value);
                }
                else if (v.BackendType == "OneToMany") {
                    if (v.FQName == forceFkColumn) {
                        trace("Got forced forceFkColumn");
                        $(v.InputId).append('<input type="number" class="form-control digits" readonly="true" />');
                        $(v.InputId + " input").val(forceFkValue);
                    } else {
                        trace("OneToMany populateForeignKeyOptions NOT IMPLEMENTED YET. " + v.FQName);
                        /* populateForeignKeyOptions(virtualTableId, v, function (event, ui) {
                            trace("OneToMany selected value:" + ui.item.id);
                            $(v.InputId).val(ui.item.id);
                            return true;
                        });*/
                    }
                } else if (v.BackendType == "ManyToMany") {
                    trace("ManyToMany populateForeignKeyOptions NOT IMPLEMENTED YET. " + v.FQName);
                    /*
                    populateForeignKeyOptions(virtualTableId, v, function (event, ui) {
                        trace(ui.item);
                        $(v.InputId).append($('<option>', {
                            value: ui.item.id,
                            text: ui.item.label
                        }));
                        return true;
                    });*/
                } else if (v.BackendType == "Calendar") {
                    $(v.InputId).datetimepicker();
                } else if (v.BackendType == "DateOnly") {
                    $(v.InputId).datepicker();
                } else if (v.BackendType == "TimeOnly") {
                    $(v.InputId).timepicker();
                } else if (v.BackendType == "Checkbox") {
                    $(v.InputId).append('<input type="checkbox" />');
                    trace(v.Value);
                    if (v.Value === "True") {
                        trace(v.Value);
                        $(v.InputId + " input").prop('checked', true);
                    }
                } else if (v.BackendType == "Integer") {
                    $(v.InputId).append('<input class="form-control digits required" maxlength="11" type="number" />');
                    $(v.InputId + " input").val(v.Value);
                } else {
                    trace("m" + v.InputId + " " + v.Value + " (" + v.BackendType + ")");
                    $(v.InputId).append('<input type="text" class="form-control" />');
                    $(v.InputId + " input").val(v.Value);
                }
            });
        }
    });
});

$('#to-many-modal a.btn-danger').on('click', function (e) {
    var virtualTableId = $(this).data('virtual-table-id');
    var pk = $(this).data('pk');
    if (pk > 0 && virtualTableId > 0) {
        $.ajax({
            url: "/ScaffoldService/DeleteItem?virtualTableId=" + virtualTableId + "&primaryKey=" + pk,
            dataType: "json",
            type: "GET",
            cache: false,
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                if (data == true) {
                    endToManyModal("Successfully deleted");
                } else {
                    logError(null, null, "Could not delete the item.");
                    trace(data);
                }
            },
            error: logError
        });
    }
    else {
        logError(null, null, "Missing vtId and pk.");
    }
});


$('#to-many-modal a.btn-success').on('click', function (e) {
    var relationId = $(this).data('relation-id');
    var virtualTableId = $(this).data('virtual-table-id');
    var pk = $(this).data('pk');
    var forceFkValue = $(this).data('force-fk-value');
    var forceFkColumn = $(this).data('force-fk-column');
    trace("Save item called with: ");
    trace($(this).data());
    var json = {
        Columns: []/*, __type: "DTOItem" */
    };
    trace(pk);
    if (pk == undefined)
        pk = 0;

    if (relationId > 0 && virtualTableId > 0 && forceFkColumn !== undefined && forceFkColumn.length > 0) {
        var columnFk = {};
        columnFk['FQName'] = forceFkColumn;
        columnFk['Value'] = forceFkValue;
        json.Columns.push(columnFk);
    }
    else {
        trace("#to-many-modal was called with incorrect parameteres");
    }
    $("#to-many-modal .form-horizontal div div").each(function (i, v) {
        $v = $(v);

        var $input = $("input", $v);
        if ($input !== null || $input !== undefined) {
            var backendType = $v.data("backend-type");
            var fqName = $v.data("fq-name");

            var cn = i;

            var column = {};

            column['FQName'] = fqName;

            column['InputId'] = $v.attr('id');
            if (backendType == "ManyToMany") {
                trace($v.attr('id') + " > option");
                column['Value'] = "";
                var items = $($v.attr('id') + " > option").map(function () {
                    column['Value'] += $(this).val() + ",";
                });
            } else if (backendType == "Checkbox") {
                column['Value'] = $input.prop('checked');
                trace("Checkbox");
            }
            else {
                column['Value'] = $input.val();
            }
            json.Columns.push(column);
        }
    });
    $.ajax({
        url: "/ScaffoldService/ChangeItem?virtualTableId=" + virtualTableId + "&primaryKey=" + pk,
        data: JSON.stringify(json),
        dataType: "json",
        type: "POST",
        cache: false,
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            if (data == true) {
                endToManyModal("Item saved.");
            } else {
                logError(null, null, "Could not save the item.");
                trace(data);
            }
        },
        error: logError 
    });
});