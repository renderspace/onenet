using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace One.Net.BLL.Utility
{
    public class ValidatorError
    {
        public string Error { get; set; }
        public string Tag { get; set; }
    }

    public static class Validator
    {
        public static List<int> CheckForAmpersand (string html)
        {
            var regex = @"&(?!(\w+|\#\d+);)";

            var finder = new Regex(regex, RegexOptions.IgnoreCase);
            var matches = finder.Matches(html);
            var results = new List<int>();
            foreach (Match m in matches)
            {
                results.Add(m.Index);
            }
            return results;
        }


        /// <summary>
        /// Whether the HTML is likely valid. Error paremeter will be empty
        /// if no errors were found.
        /// </summary>
        public static void CheckHtml(string html, ref List<ValidatorError> error)
        {
            // Store our tags in a stack
            var tags = new Stack<string>();
            var commentTags = new Stack<string>();

            error = new List<ValidatorError>();

            // Traverse entire HTML
            for (int i = 0; i < html.Length; i++)
            {
                bool isClose;
                bool isSolo;
                bool isOpen;
                
                char c = html[i];
                if (c == '<')
                {
                    string attributes;

                    string commentTag = LookForCommentTag(html, i, out isOpen, out isClose);

                    if (isOpen)
                    {
                        if (commentTags.Count > 0)
                        {
                            error.Add(new ValidatorError { Error = "Nested comments not allowed" } );
                            return;
                        }
                        commentTags.Push(commentTag);
                    }

                    i += commentTag.Length;

                    if (commentTags.Count == 0)
                    {
                        // Look ahead at this tag
                        string tag = LookAhead(html, i, out isClose, out isSolo, out attributes);

                        if (tag.ToLower() != tag)
                        {
                            error.Add(new ValidatorError { Error = "Uppercase tags are not allowed", Tag = tag});
                            return;
                        }

                        if (tag.ToLower() == "img")
                        {
                            string alt = StringTool.GetHtmlAttributeValue(attributes, "alt");
                            if (string.IsNullOrEmpty(alt))
                            {
                                error.Add(new ValidatorError { Error = "Missing alt attribute in image"});
                                return;
                            }
                        }

                        if (_soloTags.ContainsKey(tag))
                        {
                            if (!isSolo)
                            {
                                error.Add(new ValidatorError { Error = "Solo tag should be solo.", Tag = tag });
                                return;
                            }
                        }
                        else
                        {
                            if (isClose)
                            {
                                if (tags.Count == 0)
                                {
                                    error.Add(new ValidatorError { Error = "Missing closing of tag", Tag = tag });
                                    return;
                                }
                                // Tag on stack must be equal to this closing tag
                                if (tags.Peek() == tag)
                                {
                                    // Remove the start tag from the stack
                                    tags.Pop();
                                }
                                else
                                {
                                    error.Add(new ValidatorError { Error = "Mismatched closing tag", Tag = tag });
                                    return;
                                }
                            }
                            //else if (isSolo)   in reality, this check should be here, but because we decided to just force tags 
                            //{ }                that can be solo to be solo (like <param>), we have removed this check
                            else
                            {
                                tags.Push(tag);
                            }
                        }
                        i += tag.Length;
                    }
                }
                else if (c == '-')
                {
                    string commentTag = LookForCommentTag(html, i, out isOpen, out isClose);

                    if (isClose)
                    {
                        if (commentTags.Count == 0)
                        {
                            error.Add(new ValidatorError { Error = "Closing comment tag", });
                            return;
                        }
                        else
                        {
                            commentTags.Pop();
                        }
                    }
                }
            }

            foreach (string tagName in tags)
            {
                error.Add(new ValidatorError { Error = "HTML tag not closed ", Tag = tagName });
            }

            foreach (string tagName in commentTags)
            {
                error.Add(new ValidatorError { Error = "HTML comment not closed ", Tag = tagName });
            }
        }

        static private string LookForCommentTag(string html, int start, out bool isOpen, out bool isClose)
        {
            int len = html.Length;
            isOpen = isClose = false;
            if (len >= (start + 4) && html.Substring(start, 4) == "<!--")
            {
                isOpen = true;
                return "<!--";
            }
            else if (len >= (start + 3) && html.Substring(start, 3) == "-->")
            {
                isClose = true;
                return "-->";
            }
            else
                return "";
        }


        /// <summary>
        /// Called at the start of an html tag. We look forward and record information
        /// about our tag. Handles start tags, close tags, and solo tags. 'Collects'
        /// an entire tag.
        /// </summary>
        /// <returns>Tag name.</returns>
        static public string LookAhead(string html, int start, out bool isClose,
            out bool isSolo, out string attributes)
        {
            isClose = false;
            isSolo = false;
            attributes = "";

            StringBuilder tagName = new StringBuilder();

            // Stores the position of the final slash
            int slashPos = -1;

            //
            // Whether we have encountered a space
            //
            bool space = false;

            //
            // Whether we are in a quote
            //
            bool quote = false;

            //
            // Begin scanning the tag
            //
            int i;
            for (i = 0; ; i++)
            {
                // Get the position in main html
                int pos = start + i;

                // Don't go outside the html
                if (pos >= html.Length)
                {
                    return "x";
                }

                // The character we are looking at
                char c = html[pos];

                // See if a space has been encountered
                if (char.IsWhiteSpace(c))
                {
                    space = true;
                }

                // Add to our tag name if none of these are present
                if (space == false &&
                    c != '<' &&
                    c != '>' &&
                    c != '/')
                {
                    tagName.Append(c);
                }

                if (space == true &&
                    c != '<' &&
                    c != '>'/* &&
                    c != '/' */) // why would you remove a / from an attribute?
                {
                    attributes += c;
                }

                // Record position of slash if not inside a quoted area
                if (c == '/' &&
                    quote == false)
                {
                    slashPos = i;
                }

                // End at the > bracket
                if (c == '>')
                {
                    break;
                }

                // Record whether we are in a quoted area
                if (c == '\"')
                {
                    quote = !quote;
                }
            }

            //
            // Determine if this is a solo or closing tag
            //
            if (slashPos != -1)
            {
                //
                // If slash is at the end so this is solo
                //
                if (slashPos + 1 == i)
                {
                    isSolo = true;
                }
                else
                {
                    isClose = true;
                }
            }

            //
            // Return the name of the tag collected
            //
            string name = tagName.ToString();
            if (name.Length == 0)
            {
                return "empty";
            }
            else
            {
                return name;
            }
        }

        /// <summary>
        /// Tags that must be closed in the start
        /// </summary>
        static Dictionary<string, bool> _soloTags = new Dictionary<string, bool>()
        {
            {"img", true},
            {"br", true},
            {"area", true},
            {"hr", true},
            {"param", true} // we are forcing <param> to be solo
        };
    }
}
