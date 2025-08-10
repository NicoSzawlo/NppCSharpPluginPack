using Kbg.NppPluginNET;
using Kbg.NppPluginNET.PluginInfrastructure;
using Markdig;
using Markdig.Syntax;
using MarkdownToHtml;
using MarkdownToHtmlConverter;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using NppDemo.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NppDemo.Forms
{
    public partial class MarkdownRenderForm : FormBase
    {
        private static MarkdownProcessor _markdownProcessor = new MarkdownProcessor();
        private string _displayString;
        private bool _stringInitialized;

        public MarkdownRenderForm() : base(false, true)
        {
            InitializeComponent();
            PluginBase.nppData._scintillaMainHandle = PluginBase.GetCurrentScintilla();
            EditorEvents.EditorTextChanged += OnEditorTextChanged;
            treeViewHeaders.AfterSelect += treeViewHeaders_AfterSelect;
        }
        public void OnEditorTextChanged()
        {
            RenderCurrentMarkdownDocument();
        }


        private void RenderCurrentMarkdownDocument()
        {
            // Get handle to current Scintilla instance
            var editor = new ScintillaGateway(PluginBase.GetCurrentScintilla());

            // Retrieve entire document text
            int length;
            bool success = editor.TryGetLengthAsInt(out length);
            string text = editor.GetText(length + 1); // includes null terminator

            // Process with your MarkdownProcessor instance
            var html = _markdownProcessor.ConvertToHtml(text);

            // Display result in WebView2
            UpdatePreview(html);
        }
        private void treeViewHeaders_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node?.Tag is HeaderNode hn && !string.IsNullOrEmpty(hn.Id))
            {
                var safeId = hn.Id.Replace("'", "\\'");
                var script = $@"
            (function(){{
                var el = document.getElementById('{safeId}');
                if (!el) return false;
                el.scrollIntoView({{behavior:'smooth', block:'start'}});
                var old = el.style.boxShadow;
                el.style.boxShadow = '0 0 0 3px rgba(0,120,215,0.4)';
                setTimeout(function(){{ el.style.boxShadow = old; }}, 900);
                return true;
            }})()
        ";
                _ = webView.CoreWebView2?.ExecuteScriptAsync(script);
            }
        }
        // Renamed for clarity. This method is now meant to be called by your plugin's main code.
        public async Task InitializeWebViewAsync()
        {
            try
            {
                // Get the path to the current plugin's directory.
                string pluginDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string userDataFolder = Path.Combine(pluginDirectory, "WebView2UserData");

                // Ensure the directory exists before creating the environment.
                Directory.CreateDirectory(userDataFolder);

                // Create a CoreWebView2Environment with a specific user data folder.
                CoreWebView2Environment environment = await CoreWebView2Environment.CreateAsync(null, userDataFolder);

                // Now ensure the WebView2 control is initialized with this custom environment.
                await webView.EnsureCoreWebView2Async(environment);

                RenderCurrentMarkdownDocument();

                webView.CoreWebView2.DOMContentLoaded += (s, e) =>
                {
                    System.Diagnostics.Debug.WriteLine("WebView2 content has been loaded.");
                };
            }
            catch (Exception ex)
            {
                // Handle potential initialization failures gracefully.
                MessageBox.Show($"Failed to initialize WebView2: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Log this exception for further debugging if needed.
            }
        }

        /// <summary>
        /// Updates the WebView2 content with the rendered Markdown.
        /// </summary>
        /// <param name="html">The raw markdown text to render.</param>
        public void UpdatePreview(string html)
        {
            // Only update if the core environment is ready.
            if (webView.CoreWebView2 == null)
            {
                return;
            }
            _displayString = html;
                // Navigate to the HTML string.
            webView.CoreWebView2.NavigateToString(_displayString);

            
            System.Diagnostics.Debug.WriteLine($"WebView2 Size: {webView.Size}");
        }

        // Placeholder for fetching the current file content from Notepad++
        private string GetCurrentFileContent()
        {
            Uri uri = new Uri("C:\\Users\\nicos\\OneDrive\\Projekte\\NppMarkdownViewer\\demo.html");
            FileStream stream = new FileStream(uri.LocalPath, FileMode.Open, FileAccess.Read);
            StreamReader reader = new StreamReader(stream);
            string content = reader.ReadToEnd();
            return content;
        }


        private void HeaderTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node?.Tag is HeaderNode hn && !string.IsNullOrEmpty(hn.Id))
            {
                // JS to scroll to element and set a temporary highlight
                var safeId = hn.Id.Replace("'", "\\'");
                var script = $@"
                (function(){{
                    var el = document.getElementById('{safeId}');
                    if (!el) return false;
                    el.scrollIntoView({{behavior:'smooth', block:'start'}});
                    // optional flash
                    var old = el.style.boxShadow;
                    el.style.boxShadow = '0 0 0 3px rgba(0,120,215,0.4)';
                    setTimeout(function(){{ el.style.boxShadow = old; }}, 900);
                    return true;
                }})()
            ";
                // Fire-and-forget async call
                var _ = webView.CoreWebView2?.ExecuteScriptAsync(script);
            }
        }

    }
}
