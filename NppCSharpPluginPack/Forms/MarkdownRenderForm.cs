using NppDemo.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Kbg.NppPluginNET;
using System.IO;

namespace NppDemo.Forms
{
    public partial class MarkdownRenderForm : FormBase
    {
        public DarkModeTestForm darkModeTestForm;

        public MarkdownRenderForm() : base(false, true)
        {
            InitializeComponent();
            darkModeTestForm = null;

            webBrowser1.CanGoBackChanged +=
            new EventHandler(webBrowser1_CanGoBackChanged);
            webBrowser1.CanGoForwardChanged +=
                new EventHandler(webBrowser1_CanGoForwardChanged);
            webBrowser1.DocumentTitleChanged +=
                new EventHandler(webBrowser1_DocumentTitleChanged);
            webBrowser1.StatusTextChanged +=
                new EventHandler(webBrowser1_StatusTextChanged);
            webBrowser1.GoSearch();
            
        }
        private void Test()
        {
            Npp.editor.AddText(4,"oida");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Test();
        }

        // Displays the Save dialog box.
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            webBrowser1.ShowSaveAsDialog();
        }

        // Displays the Page Setup dialog box.
        private void pageSetupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            webBrowser1.ShowPageSetupDialog();
        }

        // Displays the Print dialog box.
        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            webBrowser1.ShowPrintDialog();
        }

        // Displays the Print Preview dialog box.
        private void printPreviewToolStripMenuItem_Click(
            object sender, EventArgs e)
        {
            webBrowser1.ShowPrintPreviewDialog();
        }

        // Displays the Properties dialog box.
        private void propertiesToolStripMenuItem_Click(
            object sender, EventArgs e)
        {
            webBrowser1.ShowPropertiesDialog();
        }

        // Selects all the text in the text box when the user clicks it.
        private void toolStripTextBox1_Click(object sender, EventArgs e)
        {
            toolStripTextBox1.SelectAll();
        }

        // Navigates to the URL in the address box when
        // the ENTER key is pressed while the ToolStripTextBox has focus.
        private void toolStripTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Navigate(toolStripTextBox1.Text);
            }
        }

        // Navigates to the URL in the address box when
        // the Go button is clicked.
        private void goButton_Click(object sender, EventArgs e)
        {
            Navigate(toolStripTextBox1.Text);
        }

        // Navigates to the given URL if it is valid.
        private void Navigate(String address)
        {
            string newAddress = "127.0.0.0";
            try
            {
                webBrowser1.Navigate(new Uri(newAddress));
            }
            catch (System.UriFormatException)
            {
                return;
            }
        }

        // Updates the URL in TextBoxAddress upon navigation.
        private void webBrowser1_Navigated(object sender,
            WebBrowserNavigatedEventArgs e)
        {
            toolStripTextBox1.Text = webBrowser1.Url.ToString();
        }

        // Navigates webBrowser1 to the previous page in the history.
        private void backButton_Click(object sender, EventArgs e)
        {
            webBrowser1.GoBack();
        }

        // Disables the Back button at the beginning of the navigation history.
        private void webBrowser1_CanGoBackChanged(object sender, EventArgs e)
        {
            backButton.Enabled = webBrowser1.CanGoBack;
        }

        // Navigates webBrowser1 to the next page in history.
        private void forwardButton_Click(object sender, EventArgs e)
        {
            webBrowser1.GoForward();
        }

        // Disables the Forward button at the end of navigation history.
        private void webBrowser1_CanGoForwardChanged(object sender, EventArgs e)
        {
            forwardButton.Enabled = webBrowser1.CanGoForward;
        }

        // Halts the current navigation and any sounds or animations on
        // the page.
        private void stopButton_Click(object sender, EventArgs e)
        {
            webBrowser1.Stop();
        }

        // Reloads the current page.
        private void refreshButton_Click(object sender, EventArgs e)
        {
            // Skip refresh if about:blank is loaded to avoid removing
            // content specified by the DocumentText property.
            if (!webBrowser1.Url.Equals("about:blank"))
            {
                webBrowser1.Refresh();
            }
        }

        // Navigates webBrowser1 to the home page of the current user.
        private void homeButton_Click(object sender, EventArgs e)
        {
            webBrowser1.GoHome();
        }

        // Navigates webBrowser1 to the search page of the current user.
        private void searchButton_Click(object sender, EventArgs e)
        {
            webBrowser1.GoSearch();
        }

        // Prints the current document using the current print settings.
        private void printButton_Click(object sender, EventArgs e)
        {
            webBrowser1.Print();
        }

        // Updates the status bar with the current browser status text.
        private void webBrowser1_StatusTextChanged(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = webBrowser1.StatusText;
        }

        // Updates the title bar with the current document title.
        private void webBrowser1_DocumentTitleChanged(object sender, EventArgs e)
        {
            this.Text = webBrowser1.DocumentTitle;
        }

        // Exits the application.
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
