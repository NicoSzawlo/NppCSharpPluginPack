using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;

namespace NppDemo.Forms
{
    partial class MarkdownRenderForm
    {

        private System.Windows.Forms.Button btnTest;
        private System.Windows.Forms.Button button1;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private ToolStripButton goButton;
        private ToolStrip toolStrip1;
        private WebView2 webView2;
        private Label debugLabel;

        private ToolStripStatusLabel toolStripStatusLabel1;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
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
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.goButton = new System.Windows.Forms.ToolStripButton();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.webView2 = new Microsoft.Web.WebView2.WinForms.WebView2();
            this.SuspendLayout();
            //
            // webView2
            //
            this.webView2.AllowExternalDrop = true;
            this.webView2.CreationProperties = null;
            this.webView2.DefaultBackgroundColor = System.Drawing.Color.White;
            this.webView2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webView2.Location = new System.Drawing.Point(0, 0);
            this.webView2.Name = "webView2";
            this.webView2.Size = new System.Drawing.Size(800, 450);
            this.webView2.Visible = true;
            this.webView2.TabIndex = 0;
            this.webView2.ZoomFactor = 1D;

            // In your Designer.cs, add a label for debugging
            this.debugLabel = new System.Windows.Forms.Label();
            this.debugLabel.Text = "Form is visible";
            this.debugLabel.BackColor = System.Drawing.Color.Red;
            this.debugLabel.Location = new System.Drawing.Point(0, 0);
            this.debugLabel.Size = new System.Drawing.Size(150, 20);
            this.debugLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.Controls.Add(this.debugLabel);
            this.debugLabel.BringToFront(); // Ensure it's on top

            // 
            // goButton
            // 
            this.goButton.Name = "goButton";
            this.goButton.Size = new System.Drawing.Size(26, 22);
            this.goButton.Text = "Go";
            this.goButton.Click += new System.EventHandler(this.goButton_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.goButton});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(948, 25);
            this.toolStrip1.TabIndex = 2;
            // 
            // MarkdownRenderForm
            // 
            this.ClientSize = new System.Drawing.Size(948, 478);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.webView2);
            this.Name = "MarkdownRenderForm";
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

    }
}