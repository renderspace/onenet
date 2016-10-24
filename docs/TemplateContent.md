The Template content module allows you to add templatable html content to be edited on pages in structure and rendered as structured html in frontend.
So, for example, you can have a Template content module for content to be added via 5 separate text fields, and 3 separate text areas in structure.aspx and then rendered on the frontend in custom html.

To defined a ContentTemplate you need to do the following:

1. Go to adm/Website.aspx and create a new template by entering a name in the text field and clicking Add Template button.
2. Find the newly added template in the grid below once the page refreshes and click the Edit button.
3. From the type dropdown select ContentTemplate
4. In the content you enter your html with placeholders for fields specified within curly braces { }

An example of a textfield placeholder is 
{CompanyTitle,singleline}

An example of a textarea placeholder is
{MCDescription,html}

And example of a file placeholder is
{AnExample, file}

The ContentTemplate also supports embedding the current page uri by using the following placeholder:
{currenturi,builtin}

Once done, click the Save template button to save your template. Note down your template ID for use in structure.aspx.

5. Go to structure.aspx, select a page where you want your Template content module to be rendered on.
6. Add Template content module to page.
7. Under Advanced settings of the Template content module, enter the Template Id you noted down in point 4 above.