# NppMarkdownExplorer
Notepad++ plugin based on NppCSharpPluginPack

## 🚀 Features
📄 Live Markdown Rendering
Renders Markdown files in real-time using an embedded WebView panel.

🧭 Document Tree Navigation
A listbox sidebar provides a structured outline (TreeView) of the current Markdown file, allowing quick navigation between headings.

🌐 Embedded Resource Support
Automatically loads and displays external resources such as:

Images (![]() syntax)

Links ([]() syntax)

Other file references

📁 Multi-File Switching
Quickly switch between files with hyperlinks in your markdown file.

## ✅ In Progress
### Markdown Parser
Implement a parser that reads Markdown content and converts it into a collection of intermediate objects representing headers, paragraphs, lists, images, links, etc.

These objects will later be transformed into HTML and rendered in the viewer.

### Basic Rendering Engine
Convert parsed Markdown objects to raw HTML for display.

### Navigation Tree Structure
Extract headings from the Markdown and generate a navigable TreeView in a ListBox.

### 🔧 Technical Challenges
 Replace WebView2
WebView2 currently does not work properly in the Notepad++ plugin environment. Likely switching to a native WinForms WebBrowser control (IE-based) or exploring other lightweight HTML rendering options.

## 📌 Next Steps
### Integrate Markdown Parsing + Rendering Pipeline

### Display rendered content in the WebView control

### Connect TreeView navigation to scroll/jump in WebView

### Load and display local images and external links

### File change handling (refresh view on save or switch)

## 🧭 Future Features (Once Core is Stable)
### Dark Mode / Theming Support

### Live Preview Sync with Editor Cursor

### Support GitHub-flavored Markdown extensions

### Plugin settings panel (toggle features, custom styles, etc.)
