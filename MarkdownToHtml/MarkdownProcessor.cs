using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Microsoft.Web.WebView2.WinForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MarkdownToHtml
{
    public class MarkdownProcessor
    {
        readonly string _markdownText = "---\r\n\r\n# h1 Heading 8-)\r\n## h2 Heading\r\n### h3 Heading\r\n#### h4 Heading\r\n##### h5 Heading\r\n###### h6 Heading\r\n\r\n\r\n## Horizontal Rules\r\n\r\n___\r\n\r\n---\r\n\r\n***\r\n\r\n\r\n## Typographic replacements\r\n\r\nEnaboidaoidale typographer option to see result.\r\n\r\n(c) (C) (r) (R) (tm) (TM) (p) (P) +-\r\n\r\ntest.. test... test..... test?..... test!....\r\n\r\n!!!!!! ???? ,,  -- ---\r\n\r\n\"Smartypants, double quotes\" and 'single quotes'\r\n\r\n\r\n## Emphasis\r\n\r\n**This is bold text**\r\n\r\n__This is bold text__\r\n\r\n*This is italic text*\r\n\r\n_This is italic text_\r\n\r\n~~Strikethrough~~\r\n\r\n\r\n## Blockquotes\r\n\r\n\r\n> Blockquotes can also be nested...\r\n>> ...by using additional greater-than signs right next to each other...\r\n> > > ...or with spaces between arrows.\r\n\r\n\r\n## Lists\r\n\r\nUnordered\r\n\r\n+ Create a list by starting a line with `+`, `-`, or `*`\r\n+ Sub-lists are made by indenting 2 spaces:\r\n  - Marker character change forces new list start:\r\n    * Ac tristique libero volutpat at\r\n    + Facilisis in pretium nisl aliquet\r\n    - Nulla volutpat aliquam velit\r\n+ Very easy!\r\n\r\nOrdered\r\n\r\n1. Lorem ipsum dolor sit amet\r\n2. Consectetur adipiscing elit\r\n3. Integer molestie lorem at massa\r\n\r\n\r\n1. You can use sequential numbers...\r\n1. ...or keep all the numbers as `1.`\r\n\r\nStart numbering with offset:\r\n\r\n57. foo\r\n1. bar\r\n\r\n\r\n## Code\r\n\r\nInline `code`\r\n\r\nIndented code\r\n\r\n    // Some comments\r\n    line 1 of code\r\n    line 2 of code\r\n    line 3 of code\r\n\r\n\r\nBlock code \"fences\"\r\n\r\n```\r\nSample text here...\r\n```\r\n\r\nSyntax highlighting\r\n\r\n``` js\r\nvar foo = function (bar) {\r\n  return bar++;\r\n};\r\n\r\nconsole.log(foo(5));\r\n```\r\n\r\n## Tables\r\n\r\n| Option | Description |\r\n| ------ | ----------- |\r\n| data   | path to data files to supply the data that will be passed into templates. |\r\n| engine | engine to be used for processing templates. Handlebars is the default. |\r\n| ext    | extension to be used for dest files. |\r\n\r\nRight aligned columns\r\n\r\n| Option | Description |\r\n| ------:| -----------:|\r\n| data   | path to data files to supply the data that will be passed into templates. |\r\n| engine | engine to be used for processing templates. Handlebars is the default. |\r\n| ext    | extension to be used for dest files. |\r\n\r\n\r\n## Links\r\n\r\n[link text](http://dev.nodeca.com)\r\n\r\n[link with title](http://nodeca.github.io/pica/demo/ \"title text!\")\r\n\r\nAutoconverted link https://github.com/nodeca/pica (enable linkify to see)\r\n\r\n\r\n## Images\r\n\r\n![Minion](https://octodex.github.com/images/minion.png)\r\n![Stormtroopocat](https://octodex.github.com/images/stormtroopocat.jpg \"The Stormtroopocat\")\r\n\r\nLike links, Images also have a footnote style syntax\r\n\r\n![Alt text][id]\r\n\r\nWith a reference later in the document defining the URL location:\r\n\r\n[id]: https://octodex.github.com/images/dojocat.jpg  \"The Dojocat\"\r\n\r\n\r\n## Plugins\r\n\r\nThe killer feature of `markdown-it` is very effective support of\r\n[syntax plugins](https://www.npmjs.org/browse/keyword/markdown-it-plugin).\r\n\r\n\r\n### [Emojies](https://github.com/markdown-it/markdown-it-emoji)\r\n\r\n> Classic markup: :wink: :cry: :laughing: :yum:\r\n>\r\n> Shortcuts (emoticons): :-) :-( 8-) ;)\r\n\r\nsee [how to change output](https://github.com/markdown-it/markdown-it-emoji#change-output) with twemoji.\r\n\r\n\r\n### [Subscript](https://github.com/markdown-it/markdown-it-sub) / [Superscript](https://github.com/markdown-it/markdown-it-sup)\r\n\r\n- 19^th^\r\n- H~2~O\r\n\r\n\r\n### [\\<ins>](https://github.com/markdown-it/markdown-it-ins)\r\n\r\n++Inserted text++\r\n\r\n\r\n### [\\<mark>](https://github.com/markdown-it/markdown-it-mark)\r\n\r\n==Marked text==\r\n\r\n\r\n### [Footnotes](https://github.com/markdown-it/markdown-it-footnote)\r\n\r\nFootnote 1 link[^first].\r\n\r\nFootnote 2 link[^second].\r\n\r\nInline footnote^[Text of inline footnote] definition.\r\n\r\nDuplicated footnote reference[^second].\r\n\r\n[^first]: Footnote **can have markup**\r\n\r\n    and multiple paragraphs.\r\n\r\n[^second]: Footnote text.\r\n\r\n\r\n### [Definition lists](https://github.com/markdown-it/markdown-it-deflist)\r\n\r\nTerm 1\r\n\r\n:   Definition 1\r\nwith lazy continuation.\r\n\r\nTerm 2 with *inline markup*\r\n\r\n:   Definition 2\r\n\r\n        { some code, part of Definition 2 }\r\n\r\n    Third paragraph of definition 2.\r\n\r\n_Compact style:_\r\n\r\nTerm 1\r\n  ~ Definition 1\r\n\r\nTerm 2\r\n  ~ Definition 2a\r\n  ~ Definition 2b\r\n\r\n\r\n### [Abbreviations](https://github.com/markdown-it/markdown-it-abbr)\r\n\r\nThis is HTML abbreviation example.\r\n\r\nIt converts \"HTML\", but keep intact partial entries like \"xxxHTMLyyy\" and so on.\r\n\r\n*[HTML]: Hyper Text Markup Language\r\n\r\n### [Custom containers](https://github.com/markdown-it/markdown-it-container)\r\n\r\n::: warning\r\n*here be dragons*\r\n:::";

        readonly MarkdownPipeline _pipeline;

        public MarkdownProcessor()
        {
            _pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .UseAutoIdentifiers(AutoIdentifierOptions.GitHub) // generates ids for headings
                .Build();
        }

        /// <summary>
        /// Convert markdown to HTML (headings will have id attributes).
        /// </summary>
        public string ConvertToHtml(string markdown)
        {
            if (markdown is null) throw new ArgumentNullException(nameof(markdown));
            // Optionally wrap in a container to ensure predictable layout/scroll anchors
            var htmlBody = Markdown.ToHtml(markdown, _pipeline);
            var wrapper = new StringBuilder();
            wrapper.AppendLine("<!doctype html><html><head><meta charset=\"utf-8\"></head><body>");
            wrapper.AppendLine(htmlBody);
            wrapper.AppendLine("</body></html>");
            return wrapper.ToString();
        }



        // Basic GitHub-like slugify (lowercase, remove/replace unsupported chars).
        // Keeps it deterministic and safe for id attributes.
        static string Slugify(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "heading";
            text = text.ToLowerInvariant().Trim();
            var sb = new StringBuilder();
            bool lastDash = false;
            foreach (var ch in text)
            {
                if ((ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9'))
                {
                    sb.Append(ch);
                    lastDash = false;
                }
                else if (char.IsWhiteSpace(ch) || ch == '-' || ch == '_')
                {
                    if (!lastDash)
                    {
                        sb.Append('-');
                        lastDash = true;
                    }
                }
                // skip punctuation
            }
            var result = sb.ToString().Trim('-');
            return string.IsNullOrEmpty(result) ? "heading" : result;
        }

        /// <summary>
        /// Parse headings and return a hierarchical tree of HeaderNode.
        /// Uses the same AutoIdentifier logic where possible; falls back to slugify.
        /// </summary>
        public IReadOnlyList<HeaderNode> GetHeaderTree(string markdown)
        {
            if (markdown is null) throw new ArgumentNullException(nameof(markdown));
            var document = Markdown.Parse(markdown, _pipeline);
            var headings = document.Descendants<HeadingBlock>().ToList();

            // Convert to flat nodes preserving order
            var flat = headings.Select(h =>
            {
                var text = ExtractInlineText(h.Inline);
                // AutoIdentifiers sets attribute on heading; try to read it
                var id = h.GetAttributes()?.Id;
                if (string.IsNullOrEmpty(id))
                    id = Slugify(text);
                return new HeaderNode { Text = text, Level = h.Level, Id = id };
            }).ToList();

            // Build hierarchy (stack-based)
            var roots = new List<HeaderNode>();
            var stack = new Stack<HeaderNode>();

            foreach (var node in flat)
            {
                while (stack.Count > 0 && stack.Peek().Level >= node.Level)
                    stack.Pop();

                if (stack.Count == 0)
                    roots.Add(node);
                else
                    stack.Peek().Children.Add(node);

                stack.Push(node);
            }

            return roots;
        }

        static string ExtractInlineText(ContainerInline inline)
        {
            if (inline == null) return string.Empty;
            var sb = new StringBuilder();
            foreach (var child in inline)
            {
                switch (child)
                {
                    case LiteralInline li:
                        sb.Append(li.Content.Text.Substring(li.Content.Start, li.Content.Length));
                        break;
                    case CodeInline ci:
                        sb.Append(ci.Content);
                        break;
                    case LineBreakInline _:
                        sb.Append(' ');
                        break;
                    case EmphasisInline ei:
                        sb.Append(ExtractInlineText(ei));
                        break;
                    case LinkInline li:
                        // show link text, not url
                        sb.Append(ExtractInlineText(li));
                        break;
                    default:
                        if (child is ContainerInline ciu)
                            sb.Append(ExtractInlineText(ciu));
                        break;
                }
            }
            return sb.ToString().Trim();
        }


        private static TreeNode CreateTreeNode(HeaderNode hn)
        {
            var tn = new TreeNode(hn.Text) { Tag = hn };
            foreach (var c in hn.Children)
                tn.Nodes.Add(CreateTreeNode(c));
            return tn;
        }
    }
}
