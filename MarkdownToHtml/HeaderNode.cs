using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkdownToHtml
{
    public class HeaderNode
    {
            public string Text { get; set; }
            public int Level { get; set; }    // 1..6
            public string Id { get; set; }    // anchor id
            public List<HeaderNode> Children { get; } = new List<HeaderNode>();
            public override string ToString() => Text;
    }
}
