using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkdownToHtmlConverter
{
    public ref struct DetectedMarkdownElement
    {
        public int ElementType
        {
            get; init;

        }
        public int IdentifierLength { get; init; }
        public int ContentStartIndex { get; init; }
        public int ElementEndIndex { get; init; }
        public int ContentLength { get; init; }
        
    }

    
}
