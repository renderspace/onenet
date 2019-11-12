

    $.ajaxSetup({ cache: false });

    function handleAjaxError(XMLHttpRequest, textStatus, errorThrown) {
        if (XMLHttpRequest.responseText.indexOf("Access is denied") > -1) {
            $('.sessionTimeoutError').show();
        }
        $(".loading").hide();
        logError(XMLHttpRequest, textStatus, errorThrown);
    }

    function logError(XMLHttpRequest, textStatus, errorThrown) {
        var errorToLog = "textStatus: " + textStatus + " errorThrown: " + errorThrown;
        console.log(errorToLog);
        console.log(XMLHttpRequest);
        if (XMLHttpRequest.responseText.indexOf("Access is denied") == -1) {
            //bootbox.alert({ title: "Error has occured", message: "<h4>If a problem persists, please contact the system administrator.</h4><p>The following information might be of some use to the administrator:</p> <code>errorThrown: " + errorThrown + "</code>" });
        }
        if (typeof (ga) !== 'undefined') {
            ga('send', 'event', 'JS logError', 'error', errorToLog);
        }
    }

    function GetUrlParam(paramName) {
        var oRegex = new RegExp('[\?&]' + paramName + '=([^&]+)', 'i');
        var oMatch = oRegex.exec(window.top.location.search);

        if (oMatch && oMatch.length > 1)
            return decodeURIComponent(oMatch[1]);
        else
            return '';
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
            if (checkboxes[i].type == "checkbox" && checkboxes[i].id != "gvProducts_ctl01_chkAll" && checkboxes[i].parentElement.className.indexOf('doNotAllCheck') == -1) {
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
            error: handleAjaxError
        });
    };

    function files_databind(selectedFolderId) {
        $('#files-table thead').hide();
        $('.fileManagerDeleteButtons').hide();
        $('#files-table tbody').empty();
        $('#files-table tbody').html('<tr><td colspan="6"><div class="loading"></div></td></tr>');
        var searchString = $('#ctl00_MainContent_TextBoxSearch').val();
        $.ajax({
            url: "/AdminService/ListFiles?folderId=" + selectedFolderId + "&languageId=" + languageId,
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            type: "GET",
            success: function (data) {
                var fromCK = getUrlParam('CKEditorFuncNum');
                console.log("ListFiles success.. " + (fromCK ? 'from CK' : ''));
                $('#files-table tbody').empty();
                $.map(data, function (item) {
                    var r = '<tr';
                    if (item.Id == searchString) {
                        r += ' class="very-recent" ';
                        console.log("got it");
                    }
                    r += '><td><input type="checkbox" name="fileIdToDelete" value="' + item.Id + '"  /></td><td>';
                    r += '<a href="#" class="btn btn-xs btn-primary copybutton" data-clipboard-text="' + item.Uri + '" title="Click to copy path."><span class="glyphicon glyphicon-copy"></span>' + (fromCK ? 'Select file' : 'Copy path to Clipboard') + '</a> ';
                    r += '</td><td>' + item.Icon + '</td><td>' + item.Size + 'kB';
                    r += ' <br/><a href="#" class="btn btn-xs btn-warning openFileReplace"  data-file-id="' + item.Id + '">Replace file</a>';
                    r += ' </td><td>';
                    r += item.Name;
                    r += '</td><td><a href="#" data-toggle="modal" data-target="#text-content-modal" data-file-id="' + item.Id +
                        '"  class="btn btn-info btn-xs"><span class="glyphicon glyphicon-pencil"></span> Edit</a></td></tr>';
                    $('#files-table tbody').append(r);

                    if (fromCK) {
                        $("#files-table tr th:nth-child(1), table tr td:nth-child(1)").hide();
                        $("#files-table tr th:nth-child(4), table tr td:nth-child(4)").hide();
                        // 
                        $("#files-table tr th:last-child, table tr td:last-child").hide();
                        $('.btn-danger').hide();
                        $('.addStuff').hide();
                        $('#ButtonDelete').hide();
                    }
                });

                bindCopyButtons();

                $('.openFileReplace').each(function (e) {
                    var fileId = $(this).data("file-id");
                    var dz = $(this).dropzone({
                        url: "/adm/FileManager.aspx",
                        autoProcessQueue: true,
                        previewsContainer: "#previews",
                        parallelUploads: 2,
                        previewTemplate: globalPreviewTemplate,
                        init: function () {
                            //console.log(fileId);
                            this.on('complete', function (e) {
                                console.log(e);
                                if (this.getUploadingFiles().length === 0 && this.getQueuedFiles().length === 0) {
                                    var selectedFolderId = $('#HiddenSelectedFolderId').val();
                                    console.log("complete: " + selectedFolderId);
                                    files_databind(selectedFolderId);
                                    $(".adminSection").before('<div class="alert alert-success"><p><span>Replaced file. Please double check if the file was really replaced.</span></p></div>');
                                    $("#previews").empty();
                                }
                            });
                            this.on("sending", function (file, xhr, formData) {
                                console.log("DZ sending");
                                if (fileId > 0) {
                                    formData.append("ReplaceFileId", fileId);
                                }
                            });
                        }
                    });
                });

                $('.fileManagerDeleteButtons').show();
                if (data.length > 0) {
                    $('#files-table thead').show();
                    $('.fileManagerDeleteButtons #ButtonDelete').show();
                } else {
                    $('.fileManagerDeleteButtons #ButtonDelete').hide();
                }
            },
            error: handleAjaxError
        });
    };

    function bindCopyButtons() {
        $(".copybutton").off('click');
        $(".copybutton").on('click', function (e) {
            var path = $(this).data('clipboard-text');
            copyToClipboard(path);
            e.preventDefault();
        });
    }

    function toggleRevertTextContentButton(instanceId) {

        if (instanceId > 0) {
            $.ajax({
                url: "/AdminService/IsTextContentPublished?instanceId=" + instanceId,
                contentType: 'application/json; charset=utf-8',
                dataType: 'json',
                type: "GET",
                success: function (result) {
                    if (result) {
                        $('.modal-revert-to-published').show();
                    } else {
                        $('.modal-revert-to-published').hide();
                    }
                },
                error: function (a, b, c) {
                    $('.modal-revert-to-published').hide();
                    handleAjaxError(a, b, c);
                }
            });
        } else {
            $('.modal-revert-to-published').hide();
        }
    }

    function getContent(contentId, languageId, enableHtml, enableCk) {

        $(".loading").show();

        var ckEditor = CKEDITOR.instances["content-html"];
        if (ckEditor !== undefined) {
            ckEditor.destroy();
        }
        $(".ckbox").empty();

        if (contentId > 0) {
            $.ajax({
                url: "/AdminService/GetContent?id=" + contentId + "&languageId=" + languageId,
                contentType: 'application/json; charset=utf-8',
                dataType: 'json',
                type: "GET",
                success: function (content) {

                    $("#content-title").val(content.Title);
                    $("#content-subtitle").val(content.Subtitle);
                    $("#content-teaser").val(content.Teaser);
                    $(".j_control_content_id").val(contentId);

                    setUpHtmlEditing(enableHtml, enableCk, content.Html);
                },
                error: function(a, b, c) {
                    handleAjaxError(a, b, c);
                }
                
            });
        } else {
            setUpHtmlEditing(enableHtml, enableCk, "");
        }
    }

    function generateArticleParLink(title) {

        var parLink = "";
        $(".loading").show();

        if (title.length > 0) {
            $.ajax({
                url: "/AdminService/GenerateArticleParLink?title=" + title,
                contentType: 'application/json; charset=utf-8',
                dataType: 'json',
                type: "GET",
                async: false,
                success: function (content) {

                    parLink = content;
                },
                error: handleAjaxError
            });
        } else {
            setUpHtmlEditing(enableHtml, enableCk, "");
        }

        $(".loading").show();
        return parLink;
    }

    function generateRegularParLink(title) {

        var parLink = "";
        $(".loading").show();

        if (title.length > 0) {
            $.ajax({
                url: "/AdminService/GenerateRegularParLink?title=" + title,
                contentType: 'application/json; charset=utf-8',
                dataType: 'json',
                type: "GET",
                async: false,
                success: function (content) {

                    parLink = content;
                },
                error: handleAjaxError
            });
        } else {
            setUpHtmlEditing(enableHtml, enableCk, "");
        }

        $(".loading").show();
        return parLink;
    }

    function setUpHtmlEditing(enableHtml, enableCk, html) {
        console.log("setUpHtmlEditing");
        $("#form-title").show();
        $("#form-subtitle").show();
        $("#form-teaser").show();
        $(".ckbox").html('<div class="col-sm-12"><textarea  class="form-control" id="content-html"></textarea></div>');
        var $contenthtml = $("#content-html");
        $contenthtml.val(html);
        if (enableCk === true) {
            console.log("enableCk... ignoring enableHtml");
            $(".ckbox").show();
            replaceCKEditor(document.getElementById('content-html'));
        } else {
            if (enableHtml === true) {
                var myCodeMirror = CodeMirror.fromTextArea(document.getElementById('content-html'), {
                    lineNumbers: true,
                    mode: "htmlembedded"
                });
                console.log("html not enabled...");
                $("#form-title").hide();
                $("#form-subtitle").hide();
                $("#form-teaser").hide();
                $(".ckbox").show();

                $('#content-html').data('CodeMirrorInstance', myCodeMirror);
            } else {
                console.log("enableCk false ... enableHtml false");
                $(".ckbox").hide();
            }
        }
        $(".modal-body .col-sm-9").show();
        $(".modal-footer .btn-success").show();
        $(".loading").hide();
    }

    function buildFormControl(fieldLabel, fieldId, fieldType) {
        var controlHtml = '';
        if (fieldType != 'builtin') {

            controlHtml = '<div class="form-group">';

            controlHtml += '<label class="col-sm-3 control-label">' + fieldLabel + '</label><div class="col-sm-9">';

            controlHtml += '<span class="">';
            if (fieldType == 'html')
                controlHtml += '<textarea  class="form-control ckeditor4" id="' + fieldId + '" name="' + fieldId + '"></textarea>';
            else if (fieldType == 'singleline')
                controlHtml += '<input type="text" maxlength="255" class="form-control" id="' + fieldId + '" name="' + fieldId + '" />';
            else if (fieldType == 'repeatedinput') {
                controlHtml += '<input type="hidden" class="repeatedinput_count" id="' + fieldId + '_count" name="' + fieldId + '_count" value="1" />';
                controlHtml += '<div class="repeated-controls repeated-controls-' + fieldId + '">';
                controlHtml += '<div class="inputs">';
                controlHtml += '<div><input type="text" maxlength="255" class="form-control form-repeated-control" id="' + fieldId + '_repeated_0" name="' + fieldId + '_repeated_0" /></div>';
                controlHtml += '</div>';
                controlHtml += '<a href="#" class="addRepeatedInput">Add</a>';
                controlHtml += '</div>';
            }
            else if (fieldType == 'file')
                controlHtml += '<input type="text" maxlength="255" class="form-control absrelurl" id="' + fieldId + '" name="' + fieldId + '" />';
            else
                controlHtml += '<input type="text" maxlength="255" class="form-control" id="' + fieldId + '" name="' + fieldId + '" />';
            controlHtml += '</span>';
            controlHtml += '</div>';
            controlHtml += '</div>';
        }
        return controlHtml;
    }

    function bindFormControlEvents() {
        $('.addRepeatedInput').off('click');
        $('.addRepeatedInput').on('click', function (e) {

            if (($(this).closest('.repeated-controls').prev('.repeatedinput_count')).length > 0) {
                var countId = $(this).closest('.repeated-controls').prev('.repeatedinput_count').attr('id');
                var fieldId = countId.replace('_count', '');
                var count = parseInt($('#' + countId).val());
                $('#' + countId).next('.repeated-controls').find('.inputs').append('<div><input type="text" maxlength="255" class="form-control form-repeated-control" id="' + fieldId + '_repeated_' + count + '" name="' + fieldId + '_repeated_' + count + '" /><a class="removeRepeatedInput" href="#">Remove</a></div>');
                count++;
                $('#' + countId).val(count);
                bindFormControlEvents();
            }
            e.preventDefault();
        });

        $('.removeRepeatedInput').off('click');
        $('.removeRepeatedInput').on('click', function (e) {

            if (($(this).closest('.repeated-controls').prev('.repeatedinput_count')).length > 0) {
                
                var $repeatedControls = $(this).closest('.repeated-controls');
                var countId = $(this).closest('.repeated-controls').prev('.repeatedinput_count').attr('id');

                var inputFieldId = $(this).prev('.form-control').attr('id');
                $repeatedControls.find('#' + inputFieldId).parent().remove();
               
                var fieldId = countId.replace("_count", "");

                var count = parseInt($('#' + countId).val());
                count--;
                $('#' + countId).val(count);

                var i = 0;
                $repeatedControls.find('.form-repeated-control').each(function (index, element) {
                    $(this).attr('id', fieldId + '_repeated_' + i);
                    $(this).attr('name', fieldId + '_repeated_' + i);
                    i++;
                });
                
                bindFormControlEvents();
            }
            e.preventDefault();
        });
    }

    function getContentTemplate(instanceId, templateId) {

        $(".loading").show();
        $(".j_control_content_template_instance_id").val(instanceId);
        $(".j_control_template_id").val(templateId);

        $.ajax({
            url: "/AdminService/GetTemplate?templateId=" + templateId,
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            type: "GET",
            async: false,
            success: function (template) {

                if (typeof (template) != "undefined" && typeof (template.ContentFields) != "undefined" && template.ContentFields.length > 0) {

                    if ($('.content-fields').html().length > 0) {
                        for (name in CKEDITOR.instances) {
                            CKEDITOR.instances[name].destroy()
                        }
                        $('.content-fields').html('');
                    }

                    $.each(template.ContentFields, function (index, field) {
                        // do your stuff here
                        // $('[name=' + get_field_id(field.Key) + ']').val(field.Value);
                        var controlHtml = buildFormControl(field.Key, get_field_id(field.Key), field.Value);
                        $('.content-fields').append(controlHtml);
                        bindFormControlEvents();
                        if (field.Value == 'html') {
                            var editor = CKEDITOR.instances[get_field_id(field.Key)];
                            if (!editor) {
                                replaceCKEditor(document.getElementById(get_field_id(field.Key)));
                            }
                        }
                    });

                }

            },
            error: handleAjaxError
        });

        $.ajax({
            url: "/AdminService/GetContentTemplate?instanceId=" + instanceId,
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            type: "GET",
            async: false,
            success: function (contentTemplate) {

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
                            var fieldId = get_field_id(field.Key);
                            if ($('[name=' + fieldId + '_count]').length > 0) {

                                var repeatedValues = field.Value.split("|||"); // ||| is the separator I used for repeatedinput values
                                $('[name=' + fieldId + '_count]').val(repeatedValues.length);

                                // count field exists, this means this is a repeated field.
                                var repeatedCount = parseInt($('[name=' + fieldId + '_count]').val());
                                if (repeatedCount > 0) {

                                    var repeatedLineControls = '<div class="repeated-controls repeated-controls-' + fieldId + '">';
                                    repeatedLineControls += '<div class="inputs">';
                                    for (i = 0; i < repeatedCount; i++) {
                                        var inputValue = (typeof repeatedValues[i] !== 'undefined' ? repeatedValues[i] : '');
                                        repeatedLineControls += '<div><input type="text" maxlength="255" class="form-control form-repeated-control" id="' + fieldId + '_repeated_' + i + '" name="' + fieldId + '_repeated_' + i + '" value="' + inputValue + '" /><a class="removeRepeatedInput" href="#">Remove</a></div>';
                                    }
                                    repeatedLineControls += '</div>';
                                    repeatedLineControls += '<a href="#" class="addRepeatedInput">Add</a>';
                                    repeatedLineControls += '</div>';
                                    $('.content-fields .repeated-controls.repeated-controls-' + fieldId).remove();
                                    $('[name=' + fieldId + '_count]').after(repeatedLineControls);
                                    bindFormControlEvents();
                                }
                            } else {
                                $('[name=' + get_field_id(field.Key) + ']').val(field.Value);
                            }
                        }
                    });
                }

                $(".modal-footer .btn-success").show();

            },
            error: handleAjaxError
        });

        $(".loading").hide();
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
        console.log("absrelurl");
        return this.optional(element) || /(http:\/)?(\/[\w\.\-]+)+\/?/.test(value);
    }, "Please enter valid URL");

    $.fn.modal.Constructor.prototype.enforceFocus = function () {
        var $modalElement = this.$element;
        $(document).on('focusin.modal', function (e) {
            var $parent = $(e.target.parentNode);
            if ($modalElement[0] !== e.target && !$modalElement.has(e.target).length
                // add whatever conditions you need here:
                &&
                !$parent.hasClass('cke_dialog_ui_input_select') && !$parent.hasClass('cke_dialog_ui_input_text')) {
                $modalElement.focus()
            }
        })
    };

    function replaceCKEditor(id) {
        console.log('replace CK to id:' + id)
        CKEDITOR.replace(id, {
            customConfig: '',
            toolbar: [
        { name: 'tools', items: ['Maximize', 'ShowBlocks', '-', 'Styles'] },
	    { name: 'document', groups: ['mode', 'document', 'doctools'], items: ['Source', '-'] },
	    { name: 'clipboard', groups: ['clipboard', 'undo'], items: ['Cut', 'Copy', 'Paste', '-', 'Undo', 'Redo'] },
	    '/',
	    { name: 'basicstyles', groups: ['basicstyles', 'cleanup'], items: ['Bold', 'Italic', 'Underline', 'Strike', 'Subscript', 'Superscript', '-', 'RemoveFormat'] },
	    { name: 'paragraph', groups: ['list', 'indent', 'blocks', 'align'], items: ['NumberedList', 'BulletedList', '-', 'Outdent', 'Indent', '-', 'Blockquote', '-', 'JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock'] },
	    { name: 'links', items: ['Link', 'Unlink', 'Anchor'] },
	    { name: 'insert', items: ['Image', 'Table', 'HorizontalRule', 'SpecialChar'] }
            ],
            entities_greek: false,
            forcePasteAsPlainText: true,
            entities: false,
            entities_latin: false,
            filebrowserBrowseUrl: '/adm/FileManager.aspx',
            filebrowserWindowWidth: '980',
            filebrowserWindowHeight: '600',
            filebrowserImageBrowseLinkUrl: '/adm/FileManager.aspx?type:Images',
            filebrowserImageWindowWidth: '980',
            filebrowserImageWindowHeight: '600',
            stylesSet: 'ck_styles:/site_specific/ckstyles.js',
            disableObjectResizing: true,
            templates: 'one_default_templates',
            contentsCss: '/site_specific/ck.css',
            height: 350,
            disableObjectResizing: true,
            resize_enabled: false,
            allowedContent: true
        });
    }

    if (typeof (CKEDITOR) !== 'undefined') {
        CKEDITOR.dtd.$removeEmpty['i'] = false;
    }

    $(document).ready(function () {
        loadAllToManyRelationships();

        $('.table-clickable-row tr td').on('click', function () {
            var $col = $(this).parent().children().index($(this));
            var $link = $(this).parent().children().find("a");
            if ($col > 0 && $link.length === 1) {
                eval($($link[0]).attr('href'));
            }
        });

        $('#CheckBoxShowPath').change(function () {
            var selectedFolderId = $('#HiddenSelectedFolderId').val();
            if (selectedFolderId > 0) {
                files_databind(selectedFolderId);
            }
        });

        $('[data-toggle="tooltip"]').tooltip();
        //$('[data-toggle="confirmation"]').confirmation( { btnOkLabel: 'Yes', btnOkClass: 'btn btn-sm btn-success' });
        $('.publishAll').confirmation({
            btnOkLabel: 'Yes',
            btnOkClass: 'btn btn-sm btn-success',
            onConfirm: function () {
                __doPostBack('ctl00$MainContent$LinkButtonPublishAll', '');
            }
        });

        $('.deleteAll').confirmation({
            title: 'This operation is not reversible! Are you sure?',
            btnOkLabel: 'Yes',
            href: function (e) {
                return $(this).attr('href');
            },
            btnOkClass: 'btn btn-sm btn-danger'
        });

        $('.ckeditor4').each(function (index) {
            console.log("CKEDITOR " + index + ": " + this.id);
            replaceCKEditor(this.id);
            console.log(this.id);
        });

        $("#form1").validate({
            onsubmit: false,
            highlight: function (element) {
                console.log('highlight');
                $(element).closest('.form-group').addClass('has-error');
            },
            unhighlight: function (element) {
                console.log('unhighlight');
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
                console.log($nextLinkButton);
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
            console.log('input.imageFileUploadWithPreview.onchange');
            var myId = $(this).attr('id');
            var currentFileInput = $(this);

            var reader = new FileReader();
            reader.onload = function (event) {
                console.log('reader.onload');
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
            console.log("folderId " + folderId);
            console.log(node);
            if (folderId > 0) {
                files_databind(folderId);
                console.log("nodeSelected:" + folderId);
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

        $('#text-content-modal button.btn-primary').on('click', function (e) {
            if ($(".j_control_file_id").val().length > 0) {
                $('#text-content-modal').modal('hide');
            } else {
                window.location.href = "/adm/structure.aspx";
            }
        });

        $('#text-content-modal a.btn-danger.modal-revert-to-published').on('click', function (e) {

            var start = window.performance.now();

            $(".loading").show();

            $('#text-content-modal').modal('show');

            console.log("Reverting content from modal window.");

            var saveButton = $(this);
            var content = new Object();

            content['LanguageId'] = $(".j_control_language_id").val();
            content['ContentId'] = $(".j_control_content_id").val();
            content['InstanceId'] = $('.j_control_instance_id').val();

            $.ajax({
                url: "/AdminService/RevertTextContent",
                data: JSON.stringify(content),
                contentType: 'application/json; charset=utf-8',
                dataType: 'json',
                type: "POST",
                success: function (data) {

                    $(".modal-body").hide();
                    $(".modal-footer").hide();
                    $('#text-content-modal').modal('hide');
                    $(".loading").hide();

                    var end = window.performance.now();
                    console.log('console.time RevertTextContent done ' + (end - start));
                    window.location.href = "/adm/Structure.aspx";
                },
                error: handleAjaxError
            });

        });

        $('#text-content-modal a.btn-success').on('click', function (e) {

            var start = window.performance.now();

            $(".loading").show();

            $('#text-content-modal').modal('show');

            var $saveButton = $(this);
            var content = new Object();
            content['Title'] = $("#content-title").val();
            content['Subtitle'] = $("#content-subtitle").val();
            content['Teaser'] = $("#content-teaser").val();
            console.log("Saving content from modal window. Title: " + content['Title']);
            var ckEditor = CKEDITOR.instances["content-html"];
            var myCodeMirror = $('#content-html').data('CodeMirrorInstance');

            if (ckEditor !== undefined) {
                console.log("ckEditor !== undefined");
                content['Html'] = ckEditor.getData();
            }
            else if (myCodeMirror !== undefined) {
                console.log("myCodeMirror !== undefined");
                content['Html'] = myCodeMirror.getValue();
            }
            else {
                content['Html'] = '';
            }
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
                        if ($saveButton.hasClass("modal-save-close")) {
                            if (content['FileId'].length > 0) {
                                $('#text-content-modal').modal('hide');
                                $(".loading").hide();
                            } else {

                                var end = window.performance.now();
                                console.log('console.time ChangeContent done ' + (end - start));
                                window.location.href = "/adm/structure.aspx";
                            }
                        } else {
                            $(".modal-body").show();
                            $(".modal-footer").show();
                            $(".loading").hide();
                        }
                    }
                    else {
                        logError(null, null, "error while saving - content not saved");
                    }
                },
                error: handleAjaxError
            });
        });

        $('#content-template-modal a.btn-success').on('click', function (e) {

            e.preventDefault();

            var divsWithErrors = $(this).closest('.validationGroup').find('.has-error');
            if (divsWithErrors.length == 0) {
                var contentTemplate = new Object();
                var $saveButton = $(this);

                console.log("saving:");

                contentTemplate['InstanceId'] = $(".j_control_content_template_instance_id").val();
                contentTemplate['TemplateId'] = $(".j_control_template_id").val();
                contentTemplate['LanguageId'] = languageId;
                contentTemplate['PrincipalModified'] = $('.j_control_principal').val();

                var contentFields = [];
                var repeatedKey = '';
                var repeatedCount = 0;
                var repeatedValues = '';
                var contentField = new Object();

                $('.content-fields .form-group span input').each(function () {

                    var fieldClass = $(this).attr('class');

                    if (fieldClass.indexOf('form-repeated-control') !== -1) {

                        var fieldName = $(this).attr('name');
                        if (fieldName.endsWith("_repeated_0")) {
                            contentField = new Object();
                            repeatedValues = '';
                            repeatedKey = fieldName.replace("_repeated_0", "");
                            contentField['Key'] = repeatedKey;
                            repeatedCount = parseInt($(this).closest('.repeated-controls').prev('.repeatedinput_count').val());
                        }

                        if (repeatedKey != '') {
                            var fieldValue = $('#' + $(this).attr('name')).val().trim();
                            if (fieldValue.length > 0) {
                                repeatedValues += repeatedValues.length > 0 ? '|||' + fieldValue : fieldValue;
                            }
                            contentField['Value'] = repeatedValues;
                        }

                        if (fieldName.endsWith("_repeated_" + (repeatedCount - 1))) {
                            contentFields.push(contentField);
                        }
                    } else {
                        contentField = new Object();
                        contentField['Key'] = $(this).attr('name');
                        contentField['Value'] = $('#' + $(this).attr('name')).val();
                        contentFields.push(contentField);
                    }
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

                console.log(contentTemplate);

                $.ajax({
                    url: "/AdminService/ChangeContentTemplate",
                    data: JSON.stringify(contentTemplate),
                    contentType: 'application/json; charset=utf-8',
                    dataType: 'json',
                    type: "POST",
                    success: function (data) {
                        if (data === true) {
                            if ($saveButton.hasClass("modal-save-close")) {
                                $('#content-template-modal').modal('hide');
                                window.location.href = "/adm/structure.aspx";
                            } else {
                                $(".modal-body").show();
                                $(".modal-footer").show();
                                $(".loading").hide();
                            }
                        }
                        else {
                            console.log("data:" + data);
                        }
                    },
                    error: handleAjaxError
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
            console.log("instanceId:" + instanceId);
            console.log("templateId:" + templateId);

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

            $(".modal-body").show();
            $(".modal-footer").show();

            $(".modal-body .col-sm-9").hide();
            $(".modal-footer .btn-success").hide();
            $(".modal-body input").val("");
            $(".modal-body textarea").val("");
            var contentId = $(button).data('content-id');
            console.log("contentId:" + contentId);

            var instanceId = $(button).data('instance-id');
            $(".j_control_instance_id").val(instanceId);

            toggleRevertTextContentButton(instanceId);

            var fileId = $(button).data('file-id');
            $(".j_control_file_id").val(fileId);
            var me = $(this);
            var languageId = me.data('language-id');
            console.log("languageId:" + languageId);
            $(".j_control_language_id").val(languageId);
            $(".j_control_content_id").val("");
            $(".j_control_language").html(me.data('language'));

            var enableCk = $(button).data('ck');
            console.log(enableCk);

            var enableHtml = me.data('html') === true;
            console.log("enableHtml:" + enableHtml);

            $('#content-title').focus();

            if (fileId > 0) {
                $.ajax({
                    url: "/AdminService/GetFileForEditing?id=" + fileId + "&languageId=" + languageId,
                    contentType: 'application/json; charset=utf-8',
                    dataType: 'json',
                    type: "GET",
                    success: function (data) {
                        console.log("GetFileForEditing success");
                        getContent(data.ContentId, languageId, false, false);
                        $(".j_control_file").empty().append(data.Icon);
                    },
                    error: handleAjaxError
                });
            } else if (contentId > 0) {
                $(".j_control_file").empty();
                getContent(contentId, languageId, true, enableCk);
            }
        });

        var searchTypingTimer;                //timer identifier
        var searchDoneTypingInterval = 1000;

        $('.textBoxSearchContent').on('keyup', function (e) {
            clearTimeout(searchTypingTimer);
            var keyword = $('input.textBoxSearchContent').val();
            if (keyword.length > 0) {
                searchTypingTimer = setTimeout(searchPageContentDelegate, searchDoneTypingInterval);
            }
        });

        function searchPageContentDelegate() {

            var keyword = $('input.textBoxSearchContent').val();
            
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
                        $pageContentSearchResults.append('<li><a href="/adm/Structure.aspx?spid=' + item.Id + '">[' + item.Id + '] ' + item.Title + '</a></li>');
                    });
                },
                error: function (err) {
                    $('.content-search .loading').hide();
                    handleAjaxError(err)
                }
            });
            $.ajax({
                url: "/AdminService/SearchArticles?keyword=" + keyword + "&languageId=" + languageId,
                contentType: 'application/json; charset=utf-8',
                dataType: 'json',
                type: "GET",
                success: function (data) {
                    $.each(data, function (index, item) {
                        $pageContentSearchResults.append('<li><a href="/adm/Articles.aspx?keyword=' + item.Id + '">[ARTICLE ' + item.Id + '] ' + item.Title + '</a></li>');
                    });
                },
                error: function (err) {
                    $('.content-search .loading').hide();
                    handleAjaxError(err)
                }
            });
            $.ajax({
                url: "/AdminService/SearchDictionary?keyword=" + keyword + "&languageId=" + languageId,
                contentType: 'application/json; charset=utf-8',
                dataType: 'json',
                type: "GET",
                success: function (data) {
                    $.each(data, function (index, item) {
                        $pageContentSearchResults.append('<li><a href="/adm/Dictionary.aspx?keyword=' + item.Id + '">[' + item.Id + '] ' + item.Title + '</a></li>');
                    });
                },
                error: function (err) {
                    $('.content-search .loading').hide();
                    handleAjaxError(err)
                }
            });
        }

        $('#audit-history').on('show.bs.modal', function (e) {

            $('.loading').show();
            $('.modal-body').hide();
            $('.modal-footer').hide();

            var selectedItemId = $(this).data('content-id');
            var languageId = $(this).data('language-id');
            if (selectedItemId > 0) {
                $.ajax({
                    url: "/AdminService/GetContentHistory?contentId=" + selectedItemId + "&languageId=" + languageId,
                    contentType: 'application/json; charset=utf-8',
                    dataType: 'json',
                    type: "GET",
                    success: function (data) {
                        console.log("GetContentHistory success");

                        $('#audit-history-table tbody').empty();
                        $.map(data, function (item) {
                            $('#audit-history-table tbody').append('<tr><td>' + item.DisplayLastChanged + '</td><td>' + item.Title + '</td></tr>');
                        });

                        $('.loading').hide();
                        $('.modal-body').show();
                        $('.modal-footer').show();
                    },
                    error: handleAjaxError
                });
            }
        });


        $(".nav-sidebar ul li a").each(function () {
            var anchor = $(this);
            var attrHref = anchor.attr('href');
            var url = (typeof attrHref !== typeof undefined && attrHref.length > 0) ? attrHref.trim() : "";
            if (url == "" || url == "#") {
                //unbind the __dopostback
                anchor.unbind('click');
                anchor.bind('click', function (e) {
                    e.preventDefault();
                });
                anchor.addClass('nolink');
            }
        });
        console.log("%cOne.NET DOM ready.", "color:green; background-color:yellow");

        if (window.TextBoxHumanReadableUrlClientId && window.TextBoxHumanReadableUrlClientId.length > 0 && window.TextBoxTitleClientId && window.TextBoxTitleClientId.length > 0) {

            var $textBoxHumanReadableUrl = $('#' + window.TextBoxHumanReadableUrlClientId);
            var $textBoxTitle = $('#' + window.TextBoxTitleClientId);

            if ($textBoxHumanReadableUrl.length > 0 &&
                $textBoxTitle.length > 0 &&
                $textBoxHumanReadableUrl.parent() &&
                $textBoxTitle.parent() &&
                $textBoxHumanReadableUrl.parent().parent() &&
                $textBoxTitle.parent().parent()) {
                var $textBoxHumanReadableUrlPanel = $('#' + window.TextBoxHumanReadableUrlClientId).parent().parent();
                var $textBoxTitlePanel = $('#' + window.TextBoxTitleClientId).parent().parent();

                $textBoxHumanReadableUrlPanel.detach().insertAfter($textBoxTitlePanel);

                if (window.AutoGenerateArticleParLinks) {
                    $('#' + window.TextBoxTitleClientId).on('blur', function (e) {
                        var title = $('#' + window.TextBoxTitleClientId).val();
                        var parLink = generateArticleParLink(title);
                        $('#' + window.TextBoxHumanReadableUrlClientId).val(parLink);
                    });
                }
            }
        }

    });

    function Validate(evt) {
        console.log('Validate evt');
        var $group = $(this).closest('.validationGroup');
        var isValid = true;
        $group.find(':input').each(function (i, item) {
            if (!$(item).valid()) {
                isValid = false;
                $(item).closest('.form-group').addClass('has-error');
            } else {
                $(item).closest('.form-group').removeClass('has-error');
            }
        });
        if (!isValid)
            evt.preventDefault();
        console.log(isValid);
        return isValid;
    }

    if (typeof window.FileReader === 'undefined') {
        $('.imageFileUploadStatus').html('File API MISSING!!');
        console.log('FileReader === undefined');
    }



    /*
    *
    *    ********** SCAFFOLD *************
    *
    */

    SUGGEST_ENTRIES_IN_DROPDOWN_LIMIT = 30;

    function populateForeignKeyOptions(virtualTableId, v, whenSelected) {
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
                            console.log("autocomplete: " + ui.item.id);
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
                        console.log("select changed to: " + $(this).val() + "/" + $(this).children("option").filter(":selected").text());
                        var ui = {
                            item: {
                                id: $(this).val(),
                                label: $(this).children("option").filter(":selected").text()
                            }
                        };
                        console.log("select changed to: " + ui.item.id + "/" + ui.item.label);
                        return whenSelected(event, ui);
                    });
                }
            },
            error: handleAjaxError
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
            console.log(relationId);
            console.log(pk);
            console.log(virtualTableId);
            console.log("foreignKeyColumnName;" + foreignKeyColumnName);
            console.log($me.data());
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
                    error: handleAjaxError
                });
            }
        });
    }


    function endToManyModal(message) {
        console.log(message);
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

        console.log("pk:" + primaryKey);
        console.log("relationId:" + relationId);
        console.log("forceFkValue:" + forceFkValue);
        console.log("forceFkColumn:" + forceFkColumn);
        console.log("virtualTableId:" + virtualTableId);

        var me = $(this);

        console.log("ItemEditor bind to virtualTableId: " + virtualTableId);

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
                    console.log(v.BackendType);
                    if (v.BackendType == "Display") {
                        $(v.InputId).append('<input type="text" class="form-control" readonly="true" />');
                        $(v.InputId + " input").val(v.Value);
                    }
                    else if (v.BackendType == "OneToMany") {
                        if (v.FQName == forceFkColumn) {
                            console.log("Got forced forceFkColumn");
                            $(v.InputId).append('<input type="number" class="form-control digits" readonly="true" />');
                            $(v.InputId + " input").val(forceFkValue);
                        } else {
                            console.log("OneToMany populateForeignKeyOptions NOT IMPLEMENTED YET. " + v.FQName);
                            /* populateForeignKeyOptions(virtualTableId, v, function (event, ui) {
                                console.log("OneToMany selected value:" + ui.item.id);
                                $(v.InputId).val(ui.item.id);
                                return true;
                            });*/
                        }
                    } else if (v.BackendType == "ManyToMany") {
                        console.log("ManyToMany populateForeignKeyOptions NOT IMPLEMENTED YET. " + v.FQName);
                        /*
                        populateForeignKeyOptions(virtualTableId, v, function (event, ui) {
                            console.log(ui.item);
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
                        console.log(v.Value);
                        if (v.Value === "True") {
                            console.log(v.Value);
                            $(v.InputId + " input").prop('checked', true);
                        }
                    } else if (v.BackendType == "Integer") {
                        $(v.InputId).append('<input class="form-control digits required" maxlength="11" type="number" />');
                        $(v.InputId + " input").val(v.Value);
                    } else {
                        console.log("m" + v.InputId + " " + v.Value + " (" + v.BackendType + ")");
                        $(v.InputId).append('<input type="text" class="form-control" />');
                        $(v.InputId + " input").val(v.Value);
                    }
                });
            },
            error: handleAjaxError
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
                        console.log(data);
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
        console.log("Save item called with: ");
        console.log($(this).data());
        var json = {
            Columns: []/*, __type: "DTOItem" */
        };
        console.log(pk);
        if (pk == undefined)
            pk = 0;

        if (relationId > 0 && virtualTableId > 0 && forceFkColumn !== undefined && forceFkColumn.length > 0) {
            var columnFk = {};
            columnFk['FQName'] = forceFkColumn;
            columnFk['Value'] = forceFkValue;
            json.Columns.push(columnFk);
        }
        else {
            console.log("#to-many-modal was called with incorrect parameteres");
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
                    console.log($v.attr('id') + " > option");
                    column['Value'] = "";
                    var items = $($v.attr('id') + " > option").map(function () {
                        column['Value'] += $(this).val() + ",";
                    });
                } else if (backendType == "Checkbox") {
                    column['Value'] = $input.prop('checked');
                    console.log("Checkbox");
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
                    console.log(data);
                }
            },
            error: handleAjaxError
        });
    });

    // Copies a string to the clipboard. Must be called from within an 
    // event handler such as click. May return false if it failed, but
    // this is not always possible. Browser support for Chrome 43+, 
    // Firefox 42+, Safari 10+, Edge and IE 10+.
    // IE: The clipboard feature may be disabled by an administrator. By
    // default a prompt is shown the first time the clipboard is 
    // used (per session).
    function copyToClipboard(text) {
        if (window.clipboardData && window.clipboardData.setData) {
            // IE specific code path to prevent textarea being shown while dialog is visible.
            return clipboardData.setData("Text", text);

        } else if (document.queryCommandSupported && document.queryCommandSupported("copy")) {
            var textarea = document.createElement("textarea");
            textarea.textContent = text;
            textarea.style.position = "fixed";  // Prevent scrolling to bottom of page in MS Edge.
            document.body.appendChild(textarea);
            textarea.select();
            try {
                return document.execCommand("copy");  // Security exception may be thrown by some browsers.
            } catch (ex) {
                console.warn("Copy to clipboard failed.", ex);
                return false;
            } finally {
                document.body.removeChild(textarea);
            }
        }
    }