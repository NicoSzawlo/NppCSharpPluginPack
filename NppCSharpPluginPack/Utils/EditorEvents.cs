using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NppDemo.Utils
{
    public static class EditorEvents
    {
        public static event Action EditorTextChanged;

        public static void RaiseEditorTextChanged()
            => EditorTextChanged?.Invoke();
    }
}
