<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AdminWikiMenu.ascx.cs" Inherits="OneMainWeb.Controls.AdminWikiMenu" %>
<%@ Register Src="~/AdminControls/Notifier.ascx" TagName="Notifier" TagPrefix="uc1" %>

<script src="/ckeditor4/ckeditor.js" type="text/javascript" ></script>
<script>

    // This code is generally not necessary, but it is here to demonstrate
    // how to customize specific editor instances on the fly. This fits well
    // this demo because we have editable elements (like headers) that
    // require less features.

    // var editor = CKEDITOR.inline(document.getElementById('editable'));

    // The "instanceCreated" event is fired for every editor instance created.
    /*
    CKEDITOR.on('instanceCreated', function (event) {
        var editor = event.editor,
				element = editor.element;

        // Customize editors for headers and tag list.
        // These editors don't need features like smileys, templates, iframes etc.
        if (element.is('h1', 'h2', 'h3') || element.getAttribute('id') == 'taglist') {
            // Customize the editor configurations on "configLoaded" event,
            // which is fired after the configuration file loading and
            // execution. This makes it possible to change the
            // configurations before the editor initialization takes place.
            editor.on('configLoaded', function () {

                // Remove unnecessary plugins to make the editor simpler.
                editor.config.removePlugins = 'colorbutton,find,flash,font,' +
						'forms,iframe,image,newpage,removeformat,' +
						'smiley,specialchar,stylescombo,templates';

                // Rearrange the layout of the toolbar.
                editor.config.toolbarGroups = [
						{ name: 'editing', groups: ['basicstyles', 'links'] },
						{ name: 'undo' },
						{ name: 'clipboard', groups: ['selection', 'clipboard'] },
						{ name: 'about' }
					];
            });
        }
    });*/

</script>

<script type="text/javascript">

    var savedData, newData;
    function saveCkeditorData(instance, moduleInstanceId, pageIdd) {
        newData = instance.getData();
        if (newData !== savedData) {
            savedData = newData;
            console.log('saveCkeditorData new data on mi: ' + moduleInstanceId);

            $.ajax({
                type: "POST",
                url: "/adm/ManagementWebService.asmx/ChangeTextContent",
                data: "{ pageId: " + pageIdd + ", moduleInstanceId: " + moduleInstanceId + ", html:" + JSON.stringify(newData) + " }",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("Content-type",
														 "application/json; charset=utf-8");
                },
                dataType: "json",
                success: function (msg) {
                    if (msg.d === true) {
                        var a = $("#mie_" + moduleInstanceId + " .savedMessage").show().fadeOut(2000);
                        // location.reload(true);
                    }
                    else {
                        alert('saveCkeditorData error (server complains)');
                    }
                },
                error: function (e) {
                    alert('Couldn\'t save text content');
                }
            });
        }
    };

    CKEDITOR.plugins.add('ajaxsave',
    {
        init: function (editor) {
            var pluginName = 'ajaxsave';
            editor.addCommand(pluginName,
            {
                exec: function (editor) {
                    //       alert('Ajaxsave1');
                },
                canUndo: true
            });
            editor.ui.addButton('Ajaxsave',
            {
                label: 'Save Ajax',
                command: pluginName,
                className: 'cke_button_save'
            });
        }
    });

    $(document).ready(function () {
        var pageIdRaw = $("form").attr("class");
        var regex2 = new RegExp("mi(\\d+)");
        var regex3 = new RegExp("page(\\d+)");
        var pageIdd = pageIdRaw.match(regex3)[1];

        $('#editPage').click(function () {
            $('#menubarwrapper').hide();
            $('#modulesListWrapper').show();
            $('form .mi').addClass('editing');

            $('form .mi').attr("contenteditable", true);


            // INSTANCE EDITING BUTTONS (.miEdit)
            $('form .mi').each(function (index) {
                var moduleInstanceId = $(this).attr('class').match(regex2)[1];
                var deleteButton = $('<a class="button delete">Delete</a>').click(function (e) {
                    var answer = confirm('Are you sure?');
                    if (answer) {
                        $.ajax({
                            type: "POST",
                            url: "/adm/ManagementWebService.asmx/DeleteModulesInstance",
                            data: "{ pageId: " + pageIdd + ", moduleInstanceId: " + moduleInstanceId + " }",
                            beforeSend: function (xhr) {
                                xhr.setRequestHeader("Content-type",
														 "application/json; charset=utf-8");
                            },
                            dataType: "json",
                            success: function (msg) {
                                if (msg.d === true) {
                                    location.reload(true);
                                }
                                else {
                                    alert('Couldn\'t delete module instance');
                                }
                            },
                            error: function (e) {
                                alert('DeleteModulesInstance error');
                            }
                        });
                    }
                    e.preventDefault();
                });

                var selector = '<div id=\'mie_' + moduleInstanceId + '\' class=\'miEdit\'><div><span class="savedMessage" style="display:none;">Saved.</span></div></div>';
                $(this).before($(selector));
                $(this).prev().children('div:first').append(deleteButton);
            });


            $('form .textcontent').each(function (index) {
                var moduleInstanceId = $(this).attr('class').match(regex2)[1];
                var editor = CKEDITOR.inline(document.getElementById('mi_' + moduleInstanceId).firstChild, {
                    uiColor: '#14B8C4',
                    extraPlugins: 'ajaxsave',
                    toolbar:
		                [
			                { name: 'basic', items: ['Maximize', 'ShowBlocks'] },
                            { name: 'clip', items: ['Cut', 'Copy', 'Paste'] },
                            { name: 'styles', items: ['Bold', 'Italic', 'NumberedList', 'BulletedList'] },
                            { name: 'links', items: ['Link', 'Unlink', 'Anchor'] },
                            { name: 'objects', items: ['Image', 'Table', 'HorizontalRule'] },
                            '/',
			                { name: 'templates', items: ['Templates', 'CreateDiv', 'Blockquote'] },
			                { name: 'justification', items: ['JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock', 'Styles', 'Format', 'RemoveFormat'] },
                            { name: 'about', items: ['About', 'Save', 'Ajaxsave'] },
		                ],
                    filebrowserBrowseUrl: '/ckfinder/ckfinder.html',
                    filebrowserWindowWidth: '830',
                    filebrowserWindowHeight: '600',
                    filebrowserImageBrowseLinkUrl: '/ckfinder/ckfinder.html?type=Images',
                    filebrowserImageWindowWidth: '830',
                    filebrowserImageWindowHeight: '600'
                });

                editor.on('blur', function (e) {
                    saveCkeditorData(editor, moduleInstanceId, pageIdd);
                });

                editor.on('beforeCommandExec', function (event) {
                    if (event.data.name === 'ajaxsave') {
                        event.cancel();
                        saveCkeditorData(editor, moduleInstanceId, pageIdd);
                    }
                });
            });



            // EDIT TEXT CONTENT BUTTON
            $('form .textcontent').each(function (index) {
                var moduleInstanceId = $(this).attr('class').match(regex2)[1];
                var editTextContentButton = $('<a class="button">Edit</a>').click(function (e) {
                    /*  $('#editTextContentModal').lightbox_me({
                    centered: true,
                    onLoad: function () {
                    $('#editTextContentModal').find('input:first').focus();
                    $('#editTextContentModal #LinkButtonSaveTextContent').hide();
                    $.ajax({
                    type: "POST",
                    url: "/adm/ManagementWebService.asmx/GetTextContent",
                    data: "{ pageId: " + pageIdd + ", moduleInstanceId: " + moduleInstanceId + " }",
                    beforeSend: function (xhr) {
                    xhr.setRequestHeader("Content-type",
                    "application/json; charset=utf-8");
                    },
                    dataType: "json",
                    success: function (msg) {
                    $("#HiddenFieldTextContentInstanceId").val(moduleInstanceId);
                    $(".j_control_title").val(msg.d.Title);
                    $(".j_control_subtitle").val(msg.d.SubTitle);
                    $(".j_control_teaser").text(msg.d.Teaser);
                    $(".j_control_html").text(msg.d.Html);
                    $('#editTextContentModal #LinkButtonSaveTextContent').show();
                    },
                    error: function (e) {
                    alert('GetTextContent error');
                    }
                    });
                    }
                    });
                    $('#editTextContentModal').appendTo("#aspnetForm");
                    e.preventDefault();*/
                }
					);
                $(this).children('.miEdit').append(editTextContentButton);
            });

            // DROP SOURCE (ADD MODULE INSTANCE)
            $('form .PlaceHolder').each(function (index) {
                $(this).append('<div id=\'phd_' + this.id + '\' class=\'placeHolderDrop\'>Drop new module instance here.</div>');
            });
            // DROP TARGET (ADD MODULE INSTANCE)
            $('form .PlaceHolder .placeHolderDrop').droppable({
                accept: ".ddModule",
                drop: function (e, ui) {
                    var str_sub = this.id.substr(this.id.lastIndexOf("_") + 1);
                    var draggableIdRaw = ui.draggable.attr("id");
                    var draggableId = draggableIdRaw.substr(draggableIdRaw.lastIndexOf("_") + 1);

                    $.ajax({
                        type: "POST",
                        url: "/adm/ManagementWebService.asmx/AddModulesInstance",
                        data: "{ pageId: " + pageIdd + ", moduleId: " + draggableId +
							", placeholderName: \"" + str_sub + "\" }",
                        beforeSend: function (xhr) {
                            xhr.setRequestHeader("Content-type",
												 "application/json; charset=utf-8");
                        },
                        dataType: "json",
                        success: function (msg) {
                            location.reload(true);
                            //$(divToBeWorkedOn).html(msg.d);
                        },
                        error: function (e) {
                            alert('AddModulesInstance error');
                        }
                    });
                }
            });
            // DRAG SOURCE  (ADD MODULE INSTANCE)
            $("#modulesListWrapper li").draggable({
                drag: function (event, ui) { },
                revert: 'active'/*,
					helper: function(event) {
					   return $('<div>' + $(this).children("small").text() + '</div>');
					}*/
            });
        });

        $('#footer_link').click(function () { alert('test') });

        $('#newPage').click(function (e) {
            $('#newPageModal').lightbox_me({
                centered: true,
                onLoad: function () {
                    $('#newPageModal').find('input:first').focus()
                }
            });
            $('#newPageModal').appendTo("#aspnetForm");
            e.preventDefault();
        });
    });


</script>
<link rel="stylesheet" href="/adm/css/adminWikiMenu.css" />


<div id="oneNetPreview">
    <asp:Literal runat="server" ID="LiteralInfo" EnableViewState="false"></asp:Literal>
    <uc1:Notifier ID="Notifier1" runat="server" />

    <asp:LoginView ID="LoginView1" runat="server">
        <AnonymousTemplate>
            <div id="loginwrapper"><asp:LoginStatus ID="LoginStatus1" runat="server" /></div>
        </AnonymousTemplate>
        <LoggedInTemplate>
            <h3>No rights for editing</h3>
			<div id="loginwrapper"><asp:LoginStatus ID="LoginStatus1" runat="server" /></div>
        </LoggedInTemplate>
        <RoleGroups>
                <asp:RoleGroup Roles="admin">
                        <ContentTemplate>
                            <div id="menubarwrapper">
	                            <ul class="first">
		                            <li class="l1"><a id="newPage" class="button">New page</a></li>
                                    <li class="l2"><asp:LinkButton ID="LinkButtonDelete" runat="server" onclick="LinkButtonDelete_Click" OnClientClick="return confirm('Are you sure?')">
                                        Delete page</asp:LinkButton></li>
                                    <li class="l3"><a id="editPage" class="button">Edit page</a></li>
								</ul>
								<ul class="mid">
                                    <li><asp:LinkButton ID="LinkButtonClearCache" runat="server" onclick="LinkButtonClearCache_Click" CssClass="button">Clear cache</asp:LinkButton></li>
								</ul>
								<ul class="last">
                                    <li><asp:LinkButton ID="LinkButtonLogout" runat="server" onclick="LinkButtonLogout_Click">Logout</asp:LinkButton></li>
	                            </ul>
                            </div>

                            <div id="modulesListWrapper">
                                <p>Drag and drop module to page:</p>
                                <asp:Repeater ID="RepeaterModules" runat="server">
                                    <HeaderTemplate><ul></HeaderTemplate>
                                    <FooterTemplate></ul></FooterTemplate>
                                    <ItemTemplate><li id='moduleId_<%# Eval("Id") %>' class='ddModule'><%# Eval("Name")%></li></ItemTemplate>
                                </asp:Repeater>
                            </div>

                            <div id="newPageModal">
	                            <h1>New page</h1>
                                <asp:Label ID="LabelPageTitle" runat="server" Text="Name of the new page" AssociatedControlID="TextBoxPageTitle"></asp:Label>
                                <asp:TextBox ID="TextBoxPageTitle" runat="server"></asp:TextBox>
                                <asp:LinkButton ID="LinkButtonNewPage" runat="server" onclick="LinkButtonNewPage_Click" CssClass="button">Create</asp:LinkButton>
                            </div>

                        </ContentTemplate>
                </asp:RoleGroup>
        </RoleGroups>
    </asp:LoginView>    
</div>