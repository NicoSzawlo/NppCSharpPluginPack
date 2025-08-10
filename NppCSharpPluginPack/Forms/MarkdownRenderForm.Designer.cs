using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;

namespace NppDemo.Forms
{
    partial class MarkdownRenderForm
    {

        private System.ComponentModel.IContainer components = null;
        private SplitContainer splitContainer;
        private TreeView treeViewHeaders;
        private WebView2 webView;

        private ToolStripStatusLabel toolStripStatusLabel1;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.components?.Dispose();
                this.webView?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.splitContainer = new SplitContainer();
            this.treeViewHeaders = new TreeView();
            this.webView = new WebView2();

            // splitContainer
            this.splitContainer.Dock = DockStyle.Fill;
            this.splitContainer.FixedPanel = FixedPanel.Panel1;
            this.splitContainer.IsSplitterFixed = false;
            this.splitContainer.Name = "splitContainer";
            this.splitContainer.TabIndex = 0;
            this.splitContainer.SplitterDistance = 260; // adjust as needed

            // treeViewHeaders (left pane)
            this.treeViewHeaders.Dock = DockStyle.Fill;
            this.treeViewHeaders.FullRowSelect = true;
            this.treeViewHeaders.HideSelection = false;
            this.treeViewHeaders.Name = "treeViewHeaders";
            this.treeViewHeaders.TabIndex = 0;

            //
            // webView2
            //
            this.webView.AllowExternalDrop = true;
            this.webView.CreationProperties = null;
            this.webView.DefaultBackgroundColor = System.Drawing.Color.White;
            this.webView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webView.Location = new System.Drawing.Point(0, 0);
            this.webView.Name = "webView2";
            this.webView.Size = new System.Drawing.Size(800, 450);
            this.webView.Visible = true;
            this.webView.TabIndex = 0;
            this.webView.ZoomFactor = 1D;
            // Note: CoreWebView2 initialization should be done in runtime code (e.g. Form_Load)

            // assemble
            this.splitContainer.Panel1.Controls.Add(this.treeViewHeaders);
            this.splitContainer.Panel2.Controls.Add(this.webView);

            this.Controls.Add(this.splitContainer);
            this.Name = "MarkdownPreviewForm";
            this.Text = "Markdown Preview";

            // optional: minimal size
            this.MinimumSize = new System.Drawing.Size(640, 360);
        }

    }
    #endregion
}