﻿using System;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using WC = System.Web.UI.WebControls;
using TwoControlsLibrary;

using System.Web.Profile;
using System.Globalization;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

namespace One.Net.BLL.Model.Web
{
    public class AdminHelper
    {
        public static void TranslateControls(ControlCollection controls, string localizationFile)
        {
            string translationLabel = "$";

            foreach (Control ctrl in controls)
            {
                if (ctrl is WC.DropDownList)
                {
                    WC.DropDownList ddlCtrl = ctrl as WC.DropDownList;
                    if (ddlCtrl.Items.Count > 0)
                    {
                        foreach (WC.ListItem item in ddlCtrl.Items)
                        {
                            if (item.Text.StartsWith(translationLabel))
                                item.Text = ResourceManager.GetString(item.Text, localizationFile);
                        }
                    }
                }
                if (ctrl is HtmlTitle)
                {
                    HtmlTitle htmlTitle = ctrl as HtmlTitle;
                    if (htmlTitle.Text.StartsWith(translationLabel))
                        htmlTitle.Text = ResourceManager.GetString(htmlTitle.Text, localizationFile);
                }
                else if (ctrl is LiteralControl)
                {
                    LiteralControl literalCtrl = ctrl as LiteralControl;
                    if (literalCtrl.Text != null && literalCtrl.Text.StartsWith(translationLabel))
                        literalCtrl.Text = ResourceManager.GetString(literalCtrl.Text, localizationFile);
                }
                else if (ctrl is WC.LinkButton)
                {
                    WC.LinkButton button = ctrl as WC.LinkButton;
                    if (button.Text.StartsWith(translationLabel))
                        button.Text = ResourceManager.GetString(button.Text, localizationFile);
                }
                else if (ctrl is WC.Button)
                {
                    WC.Button button = ctrl as WC.Button;
                    if (button.Text.StartsWith(translationLabel))
                        button.Text = ResourceManager.GetString(button.Text, localizationFile);
                }
                else if (ctrl is WC.CheckBox)
                {
                    WC.CheckBox button = ctrl as WC.CheckBox;
                    if (button.Text.StartsWith(translationLabel))
                        button.Text = ResourceManager.GetString(button.Text, localizationFile);
                }
                else if (ctrl is WC.Label)
                {
                    WC.Label labelCtrl = ctrl as WC.Label;
                    if (labelCtrl.Text.StartsWith(translationLabel))
                        labelCtrl.Text = ResourceManager.GetString(labelCtrl.Text, localizationFile);
                }
                else if (ctrl is WC.Literal)
                {
                    WC.Literal literalCtrl = ctrl as WC.Literal;
                    if (literalCtrl.Text.StartsWith(translationLabel))
                        literalCtrl.Text = ResourceManager.GetString(literalCtrl.Text, localizationFile);
                }
                else if (ctrl is HtmlAnchor)
                {
                    HtmlAnchor aCtrl = ctrl as HtmlAnchor;
                    if (aCtrl.InnerText.StartsWith(translationLabel))
                        aCtrl.InnerText = ResourceManager.GetString(aCtrl.InnerText, localizationFile);
                }
                else if (ctrl is WC.TableCell)
                {
                    WC.TableCell tableCell = ctrl as WC.TableCell;
                    if (tableCell.Text.StartsWith(translationLabel))
                        tableCell.Text = ResourceManager.GetString(tableCell.Text, localizationFile);
                }
                else if (ctrl is WC.Menu)
                {
                    WC.Menu menu = ctrl as WC.Menu;
                    foreach (WC.MenuItem it in menu.Items)
                    {
                        if (it.Text.StartsWith(translationLabel))
                            it.Text = ResourceManager.GetString(it.Text, localizationFile);
                    }
                }
                else if (ctrl is TwoControlsLibrary.TabularMultiView)
                {
                    TwoControlsLibrary.TabularMultiView mv = ctrl as TwoControlsLibrary.TabularMultiView;
                    foreach (TwoControlsLibrary.TabularView tv in mv.Views)
                    {
                        if (tv.TabName.StartsWith(translationLabel))
                            tv.TabName = ResourceManager.GetString(tv.TabName, localizationFile);
                    }
                }
                else if (ctrl is ModalPanel)
                {
                    var p = ctrl as ModalPanel;
                    // TODO
                }
                else if (ctrl is DateEntry)
                {
                    DateEntry c = ctrl as DateEntry;
                    if (c.Text.StartsWith(translationLabel))
                        c.Text = ResourceManager.GetString(c.Text, localizationFile);
                    if (c.FormatMessage.StartsWith(translationLabel))
                        c.FormatMessage = ResourceManager.GetString(c.Text, localizationFile);
                    if (c.RequiredMessage.StartsWith(translationLabel))
                        c.RequiredMessage = ResourceManager.GetString(c.Text, localizationFile);
                }
                else if (ctrl is TwoControlsLibrary.Input)
                {
                    TwoControlsLibrary.Input c = ctrl as TwoControlsLibrary.Input;
                    if (c.Text.StartsWith(translationLabel))
                        c.Text = ResourceManager.GetString(c.Text, localizationFile);
                    if (c.RequiredMessage.StartsWith(translationLabel))
                        c.RequiredMessage = ResourceManager.GetString(c.RequiredMessage, localizationFile);
                }
                else if (ctrl is TwoControlsLibrary.ValidInput)
                {
                    TwoControlsLibrary.ValidInput c = ctrl as TwoControlsLibrary.ValidInput;
                    if (c.Text.StartsWith(translationLabel))
                        c.Text = ResourceManager.GetString(c.Text, localizationFile);
                    if (c.RequiredMessage.StartsWith(translationLabel))
                        c.RequiredMessage = ResourceManager.GetString(c.RequiredMessage, localizationFile);
                }
                else if (ctrl is TwoControlsLibrary.InfoLabel)
                {
                    TwoControlsLibrary.InfoLabel c = ctrl as TwoControlsLibrary.InfoLabel;
                    if (c.Text.StartsWith(translationLabel))
                        c.Text = ResourceManager.GetString(c.Text, localizationFile);
                }
                else if (ctrl is TwoControlsLibrary.LabeledCheckBox)
                {
                    TwoControlsLibrary.LabeledCheckBox c = ctrl as TwoControlsLibrary.LabeledCheckBox;
                    if (c.Text.StartsWith(translationLabel))
                        c.Text = ResourceManager.GetString(c.Text, localizationFile);
                }
                else if (ctrl is WC.ValidationSummary)
                {
                    WC.ValidationSummary c = ctrl as WC.ValidationSummary;
                    if (c.HeaderText.StartsWith(translationLabel))
                        c.HeaderText = ResourceManager.GetString(c.HeaderText, localizationFile);
                }
                else if (ctrl is HtmlGenericControl)
                {
                    HtmlGenericControl control = ctrl as HtmlGenericControl;
                    try
                    {
                        if (control.InnerText.StartsWith(translationLabel))
                        {
                            switch (control.TagName)
                            {
                                case "h1":
                                case "h2":
                                case "h3":
                                case "h4":
                                case "h5":
                                case "h6":
                                case "legend":
                                case "span":
                                case "label":
                                    { control.InnerText = ResourceManager.GetString(control.InnerText, localizationFile); } break;
                            }
                        }

                    }
                    catch { }
                }
                TranslateControls(ctrl.Controls, localizationFile);
            }
        }
    }
}
