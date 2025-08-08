using Kbg.NppPluginNET;
using Microsoft.Web.WebView2.Core;
using NppDemo.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
//using MarkdownToHtmlConverter;

namespace NppDemo.Forms
{
    public partial class MarkdownRenderForm : FormBase
    {
        public DarkModeTestForm darkModeTestForm;

        public MarkdownRenderForm() : base(false, true)
        {
            InitializeComponent();
            
            darkModeTestForm = null;
            Test();
            
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
                await webView2.EnsureCoreWebView2Async(environment);

                webView2.CoreWebView2.DOMContentLoaded += (s, e) =>
                {
                    System.Diagnostics.Debug.WriteLine("WebView2 content has been loaded.");
                };

                // Initialization succeeded.
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
        /// <param name="markdown">The raw markdown text to render.</param>
        public void UpdatePreview(string markdown)
        {
            // Only update if the core environment is ready.
            if (webView2.CoreWebView2 == null)
            {
                return;
            }
            
            // Navigate to the HTML string.
            webView2.CoreWebView2.NavigateToString(markdown);
            System.Diagnostics.Debug.WriteLine($"WebView2 Size: {webView2.Size}");
            webView2.Invalidate();
            webView2.Refresh();
            webView2.BringToFront();
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

        private void Test()
        {

            //UpdatePreview(GetCurrentFileContent());
        }

        // Navigates to the URL in the address box when
        // the Go button is clicked.
        private void goButton_Click(object sender, EventArgs e)
        {
            Test();
        }
    }
}
