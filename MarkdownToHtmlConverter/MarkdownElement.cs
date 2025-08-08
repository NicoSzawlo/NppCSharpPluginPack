using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkdownToHtmlConverter
{
    public class MarkdownElement
    {
        public enum MarkdownElementType
        {
            None,
            Heading,
            Bold,
            Italic,
            HorizontalRule,
            TypographicReplacements,
            Emphasis,
            Blockquotes,
            Lists,
            Code,
            Tables,
            Links,
            Images,
            Plugins
        }

        public MarkdownElementType ElementType
        {
            get; set;

        }
        
        public int IdentifierLength { get; set; }
        public int EndLength { get; set; }
        public int ContentStartIndex { get; set; }
        public int ElementEndIndex { get; set; }
        public int ContentLength { get; set; }
        public string Content { get; set; }

    }

    
}
