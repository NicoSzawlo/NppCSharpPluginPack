﻿// NPP plugin platform for .Net v0.94.00 by Kasper B. Graversen etc.
using System;
using System.Runtime.InteropServices;
using System.Text;
using static Kbg.NppPluginNET.PluginInfrastructure.Win32;

namespace Kbg.NppPluginNET.PluginInfrastructure
{
    /// <summary>
    /// This it the plugin-writers primary interface to Notepad++/Scintilla.
    /// It takes away all the complexity with command numbers and Int-pointer casting.
    ///
    /// See http://www.scintilla.org/ScintillaDoc.html for further details.
    /// </summary>
    public class ScintillaGateway : IScintillaGateway
    {
        private const int Unused = 0;

        private readonly IntPtr scintilla;

        public static readonly int LengthZeroTerminator = "\0".Length;

        /// <summary>
        /// if bytes is null, returns null.<br></br>
        /// Else, returns bytes decoded from UTF-8 as a string, with all trailing NULL bytes stripped off.
        /// </summary>
        public static string Utf8BytesToNullStrippedString(byte[] bytes)
        {
            if (bytes is null)
                return null;
            int lastNullCharPos = bytes.Length - 1;
            // this only bypasses NULL chars because no char
            // other than NULL can have any 0-valued bytes in UTF-8.
            // See https://en.wikipedia.org/wiki/UTF-8#Encoding
            for (; lastNullCharPos >= 0 && bytes[lastNullCharPos] == '\x00'; lastNullCharPos--) { }
            return Encoding.UTF8.GetString(bytes, 0, lastNullCharPos + 1);
        }

        /// <summary>
        /// Recall that all Scintilla methods have the signature 
        /// (scintilla* scin, SciMsg msg, void* wParam, void* lParam) -&gt; void*<br></br>
        /// Many of these scintilla methods are bimodal in the following way<br></br>
        /// * if lParam is 0, return the length of the buffer to be filled and have no side effects. The wParam may be involved in telling Scintilla how big the buffer needs to be.<br></br>
        /// * if lParam is greater than 0, it is assumed to be a pointer to a buffer. Now the wParam indicates what the text will need to be.<br></br><br></br>
        /// This sets lParam to 0 to get the length, allocates a buffer of that length,<br></br>
        /// uses the second mode to fill a buffer,<br></br>
        /// and returns a string of the UTF8-decoded buffer with all trailing '\x00' chars stripped off.
        /// </summary>
        /// <param name="msg">message to send</param>
        /// <param name="wParam">another parameter for defining what the buffer should contain</param>
        /// <returns></returns>
        private unsafe string GetNullStrippedStringFromMessageThatReturnsLength(SciMsg msg, IntPtr wParam=default)
        {
            int length = Win32.SendMessage(scintilla, msg, wParam, (IntPtr)Unused).ToInt32();
            byte[] textBuffer = new byte[length];
            fixed (byte* textPtr = textBuffer)
            {
                Win32.SendMessage(scintilla, msg, wParam, (IntPtr)textPtr);
                return Utf8BytesToNullStrippedString(textBuffer);
            }
        }

        private byte[] GetNullTerminatedUTF8Bytes(string text)
        {
            int length = Encoding.UTF8.GetByteCount(text);
            byte[] bytes = new byte[length + 1];
            int lengthWritten = Encoding.UTF8.GetBytes(text, 0, text.Length, bytes, 0);
            //if (lengthWritten != length)
            //    throw new Exception("not sure what we would do here");
            return bytes;
        }

        public ScintillaGateway(IntPtr scintilla)
        {
            this.scintilla = scintilla;
        }

        public int GetSelectionLength()
        {
            var selectionLength = (int) Win32.SendMessage(scintilla, SciMsg.SCI_GETSELTEXT, Unused, Unused) - LengthZeroTerminator;
            return selectionLength;
        }

        public void AppendTextAndMoveCursor(string text)
        {
            AppendText(text.Length, text);
            GotoPos(GetCurrentPos() + text.Length);
        }

        public void InsertTextAndMoveCursor(string text)
        {
            var currentPos = GetCurrentPos();
            InsertText(currentPos, text);
            GotoPos(currentPos + text.Length);
        }

        public void SelectCurrentLine()
        {
            int line = GetCurrentLineNumber();
            SetSelection(PositionFromLine(line), PositionFromLine(line + 1));
        }

        /// <summary>
        /// clears the selection without changing the position of the cursor
        /// </summary>
        public void ClearSelectionToCursor()
        {
            var pos = GetCurrentPos();
            SetSelection(pos, pos);
        }

        /// <summary>
        /// Get the current line from the current position
        /// </summary>
        public int GetCurrentLineNumber()
        {
            return LineFromPosition(GetCurrentPos());
        }

        /// <summary>
        /// Get the scroll information for the current Scintilla window.
        /// </summary>
        /// <param name="mask">Arguments for the scroll information such as tracking</param>
        /// <param name="scrollBar">Which scroll bar information are you looking for</param>
        /// <returns>A ScrollInfo struct with information of the current scroll state</returns>
        public ScrollInfo GetScrollInfo(ScrollInfoMask mask = ScrollInfoMask.SIF_ALL, ScrollInfoBar scrollBar = ScrollInfoBar.SB_BOTH)
        {
            ScrollInfo scrollInfo = new ScrollInfo();
            scrollInfo.cbSize = (uint)Marshal.SizeOf(scrollInfo);
            scrollInfo.fMask = (uint)mask;
            Win32.GetScrollInfo(scintilla, (int)scrollBar, ref scrollInfo);
            return scrollInfo;
        }

        /* ++Autogenerated -- start of section automatically generated from Scintilla.iface */
        /// <summary>Add text to the document at current position. (Scintilla feature 2001)</summary>
        public unsafe void AddText(int length, string text)
        {
            fixed (byte* textPtr = Encoding.UTF8.GetBytes(text))
            {
                Win32.SendMessage(scintilla, SciMsg.SCI_ADDTEXT, (IntPtr) length, (IntPtr) textPtr);
            }
        }

        /// <summary>Add array of cells to document. (Scintilla feature 2002)</summary>
        public unsafe void AddStyledText(int length, Cells c)
        {
            fixed (char* cPtr = c.Value)
            {
                Win32.SendMessage(scintilla, SciMsg.SCI_ADDSTYLEDTEXT, (IntPtr) length, (IntPtr) cPtr);
            }
        }

        /// <summary>Insert string at a position. (Scintilla feature 2003)</summary>
        public unsafe void InsertText(int pos, string text)
        {
            fixed (byte* textPtr = GetNullTerminatedUTF8Bytes(text))
            {
                Win32.SendMessage(scintilla, SciMsg.SCI_INSERTTEXT, (IntPtr) pos, (IntPtr) textPtr);
            }
        }

        /// <summary>Change the text that is being inserted in response to SC_MOD_INSERTCHECK (Scintilla feature 2672)</summary>
        public unsafe void ChangeInsertion(int length, string text)
        {
            fixed (byte* textPtr = Encoding.UTF8.GetBytes(text))
            {
                Win32.SendMessage(scintilla, SciMsg.SCI_CHANGEINSERTION, (IntPtr) length, (IntPtr) textPtr);
            }
        }

        /// <summary>Delete all text in the document. (Scintilla feature 2004)</summary>
        public void ClearAll()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_CLEARALL, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Delete a range of text in the document. (Scintilla feature 2645)</summary>
        public void DeleteRange(int start, int lengthDelete)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_DELETERANGE, (IntPtr) start, (IntPtr) lengthDelete);
        }

        /// <summary>Set all style bytes to 0, remove all folding information. (Scintilla feature 2005)</summary>
        public void ClearDocumentStyle()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_CLEARDOCUMENTSTYLE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Returns the number of bytes in the document. (Scintilla feature 2006)</summary>
        public long GetLength()
        {
            return (long)Win32.SendMessage(scintilla, SciMsg.SCI_GETLENGTH, (IntPtr) Unused, (IntPtr) Unused);
        }

        public bool TryGetLengthAsInt(out int result)
        {
            long longRes = GetLength();
            if (longRes > int.MaxValue)
            {
                result = -1;
                return false;
            }
            result = (int)longRes;
            return true;
        }

        /// <summary>Returns the character byte at the position. (Scintilla feature 2007)</summary>
        public int GetCharAt(int pos)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETCHARAT, (IntPtr) pos, (IntPtr) Unused);
        }

        /// <summary>Returns the position of the caret. (Scintilla feature 2008)</summary>
        public int GetCurrentPos()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETCURRENTPOS, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Returns the position of the opposite end of the selection to the caret. (Scintilla feature 2009)</summary>
        public int GetAnchor()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETANCHOR, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Returns the style byte at the position. (Scintilla feature 2010)</summary>
        public int GetStyleAt(int pos)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETSTYLEAT, (IntPtr) pos, (IntPtr) Unused);
        }

        /// <summary>Redoes the next action on the undo history. (Scintilla feature 2011)</summary>
        public void Redo()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_REDO, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Choose between collecting actions into the undo
        /// history and discarding them.
        /// (Scintilla feature 2012)
        /// </summary>
        public void SetUndoCollection(bool collectUndo)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETUNDOCOLLECTION, new IntPtr(collectUndo ? 1 : 0), (IntPtr) Unused);
        }

        /// <summary>Select all the text in the document. (Scintilla feature 2013)</summary>
        public void SelectAll()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SELECTALL, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Remember the current position in the undo history as the position
        /// at which the document was saved.
        /// (Scintilla feature 2014)
        /// </summary>
        public void SetSavePoint()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETSAVEPOINT, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Retrieve a buffer of cells.
        /// Returns the number of bytes in the buffer not including terminating NULs.
        /// (Scintilla feature 2015)
        /// </summary>
        public int GetStyledText(TextRange tr)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETSTYLEDTEXT, (IntPtr) Unused, tr.NativePointer);
        }

        /// <summary>Are there any redoable actions in the undo history? (Scintilla feature 2016)</summary>
        public bool CanRedo()
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_CANREDO, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Retrieve the line number at which a particular marker is located. (Scintilla feature 2017)</summary>
        public int MarkerLineFromHandle(int markerHandle)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_MARKERLINEFROMHANDLE, (IntPtr) markerHandle, (IntPtr) Unused);
        }

        /// <summary>Delete a marker. (Scintilla feature 2018)</summary>
        public void MarkerDeleteHandle(int markerHandle)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_MARKERDELETEHANDLE, (IntPtr) markerHandle, (IntPtr) Unused);
        }

        /// <summary>Is undo history being collected? (Scintilla feature 2019)</summary>
        public bool GetUndoCollection()
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETUNDOCOLLECTION, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Are white space characters currently visible?
        /// Returns one of SCWS_* constants.
        /// (Scintilla feature 2020)
        /// </summary>
        public WhiteSpace GetViewWS()
        {
            return (WhiteSpace)Win32.SendMessage(scintilla, SciMsg.SCI_GETVIEWWS, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Make white space characters invisible, always visible or visible outside indentation. (Scintilla feature 2021)</summary>
        public void SetViewWS(WhiteSpace viewWS)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETVIEWWS, (IntPtr) viewWS, (IntPtr) Unused);
        }

        /// <summary>
        /// Retrieve the current tab draw mode.
        /// Returns one of SCTD_* constants.
        /// (Scintilla feature 2698)
        /// </summary>
        public TabDrawMode GetTabDrawMode()
        {
            return (TabDrawMode)Win32.SendMessage(scintilla, SciMsg.SCI_GETTABDRAWMODE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set how tabs are drawn when visible. (Scintilla feature 2699)</summary>
        public void SetTabDrawMode(TabDrawMode tabDrawMode)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETTABDRAWMODE, (IntPtr) tabDrawMode, (IntPtr) Unused);
        }

        /// <summary>Find the position from a point within the window. (Scintilla feature 2022)</summary>
        public int PositionFromPoint(int x, int y)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_POSITIONFROMPOINT, (IntPtr) x, (IntPtr) y);
        }

        /// <summary>
        /// Find the position from a point within the window but return
        /// INVALID_POSITION if not close to text.
        /// (Scintilla feature 2023)
        /// </summary>
        public int PositionFromPointClose(int x, int y)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_POSITIONFROMPOINTCLOSE, (IntPtr) x, (IntPtr) y);
        }

        /// <summary>Set caret to start of a line and ensure it is visible. (Scintilla feature 2024)</summary>
        public void GotoLine(int line)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_GOTOLINE, (IntPtr) line, (IntPtr) Unused);
        }

        /// <summary>Set caret to a position and ensure it is visible. (Scintilla feature 2025)</summary>
        public void GotoPos(int caret)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_GOTOPOS, (IntPtr) caret, (IntPtr) Unused);
        }

        /// <summary>
        /// Set the selection anchor to a position. The anchor is the opposite
        /// end of the selection from the caret.
        /// (Scintilla feature 2026)
        /// </summary>
        public void SetAnchor(int anchor)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETANCHOR, (IntPtr) anchor, (IntPtr) Unused);
        }

        /// <summary>
        /// Retrieve the text of the line containing the caret.
        /// Returns the index of the caret on the line.
        /// Result is NUL-terminated.
        /// (Scintilla feature 2027)
        /// </summary>
        public unsafe string GetCurLine()
        {
            return GetNullStrippedStringFromMessageThatReturnsLength(SciMsg.SCI_GETCURLINE);
        }

        /// <summary>Retrieve the position of the last correctly styled character. (Scintilla feature 2028)</summary>
        public int GetEndStyled()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETENDSTYLED, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Convert all line endings in the document to one mode. (Scintilla feature 2029)</summary>
        public void ConvertEOLs(EndOfLine eolMode)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_CONVERTEOLS, (IntPtr) eolMode, (IntPtr) Unused);
        }

        /// <summary>Retrieve the current end of line mode - one of CRLF, CR, or LF. (Scintilla feature 2030)</summary>
        public EndOfLine GetEOLMode()
        {
            return (EndOfLine)Win32.SendMessage(scintilla, SciMsg.SCI_GETEOLMODE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set the current end of line mode. (Scintilla feature 2031)</summary>
        public void SetEOLMode(EndOfLine eolMode)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETEOLMODE, (IntPtr) eolMode, (IntPtr) Unused);
        }

        /// <summary>
        /// Set the current styling position to start.
        /// The unused parameter is no longer used and should be set to 0.
        /// (Scintilla feature 2032)
        /// </summary>
        public void StartStyling(int start, int unused)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_STARTSTYLING, (IntPtr) start, (IntPtr) unused);
        }

        /// <summary>
        /// Change style from current styling position for length characters to a style
        /// and move the current styling position to after this newly styled segment.
        /// (Scintilla feature 2033)
        /// </summary>
        public void SetStyling(int length, int style)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETSTYLING, (IntPtr) length, (IntPtr) style);
        }

        /// <summary>Is drawing done first into a buffer or direct to the screen? (Scintilla feature 2034)</summary>
        public bool GetBufferedDraw()
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETBUFFEREDDRAW, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// If drawing is buffered then each line of text is drawn into a bitmap buffer
        /// before drawing it to the screen to avoid flicker.
        /// (Scintilla feature 2035)
        /// </summary>
        public void SetBufferedDraw(bool buffered)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETBUFFEREDDRAW, new IntPtr(buffered ? 1 : 0), (IntPtr) Unused);
        }

        /// <summary>Change the visible size of a tab to be a multiple of the width of a space character. (Scintilla feature 2036)</summary>
        public void SetTabWidth(int tabWidth)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETTABWIDTH, (IntPtr) tabWidth, (IntPtr) Unused);
        }

        /// <summary>Retrieve the visible size of a tab. (Scintilla feature 2121)</summary>
        public int GetTabWidth()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETTABWIDTH, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Clear explicit tabstops on a line. (Scintilla feature 2675)</summary>
        public void ClearTabStops(int line)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_CLEARTABSTOPS, (IntPtr) line, (IntPtr) Unused);
        }

        /// <summary>Add an explicit tab stop for a line. (Scintilla feature 2676)</summary>
        public void AddTabStop(int line, int x)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_ADDTABSTOP, (IntPtr) line, (IntPtr) x);
        }

        /// <summary>Find the next explicit tab stop position on a line after a position. (Scintilla feature 2677)</summary>
        public int GetNextTabStop(int line, int x)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETNEXTTABSTOP, (IntPtr) line, (IntPtr) x);
        }

        /// <summary>
        /// Set the code page used to interpret the bytes of the document as characters.
        /// The SC_CP_UTF8 value can be used to enter Unicode mode.
        /// (Scintilla feature 2037)
        /// </summary>
        public void SetCodePage(int codePage)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETCODEPAGE, (IntPtr) codePage, (IntPtr) Unused);
        }

        /// <summary>Is the IME displayed in a window or inline? (Scintilla feature 2678)</summary>
        public IMEInteraction GetIMEInteraction()
        {
            return (IMEInteraction)Win32.SendMessage(scintilla, SciMsg.SCI_GETIMEINTERACTION, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Choose to display the the IME in a winow or inline. (Scintilla feature 2679)</summary>
        public void SetIMEInteraction(IMEInteraction imeInteraction)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETIMEINTERACTION, (IntPtr) imeInteraction, (IntPtr) Unused);
        }

        /// <summary>Set the symbol used for a particular marker number. (Scintilla feature 2040)</summary>
        public void MarkerDefine(int markerNumber, MarkerSymbol markerSymbol)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_MARKERDEFINE, (IntPtr) markerNumber, (IntPtr) markerSymbol);
        }

        /// <summary>Set the foreground colour used for a particular marker number. (Scintilla feature 2041)</summary>
        public void MarkerSetFore(int markerNumber, Colour fore)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_MARKERSETFORE, (IntPtr) markerNumber, fore.Value);
        }

        /// <summary>Set the background colour used for a particular marker number. (Scintilla feature 2042)</summary>
        public void MarkerSetBack(int markerNumber, Colour back)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_MARKERSETBACK, (IntPtr) markerNumber, back.Value);
        }

        /// <summary>Set the background colour used for a particular marker number when its folding block is selected. (Scintilla feature 2292)</summary>
        public void MarkerSetBackSelected(int markerNumber, Colour back)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_MARKERSETBACKSELECTED, (IntPtr) markerNumber, back.Value);
        }

        /// <summary>Enable/disable highlight for current folding bloc (smallest one that contains the caret) (Scintilla feature 2293)</summary>
        public void MarkerEnableHighlight(bool enabled)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_MARKERENABLEHIGHLIGHT, new IntPtr(enabled ? 1 : 0), (IntPtr) Unused);
        }

        /// <summary>Add a marker to a line, returning an ID which can be used to find or delete the marker. (Scintilla feature 2043)</summary>
        public int MarkerAdd(int line, int markerNumber)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_MARKERADD, (IntPtr) line, (IntPtr) markerNumber);
        }

        /// <summary>Delete a marker from a line. (Scintilla feature 2044)</summary>
        public void MarkerDelete(int line, int markerNumber)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_MARKERDELETE, (IntPtr) line, (IntPtr) markerNumber);
        }

        /// <summary>Delete all markers with a particular number from all lines. (Scintilla feature 2045)</summary>
        public void MarkerDeleteAll(int markerNumber)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_MARKERDELETEALL, (IntPtr) markerNumber, (IntPtr) Unused);
        }

        /// <summary>Get a bit mask of all the markers set on a line. (Scintilla feature 2046)</summary>
        public int MarkerGet(int line)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_MARKERGET, (IntPtr) line, (IntPtr) Unused);
        }

        /// <summary>
        /// Find the next line at or after lineStart that includes a marker in mask.
        /// Return -1 when no more lines.
        /// (Scintilla feature 2047)
        /// </summary>
        public int MarkerNext(int lineStart, int markerMask)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_MARKERNEXT, (IntPtr) lineStart, (IntPtr) markerMask);
        }

        /// <summary>Find the previous line before lineStart that includes a marker in mask. (Scintilla feature 2048)</summary>
        public int MarkerPrevious(int lineStart, int markerMask)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_MARKERPREVIOUS, (IntPtr) lineStart, (IntPtr) markerMask);
        }

        /// <summary>Define a marker from a pixmap. (Scintilla feature 2049)</summary>
        public unsafe void MarkerDefinePixmap(int markerNumber, string pixmap)
        {
            fixed (byte* pixmapPtr = Encoding.UTF8.GetBytes(pixmap))
            {
                Win32.SendMessage(scintilla, SciMsg.SCI_MARKERDEFINEPIXMAP, (IntPtr) markerNumber, (IntPtr) pixmapPtr);
            }
        }

        /// <summary>Add a set of markers to a line. (Scintilla feature 2466)</summary>
        public void MarkerAddSet(int line, int markerSet)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_MARKERADDSET, (IntPtr) line, (IntPtr) markerSet);
        }

        /// <summary>Set the alpha used for a marker that is drawn in the text area, not the margin. (Scintilla feature 2476)</summary>
        public void MarkerSetAlpha(int markerNumber, Alpha alpha)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_MARKERSETALPHA, (IntPtr) markerNumber, (IntPtr) alpha);
        }

        /// <summary>Set a margin to be either numeric or symbolic. (Scintilla feature 2240)</summary>
        public void SetMarginTypeN(int margin, MarginType marginType)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETMARGINTYPEN, (IntPtr) margin, (IntPtr) marginType);
        }

        /// <summary>Retrieve the type of a margin. (Scintilla feature 2241)</summary>
        public MarginType GetMarginTypeN(int margin)
        {
            return (MarginType)Win32.SendMessage(scintilla, SciMsg.SCI_GETMARGINTYPEN, (IntPtr) margin, (IntPtr) Unused);
        }

        /// <summary>Set the width of a margin to a width expressed in pixels. (Scintilla feature 2242)</summary>
        public void SetMarginWidthN(int margin, int pixelWidth)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETMARGINWIDTHN, (IntPtr) margin, (IntPtr) pixelWidth);
        }

        /// <summary>Retrieve the width of a margin in pixels. (Scintilla feature 2243)</summary>
        public int GetMarginWidthN(int margin)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETMARGINWIDTHN, (IntPtr) margin, (IntPtr) Unused);
        }

        /// <summary>Set a mask that determines which markers are displayed in a margin. (Scintilla feature 2244)</summary>
        public void SetMarginMaskN(int margin, int mask)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETMARGINMASKN, (IntPtr) margin, (IntPtr) mask);
        }

        /// <summary>Retrieve the marker mask of a margin. (Scintilla feature 2245)</summary>
        public int GetMarginMaskN(int margin)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETMARGINMASKN, (IntPtr) margin, (IntPtr) Unused);
        }

        /// <summary>Make a margin sensitive or insensitive to mouse clicks. (Scintilla feature 2246)</summary>
        public void SetMarginSensitiveN(int margin, bool sensitive)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETMARGINSENSITIVEN, (IntPtr) margin, new IntPtr(sensitive ? 1 : 0));
        }

        /// <summary>Retrieve the mouse click sensitivity of a margin. (Scintilla feature 2247)</summary>
        public bool GetMarginSensitiveN(int margin)
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETMARGINSENSITIVEN, (IntPtr) margin, (IntPtr) Unused);
        }

        /// <summary>Set the cursor shown when the mouse is inside a margin. (Scintilla feature 2248)</summary>
        public void SetMarginCursorN(int margin, CursorShape cursor)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETMARGINCURSORN, (IntPtr) margin, (IntPtr) cursor);
        }

        /// <summary>Retrieve the cursor shown in a margin. (Scintilla feature 2249)</summary>
        public CursorShape GetMarginCursorN(int margin)
        {
            return (CursorShape)Win32.SendMessage(scintilla, SciMsg.SCI_GETMARGINCURSORN, (IntPtr) margin, (IntPtr) Unused);
        }

        /// <summary>Set the background colour of a margin. Only visible for SC_MARGIN_COLOUR. (Scintilla feature 2250)</summary>
        public void SetMarginBackN(int margin, Colour back)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETMARGINBACKN, (IntPtr) margin, back.Value);
        }

        /// <summary>Retrieve the background colour of a margin (Scintilla feature 2251)</summary>
        public Colour GetMarginBackN(int margin)
        {
            return new Colour((int) Win32.SendMessage(scintilla, SciMsg.SCI_GETMARGINBACKN, (IntPtr) margin, (IntPtr) Unused));
        }

        /// <summary>Allocate a non-standard number of margins. (Scintilla feature 2252)</summary>
        public void SetMargins(int margins)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETMARGINS, (IntPtr) margins, (IntPtr) Unused);
        }

        /// <summary>How many margins are there?. (Scintilla feature 2253)</summary>
        public int GetMargins()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETMARGINS, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Clear all the styles and make equivalent to the global default style. (Scintilla feature 2050)</summary>
        public void StyleClearAll()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_STYLECLEARALL, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set the foreground colour of a style. (Scintilla feature 2051)</summary>
        public void StyleSetFore(int style, Colour fore)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_STYLESETFORE, (IntPtr) style, fore.Value);
        }

        /// <summary>Set the background colour of a style. (Scintilla feature 2052)</summary>
        public void StyleSetBack(int style, Colour back)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_STYLESETBACK, (IntPtr) style, back.Value);
        }

        /// <summary>Set a style to be bold or not. (Scintilla feature 2053)</summary>
        public void StyleSetBold(int style, bool bold)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_STYLESETBOLD, (IntPtr) style, new IntPtr(bold ? 1 : 0));
        }

        /// <summary>Set a style to be italic or not. (Scintilla feature 2054)</summary>
        public void StyleSetItalic(int style, bool italic)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_STYLESETITALIC, (IntPtr) style, new IntPtr(italic ? 1 : 0));
        }

        /// <summary>Set the size of characters of a style. (Scintilla feature 2055)</summary>
        public void StyleSetSize(int style, int sizePoints)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_STYLESETSIZE, (IntPtr) style, (IntPtr) sizePoints);
        }

        /// <summary>Set the font of a style. (Scintilla feature 2056)</summary>
        public unsafe void StyleSetFont(int style, string fontName)
        {
            fixed (byte* fontNamePtr = Encoding.UTF8.GetBytes(fontName))
            {
                Win32.SendMessage(scintilla, SciMsg.SCI_STYLESETFONT, (IntPtr) style, (IntPtr) fontNamePtr);
            }
        }

        /// <summary>Set a style to have its end of line filled or not. (Scintilla feature 2057)</summary>
        public void StyleSetEOLFilled(int style, bool eolFilled)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_STYLESETEOLFILLED, (IntPtr) style, new IntPtr(eolFilled ? 1 : 0));
        }

        /// <summary>Reset the default style to its state at startup (Scintilla feature 2058)</summary>
        public void StyleResetDefault()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_STYLERESETDEFAULT, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set a style to be underlined or not. (Scintilla feature 2059)</summary>
        public void StyleSetUnderline(int style, bool underline)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_STYLESETUNDERLINE, (IntPtr) style, new IntPtr(underline ? 1 : 0));
        }

        /// <summary>Get the foreground colour of a style. (Scintilla feature 2481)</summary>
        public Colour StyleGetFore(int style)
        {
            return new Colour((int) Win32.SendMessage(scintilla, SciMsg.SCI_STYLEGETFORE, (IntPtr) style, (IntPtr) Unused));
        }

        /// <summary>Get the background colour of a style. (Scintilla feature 2482)</summary>
        public Colour StyleGetBack(int style)
        {
            return new Colour((int) Win32.SendMessage(scintilla, SciMsg.SCI_STYLEGETBACK, (IntPtr) style, (IntPtr) Unused));
        }

        /// <summary>Get is a style bold or not. (Scintilla feature 2483)</summary>
        public bool StyleGetBold(int style)
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_STYLEGETBOLD, (IntPtr) style, (IntPtr) Unused);
        }

        /// <summary>Get is a style italic or not. (Scintilla feature 2484)</summary>
        public bool StyleGetItalic(int style)
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_STYLEGETITALIC, (IntPtr) style, (IntPtr) Unused);
        }

        /// <summary>Get the size of characters of a style. (Scintilla feature 2485)</summary>
        public int StyleGetSize(int style)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_STYLEGETSIZE, (IntPtr) style, (IntPtr) Unused);
        }

        /// <summary>
        /// Get the font of a style.
        /// Returns the length of the fontName
        /// Result is NUL-terminated.
        /// (Scintilla feature 2486)
        /// </summary>
        public unsafe string StyleGetFont(int style)
        {
            return GetNullStrippedStringFromMessageThatReturnsLength(SciMsg.SCI_STYLEGETFONT, (IntPtr)style);
        }

        /// <summary>Get is a style to have its end of line filled or not. (Scintilla feature 2487)</summary>
        public bool StyleGetEOLFilled(int style)
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_STYLEGETEOLFILLED, (IntPtr) style, (IntPtr) Unused);
        }

        /// <summary>Get is a style underlined or not. (Scintilla feature 2488)</summary>
        public bool StyleGetUnderline(int style)
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_STYLEGETUNDERLINE, (IntPtr) style, (IntPtr) Unused);
        }

        /// <summary>Get is a style mixed case, or to force upper or lower case. (Scintilla feature 2489)</summary>
        public CaseVisible StyleGetCase(int style)
        {
            return (CaseVisible)Win32.SendMessage(scintilla, SciMsg.SCI_STYLEGETCASE, (IntPtr) style, (IntPtr) Unused);
        }

        /// <summary>Get the character get of the font in a style. (Scintilla feature 2490)</summary>
        public CharacterSet StyleGetCharacterSet(int style)
        {
            return (CharacterSet)Win32.SendMessage(scintilla, SciMsg.SCI_STYLEGETCHARACTERSET, (IntPtr) style, (IntPtr) Unused);
        }

        /// <summary>Get is a style visible or not. (Scintilla feature 2491)</summary>
        public bool StyleGetVisible(int style)
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_STYLEGETVISIBLE, (IntPtr) style, (IntPtr) Unused);
        }

        /// <summary>
        /// Get is a style changeable or not (read only).
        /// Experimental feature, currently buggy.
        /// (Scintilla feature 2492)
        /// </summary>
        public bool StyleGetChangeable(int style)
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_STYLEGETCHANGEABLE, (IntPtr) style, (IntPtr) Unused);
        }

        /// <summary>Get is a style a hotspot or not. (Scintilla feature 2493)</summary>
        public bool StyleGetHotSpot(int style)
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_STYLEGETHOTSPOT, (IntPtr) style, (IntPtr) Unused);
        }

        /// <summary>Set a style to be mixed case, or to force upper or lower case. (Scintilla feature 2060)</summary>
        public void StyleSetCase(int style, CaseVisible caseVisible)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_STYLESETCASE, (IntPtr) style, (IntPtr) caseVisible);
        }

        /// <summary>Set the size of characters of a style. Size is in points multiplied by 100. (Scintilla feature 2061)</summary>
        public void StyleSetSizeFractional(int style, int sizeHundredthPoints)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_STYLESETSIZEFRACTIONAL, (IntPtr) style, (IntPtr) sizeHundredthPoints);
        }

        /// <summary>Get the size of characters of a style in points multiplied by 100 (Scintilla feature 2062)</summary>
        public int StyleGetSizeFractional(int style)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_STYLEGETSIZEFRACTIONAL, (IntPtr) style, (IntPtr) Unused);
        }

        /// <summary>Set the weight of characters of a style. (Scintilla feature 2063)</summary>
        public void StyleSetWeight(int style, FontWeight weight)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_STYLESETWEIGHT, (IntPtr) style, (IntPtr) weight);
        }

        /// <summary>Get the weight of characters of a style. (Scintilla feature 2064)</summary>
        public FontWeight StyleGetWeight(int style)
        {
            return (FontWeight)Win32.SendMessage(scintilla, SciMsg.SCI_STYLEGETWEIGHT, (IntPtr) style, (IntPtr) Unused);
        }

        /// <summary>Set the character set of the font in a style. (Scintilla feature 2066)</summary>
        public void StyleSetCharacterSet(int style, CharacterSet characterSet)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_STYLESETCHARACTERSET, (IntPtr) style, (IntPtr) characterSet);
        }

        /// <summary>Set a style to be a hotspot or not. (Scintilla feature 2409)</summary>
        public void StyleSetHotSpot(int style, bool hotspot)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_STYLESETHOTSPOT, (IntPtr) style, new IntPtr(hotspot ? 1 : 0));
        }

        /// <summary>Set the foreground colour of the main and additional selections and whether to use this setting. (Scintilla feature 2067)</summary>
        public void SetSelFore(bool useSetting, Colour fore)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETSELFORE, new IntPtr(useSetting ? 1 : 0), fore.Value);
        }

        /// <summary>Set the background colour of the main and additional selections and whether to use this setting. (Scintilla feature 2068)</summary>
        public void SetSelBack(bool useSetting, Colour back)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETSELBACK, new IntPtr(useSetting ? 1 : 0), back.Value);
        }

        /// <summary>Get the alpha of the selection. (Scintilla feature 2477)</summary>
        public Alpha GetSelAlpha()
        {
            return (Alpha)Win32.SendMessage(scintilla, SciMsg.SCI_GETSELALPHA, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set the alpha of the selection. (Scintilla feature 2478)</summary>
        public void SetSelAlpha(Alpha alpha)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETSELALPHA, (IntPtr) alpha, (IntPtr) Unused);
        }

        /// <summary>Is the selection end of line filled? (Scintilla feature 2479)</summary>
        public bool GetSelEOLFilled()
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETSELEOLFILLED, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set the selection to have its end of line filled or not. (Scintilla feature 2480)</summary>
        public void SetSelEOLFilled(bool filled)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETSELEOLFILLED, new IntPtr(filled ? 1 : 0), (IntPtr) Unused);
        }

        /// <summary>Set the foreground colour of the caret. (Scintilla feature 2069)</summary>
        public void SetCaretFore(Colour fore)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETCARETFORE, fore.Value, (IntPtr) Unused);
        }

        /// <summary>When key+modifier combination keyDefinition is pressed perform sciCommand. (Scintilla feature 2070)</summary>
        public void AssignCmdKey(KeyModifier keyDefinition, int sciCommand)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_ASSIGNCMDKEY, keyDefinition.Value, (IntPtr) sciCommand);
        }

        /// <summary>When key+modifier combination keyDefinition is pressed do nothing. (Scintilla feature 2071)</summary>
        public void ClearCmdKey(KeyModifier keyDefinition)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_CLEARCMDKEY, keyDefinition.Value, (IntPtr) Unused);
        }

        /// <summary>Drop all key mappings. (Scintilla feature 2072)</summary>
        public void ClearAllCmdKeys()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_CLEARALLCMDKEYS, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set the styles for a segment of the document. (Scintilla feature 2073)</summary>
        public unsafe void SetStylingEx(int length, string styles)
        {
            fixed (byte* stylesPtr = Encoding.UTF8.GetBytes(styles))
            {
                Win32.SendMessage(scintilla, SciMsg.SCI_SETSTYLINGEX, (IntPtr) length, (IntPtr) stylesPtr);
            }
        }

        /// <summary>Set a style to be visible or not. (Scintilla feature 2074)</summary>
        public void StyleSetVisible(int style, bool visible)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_STYLESETVISIBLE, (IntPtr) style, new IntPtr(visible ? 1 : 0));
        }

        /// <summary>Get the time in milliseconds that the caret is on and off. (Scintilla feature 2075)</summary>
        public int GetCaretPeriod()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETCARETPERIOD, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Get the time in milliseconds that the caret is on and off. 0 = steady on. (Scintilla feature 2076)</summary>
        public void SetCaretPeriod(int periodMilliseconds)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETCARETPERIOD, (IntPtr) periodMilliseconds, (IntPtr) Unused);
        }

        /// <summary>
        /// Set the set of characters making up words for when moving or selecting by word.
        /// First sets defaults like SetCharsDefault.
        /// (Scintilla feature 2077)
        /// </summary>
        public unsafe void SetWordChars(string characters)
        {
            fixed (byte* charactersPtr = Encoding.UTF8.GetBytes(characters))
            {
                Win32.SendMessage(scintilla, SciMsg.SCI_SETWORDCHARS, (IntPtr) Unused, (IntPtr) charactersPtr);
            }
        }

        /// <summary>
        /// Get the set of characters making up words for when moving or selecting by word.
        /// Returns the number of characters
        /// (Scintilla feature 2646)
        /// </summary>
        public unsafe string GetWordChars()
        {
            return GetNullStrippedStringFromMessageThatReturnsLength(SciMsg.SCI_GETWORDCHARS);
        }

        /// <summary>Set the number of characters to have directly indexed categories (Scintilla feature 2720)</summary>
        public void SetCharacterCategoryOptimization(int countCharacters)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETCHARACTERCATEGORYOPTIMIZATION, (IntPtr) countCharacters, (IntPtr) Unused);
        }

        /// <summary>Get the number of characters to have directly indexed categories (Scintilla feature 2721)</summary>
        public int GetCharacterCategoryOptimization()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETCHARACTERCATEGORYOPTIMIZATION, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Start a sequence of actions that is undone and redone as a unit.
        /// May be nested.
        /// (Scintilla feature 2078)
        /// </summary>
        public void BeginUndoAction()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_BEGINUNDOACTION, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>End a sequence of actions that is undone and redone as a unit. (Scintilla feature 2079)</summary>
        public void EndUndoAction()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_ENDUNDOACTION, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set an indicator to plain, squiggle or TT. (Scintilla feature 2080)</summary>
        public void IndicSetStyle(int indicator, IndicatorStyle indicatorStyle)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_INDICSETSTYLE, (IntPtr) indicator, (IntPtr) indicatorStyle);
        }

        /// <summary>Retrieve the style of an indicator. (Scintilla feature 2081)</summary>
        public IndicatorStyle IndicGetStyle(int indicator)
        {
            return (IndicatorStyle)Win32.SendMessage(scintilla, SciMsg.SCI_INDICGETSTYLE, (IntPtr) indicator, (IntPtr) Unused);
        }

        /// <summary>Set the foreground colour of an indicator. (Scintilla feature 2082)</summary>
        public void IndicSetFore(int indicator, Colour fore)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_INDICSETFORE, (IntPtr) indicator, fore.Value);
        }

        /// <summary>Retrieve the foreground colour of an indicator. (Scintilla feature 2083)</summary>
        public Colour IndicGetFore(int indicator)
        {
            return new Colour((int) Win32.SendMessage(scintilla, SciMsg.SCI_INDICGETFORE, (IntPtr) indicator, (IntPtr) Unused));
        }

        /// <summary>Set an indicator to draw under text or over(default). (Scintilla feature 2510)</summary>
        public void IndicSetUnder(int indicator, bool under)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_INDICSETUNDER, (IntPtr) indicator, new IntPtr(under ? 1 : 0));
        }

        /// <summary>Retrieve whether indicator drawn under or over text. (Scintilla feature 2511)</summary>
        public bool IndicGetUnder(int indicator)
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_INDICGETUNDER, (IntPtr) indicator, (IntPtr) Unused);
        }

        /// <summary>Set a hover indicator to plain, squiggle or TT. (Scintilla feature 2680)</summary>
        public void IndicSetHoverStyle(int indicator, IndicatorStyle indicatorStyle)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_INDICSETHOVERSTYLE, (IntPtr) indicator, (IntPtr) indicatorStyle);
        }

        /// <summary>Retrieve the hover style of an indicator. (Scintilla feature 2681)</summary>
        public IndicatorStyle IndicGetHoverStyle(int indicator)
        {
            return (IndicatorStyle)Win32.SendMessage(scintilla, SciMsg.SCI_INDICGETHOVERSTYLE, (IntPtr) indicator, (IntPtr) Unused);
        }

        /// <summary>Set the foreground hover colour of an indicator. (Scintilla feature 2682)</summary>
        public void IndicSetHoverFore(int indicator, Colour fore)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_INDICSETHOVERFORE, (IntPtr) indicator, fore.Value);
        }

        /// <summary>Retrieve the foreground hover colour of an indicator. (Scintilla feature 2683)</summary>
        public Colour IndicGetHoverFore(int indicator)
        {
            return new Colour((int) Win32.SendMessage(scintilla, SciMsg.SCI_INDICGETHOVERFORE, (IntPtr) indicator, (IntPtr) Unused));
        }

        /// <summary>Set the attributes of an indicator. (Scintilla feature 2684)</summary>
        public void IndicSetFlags(int indicator, IndicFlag flags)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_INDICSETFLAGS, (IntPtr) indicator, (IntPtr) flags);
        }

        /// <summary>Retrieve the attributes of an indicator. (Scintilla feature 2685)</summary>
        public IndicFlag IndicGetFlags(int indicator)
        {
            return (IndicFlag)Win32.SendMessage(scintilla, SciMsg.SCI_INDICGETFLAGS, (IntPtr) indicator, (IntPtr) Unused);
        }

        /// <summary>Set the foreground colour of all whitespace and whether to use this setting. (Scintilla feature 2084)</summary>
        public void SetWhitespaceFore(bool useSetting, Colour fore)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETWHITESPACEFORE, new IntPtr(useSetting ? 1 : 0), fore.Value);
        }

        /// <summary>Set the background colour of all whitespace and whether to use this setting. (Scintilla feature 2085)</summary>
        public void SetWhitespaceBack(bool useSetting, Colour back)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETWHITESPACEBACK, new IntPtr(useSetting ? 1 : 0), back.Value);
        }

        /// <summary>Set the size of the dots used to mark space characters. (Scintilla feature 2086)</summary>
        public void SetWhitespaceSize(int size)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETWHITESPACESIZE, (IntPtr) size, (IntPtr) Unused);
        }

        /// <summary>Get the size of the dots used to mark space characters. (Scintilla feature 2087)</summary>
        public int GetWhitespaceSize()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETWHITESPACESIZE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Used to hold extra styling information for each line. (Scintilla feature 2092)</summary>
        public void SetLineState(int line, int state)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETLINESTATE, (IntPtr) line, (IntPtr) state);
        }

        /// <summary>Retrieve the extra styling information for a line. (Scintilla feature 2093)</summary>
        public int GetLineState(int line)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETLINESTATE, (IntPtr) line, (IntPtr) Unused);
        }

        /// <summary>Retrieve the last line number that has line state. (Scintilla feature 2094)</summary>
        public int GetMaxLineState()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETMAXLINESTATE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Is the background of the line containing the caret in a different colour? (Scintilla feature 2095)</summary>
        public bool GetCaretLineVisible()
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETCARETLINEVISIBLE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Display the background of the line containing the caret in a different colour. (Scintilla feature 2096)</summary>
        public void SetCaretLineVisible(bool show)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETCARETLINEVISIBLE, new IntPtr(show ? 1 : 0), (IntPtr) Unused);
        }

        /// <summary>Get the colour of the background of the line containing the caret. (Scintilla feature 2097)</summary>
        public Colour GetCaretLineBack()
        {
            return new Colour((int) Win32.SendMessage(scintilla, SciMsg.SCI_GETCARETLINEBACK, (IntPtr) Unused, (IntPtr) Unused));
        }

        /// <summary>Set the colour of the background of the line containing the caret. (Scintilla feature 2098)</summary>
        public void SetCaretLineBack(Colour back)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETCARETLINEBACK, back.Value, (IntPtr) Unused);
        }

        /// <summary>
        /// Retrieve the caret line frame width.
        /// Width = 0 means this option is disabled.
        /// (Scintilla feature 2704)
        /// </summary>
        public int GetCaretLineFrame()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETCARETLINEFRAME, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Display the caret line framed.
        /// Set width != 0 to enable this option and width = 0 to disable it.
        /// (Scintilla feature 2705)
        /// </summary>
        public void SetCaretLineFrame(int width)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETCARETLINEFRAME, (IntPtr) width, (IntPtr) Unused);
        }

        /// <summary>
        /// Set a style to be changeable or not (read only).
        /// Experimental feature, currently buggy.
        /// (Scintilla feature 2099)
        /// </summary>
        public void StyleSetChangeable(int style, bool changeable)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_STYLESETCHANGEABLE, (IntPtr) style, new IntPtr(changeable ? 1 : 0));
        }

        /// <summary>
        /// Display a auto-completion list.
        /// The lengthEntered parameter indicates how many characters before
        /// the caret should be used to provide context.
        /// (Scintilla feature 2100)
        /// </summary>
        public unsafe void AutoCShow(int lengthEntered, string itemList)
        {
            fixed (byte* itemListPtr = Encoding.UTF8.GetBytes(itemList))
            {
                Win32.SendMessage(scintilla, SciMsg.SCI_AUTOCSHOW, (IntPtr) lengthEntered, (IntPtr) itemListPtr);
            }
        }

        /// <summary>Remove the auto-completion list from the screen. (Scintilla feature 2101)</summary>
        public void AutoCCancel()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_AUTOCCANCEL, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Is there an auto-completion list visible? (Scintilla feature 2102)</summary>
        public bool AutoCActive()
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_AUTOCACTIVE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Retrieve the position of the caret when the auto-completion list was displayed. (Scintilla feature 2103)</summary>
        public int AutoCPosStart()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_AUTOCPOSSTART, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>User has selected an item so remove the list and insert the selection. (Scintilla feature 2104)</summary>
        public void AutoCComplete()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_AUTOCCOMPLETE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Define a set of character that when typed cancel the auto-completion list. (Scintilla feature 2105)</summary>
        public unsafe void AutoCStops(string characterSet)
        {
            fixed (byte* characterSetPtr = Encoding.UTF8.GetBytes(characterSet))
            {
                Win32.SendMessage(scintilla, SciMsg.SCI_AUTOCSTOPS, (IntPtr) Unused, (IntPtr) characterSetPtr);
            }
        }

        /// <summary>
        /// Change the separator character in the string setting up an auto-completion list.
        /// Default is space but can be changed if items contain space.
        /// (Scintilla feature 2106)
        /// </summary>
        public void AutoCSetSeparator(int separatorCharacter)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_AUTOCSETSEPARATOR, (IntPtr) separatorCharacter, (IntPtr) Unused);
        }

        /// <summary>Retrieve the auto-completion list separator character. (Scintilla feature 2107)</summary>
        public int AutoCGetSeparator()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_AUTOCGETSEPARATOR, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Select the item in the auto-completion list that starts with a string. (Scintilla feature 2108)</summary>
        public unsafe void AutoCSelect(string select)
        {
            fixed (byte* selectPtr = Encoding.UTF8.GetBytes(select))
            {
                Win32.SendMessage(scintilla, SciMsg.SCI_AUTOCSELECT, (IntPtr) Unused, (IntPtr) selectPtr);
            }
        }

        /// <summary>
        /// Should the auto-completion list be cancelled if the user backspaces to a
        /// position before where the box was created.
        /// (Scintilla feature 2110)
        /// </summary>
        public void AutoCSetCancelAtStart(bool cancel)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_AUTOCSETCANCELATSTART, new IntPtr(cancel ? 1 : 0), (IntPtr) Unused);
        }

        /// <summary>Retrieve whether auto-completion cancelled by backspacing before start. (Scintilla feature 2111)</summary>
        public bool AutoCGetCancelAtStart()
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_AUTOCGETCANCELATSTART, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Define a set of characters that when typed will cause the autocompletion to
        /// choose the selected item.
        /// (Scintilla feature 2112)
        /// </summary>
        public unsafe void AutoCSetFillUps(string characterSet)
        {
            fixed (byte* characterSetPtr = Encoding.UTF8.GetBytes(characterSet))
            {
                Win32.SendMessage(scintilla, SciMsg.SCI_AUTOCSETFILLUPS, (IntPtr) Unused, (IntPtr) characterSetPtr);
            }
        }

        /// <summary>Should a single item auto-completion list automatically choose the item. (Scintilla feature 2113)</summary>
        public void AutoCSetChooseSingle(bool chooseSingle)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_AUTOCSETCHOOSESINGLE, new IntPtr(chooseSingle ? 1 : 0), (IntPtr) Unused);
        }

        /// <summary>Retrieve whether a single item auto-completion list automatically choose the item. (Scintilla feature 2114)</summary>
        public bool AutoCGetChooseSingle()
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_AUTOCGETCHOOSESINGLE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set whether case is significant when performing auto-completion searches. (Scintilla feature 2115)</summary>
        public void AutoCSetIgnoreCase(bool ignoreCase)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_AUTOCSETIGNORECASE, new IntPtr(ignoreCase ? 1 : 0), (IntPtr) Unused);
        }

        /// <summary>Retrieve state of ignore case flag. (Scintilla feature 2116)</summary>
        public bool AutoCGetIgnoreCase()
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_AUTOCGETIGNORECASE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Display a list of strings and send notification when user chooses one. (Scintilla feature 2117)</summary>
        public unsafe void UserListShow(int listType, string itemList)
        {
            fixed (byte* itemListPtr = Encoding.UTF8.GetBytes(itemList))
            {
                Win32.SendMessage(scintilla, SciMsg.SCI_USERLISTSHOW, (IntPtr) listType, (IntPtr) itemListPtr);
            }
        }

        /// <summary>Set whether or not autocompletion is hidden automatically when nothing matches. (Scintilla feature 2118)</summary>
        public void AutoCSetAutoHide(bool autoHide)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_AUTOCSETAUTOHIDE, new IntPtr(autoHide ? 1 : 0), (IntPtr) Unused);
        }

        /// <summary>Retrieve whether or not autocompletion is hidden automatically when nothing matches. (Scintilla feature 2119)</summary>
        public bool AutoCGetAutoHide()
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_AUTOCGETAUTOHIDE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Set whether or not autocompletion deletes any word characters
        /// after the inserted text upon completion.
        /// (Scintilla feature 2270)
        /// </summary>
        public void AutoCSetDropRestOfWord(bool dropRestOfWord)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_AUTOCSETDROPRESTOFWORD, new IntPtr(dropRestOfWord ? 1 : 0), (IntPtr) Unused);
        }

        /// <summary>
        /// Retrieve whether or not autocompletion deletes any word characters
        /// after the inserted text upon completion.
        /// (Scintilla feature 2271)
        /// </summary>
        public bool AutoCGetDropRestOfWord()
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_AUTOCGETDROPRESTOFWORD, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Register an XPM image for use in autocompletion lists. (Scintilla feature 2405)</summary>
        public unsafe void RegisterImage(int type, string xpmData)
        {
            fixed (byte* xpmDataPtr = Encoding.UTF8.GetBytes(xpmData))
            {
                Win32.SendMessage(scintilla, SciMsg.SCI_REGISTERIMAGE, (IntPtr) type, (IntPtr) xpmDataPtr);
            }
        }

        /// <summary>Clear all the registered XPM images. (Scintilla feature 2408)</summary>
        public void ClearRegisteredImages()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_CLEARREGISTEREDIMAGES, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Retrieve the auto-completion list type-separator character. (Scintilla feature 2285)</summary>
        public int AutoCGetTypeSeparator()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_AUTOCGETTYPESEPARATOR, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Change the type-separator character in the string setting up an auto-completion list.
        /// Default is '?' but can be changed if items contain '?'.
        /// (Scintilla feature 2286)
        /// </summary>
        public void AutoCSetTypeSeparator(int separatorCharacter)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_AUTOCSETTYPESEPARATOR, (IntPtr) separatorCharacter, (IntPtr) Unused);
        }

        /// <summary>
        /// Set the maximum width, in characters, of auto-completion and user lists.
        /// Set to 0 to autosize to fit longest item, which is the default.
        /// (Scintilla feature 2208)
        /// </summary>
        public void AutoCSetMaxWidth(int characterCount)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_AUTOCSETMAXWIDTH, (IntPtr) characterCount, (IntPtr) Unused);
        }

        /// <summary>Get the maximum width, in characters, of auto-completion and user lists. (Scintilla feature 2209)</summary>
        public int AutoCGetMaxWidth()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_AUTOCGETMAXWIDTH, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Set the maximum height, in rows, of auto-completion and user lists.
        /// The default is 5 rows.
        /// (Scintilla feature 2210)
        /// </summary>
        public void AutoCSetMaxHeight(int rowCount)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_AUTOCSETMAXHEIGHT, (IntPtr) rowCount, (IntPtr) Unused);
        }

        /// <summary>Set the maximum height, in rows, of auto-completion and user lists. (Scintilla feature 2211)</summary>
        public int AutoCGetMaxHeight()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_AUTOCGETMAXHEIGHT, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set the number of spaces used for one level of indentation. (Scintilla feature 2122)</summary>
        public void SetIndent(int indentSize)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETINDENT, (IntPtr) indentSize, (IntPtr) Unused);
        }

        /// <summary>Retrieve indentation size. (Scintilla feature 2123)</summary>
        public int GetIndent()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETINDENT, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Indentation will only use space characters if useTabs is false, otherwise
        /// it will use a combination of tabs and spaces.
        /// (Scintilla feature 2124)
        /// </summary>
        public void SetUseTabs(bool useTabs)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETUSETABS, new IntPtr(useTabs ? 1 : 0), (IntPtr) Unused);
        }

        /// <summary>Retrieve whether tabs will be used in indentation. (Scintilla feature 2125)</summary>
        public bool GetUseTabs()
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETUSETABS, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Change the indentation of a line to a number of columns. (Scintilla feature 2126)</summary>
        public void SetLineIndentation(int line, int indentation)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETLINEINDENTATION, (IntPtr) line, (IntPtr) indentation);
        }

        /// <summary>Retrieve the number of columns that a line is indented. (Scintilla feature 2127)</summary>
        public int GetLineIndentation(int line)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETLINEINDENTATION, (IntPtr) line, (IntPtr) Unused);
        }

        /// <summary>Retrieve the position before the first non indentation character on a line. (Scintilla feature 2128)</summary>
        public int GetLineIndentPosition(int line)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETLINEINDENTPOSITION, (IntPtr) line, (IntPtr) Unused);
        }

        /// <summary>Retrieve the column number of a position, taking tab width into account. (Scintilla feature 2129)</summary>
        public int GetColumn(int pos)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETCOLUMN, (IntPtr) pos, (IntPtr) Unused);
        }

        /// <summary>Count characters between two positions. (Scintilla feature 2633)</summary>
        public int CountCharacters(int start, int end)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_COUNTCHARACTERS, (IntPtr) start, (IntPtr) end);
        }

        /// <summary>Count code units between two positions. (Scintilla feature 2715)</summary>
        public int CountCodeUnits(int start, int end)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_COUNTCODEUNITS, (IntPtr) start, (IntPtr) end);
        }

        /// <summary>Show or hide the horizontal scroll bar. (Scintilla feature 2130)</summary>
        public void SetHScrollBar(bool visible)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETHSCROLLBAR, new IntPtr(visible ? 1 : 0), (IntPtr) Unused);
        }

        /// <summary>Is the horizontal scroll bar visible? (Scintilla feature 2131)</summary>
        public bool GetHScrollBar()
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETHSCROLLBAR, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Show or hide indentation guides. (Scintilla feature 2132)</summary>
        public void SetIndentationGuides(IndentView indentView)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETINDENTATIONGUIDES, (IntPtr) indentView, (IntPtr) Unused);
        }

        /// <summary>Are the indentation guides visible? (Scintilla feature 2133)</summary>
        public IndentView GetIndentationGuides()
        {
            return (IndentView)Win32.SendMessage(scintilla, SciMsg.SCI_GETINDENTATIONGUIDES, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Set the highlighted indentation guide column.
        /// 0 = no highlighted guide.
        /// (Scintilla feature 2134)
        /// </summary>
        public void SetHighlightGuide(int column)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETHIGHLIGHTGUIDE, (IntPtr) column, (IntPtr) Unused);
        }

        /// <summary>Get the highlighted indentation guide column. (Scintilla feature 2135)</summary>
        public int GetHighlightGuide()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETHIGHLIGHTGUIDE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Get the position after the last visible characters on a line. (Scintilla feature 2136)</summary>
        public int GetLineEndPosition(int line)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETLINEENDPOSITION, (IntPtr) line, (IntPtr) Unused);
        }

        /// <summary>Get the code page used to interpret the bytes of the document as characters. (Scintilla feature 2137)</summary>
        public int GetCodePage()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETCODEPAGE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Get the foreground colour of the caret. (Scintilla feature 2138)</summary>
        public Colour GetCaretFore()
        {
            return new Colour((int) Win32.SendMessage(scintilla, SciMsg.SCI_GETCARETFORE, (IntPtr) Unused, (IntPtr) Unused));
        }

        /// <summary>In read-only mode? (Scintilla feature 2140)</summary>
        public bool GetReadOnly()
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETREADONLY, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Sets the position of the caret. (Scintilla feature 2141)</summary>
        public void SetCurrentPos(int caret)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETCURRENTPOS, (IntPtr) caret, (IntPtr) Unused);
        }

        /// <summary>Sets the position that starts the selection - this becomes the anchor. (Scintilla feature 2142)</summary>
        public void SetSelectionStart(int anchor)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETSELECTIONSTART, (IntPtr) anchor, (IntPtr) Unused);
        }

        /// <summary>Returns the position at the start of the selection. (Scintilla feature 2143)</summary>
        public int GetSelectionStart()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETSELECTIONSTART, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Sets the position that ends the selection - this becomes the caret. (Scintilla feature 2144)</summary>
        public void SetSelectionEnd(int caret)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETSELECTIONEND, (IntPtr) caret, (IntPtr) Unused);
        }

        /// <summary>Returns the position at the end of the selection. (Scintilla feature 2145)</summary>
        public int GetSelectionEnd()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETSELECTIONEND, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set caret to a position, while removing any existing selection. (Scintilla feature 2556)</summary>
        public void SetEmptySelection(int caret)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETEMPTYSELECTION, (IntPtr) caret, (IntPtr) Unused);
        }

        /// <summary>Sets the print magnification added to the point size of each style for printing. (Scintilla feature 2146)</summary>
        public void SetPrintMagnification(int magnification)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETPRINTMAGNIFICATION, (IntPtr) magnification, (IntPtr) Unused);
        }

        /// <summary>Returns the print magnification. (Scintilla feature 2147)</summary>
        public int GetPrintMagnification()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETPRINTMAGNIFICATION, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Modify colours when printing for clearer printed text. (Scintilla feature 2148)</summary>
        public void SetPrintColourMode(PrintOption mode)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETPRINTCOLOURMODE, (IntPtr) mode, (IntPtr) Unused);
        }

        /// <summary>Returns the print colour mode. (Scintilla feature 2149)</summary>
        public PrintOption GetPrintColourMode()
        {
            return (PrintOption)Win32.SendMessage(scintilla, SciMsg.SCI_GETPRINTCOLOURMODE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Find some text in the document. (Scintilla feature 2150)</summary>
        public int FindText(FindOption searchFlags, TextToFind ft)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_FINDTEXT, (IntPtr) searchFlags, ft.NativePointer);
        }

        /// <summary>Retrieve the display line at the top of the display. (Scintilla feature 2152)</summary>
        public int GetFirstVisibleLine()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETFIRSTVISIBLELINE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Retrieve the contents of a line.
        /// Returns the length of the line.
        /// (Scintilla feature 2153)
        /// </summary>
        public unsafe string GetLine(int line)
        {
            return GetNullStrippedStringFromMessageThatReturnsLength(SciMsg.SCI_GETLINE, (IntPtr)line);
        }

        /// <summary>Returns the number of lines in the document. There is always at least one. (Scintilla feature 2154)</summary>
        public int GetLineCount()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETLINECOUNT, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Sets the size in pixels of the left margin. (Scintilla feature 2155)</summary>
        public void SetMarginLeft(int pixelWidth)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETMARGINLEFT, (IntPtr) Unused, (IntPtr) pixelWidth);
        }

        /// <summary>Returns the size in pixels of the left margin. (Scintilla feature 2156)</summary>
        public int GetMarginLeft()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETMARGINLEFT, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Sets the size in pixels of the right margin. (Scintilla feature 2157)</summary>
        public void SetMarginRight(int pixelWidth)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETMARGINRIGHT, (IntPtr) Unused, (IntPtr) pixelWidth);
        }

        /// <summary>Returns the size in pixels of the right margin. (Scintilla feature 2158)</summary>
        public int GetMarginRight()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETMARGINRIGHT, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Is the document different from when it was last saved? (Scintilla feature 2159)</summary>
        public bool GetModify()
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETMODIFY, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Select a range of text. (Scintilla feature 2160)</summary>
        public void SetSel(int anchor, int caret)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETSEL, (IntPtr) anchor, (IntPtr) caret);
        }

        /// <summary>
        /// Retrieve the selected text.
        /// Return the length of the text.
        /// Result is NUL-terminated.
        /// (Scintilla feature 2161)
        /// </summary>
        public unsafe string GetSelText()
        {
            return GetNullStrippedStringFromMessageThatReturnsLength(SciMsg.SCI_GETSELTEXT);
        }

        /// <summary>
        /// Retrieve a range of text.
        /// Return the length of the text.
        /// (Scintilla feature 2162)
        /// </summary>
        public int GetTextRange(TextRange tr)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETTEXTRANGE, (IntPtr) Unused, tr.NativePointer);
        }

        /// <summary>Draw the selection either highlighted or in normal (non-highlighted) style. (Scintilla feature 2163)</summary>
        public void HideSelection(bool hide)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_HIDESELECTION, new IntPtr(hide ? 1 : 0), (IntPtr) Unused);
        }

        /// <summary>Retrieve the x value of the point in the window where a position is displayed. (Scintilla feature 2164)</summary>
        public int PointXFromPosition(int pos)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_POINTXFROMPOSITION, (IntPtr) Unused, (IntPtr) pos);
        }

        /// <summary>Retrieve the y value of the point in the window where a position is displayed. (Scintilla feature 2165)</summary>
        public int PointYFromPosition(int pos)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_POINTYFROMPOSITION, (IntPtr) Unused, (IntPtr) pos);
        }

        /// <summary>Retrieve the line containing a position. (Scintilla feature 2166)</summary>
        public int LineFromPosition(int pos)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_LINEFROMPOSITION, (IntPtr) pos, (IntPtr) Unused);
        }

        /// <summary>Retrieve the position at the start of a line. (Scintilla feature 2167)</summary>
        public int PositionFromLine(int line)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_POSITIONFROMLINE, (IntPtr) line, (IntPtr) Unused);
        }

        /// <summary>Scroll horizontally and vertically. (Scintilla feature 2168)</summary>
        public void LineScroll(int columns, int lines)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_LINESCROLL, (IntPtr) columns, (IntPtr) lines);
        }

        /// <summary>Ensure the caret is visible. (Scintilla feature 2169)</summary>
        public void ScrollCaret()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SCROLLCARET, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Scroll the argument positions and the range between them into view giving
        /// priority to the primary position then the secondary position.
        /// This may be used to make a search match visible.
        /// (Scintilla feature 2569)
        /// </summary>
        public void ScrollRange(int secondary, int primary)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SCROLLRANGE, (IntPtr) secondary, (IntPtr) primary);
        }

        /// <summary>Replace the selected text with the argument text. (Scintilla feature 2170)</summary>
        public unsafe void ReplaceSel(string text)
        {
            fixed (byte* textPtr = Encoding.UTF8.GetBytes(text))
            {
                Win32.SendMessage(scintilla, SciMsg.SCI_REPLACESEL, (IntPtr) Unused, (IntPtr) textPtr);
            }
        }

        /// <summary>Set to read only or read write. (Scintilla feature 2171)</summary>
        public void SetReadOnly(bool readOnly)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETREADONLY, new IntPtr(readOnly ? 1 : 0), (IntPtr) Unused);
        }

        /// <summary>Null operation. (Scintilla feature 2172)</summary>
        public void Null()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_NULL, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Will a paste succeed? (Scintilla feature 2173)</summary>
        public bool CanPaste()
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_CANPASTE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Are there any undoable actions in the undo history? (Scintilla feature 2174)</summary>
        public bool CanUndo()
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_CANUNDO, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Delete the undo history. (Scintilla feature 2175)</summary>
        public void EmptyUndoBuffer()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_EMPTYUNDOBUFFER, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Undo one action in the undo history. (Scintilla feature 2176)</summary>
        public void Undo()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_UNDO, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Cut the selection to the clipboard. (Scintilla feature 2177)</summary>
        public void Cut()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_CUT, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Copy the selection to the clipboard. (Scintilla feature 2178)</summary>
        public void Copy()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_COPY, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Paste the contents of the clipboard into the document replacing the selection. (Scintilla feature 2179)</summary>
        public void Paste()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_PASTE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Clear the selection. (Scintilla feature 2180)</summary>
        public void Clear()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_CLEAR, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Replace the contents of the document with the argument text. (Scintilla feature 2181)</summary>
        public unsafe void SetText(string text)
        {
            fixed (byte* textPtr = GetNullTerminatedUTF8Bytes(text))
            {
                Win32.SendMessage(scintilla, SciMsg.SCI_SETTEXT, (IntPtr) Unused, (IntPtr) textPtr);
            }
        }

        public unsafe string GetText(int length = -1)
        {
            if (length < 1 && !TryGetLengthAsInt(out length))
                return "";
            byte[] textBuffer = new byte[length];
            fixed (byte* textPtr = textBuffer)
            {
                Win32.SendMessage(scintilla, SciMsg.SCI_GETTEXT, length, (IntPtr)textPtr);
                return Utf8BytesToNullStrippedString(textBuffer);
            }
        }

        /// <summary>Retrieve the number of characters in the document. (Scintilla feature 2183)</summary>
        public int GetTextLength()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETTEXTLENGTH, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Retrieve a pointer to a function that processes messages for this Scintilla. (Scintilla feature 2184)</summary>
        public IntPtr GetDirectFunction()
        {
            return Win32.SendMessage(scintilla, SciMsg.SCI_GETDIRECTFUNCTION, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Retrieve a pointer value to use as the first argument when calling
        /// the function returned by GetDirectFunction.
        /// (Scintilla feature 2185)
        /// </summary>
        public IntPtr GetDirectPointer()
        {
            return Win32.SendMessage(scintilla, SciMsg.SCI_GETDIRECTPOINTER, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set to overtype (true) or insert mode. (Scintilla feature 2186)</summary>
        public void SetOvertype(bool overType)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETOVERTYPE, new IntPtr(overType ? 1 : 0), (IntPtr) Unused);
        }

        /// <summary>Returns true if overtype mode is active otherwise false is returned. (Scintilla feature 2187)</summary>
        public bool GetOvertype()
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETOVERTYPE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set the width of the insert mode caret. (Scintilla feature 2188)</summary>
        public void SetCaretWidth(int pixelWidth)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETCARETWIDTH, (IntPtr) pixelWidth, (IntPtr) Unused);
        }

        /// <summary>Returns the width of the insert mode caret. (Scintilla feature 2189)</summary>
        public int GetCaretWidth()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETCARETWIDTH, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Sets the position that starts the target which is used for updating the
        /// document without affecting the scroll position.
        /// (Scintilla feature 2190)
        /// </summary>
        public void SetTargetStart(int start)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETTARGETSTART, (IntPtr) start, (IntPtr) Unused);
        }

        /// <summary>Get the position that starts the target. (Scintilla feature 2191)</summary>
        public int GetTargetStart()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETTARGETSTART, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Sets the position that ends the target which is used for updating the
        /// document without affecting the scroll position.
        /// (Scintilla feature 2192)
        /// </summary>
        public void SetTargetEnd(int end)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETTARGETEND, (IntPtr) end, (IntPtr) Unused);
        }

        /// <summary>Get the position that ends the target. (Scintilla feature 2193)</summary>
        public int GetTargetEnd()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETTARGETEND, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Sets both the start and end of the target in one call. (Scintilla feature 2686)</summary>
        public void SetTargetRange(int start, int end)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETTARGETRANGE, (IntPtr) start, (IntPtr) end);
        }

        /// <summary>Retrieve the text in the target. (Scintilla feature 2687)</summary>
        public unsafe string GetTargetText()
        {
            return GetNullStrippedStringFromMessageThatReturnsLength(SciMsg.SCI_GETTARGETTEXT);
        }

        /// <summary>Make the target range start and end be the same as the selection range start and end. (Scintilla feature 2287)</summary>
        public void TargetFromSelection()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_TARGETFROMSELECTION, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Sets the target to the whole document. (Scintilla feature 2690)</summary>
        public void TargetWholeDocument()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_TARGETWHOLEDOCUMENT, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Replace the target text with the argument text.
        /// Text is counted so it can contain NULs.
        /// Returns the length of the replacement text.
        /// (Scintilla feature 2194)
        /// </summary>
        public unsafe int ReplaceTarget(int length, string text)
        {
            fixed (byte* textPtr = Encoding.UTF8.GetBytes(text))
            {
                return (int)Win32.SendMessage(scintilla, SciMsg.SCI_REPLACETARGET, (IntPtr) length, (IntPtr) textPtr);
            }
        }

        /// <summary>
        /// Replace the target text with the argument text after \d processing.
        /// Text is counted so it can contain NULs.
        /// Looks for \d where d is between 1 and 9 and replaces these with the strings
        /// matched in the last search operation which were surrounded by \( and \).
        /// Returns the length of the replacement text including any change
        /// caused by processing the \d patterns.
        /// (Scintilla feature 2195)
        /// </summary>
        public unsafe int ReplaceTargetRE(int length, string text)
        {
            fixed (byte* textPtr = Encoding.UTF8.GetBytes(text))
            {
                return (int)Win32.SendMessage(scintilla, SciMsg.SCI_REPLACETARGETRE, (IntPtr) length, (IntPtr) textPtr);
            }
        }

        /// <summary>
        /// Search for a counted string in the target and set the target to the found
        /// range. Text is counted so it can contain NULs.
        /// Returns start of found range or -1 for failure in which case target is not moved.
        /// (Scintilla feature 2197)
        /// </summary>
        public unsafe int SearchInTarget(int length, string text)
        {
            fixed (byte* textPtr = Encoding.UTF8.GetBytes(text))
            {
                return (int)Win32.SendMessage(scintilla, SciMsg.SCI_SEARCHINTARGET, (IntPtr) length, (IntPtr) textPtr);
            }
        }

        /// <summary>Set the search flags used by SearchInTarget. (Scintilla feature 2198)</summary>
        public void SetSearchFlags(FindOption searchFlags)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETSEARCHFLAGS, (IntPtr) searchFlags, (IntPtr) Unused);
        }

        /// <summary>Get the search flags used by SearchInTarget. (Scintilla feature 2199)</summary>
        public FindOption GetSearchFlags()
        {
            return (FindOption)Win32.SendMessage(scintilla, SciMsg.SCI_GETSEARCHFLAGS, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Show a call tip containing a definition near position pos. (Scintilla feature 2200)</summary>
        public unsafe void CallTipShow(int pos, string definition)
        {
            fixed (byte* definitionPtr = Encoding.UTF8.GetBytes(definition))
            {
                Win32.SendMessage(scintilla, SciMsg.SCI_CALLTIPSHOW, (IntPtr) pos, (IntPtr) definitionPtr);
            }
        }

        /// <summary>Remove the call tip from the screen. (Scintilla feature 2201)</summary>
        public void CallTipCancel()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_CALLTIPCANCEL, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Is there an active call tip? (Scintilla feature 2202)</summary>
        public bool CallTipActive()
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_CALLTIPACTIVE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Retrieve the position where the caret was before displaying the call tip. (Scintilla feature 2203)</summary>
        public int CallTipPosStart()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_CALLTIPPOSSTART, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set the start position in order to change when backspacing removes the calltip. (Scintilla feature 2214)</summary>
        public void CallTipSetPosStart(int posStart)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_CALLTIPSETPOSSTART, (IntPtr) posStart, (IntPtr) Unused);
        }

        /// <summary>Highlight a segment of the definition. (Scintilla feature 2204)</summary>
        public void CallTipSetHlt(int highlightStart, int highlightEnd)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_CALLTIPSETHLT, (IntPtr) highlightStart, (IntPtr) highlightEnd);
        }

        /// <summary>Set the background colour for the call tip. (Scintilla feature 2205)</summary>
        public void CallTipSetBack(Colour back)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_CALLTIPSETBACK, back.Value, (IntPtr) Unused);
        }

        /// <summary>Set the foreground colour for the call tip. (Scintilla feature 2206)</summary>
        public void CallTipSetFore(Colour fore)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_CALLTIPSETFORE, fore.Value, (IntPtr) Unused);
        }

        /// <summary>Set the foreground colour for the highlighted part of the call tip. (Scintilla feature 2207)</summary>
        public void CallTipSetForeHlt(Colour fore)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_CALLTIPSETFOREHLT, fore.Value, (IntPtr) Unused);
        }

        /// <summary>Enable use of STYLE_CALLTIP and set call tip tab size in pixels. (Scintilla feature 2212)</summary>
        public void CallTipUseStyle(int tabSize)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_CALLTIPUSESTYLE, (IntPtr) tabSize, (IntPtr) Unused);
        }

        /// <summary>Set position of calltip, above or below text. (Scintilla feature 2213)</summary>
        public void CallTipSetPosition(bool above)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_CALLTIPSETPOSITION, new IntPtr(above ? 1 : 0), (IntPtr) Unused);
        }

        /// <summary>Find the display line of a document line taking hidden lines into account. (Scintilla feature 2220)</summary>
        public int VisibleFromDocLine(int docLine)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_VISIBLEFROMDOCLINE, (IntPtr) docLine, (IntPtr) Unused);
        }

        /// <summary>Find the document line of a display line taking hidden lines into account. (Scintilla feature 2221)</summary>
        public int DocLineFromVisible(int displayLine)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_DOCLINEFROMVISIBLE, (IntPtr) displayLine, (IntPtr) Unused);
        }

        /// <summary>The number of display lines needed to wrap a document line (Scintilla feature 2235)</summary>
        public int WrapCount(int docLine)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_WRAPCOUNT, (IntPtr) docLine, (IntPtr) Unused);
        }

        /// <summary>
        /// Set the fold level of a line.
        /// This encodes an integer level along with flags indicating whether the
        /// line is a header and whether it is effectively white space.
        /// (Scintilla feature 2222)
        /// </summary>
        public void SetFoldLevel(int line, FoldLevel level)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETFOLDLEVEL, (IntPtr) line, (IntPtr) level);
        }

        /// <summary>Retrieve the fold level of a line. (Scintilla feature 2223)</summary>
        public FoldLevel GetFoldLevel(int line)
        {
            return (FoldLevel)Win32.SendMessage(scintilla, SciMsg.SCI_GETFOLDLEVEL, (IntPtr) line, (IntPtr) Unused);
        }

        /// <summary>Find the last child line of a header line. (Scintilla feature 2224)</summary>
        public int GetLastChild(int line, FoldLevel level)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETLASTCHILD, (IntPtr) line, (IntPtr) level);
        }

        /// <summary>Find the parent line of a child line. (Scintilla feature 2225)</summary>
        public int GetFoldParent(int line)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETFOLDPARENT, (IntPtr) line, (IntPtr) Unused);
        }

        /// <summary>Make a range of lines visible. (Scintilla feature 2226)</summary>
        public void ShowLines(int lineStart, int lineEnd)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SHOWLINES, (IntPtr) lineStart, (IntPtr) lineEnd);
        }

        /// <summary>Make a range of lines invisible. (Scintilla feature 2227)</summary>
        public void HideLines(int lineStart, int lineEnd)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_HIDELINES, (IntPtr) lineStart, (IntPtr) lineEnd);
        }

        /// <summary>Is a line visible? (Scintilla feature 2228)</summary>
        public bool GetLineVisible(int line)
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETLINEVISIBLE, (IntPtr) line, (IntPtr) Unused);
        }

        /// <summary>Are all lines visible? (Scintilla feature 2236)</summary>
        public bool GetAllLinesVisible()
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETALLLINESVISIBLE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Show the children of a header line. (Scintilla feature 2229)</summary>
        public void SetFoldExpanded(int line, bool expanded)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETFOLDEXPANDED, (IntPtr) line, new IntPtr(expanded ? 1 : 0));
        }

        /// <summary>Is a header line expanded? (Scintilla feature 2230)</summary>
        public bool GetFoldExpanded(int line)
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETFOLDEXPANDED, (IntPtr) line, (IntPtr) Unused);
        }

        /// <summary>Switch a header line between expanded and contracted. (Scintilla feature 2231)</summary>
        public void ToggleFold(int line)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_TOGGLEFOLD, (IntPtr) line, (IntPtr) Unused);
        }

        /// <summary>Switch a header line between expanded and contracted and show some text after the line. (Scintilla feature 2700)</summary>
        public unsafe void ToggleFoldShowText(int line, string text)
        {
            fixed (byte* textPtr = Encoding.UTF8.GetBytes(text))
            {
                Win32.SendMessage(scintilla, SciMsg.SCI_TOGGLEFOLDSHOWTEXT, (IntPtr) line, (IntPtr) textPtr);
            }
        }

        /// <summary>Set the style of fold display text. (Scintilla feature 2701)</summary>
        public void FoldDisplayTextSetStyle(FoldDisplayTextStyle style)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_FOLDDISPLAYTEXTSETSTYLE, (IntPtr) style, (IntPtr) Unused);
        }

        /// <summary>Get the style of fold display text. (Scintilla feature 2707)</summary>
        public FoldDisplayTextStyle FoldDisplayTextGetStyle()
        {
            return (FoldDisplayTextStyle)Win32.SendMessage(scintilla, SciMsg.SCI_FOLDDISPLAYTEXTGETSTYLE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set the default fold display text. (Scintilla feature 2722)</summary>
        public unsafe void SetDefaultFoldDisplayText(string text)
        {
            fixed (byte* textPtr = Encoding.UTF8.GetBytes(text))
            {
                Win32.SendMessage(scintilla, SciMsg.SCI_SETDEFAULTFOLDDISPLAYTEXT, (IntPtr) Unused, (IntPtr) textPtr);
            }
        }

        /// <summary>Get the default fold display text. (Scintilla feature 2723)</summary>
        public unsafe string GetDefaultFoldDisplayText()
        {
            return GetNullStrippedStringFromMessageThatReturnsLength(SciMsg.SCI_GETDEFAULTFOLDDISPLAYTEXT);
        }

        /// <summary>Expand or contract a fold header. (Scintilla feature 2237)</summary>
        public void FoldLine(int line, FoldAction action)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_FOLDLINE, (IntPtr) line, (IntPtr) action);
        }

        /// <summary>Expand or contract a fold header and its children. (Scintilla feature 2238)</summary>
        public void FoldChildren(int line, FoldAction action)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_FOLDCHILDREN, (IntPtr) line, (IntPtr) action);
        }

        /// <summary>Expand a fold header and all children. Use the level argument instead of the line's current level. (Scintilla feature 2239)</summary>
        public void ExpandChildren(int line, FoldLevel level)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_EXPANDCHILDREN, (IntPtr) line, (IntPtr) level);
        }

        /// <summary>Expand or contract all fold headers. (Scintilla feature 2662)</summary>
        public void FoldAll(FoldAction action)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_FOLDALL, (IntPtr) action, (IntPtr) Unused);
        }

        /// <summary>Ensure a particular line is visible by expanding any header line hiding it. (Scintilla feature 2232)</summary>
        public void EnsureVisible(int line)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_ENSUREVISIBLE, (IntPtr) line, (IntPtr) Unused);
        }

        /// <summary>Set automatic folding behaviours. (Scintilla feature 2663)</summary>
        public void SetAutomaticFold(AutomaticFold automaticFold)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETAUTOMATICFOLD, (IntPtr) automaticFold, (IntPtr) Unused);
        }

        /// <summary>Get automatic folding behaviours. (Scintilla feature 2664)</summary>
        public AutomaticFold GetAutomaticFold()
        {
            return (AutomaticFold)Win32.SendMessage(scintilla, SciMsg.SCI_GETAUTOMATICFOLD, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set some style options for folding. (Scintilla feature 2233)</summary>
        public void SetFoldFlags(FoldFlag flags)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETFOLDFLAGS, (IntPtr) flags, (IntPtr) Unused);
        }

        /// <summary>
        /// Ensure a particular line is visible by expanding any header line hiding it.
        /// Use the currently set visibility policy to determine which range to display.
        /// (Scintilla feature 2234)
        /// </summary>
        public void EnsureVisibleEnforcePolicy(int line)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_ENSUREVISIBLEENFORCEPOLICY, (IntPtr) line, (IntPtr) Unused);
        }

        /// <summary>Sets whether a tab pressed when caret is within indentation indents. (Scintilla feature 2260)</summary>
        public void SetTabIndents(bool tabIndents)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETTABINDENTS, new IntPtr(tabIndents ? 1 : 0), (IntPtr) Unused);
        }

        /// <summary>Does a tab pressed when caret is within indentation indent? (Scintilla feature 2261)</summary>
        public bool GetTabIndents()
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETTABINDENTS, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Sets whether a backspace pressed when caret is within indentation unindents. (Scintilla feature 2262)</summary>
        public void SetBackSpaceUnIndents(bool bsUnIndents)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETBACKSPACEUNINDENTS, new IntPtr(bsUnIndents ? 1 : 0), (IntPtr) Unused);
        }

        /// <summary>Does a backspace pressed when caret is within indentation unindent? (Scintilla feature 2263)</summary>
        public bool GetBackSpaceUnIndents()
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETBACKSPACEUNINDENTS, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Sets the time the mouse must sit still to generate a mouse dwell event. (Scintilla feature 2264)</summary>
        public void SetMouseDwellTime(int periodMilliseconds)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETMOUSEDWELLTIME, (IntPtr) periodMilliseconds, (IntPtr) Unused);
        }

        /// <summary>Retrieve the time the mouse must sit still to generate a mouse dwell event. (Scintilla feature 2265)</summary>
        public int GetMouseDwellTime()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETMOUSEDWELLTIME, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Get position of start of word. (Scintilla feature 2266)</summary>
        public int WordStartPosition(int pos, bool onlyWordCharacters)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_WORDSTARTPOSITION, (IntPtr) pos, new IntPtr(onlyWordCharacters ? 1 : 0));
        }

        /// <summary>Get position of end of word. (Scintilla feature 2267)</summary>
        public int WordEndPosition(int pos, bool onlyWordCharacters)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_WORDENDPOSITION, (IntPtr) pos, new IntPtr(onlyWordCharacters ? 1 : 0));
        }

        /// <summary>Is the range start..end considered a word? (Scintilla feature 2691)</summary>
        public bool IsRangeWord(int start, int end)
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_ISRANGEWORD, (IntPtr) start, (IntPtr) end);
        }

        /// <summary>Sets limits to idle styling. (Scintilla feature 2692)</summary>
        public void SetIdleStyling(IdleStyling idleStyling)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETIDLESTYLING, (IntPtr) idleStyling, (IntPtr) Unused);
        }

        /// <summary>Retrieve the limits to idle styling. (Scintilla feature 2693)</summary>
        public IdleStyling GetIdleStyling()
        {
            return (IdleStyling)Win32.SendMessage(scintilla, SciMsg.SCI_GETIDLESTYLING, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Sets whether text is word wrapped. (Scintilla feature 2268)</summary>
        public void SetWrapMode(Wrap wrapMode)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETWRAPMODE, (IntPtr) wrapMode, (IntPtr) Unused);
        }

        /// <summary>Retrieve whether text is word wrapped. (Scintilla feature 2269)</summary>
        public Wrap GetWrapMode()
        {
            return (Wrap)Win32.SendMessage(scintilla, SciMsg.SCI_GETWRAPMODE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set the display mode of visual flags for wrapped lines. (Scintilla feature 2460)</summary>
        public void SetWrapVisualFlags(WrapVisualFlag wrapVisualFlags)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETWRAPVISUALFLAGS, (IntPtr) wrapVisualFlags, (IntPtr) Unused);
        }

        /// <summary>Retrive the display mode of visual flags for wrapped lines. (Scintilla feature 2461)</summary>
        public WrapVisualFlag GetWrapVisualFlags()
        {
            return (WrapVisualFlag)Win32.SendMessage(scintilla, SciMsg.SCI_GETWRAPVISUALFLAGS, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set the location of visual flags for wrapped lines. (Scintilla feature 2462)</summary>
        public void SetWrapVisualFlagsLocation(WrapVisualLocation wrapVisualFlagsLocation)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETWRAPVISUALFLAGSLOCATION, (IntPtr) wrapVisualFlagsLocation, (IntPtr) Unused);
        }

        /// <summary>Retrive the location of visual flags for wrapped lines. (Scintilla feature 2463)</summary>
        public WrapVisualLocation GetWrapVisualFlagsLocation()
        {
            return (WrapVisualLocation)Win32.SendMessage(scintilla, SciMsg.SCI_GETWRAPVISUALFLAGSLOCATION, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set the start indent for wrapped lines. (Scintilla feature 2464)</summary>
        public void SetWrapStartIndent(int indent)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETWRAPSTARTINDENT, (IntPtr) indent, (IntPtr) Unused);
        }

        /// <summary>Retrive the start indent for wrapped lines. (Scintilla feature 2465)</summary>
        public int GetWrapStartIndent()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETWRAPSTARTINDENT, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Sets how wrapped sublines are placed. Default is fixed. (Scintilla feature 2472)</summary>
        public void SetWrapIndentMode(WrapIndentMode wrapIndentMode)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETWRAPINDENTMODE, (IntPtr) wrapIndentMode, (IntPtr) Unused);
        }

        /// <summary>Retrieve how wrapped sublines are placed. Default is fixed. (Scintilla feature 2473)</summary>
        public WrapIndentMode GetWrapIndentMode()
        {
            return (WrapIndentMode)Win32.SendMessage(scintilla, SciMsg.SCI_GETWRAPINDENTMODE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Sets the degree of caching of layout information. (Scintilla feature 2272)</summary>
        public void SetLayoutCache(LineCache cacheMode)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETLAYOUTCACHE, (IntPtr) cacheMode, (IntPtr) Unused);
        }

        /// <summary>Retrieve the degree of caching of layout information. (Scintilla feature 2273)</summary>
        public LineCache GetLayoutCache()
        {
            return (LineCache)Win32.SendMessage(scintilla, SciMsg.SCI_GETLAYOUTCACHE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Sets the document width assumed for scrolling. (Scintilla feature 2274)</summary>
        public void SetScrollWidth(int pixelWidth)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETSCROLLWIDTH, (IntPtr) pixelWidth, (IntPtr) Unused);
        }

        /// <summary>Retrieve the document width assumed for scrolling. (Scintilla feature 2275)</summary>
        public int GetScrollWidth()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETSCROLLWIDTH, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Sets whether the maximum width line displayed is used to set scroll width. (Scintilla feature 2516)</summary>
        public void SetScrollWidthTracking(bool tracking)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETSCROLLWIDTHTRACKING, new IntPtr(tracking ? 1 : 0), (IntPtr) Unused);
        }

        /// <summary>Retrieve whether the scroll width tracks wide lines. (Scintilla feature 2517)</summary>
        public bool GetScrollWidthTracking()
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETSCROLLWIDTHTRACKING, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Measure the pixel width of some text in a particular style.
        /// NUL terminated text argument.
        /// Does not handle tab or control characters.
        /// (Scintilla feature 2276)
        /// </summary>
        public unsafe int TextWidth(int style, string text)
        {
            fixed (byte* textPtr = Encoding.UTF8.GetBytes(text))
            {
                return (int)Win32.SendMessage(scintilla, SciMsg.SCI_TEXTWIDTH, (IntPtr) style, (IntPtr) textPtr);
            }
        }

        /// <summary>
        /// Sets the scroll range so that maximum scroll position has
        /// the last line at the bottom of the view (default).
        /// Setting this to false allows scrolling one page below the last line.
        /// (Scintilla feature 2277)
        /// </summary>
        public void SetEndAtLastLine(bool endAtLastLine)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETENDATLASTLINE, new IntPtr(endAtLastLine ? 1 : 0), (IntPtr) Unused);
        }

        /// <summary>
        /// Retrieve whether the maximum scroll position has the last
        /// line at the bottom of the view.
        /// (Scintilla feature 2278)
        /// </summary>
        public bool GetEndAtLastLine()
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETENDATLASTLINE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Retrieve the height of a particular line of text in pixels. (Scintilla feature 2279)</summary>
        public int TextHeight(int line)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_TEXTHEIGHT, (IntPtr) line, (IntPtr) Unused);
        }

        /// <summary>Show or hide the vertical scroll bar. (Scintilla feature 2280)</summary>
        public void SetVScrollBar(bool visible)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETVSCROLLBAR, new IntPtr(visible ? 1 : 0), (IntPtr) Unused);
        }

        /// <summary>Is the vertical scroll bar visible? (Scintilla feature 2281)</summary>
        public bool GetVScrollBar()
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETVSCROLLBAR, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Append a string to the end of the document without changing the selection. (Scintilla feature 2282)</summary>
        public unsafe void AppendText(int length, string text)
        {
            fixed (byte* textPtr = Encoding.UTF8.GetBytes(text))
            {
                Win32.SendMessage(scintilla, SciMsg.SCI_APPENDTEXT, (IntPtr) length, (IntPtr) textPtr);
            }
        }

        /// <summary>How many phases is drawing done in? (Scintilla feature 2673)</summary>
        public PhasesDraw GetPhasesDraw()
        {
            return (PhasesDraw)Win32.SendMessage(scintilla, SciMsg.SCI_GETPHASESDRAW, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// In one phase draw, text is drawn in a series of rectangular blocks with no overlap.
        /// In two phase draw, text is drawn in a series of lines allowing runs to overlap horizontally.
        /// In multiple phase draw, each element is drawn over the whole drawing area, allowing text
        /// to overlap from one line to the next.
        /// (Scintilla feature 2674)
        /// </summary>
        public void SetPhasesDraw(PhasesDraw phases)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETPHASESDRAW, (IntPtr) phases, (IntPtr) Unused);
        }

        /// <summary>Choose the quality level for text from the FontQuality enumeration. (Scintilla feature 2611)</summary>
        public void SetFontQuality(FontQuality fontQuality)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETFONTQUALITY, (IntPtr) fontQuality, (IntPtr) Unused);
        }

        /// <summary>Retrieve the quality level for text. (Scintilla feature 2612)</summary>
        public FontQuality GetFontQuality()
        {
            return (FontQuality)Win32.SendMessage(scintilla, SciMsg.SCI_GETFONTQUALITY, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Scroll so that a display line is at the top of the display. (Scintilla feature 2613)</summary>
        public void SetFirstVisibleLine(int displayLine)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETFIRSTVISIBLELINE, (IntPtr) displayLine, (IntPtr) Unused);
        }

        /// <summary>Change the effect of pasting when there are multiple selections. (Scintilla feature 2614)</summary>
        public void SetMultiPaste(MultiPaste multiPaste)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETMULTIPASTE, (IntPtr) multiPaste, (IntPtr) Unused);
        }

        /// <summary>Retrieve the effect of pasting when there are multiple selections. (Scintilla feature 2615)</summary>
        public MultiPaste GetMultiPaste()
        {
            return (MultiPaste)Win32.SendMessage(scintilla, SciMsg.SCI_GETMULTIPASTE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Retrieve the value of a tag from a regular expression search.
        /// Result is NUL-terminated.
        /// (Scintilla feature 2616)
        /// </summary>
        public unsafe string GetTag(int tagNumber)
        {
            if (tagNumber < 0)
            {
                throw new ArgumentException("tagNumber must be non-negative integer");
            }
            return GetNullStrippedStringFromMessageThatReturnsLength(SciMsg.SCI_GETTAG, (IntPtr)tagNumber);
        }

        /// <summary>Join the lines in the target. (Scintilla feature 2288)</summary>
        public void LinesJoin()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_LINESJOIN, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Split the lines in the target into lines that are less wide than pixelWidth
        /// where possible.
        /// (Scintilla feature 2289)
        /// </summary>
        public void LinesSplit(int pixelWidth)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_LINESSPLIT, (IntPtr) pixelWidth, (IntPtr) Unused);
        }

        /// <summary>Set one of the colours used as a chequerboard pattern in the fold margin (Scintilla feature 2290)</summary>
        public void SetFoldMarginColour(bool useSetting, Colour back)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETFOLDMARGINCOLOUR, new IntPtr(useSetting ? 1 : 0), back.Value);
        }

        /// <summary>Set the other colour used as a chequerboard pattern in the fold margin (Scintilla feature 2291)</summary>
        public void SetFoldMarginHiColour(bool useSetting, Colour fore)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETFOLDMARGINHICOLOUR, new IntPtr(useSetting ? 1 : 0), fore.Value);
        }

        /// <summary>Enable or disable accessibility. (Scintilla feature 2702)</summary>
        public void SetAccessibility(Accessibility accessibility)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETACCESSIBILITY, (IntPtr) accessibility, (IntPtr) Unused);
        }

        /// <summary>Report accessibility status. (Scintilla feature 2703)</summary>
        public Accessibility GetAccessibility()
        {
            return (Accessibility)Win32.SendMessage(scintilla, SciMsg.SCI_GETACCESSIBILITY, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Move caret down one line. (Scintilla feature 2300)</summary>
        public void LineDown()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_LINEDOWN, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Move caret down one line extending selection to new caret position. (Scintilla feature 2301)</summary>
        public void LineDownExtend()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_LINEDOWNEXTEND, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Move caret up one line. (Scintilla feature 2302)</summary>
        public void LineUp()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_LINEUP, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Move caret up one line extending selection to new caret position. (Scintilla feature 2303)</summary>
        public void LineUpExtend()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_LINEUPEXTEND, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Move caret left one character. (Scintilla feature 2304)</summary>
        public void CharLeft()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_CHARLEFT, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Move caret left one character extending selection to new caret position. (Scintilla feature 2305)</summary>
        public void CharLeftExtend()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_CHARLEFTEXTEND, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Move caret right one character. (Scintilla feature 2306)</summary>
        public void CharRight()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_CHARRIGHT, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Move caret right one character extending selection to new caret position. (Scintilla feature 2307)</summary>
        public void CharRightExtend()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_CHARRIGHTEXTEND, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Move caret left one word. (Scintilla feature 2308)</summary>
        public void WordLeft()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_WORDLEFT, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Move caret left one word extending selection to new caret position. (Scintilla feature 2309)</summary>
        public void WordLeftExtend()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_WORDLEFTEXTEND, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Move caret right one word. (Scintilla feature 2310)</summary>
        public void WordRight()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_WORDRIGHT, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Move caret right one word extending selection to new caret position. (Scintilla feature 2311)</summary>
        public void WordRightExtend()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_WORDRIGHTEXTEND, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Move caret to first position on line. (Scintilla feature 2312)</summary>
        public void Home()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_HOME, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Move caret to first position on line extending selection to new caret position. (Scintilla feature 2313)</summary>
        public void HomeExtend()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_HOMEEXTEND, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Move caret to last position on line. (Scintilla feature 2314)</summary>
        public void LineEnd()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_LINEEND, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Move caret to last position on line extending selection to new caret position. (Scintilla feature 2315)</summary>
        public void LineEndExtend()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_LINEENDEXTEND, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Move caret to first position in document. (Scintilla feature 2316)</summary>
        public void DocumentStart()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_DOCUMENTSTART, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Move caret to first position in document extending selection to new caret position. (Scintilla feature 2317)</summary>
        public void DocumentStartExtend()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_DOCUMENTSTARTEXTEND, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Move caret to last position in document. (Scintilla feature 2318)</summary>
        public void DocumentEnd()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_DOCUMENTEND, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Move caret to last position in document extending selection to new caret position. (Scintilla feature 2319)</summary>
        public void DocumentEndExtend()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_DOCUMENTENDEXTEND, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Move caret one page up. (Scintilla feature 2320)</summary>
        public void PageUp()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_PAGEUP, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Move caret one page up extending selection to new caret position. (Scintilla feature 2321)</summary>
        public void PageUpExtend()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_PAGEUPEXTEND, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Move caret one page down. (Scintilla feature 2322)</summary>
        public void PageDown()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_PAGEDOWN, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Move caret one page down extending selection to new caret position. (Scintilla feature 2323)</summary>
        public void PageDownExtend()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_PAGEDOWNEXTEND, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Switch from insert to overtype mode or the reverse. (Scintilla feature 2324)</summary>
        public void EditToggleOvertype()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_EDITTOGGLEOVERTYPE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Cancel any modes such as call tip or auto-completion list display. (Scintilla feature 2325)</summary>
        public void Cancel()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_CANCEL, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Delete the selection or if no selection, the character before the caret. (Scintilla feature 2326)</summary>
        public void DeleteBack()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_DELETEBACK, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// If selection is empty or all on one line replace the selection with a tab character.
        /// If more than one line selected, indent the lines.
        /// (Scintilla feature 2327)
        /// </summary>
        public void Tab()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_TAB, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Dedent the selected lines. (Scintilla feature 2328)</summary>
        public void BackTab()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_BACKTAB, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Insert a new line, may use a CRLF, CR or LF depending on EOL mode. (Scintilla feature 2329)</summary>
        public void NewLine()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_NEWLINE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Insert a Form Feed character. (Scintilla feature 2330)</summary>
        public void FormFeed()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_FORMFEED, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Move caret to before first visible character on line.
        /// If already there move to first character on line.
        /// (Scintilla feature 2331)
        /// </summary>
        public void VCHome()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_VCHOME, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Like VCHome but extending selection to new caret position. (Scintilla feature 2332)</summary>
        public void VCHomeExtend()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_VCHOMEEXTEND, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Magnify the displayed text by increasing the sizes by 1 point. (Scintilla feature 2333)</summary>
        public void ZoomIn()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_ZOOMIN, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Make the displayed text smaller by decreasing the sizes by 1 point. (Scintilla feature 2334)</summary>
        public void ZoomOut()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_ZOOMOUT, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Delete the word to the left of the caret. (Scintilla feature 2335)</summary>
        public void DelWordLeft()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_DELWORDLEFT, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Delete the word to the right of the caret. (Scintilla feature 2336)</summary>
        public void DelWordRight()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_DELWORDRIGHT, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Delete the word to the right of the caret, but not the trailing non-word characters. (Scintilla feature 2518)</summary>
        public void DelWordRightEnd()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_DELWORDRIGHTEND, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Cut the line containing the caret. (Scintilla feature 2337)</summary>
        public void LineCut()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_LINECUT, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Delete the line containing the caret. (Scintilla feature 2338)</summary>
        public void LineDelete()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_LINEDELETE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Switch the current line with the previous. (Scintilla feature 2339)</summary>
        public void LineTranspose()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_LINETRANSPOSE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Reverse order of selected lines. (Scintilla feature 2354)</summary>
        public void LineReverse()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_LINEREVERSE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Duplicate the current line. (Scintilla feature 2404)</summary>
        public void LineDuplicate()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_LINEDUPLICATE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Transform the selection to lower case. (Scintilla feature 2340)</summary>
        public void LowerCase()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_LOWERCASE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Transform the selection to upper case. (Scintilla feature 2341)</summary>
        public void UpperCase()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_UPPERCASE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Scroll the document down, keeping the caret visible. (Scintilla feature 2342)</summary>
        public void LineScrollDown()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_LINESCROLLDOWN, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Scroll the document up, keeping the caret visible. (Scintilla feature 2343)</summary>
        public void LineScrollUp()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_LINESCROLLUP, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Delete the selection or if no selection, the character before the caret.
        /// Will not delete the character before at the start of a line.
        /// (Scintilla feature 2344)
        /// </summary>
        public void DeleteBackNotLine()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_DELETEBACKNOTLINE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Move caret to first position on display line. (Scintilla feature 2345)</summary>
        public void HomeDisplay()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_HOMEDISPLAY, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Move caret to first position on display line extending selection to
        /// new caret position.
        /// (Scintilla feature 2346)
        /// </summary>
        public void HomeDisplayExtend()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_HOMEDISPLAYEXTEND, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Move caret to last position on display line. (Scintilla feature 2347)</summary>
        public void LineEndDisplay()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_LINEENDDISPLAY, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Move caret to last position on display line extending selection to new
        /// caret position.
        /// (Scintilla feature 2348)
        /// </summary>
        public void LineEndDisplayExtend()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_LINEENDDISPLAYEXTEND, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Like Home but when word-wrap is enabled goes first to start of display line
        /// HomeDisplay, then to start of document line Home.
        /// (Scintilla feature 2349)
        /// </summary>
        public void HomeWrap()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_HOMEWRAP, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Like HomeExtend but when word-wrap is enabled extends first to start of display line
        /// HomeDisplayExtend, then to start of document line HomeExtend.
        /// (Scintilla feature 2450)
        /// </summary>
        public void HomeWrapExtend()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_HOMEWRAPEXTEND, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Like LineEnd but when word-wrap is enabled goes first to end of display line
        /// LineEndDisplay, then to start of document line LineEnd.
        /// (Scintilla feature 2451)
        /// </summary>
        public void LineEndWrap()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_LINEENDWRAP, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Like LineEndExtend but when word-wrap is enabled extends first to end of display line
        /// LineEndDisplayExtend, then to start of document line LineEndExtend.
        /// (Scintilla feature 2452)
        /// </summary>
        public void LineEndWrapExtend()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_LINEENDWRAPEXTEND, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Like VCHome but when word-wrap is enabled goes first to start of display line
        /// VCHomeDisplay, then behaves like VCHome.
        /// (Scintilla feature 2453)
        /// </summary>
        public void VCHomeWrap()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_VCHOMEWRAP, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Like VCHomeExtend but when word-wrap is enabled extends first to start of display line
        /// VCHomeDisplayExtend, then behaves like VCHomeExtend.
        /// (Scintilla feature 2454)
        /// </summary>
        public void VCHomeWrapExtend()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_VCHOMEWRAPEXTEND, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Copy the line containing the caret. (Scintilla feature 2455)</summary>
        public void LineCopy()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_LINECOPY, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Move the caret inside current view if it's not there already. (Scintilla feature 2401)</summary>
        public void MoveCaretInsideView()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_MOVECARETINSIDEVIEW, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>How many characters are on a line, including end of line characters? (Scintilla feature 2350)</summary>
        public int LineLength(int line)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_LINELENGTH, (IntPtr) line, (IntPtr) Unused);
        }

        /// <summary>Highlight the characters at two positions. (Scintilla feature 2351)</summary>
        public void BraceHighlight(int posA, int posB)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_BRACEHIGHLIGHT, (IntPtr) posA, (IntPtr) posB);
        }

        /// <summary>Use specified indicator to highlight matching braces instead of changing their style. (Scintilla feature 2498)</summary>
        public void BraceHighlightIndicator(bool useSetting, int indicator)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_BRACEHIGHLIGHTINDICATOR, new IntPtr(useSetting ? 1 : 0), (IntPtr) indicator);
        }

        /// <summary>Highlight the character at a position indicating there is no matching brace. (Scintilla feature 2352)</summary>
        public void BraceBadLight(int pos)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_BRACEBADLIGHT, (IntPtr) pos, (IntPtr) Unused);
        }

        /// <summary>Use specified indicator to highlight non matching brace instead of changing its style. (Scintilla feature 2499)</summary>
        public void BraceBadLightIndicator(bool useSetting, int indicator)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_BRACEBADLIGHTINDICATOR, new IntPtr(useSetting ? 1 : 0), (IntPtr) indicator);
        }

        /// <summary>
        /// Find the position of a matching brace or INVALID_POSITION if no match.
        /// The maxReStyle must be 0 for now. It may be defined in a future release.
        /// (Scintilla feature 2353)
        /// </summary>
        public int BraceMatch(int pos, int maxReStyle)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_BRACEMATCH, (IntPtr) pos, (IntPtr) maxReStyle);
        }

        /// <summary>Are the end of line characters visible? (Scintilla feature 2355)</summary>
        public bool GetViewEOL()
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETVIEWEOL, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Make the end of line characters visible or invisible. (Scintilla feature 2356)</summary>
        public void SetViewEOL(bool visible)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETVIEWEOL, new IntPtr(visible ? 1 : 0), (IntPtr) Unused);
        }

        /// <summary>Retrieve a pointer to the document object. (Scintilla feature 2357)</summary>
        public IntPtr GetDocPointer()
        {
            return Win32.SendMessage(scintilla, SciMsg.SCI_GETDOCPOINTER, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Change the document object used. (Scintilla feature 2358)</summary>
        public void SetDocPointer(IntPtr doc)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETDOCPOINTER, (IntPtr) Unused, (IntPtr) doc);
        }

        /// <summary>Set which document modification events are sent to the container. (Scintilla feature 2359)</summary>
        public void SetModEventMask(ModificationFlags eventMask)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETMODEVENTMASK, (IntPtr) eventMask, (IntPtr) Unused);
        }

        /// <summary>Retrieve the column number which text should be kept within. (Scintilla feature 2360)</summary>
        public int GetEdgeColumn()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETEDGECOLUMN, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Set the column number of the edge.
        /// If text goes past the edge then it is highlighted.
        /// (Scintilla feature 2361)
        /// </summary>
        public void SetEdgeColumn(int column)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETEDGECOLUMN, (IntPtr) column, (IntPtr) Unused);
        }

        /// <summary>Retrieve the edge highlight mode. (Scintilla feature 2362)</summary>
        public EdgeVisualStyle GetEdgeMode()
        {
            return (EdgeVisualStyle)Win32.SendMessage(scintilla, SciMsg.SCI_GETEDGEMODE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// The edge may be displayed by a line (EDGE_LINE/EDGE_MULTILINE) or by highlighting text that
        /// goes beyond it (EDGE_BACKGROUND) or not displayed at all (EDGE_NONE).
        /// (Scintilla feature 2363)
        /// </summary>
        public void SetEdgeMode(EdgeVisualStyle edgeMode)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETEDGEMODE, (IntPtr) edgeMode, (IntPtr) Unused);
        }

        /// <summary>Retrieve the colour used in edge indication. (Scintilla feature 2364)</summary>
        public Colour GetEdgeColour()
        {
            return new Colour((int) Win32.SendMessage(scintilla, SciMsg.SCI_GETEDGECOLOUR, (IntPtr) Unused, (IntPtr) Unused));
        }

        /// <summary>Change the colour used in edge indication. (Scintilla feature 2365)</summary>
        public void SetEdgeColour(Colour edgeColour)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETEDGECOLOUR, edgeColour.Value, (IntPtr) Unused);
        }

        /// <summary>Add a new vertical edge to the view. (Scintilla feature 2694)</summary>
        public void MultiEdgeAddLine(int column, Colour edgeColour)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_MULTIEDGEADDLINE, (IntPtr) column, edgeColour.Value);
        }

        /// <summary>Clear all vertical edges. (Scintilla feature 2695)</summary>
        public void MultiEdgeClearAll()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_MULTIEDGECLEARALL, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Sets the current caret position to be the search anchor. (Scintilla feature 2366)</summary>
        public void SearchAnchor()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SEARCHANCHOR, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Find some text starting at the search anchor.
        /// Does not ensure the selection is visible.
        /// (Scintilla feature 2367)
        /// </summary>
        public unsafe int SearchNext(FindOption searchFlags, string text)
        {
            fixed (byte* textPtr = Encoding.UTF8.GetBytes(text))
            {
                return (int)Win32.SendMessage(scintilla, SciMsg.SCI_SEARCHNEXT, (IntPtr) searchFlags, (IntPtr) textPtr);
            }
        }

        /// <summary>
        /// Find some text starting at the search anchor and moving backwards.
        /// Does not ensure the selection is visible.
        /// (Scintilla feature 2368)
        /// </summary>
        public unsafe int SearchPrev(FindOption searchFlags, string text)
        {
            fixed (byte* textPtr = Encoding.UTF8.GetBytes(text))
            {
                return (int)Win32.SendMessage(scintilla, SciMsg.SCI_SEARCHPREV, (IntPtr) searchFlags, (IntPtr) textPtr);
            }
        }

        /// <summary>Retrieves the number of lines completely visible. (Scintilla feature 2370)</summary>
        public int LinesOnScreen()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_LINESONSCREEN, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Set whether a pop up menu is displayed automatically when the user presses
        /// the wrong mouse button on certain areas.
        /// (Scintilla feature 2371)
        /// </summary>
        public void UsePopUp(PopUp popUpMode)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_USEPOPUP, (IntPtr) popUpMode, (IntPtr) Unused);
        }

        /// <summary>Is the selection rectangular? The alternative is the more common stream selection. (Scintilla feature 2372)</summary>
        public bool SelectionIsRectangle()
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_SELECTIONISRECTANGLE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Set the zoom level. This number of points is added to the size of all fonts.
        /// It may be positive to magnify or negative to reduce.
        /// (Scintilla feature 2373)
        /// </summary>
        public void SetZoom(int zoomInPoints)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETZOOM, (IntPtr) zoomInPoints, (IntPtr) Unused);
        }

        /// <summary>Retrieve the zoom level. (Scintilla feature 2374)</summary>
        public int GetZoom()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETZOOM, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Create a new document object.
        /// Starts with reference count of 1 and not selected into editor.
        /// (Scintilla feature 2375)
        /// </summary>
        public IntPtr CreateDocument(int bytes, DocumentOption documentOptions)
        {
            return Win32.SendMessage(scintilla, SciMsg.SCI_CREATEDOCUMENT, (IntPtr) bytes, (IntPtr) documentOptions);
        }

        /// <summary>Extend life of document. (Scintilla feature 2376)</summary>
        public void AddRefDocument(IntPtr doc)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_ADDREFDOCUMENT, (IntPtr) Unused, (IntPtr) doc);
        }

        /// <summary>Release a reference to the document, deleting document if it fades to black. (Scintilla feature 2377)</summary>
        public void ReleaseDocument(IntPtr doc)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_RELEASEDOCUMENT, (IntPtr) Unused, (IntPtr) doc);
        }

        /// <summary>Get which document options are set. (Scintilla feature 2379)</summary>
        public DocumentOption GetDocumentOptions()
        {
            return (DocumentOption)Win32.SendMessage(scintilla, SciMsg.SCI_GETDOCUMENTOPTIONS, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Get which document modification events are sent to the container. (Scintilla feature 2378)</summary>
        public ModificationFlags GetModEventMask()
        {
            return (ModificationFlags)Win32.SendMessage(scintilla, SciMsg.SCI_GETMODEVENTMASK, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set whether command events are sent to the container. (Scintilla feature 2717)</summary>
        public void SetCommandEvents(bool commandEvents)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETCOMMANDEVENTS, new IntPtr(commandEvents ? 1 : 0), (IntPtr) Unused);
        }

        /// <summary>Get whether command events are sent to the container. (Scintilla feature 2718)</summary>
        public bool GetCommandEvents()
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETCOMMANDEVENTS, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Change internal focus flag. (Scintilla feature 2380)</summary>
        public void SetFocus(bool focus)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETFOCUS, new IntPtr(focus ? 1 : 0), (IntPtr) Unused);
        }

        /// <summary>Get internal focus flag. (Scintilla feature 2381)</summary>
        public bool GetFocus()
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETFOCUS, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Change error status - 0 = OK. (Scintilla feature 2382)</summary>
        public void SetStatus(Status status)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETSTATUS, (IntPtr) status, (IntPtr) Unused);
        }

        /// <summary>Get error status. (Scintilla feature 2383)</summary>
        public Status GetStatus()
        {
            return (Status)Win32.SendMessage(scintilla, SciMsg.SCI_GETSTATUS, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set whether the mouse is captured when its button is pressed. (Scintilla feature 2384)</summary>
        public void SetMouseDownCaptures(bool captures)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETMOUSEDOWNCAPTURES, new IntPtr(captures ? 1 : 0), (IntPtr) Unused);
        }

        /// <summary>Get whether mouse gets captured. (Scintilla feature 2385)</summary>
        public bool GetMouseDownCaptures()
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETMOUSEDOWNCAPTURES, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set whether the mouse wheel can be active outside the window. (Scintilla feature 2696)</summary>
        public void SetMouseWheelCaptures(bool captures)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETMOUSEWHEELCAPTURES, new IntPtr(captures ? 1 : 0), (IntPtr) Unused);
        }

        /// <summary>Get whether mouse wheel can be active outside the window. (Scintilla feature 2697)</summary>
        public bool GetMouseWheelCaptures()
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETMOUSEWHEELCAPTURES, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Sets the cursor to one of the SC_CURSOR* values. (Scintilla feature 2386)</summary>
        public void SetCursor(CursorShape cursorType)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETCURSOR, (IntPtr) cursorType, (IntPtr) Unused);
        }

        /// <summary>Get cursor type. (Scintilla feature 2387)</summary>
        public CursorShape GetCursor()
        {
            return (CursorShape)Win32.SendMessage(scintilla, SciMsg.SCI_GETCURSOR, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Change the way control characters are displayed:
        /// If symbol is < 32, keep the drawn way, else, use the given character.
        /// (Scintilla feature 2388)
        /// </summary>
        public void SetControlCharSymbol(int symbol)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETCONTROLCHARSYMBOL, (IntPtr) symbol, (IntPtr) Unused);
        }

        /// <summary>Get the way control characters are displayed. (Scintilla feature 2389)</summary>
        public int GetControlCharSymbol()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETCONTROLCHARSYMBOL, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Move to the previous change in capitalisation. (Scintilla feature 2390)</summary>
        public void WordPartLeft()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_WORDPARTLEFT, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Move to the previous change in capitalisation extending selection
        /// to new caret position.
        /// (Scintilla feature 2391)
        /// </summary>
        public void WordPartLeftExtend()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_WORDPARTLEFTEXTEND, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Move to the change next in capitalisation. (Scintilla feature 2392)</summary>
        public void WordPartRight()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_WORDPARTRIGHT, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Move to the next change in capitalisation extending selection
        /// to new caret position.
        /// (Scintilla feature 2393)
        /// </summary>
        public void WordPartRightExtend()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_WORDPARTRIGHTEXTEND, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Set the way the display area is determined when a particular line
        /// is to be moved to by Find, FindNext, GotoLine, etc.
        /// (Scintilla feature 2394)
        /// </summary>
        public void SetVisiblePolicy(VisiblePolicy visiblePolicy, int visibleSlop)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETVISIBLEPOLICY, (IntPtr) visiblePolicy, (IntPtr) visibleSlop);
        }

        /// <summary>Delete back from the current position to the start of the line. (Scintilla feature 2395)</summary>
        public void DelLineLeft()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_DELLINELEFT, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Delete forwards from the current position to the end of the line. (Scintilla feature 2396)</summary>
        public void DelLineRight()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_DELLINERIGHT, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set the xOffset (ie, horizontal scroll position). (Scintilla feature 2397)</summary>
        public void SetXOffset(int xOffset)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETXOFFSET, (IntPtr) xOffset, (IntPtr) Unused);
        }

        /// <summary>Get the xOffset (ie, horizontal scroll position). (Scintilla feature 2398)</summary>
        public int GetXOffset()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETXOFFSET, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set the last x chosen value to be the caret x position. (Scintilla feature 2399)</summary>
        public void ChooseCaretX()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_CHOOSECARETX, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set the focus to this Scintilla widget. (Scintilla feature 2400)</summary>
        public void GrabFocus()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_GRABFOCUS, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Set the way the caret is kept visible when going sideways.
        /// The exclusion zone is given in pixels.
        /// (Scintilla feature 2402)
        /// </summary>
        public void SetXCaretPolicy(CaretPolicy caretPolicy, int caretSlop)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETXCARETPOLICY, (IntPtr) caretPolicy, (IntPtr) caretSlop);
        }

        /// <summary>
        /// Set the way the line the caret is on is kept visible.
        /// The exclusion zone is given in lines.
        /// (Scintilla feature 2403)
        /// </summary>
        public void SetYCaretPolicy(CaretPolicy caretPolicy, int caretSlop)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETYCARETPOLICY, (IntPtr) caretPolicy, (IntPtr) caretSlop);
        }

        /// <summary>Set printing to line wrapped (SC_WRAP_WORD) or not line wrapped (SC_WRAP_NONE). (Scintilla feature 2406)</summary>
        public void SetPrintWrapMode(Wrap wrapMode)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETPRINTWRAPMODE, (IntPtr) wrapMode, (IntPtr) Unused);
        }

        /// <summary>Is printing line wrapped? (Scintilla feature 2407)</summary>
        public Wrap GetPrintWrapMode()
        {
            return (Wrap)Win32.SendMessage(scintilla, SciMsg.SCI_GETPRINTWRAPMODE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set a fore colour for active hotspots. (Scintilla feature 2410)</summary>
        public void SetHotspotActiveFore(bool useSetting, Colour fore)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETHOTSPOTACTIVEFORE, new IntPtr(useSetting ? 1 : 0), fore.Value);
        }

        /// <summary>Get the fore colour for active hotspots. (Scintilla feature 2494)</summary>
        public Colour GetHotspotActiveFore()
        {
            return new Colour((int) Win32.SendMessage(scintilla, SciMsg.SCI_GETHOTSPOTACTIVEFORE, (IntPtr) Unused, (IntPtr) Unused));
        }

        /// <summary>Set a back colour for active hotspots. (Scintilla feature 2411)</summary>
        public void SetHotspotActiveBack(bool useSetting, Colour back)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETHOTSPOTACTIVEBACK, new IntPtr(useSetting ? 1 : 0), back.Value);
        }

        /// <summary>Get the back colour for active hotspots. (Scintilla feature 2495)</summary>
        public Colour GetHotspotActiveBack()
        {
            return new Colour((int) Win32.SendMessage(scintilla, SciMsg.SCI_GETHOTSPOTACTIVEBACK, (IntPtr) Unused, (IntPtr) Unused));
        }

        /// <summary>Enable / Disable underlining active hotspots. (Scintilla feature 2412)</summary>
        public void SetHotspotActiveUnderline(bool underline)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETHOTSPOTACTIVEUNDERLINE, new IntPtr(underline ? 1 : 0), (IntPtr) Unused);
        }

        /// <summary>Get whether underlining for active hotspots. (Scintilla feature 2496)</summary>
        public bool GetHotspotActiveUnderline()
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETHOTSPOTACTIVEUNDERLINE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Limit hotspots to single line so hotspots on two lines don't merge. (Scintilla feature 2421)</summary>
        public void SetHotspotSingleLine(bool singleLine)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETHOTSPOTSINGLELINE, new IntPtr(singleLine ? 1 : 0), (IntPtr) Unused);
        }

        /// <summary>Get the HotspotSingleLine property (Scintilla feature 2497)</summary>
        public bool GetHotspotSingleLine()
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETHOTSPOTSINGLELINE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Move caret down one paragraph (delimited by empty lines). (Scintilla feature 2413)</summary>
        public void ParaDown()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_PARADOWN, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Extend selection down one paragraph (delimited by empty lines). (Scintilla feature 2414)</summary>
        public void ParaDownExtend()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_PARADOWNEXTEND, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Move caret up one paragraph (delimited by empty lines). (Scintilla feature 2415)</summary>
        public void ParaUp()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_PARAUP, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Extend selection up one paragraph (delimited by empty lines). (Scintilla feature 2416)</summary>
        public void ParaUpExtend()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_PARAUPEXTEND, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Given a valid document position, return the previous position taking code
        /// page into account. Returns 0 if passed 0.
        /// (Scintilla feature 2417)
        /// </summary>
        public int PositionBefore(int pos)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_POSITIONBEFORE, (IntPtr) pos, (IntPtr) Unused);
        }

        /// <summary>
        /// Given a valid document position, return the next position taking code
        /// page into account. Maximum value returned is the last position in the document.
        /// (Scintilla feature 2418)
        /// </summary>
        public int PositionAfter(int pos)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_POSITIONAFTER, (IntPtr) pos, (IntPtr) Unused);
        }

        /// <summary>
        /// Given a valid document position, return a position that differs in a number
        /// of characters. Returned value is always between 0 and last position in document.
        /// (Scintilla feature 2670)
        /// </summary>
        public int PositionRelative(int pos, int relative)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_POSITIONRELATIVE, (IntPtr) pos, (IntPtr) relative);
        }

        /// <summary>
        /// Given a valid document position, return a position that differs in a number
        /// of UTF-16 code units. Returned value is always between 0 and last position in document.
        /// The result may point half way (2 bytes) inside a non-BMP character.
        /// (Scintilla feature 2716)
        /// </summary>
        public int PositionRelativeCodeUnits(int pos, int relative)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_POSITIONRELATIVECODEUNITS, (IntPtr) pos, (IntPtr) relative);
        }

        /// <summary>Copy a range of text to the clipboard. Positions are clipped into the document. (Scintilla feature 2419)</summary>
        public void CopyRange(int start, int end)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_COPYRANGE, (IntPtr) start, (IntPtr) end);
        }

        /// <summary>Copy argument text to the clipboard. (Scintilla feature 2420)</summary>
        public unsafe void CopyText(int length, string text)
        {
            fixed (byte* textPtr = Encoding.UTF8.GetBytes(text))
            {
                Win32.SendMessage(scintilla, SciMsg.SCI_COPYTEXT, (IntPtr) length, (IntPtr) textPtr);
            }
        }

        /// <summary>
        /// Set the selection mode to stream (SC_SEL_STREAM) or rectangular (SC_SEL_RECTANGLE/SC_SEL_THIN) or
        /// by lines (SC_SEL_LINES).
        /// (Scintilla feature 2422)
        /// </summary>
        public void SetSelectionMode(SelectionMode selectionMode)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETSELECTIONMODE, (IntPtr) selectionMode, (IntPtr) Unused);
        }

        /// <summary>Get the mode of the current selection. (Scintilla feature 2423)</summary>
        public SelectionMode GetSelectionMode()
        {
            return (SelectionMode)Win32.SendMessage(scintilla, SciMsg.SCI_GETSELECTIONMODE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Get whether or not regular caret moves will extend or reduce the selection. (Scintilla feature 2706)</summary>
        public bool GetMoveExtendsSelection()
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETMOVEEXTENDSSELECTION, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Retrieve the position of the start of the selection at the given line (INVALID_POSITION if no selection on this line). (Scintilla feature 2424)</summary>
        public int GetLineSelStartPosition(int line)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETLINESELSTARTPOSITION, (IntPtr) line, (IntPtr) Unused);
        }

        /// <summary>Retrieve the position of the end of the selection at the given line (INVALID_POSITION if no selection on this line). (Scintilla feature 2425)</summary>
        public int GetLineSelEndPosition(int line)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETLINESELENDPOSITION, (IntPtr) line, (IntPtr) Unused);
        }

        /// <summary>Move caret down one line, extending rectangular selection to new caret position. (Scintilla feature 2426)</summary>
        public void LineDownRectExtend()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_LINEDOWNRECTEXTEND, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Move caret up one line, extending rectangular selection to new caret position. (Scintilla feature 2427)</summary>
        public void LineUpRectExtend()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_LINEUPRECTEXTEND, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Move caret left one character, extending rectangular selection to new caret position. (Scintilla feature 2428)</summary>
        public void CharLeftRectExtend()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_CHARLEFTRECTEXTEND, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Move caret right one character, extending rectangular selection to new caret position. (Scintilla feature 2429)</summary>
        public void CharRightRectExtend()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_CHARRIGHTRECTEXTEND, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Move caret to first position on line, extending rectangular selection to new caret position. (Scintilla feature 2430)</summary>
        public void HomeRectExtend()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_HOMERECTEXTEND, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Move caret to before first visible character on line.
        /// If already there move to first character on line.
        /// In either case, extend rectangular selection to new caret position.
        /// (Scintilla feature 2431)
        /// </summary>
        public void VCHomeRectExtend()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_VCHOMERECTEXTEND, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Move caret to last position on line, extending rectangular selection to new caret position. (Scintilla feature 2432)</summary>
        public void LineEndRectExtend()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_LINEENDRECTEXTEND, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Move caret one page up, extending rectangular selection to new caret position. (Scintilla feature 2433)</summary>
        public void PageUpRectExtend()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_PAGEUPRECTEXTEND, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Move caret one page down, extending rectangular selection to new caret position. (Scintilla feature 2434)</summary>
        public void PageDownRectExtend()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_PAGEDOWNRECTEXTEND, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Move caret to top of page, or one page up if already at top of page. (Scintilla feature 2435)</summary>
        public void StutteredPageUp()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_STUTTEREDPAGEUP, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Move caret to top of page, or one page up if already at top of page, extending selection to new caret position. (Scintilla feature 2436)</summary>
        public void StutteredPageUpExtend()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_STUTTEREDPAGEUPEXTEND, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Move caret to bottom of page, or one page down if already at bottom of page. (Scintilla feature 2437)</summary>
        public void StutteredPageDown()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_STUTTEREDPAGEDOWN, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Move caret to bottom of page, or one page down if already at bottom of page, extending selection to new caret position. (Scintilla feature 2438)</summary>
        public void StutteredPageDownExtend()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_STUTTEREDPAGEDOWNEXTEND, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Move caret left one word, position cursor at end of word. (Scintilla feature 2439)</summary>
        public void WordLeftEnd()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_WORDLEFTEND, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Move caret left one word, position cursor at end of word, extending selection to new caret position. (Scintilla feature 2440)</summary>
        public void WordLeftEndExtend()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_WORDLEFTENDEXTEND, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Move caret right one word, position cursor at end of word. (Scintilla feature 2441)</summary>
        public void WordRightEnd()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_WORDRIGHTEND, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Move caret right one word, position cursor at end of word, extending selection to new caret position. (Scintilla feature 2442)</summary>
        public void WordRightEndExtend()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_WORDRIGHTENDEXTEND, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Set the set of characters making up whitespace for when moving or selecting by word.
        /// Should be called after SetWordChars.
        /// (Scintilla feature 2443)
        /// </summary>
        public unsafe void SetWhitespaceChars(string characters)
        {
            fixed (byte* charactersPtr = Encoding.UTF8.GetBytes(characters))
            {
                Win32.SendMessage(scintilla, SciMsg.SCI_SETWHITESPACECHARS, (IntPtr) Unused, (IntPtr) charactersPtr);
            }
        }

        /// <summary>Get the set of characters making up whitespace for when moving or selecting by word. (Scintilla feature 2647)</summary>
        public unsafe string GetWhitespaceChars()
        {
            return GetNullStrippedStringFromMessageThatReturnsLength(SciMsg.SCI_GETWHITESPACECHARS);
        }

        /// <summary>
        /// Set the set of characters making up punctuation characters
        /// Should be called after SetWordChars.
        /// (Scintilla feature 2648)
        /// </summary>
        public unsafe void SetPunctuationChars(string characters)
        {
            fixed (byte* charactersPtr = Encoding.UTF8.GetBytes(characters))
            {
                Win32.SendMessage(scintilla, SciMsg.SCI_SETPUNCTUATIONCHARS, (IntPtr) Unused, (IntPtr) charactersPtr);
            }
        }

        /// <summary>Get the set of characters making up punctuation characters (Scintilla feature 2649)</summary>
        public unsafe string GetPunctuationChars()
        {
            return GetNullStrippedStringFromMessageThatReturnsLength(SciMsg.SCI_GETPUNCTUATIONCHARS);
        }

        /// <summary>Reset the set of characters for whitespace and word characters to the defaults. (Scintilla feature 2444)</summary>
        public void SetCharsDefault()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETCHARSDEFAULT, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Get currently selected item position in the auto-completion list (Scintilla feature 2445)</summary>
        public int AutoCGetCurrent()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_AUTOCGETCURRENT, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Get currently selected item text in the auto-completion list
        /// Returns the length of the item text
        /// Result is NUL-terminated.
        /// (Scintilla feature 2610)
        /// </summary>
        public unsafe string AutoCGetCurrentText()
        {
            return GetNullStrippedStringFromMessageThatReturnsLength(SciMsg.SCI_AUTOCGETCURRENTTEXT);
        }

        /// <summary>Set auto-completion case insensitive behaviour to either prefer case-sensitive matches or have no preference. (Scintilla feature 2634)</summary>
        public void AutoCSetCaseInsensitiveBehaviour(CaseInsensitiveBehaviour behaviour)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_AUTOCSETCASEINSENSITIVEBEHAVIOUR, (IntPtr) behaviour, (IntPtr) Unused);
        }

        /// <summary>Get auto-completion case insensitive behaviour. (Scintilla feature 2635)</summary>
        public CaseInsensitiveBehaviour AutoCGetCaseInsensitiveBehaviour()
        {
            return (CaseInsensitiveBehaviour)Win32.SendMessage(scintilla, SciMsg.SCI_AUTOCGETCASEINSENSITIVEBEHAVIOUR, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Change the effect of autocompleting when there are multiple selections. (Scintilla feature 2636)</summary>
        public void AutoCSetMulti(MultiAutoComplete multi)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_AUTOCSETMULTI, (IntPtr) multi, (IntPtr) Unused);
        }

        /// <summary>Retrieve the effect of autocompleting when there are multiple selections. (Scintilla feature 2637)</summary>
        public MultiAutoComplete AutoCGetMulti()
        {
            return (MultiAutoComplete)Win32.SendMessage(scintilla, SciMsg.SCI_AUTOCGETMULTI, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set the way autocompletion lists are ordered. (Scintilla feature 2660)</summary>
        public void AutoCSetOrder(Ordering order)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_AUTOCSETORDER, (IntPtr) order, (IntPtr) Unused);
        }

        /// <summary>Get the way autocompletion lists are ordered. (Scintilla feature 2661)</summary>
        public Ordering AutoCGetOrder()
        {
            return (Ordering)Win32.SendMessage(scintilla, SciMsg.SCI_AUTOCGETORDER, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Enlarge the document to a particular size of text bytes. (Scintilla feature 2446)</summary>
        public void Allocate(int bytes)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_ALLOCATE, (IntPtr) bytes, (IntPtr) Unused);
        }

        /// <summary>
        /// Returns the target converted to UTF8.
        /// Return the length in bytes.
        /// (Scintilla feature 2447)
        /// </summary>
        public unsafe string TargetAsUTF8()
        {
            return GetNullStrippedStringFromMessageThatReturnsLength(SciMsg.SCI_TARGETASUTF8);
        }

        /// <summary>
        /// Set the length of the utf8 argument for calling EncodedFromUTF8.
        /// Set to -1 and the string will be measured to the first nul.
        /// (Scintilla feature 2448)
        /// </summary>
        public void SetLengthForEncode(int bytes)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETLENGTHFORENCODE, (IntPtr) bytes, (IntPtr) Unused);
        }

        /// <summary>
        /// Translates a UTF8 string into the document encoding.
        /// Return the length of the result in bytes.
        /// On error return 0.
        /// (Scintilla feature 2449)
        /// </summary>
        public unsafe string EncodedFromUTF8(string utf8)
        {
            fixed (byte* utf8Ptr = Encoding.UTF8.GetBytes(utf8))
            {
                return GetNullStrippedStringFromMessageThatReturnsLength(SciMsg.SCI_ENCODEDFROMUTF8, (IntPtr)utf8Ptr);
            }
        }

        /// <summary>
        /// Find the position of a column on a line taking into account tabs and
        /// multi-byte characters. If beyond end of line, return line end position.
        /// (Scintilla feature 2456)
        /// </summary>
        public int FindColumn(int line, int column)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_FINDCOLUMN, (IntPtr) line, (IntPtr) column);
        }

        /// <summary>Can the caret preferred x position only be changed by explicit movement commands? (Scintilla feature 2457)</summary>
        public CaretSticky GetCaretSticky()
        {
            return (CaretSticky)Win32.SendMessage(scintilla, SciMsg.SCI_GETCARETSTICKY, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Stop the caret preferred x position changing when the user types. (Scintilla feature 2458)</summary>
        public void SetCaretSticky(CaretSticky useCaretStickyBehaviour)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETCARETSTICKY, (IntPtr) useCaretStickyBehaviour, (IntPtr) Unused);
        }

        /// <summary>Switch between sticky and non-sticky: meant to be bound to a key. (Scintilla feature 2459)</summary>
        public void ToggleCaretSticky()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_TOGGLECARETSTICKY, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Enable/Disable convert-on-paste for line endings (Scintilla feature 2467)</summary>
        public void SetPasteConvertEndings(bool convert)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETPASTECONVERTENDINGS, new IntPtr(convert ? 1 : 0), (IntPtr) Unused);
        }

        /// <summary>Get convert-on-paste setting (Scintilla feature 2468)</summary>
        public bool GetPasteConvertEndings()
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETPASTECONVERTENDINGS, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Duplicate the selection. If selection empty duplicate the line containing the caret. (Scintilla feature 2469)</summary>
        public void SelectionDuplicate()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SELECTIONDUPLICATE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set background alpha of the caret line. (Scintilla feature 2470)</summary>
        public void SetCaretLineBackAlpha(Alpha alpha)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETCARETLINEBACKALPHA, (IntPtr) alpha, (IntPtr) Unused);
        }

        /// <summary>Get the background alpha of the caret line. (Scintilla feature 2471)</summary>
        public Alpha GetCaretLineBackAlpha()
        {
            return (Alpha)Win32.SendMessage(scintilla, SciMsg.SCI_GETCARETLINEBACKALPHA, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set the style of the caret to be drawn. (Scintilla feature 2512)</summary>
        public void SetCaretStyle(CaretStyle caretStyle)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETCARETSTYLE, (IntPtr) caretStyle, (IntPtr) Unused);
        }

        /// <summary>Returns the current style of the caret. (Scintilla feature 2513)</summary>
        public CaretStyle GetCaretStyle()
        {
            return (CaretStyle)Win32.SendMessage(scintilla, SciMsg.SCI_GETCARETSTYLE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set the indicator used for IndicatorFillRange and IndicatorClearRange (Scintilla feature 2500)</summary>
        public void SetIndicatorCurrent(int indicator)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETINDICATORCURRENT, (IntPtr) indicator, (IntPtr) Unused);
        }

        /// <summary>Get the current indicator (Scintilla feature 2501)</summary>
        public int GetIndicatorCurrent()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETINDICATORCURRENT, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set the value used for IndicatorFillRange (Scintilla feature 2502)</summary>
        public void SetIndicatorValue(int value)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETINDICATORVALUE, (IntPtr) value, (IntPtr) Unused);
        }

        /// <summary>Get the current indicator value (Scintilla feature 2503)</summary>
        public int GetIndicatorValue()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETINDICATORVALUE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Turn a indicator on over a range. (Scintilla feature 2504)</summary>
        public void IndicatorFillRange(int start, int lengthFill)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_INDICATORFILLRANGE, (IntPtr) start, (IntPtr) lengthFill);
        }

        /// <summary>Turn a indicator off over a range. (Scintilla feature 2505)</summary>
        public void IndicatorClearRange(int start, int lengthClear)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_INDICATORCLEARRANGE, (IntPtr) start, (IntPtr) lengthClear);
        }

        /// <summary>Are any indicators present at pos? (Scintilla feature 2506)</summary>
        public int IndicatorAllOnFor(int pos)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_INDICATORALLONFOR, (IntPtr) pos, (IntPtr) Unused);
        }

        /// <summary>What value does a particular indicator have at a position? (Scintilla feature 2507)</summary>
        public int IndicatorValueAt(int indicator, int pos)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_INDICATORVALUEAT, (IntPtr) indicator, (IntPtr) pos);
        }

        /// <summary>Where does a particular indicator start? (Scintilla feature 2508)</summary>
        public int IndicatorStart(int indicator, int pos)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_INDICATORSTART, (IntPtr) indicator, (IntPtr) pos);
        }

        /// <summary>Where does a particular indicator end? (Scintilla feature 2509)</summary>
        public int IndicatorEnd(int indicator, int pos)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_INDICATOREND, (IntPtr) indicator, (IntPtr) pos);
        }

        /// <summary>Set number of entries in position cache (Scintilla feature 2514)</summary>
        public void SetPositionCache(int size)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETPOSITIONCACHE, (IntPtr) size, (IntPtr) Unused);
        }

        /// <summary>How many entries are allocated to the position cache? (Scintilla feature 2515)</summary>
        public int GetPositionCache()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETPOSITIONCACHE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Copy the selection, if selection empty copy the line with the caret (Scintilla feature 2519)</summary>
        public void CopyAllowLine()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_COPYALLOWLINE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Compact the document buffer and return a read-only pointer to the
        /// characters in the document.
        /// (Scintilla feature 2520)
        /// </summary>
        public IntPtr GetCharacterPointer()
        {
            return Win32.SendMessage(scintilla, SciMsg.SCI_GETCHARACTERPOINTER, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Return a read-only pointer to a range of characters in the document.
        /// May move the gap so that the range is contiguous, but will only move up
        /// to lengthRange bytes.
        /// (Scintilla feature 2643)
        /// </summary>
        public IntPtr GetRangePointer(int start, int lengthRange)
        {
            return Win32.SendMessage(scintilla, SciMsg.SCI_GETRANGEPOINTER, (IntPtr) start, (IntPtr) lengthRange);
        }

        /// <summary>
        /// Return a position which, to avoid performance costs, should not be within
        /// the range of a call to GetRangePointer.
        /// (Scintilla feature 2644)
        /// </summary>
        public int GetGapPosition()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETGAPPOSITION, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set the alpha fill colour of the given indicator. (Scintilla feature 2523)</summary>
        public void IndicSetAlpha(int indicator, Alpha alpha)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_INDICSETALPHA, (IntPtr) indicator, (IntPtr) alpha);
        }

        /// <summary>Get the alpha fill colour of the given indicator. (Scintilla feature 2524)</summary>
        public Alpha IndicGetAlpha(int indicator)
        {
            return (Alpha)Win32.SendMessage(scintilla, SciMsg.SCI_INDICGETALPHA, (IntPtr) indicator, (IntPtr) Unused);
        }

        /// <summary>Set the alpha outline colour of the given indicator. (Scintilla feature 2558)</summary>
        public void IndicSetOutlineAlpha(int indicator, Alpha alpha)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_INDICSETOUTLINEALPHA, (IntPtr) indicator, (IntPtr) alpha);
        }

        /// <summary>Get the alpha outline colour of the given indicator. (Scintilla feature 2559)</summary>
        public Alpha IndicGetOutlineAlpha(int indicator)
        {
            return (Alpha)Win32.SendMessage(scintilla, SciMsg.SCI_INDICGETOUTLINEALPHA, (IntPtr) indicator, (IntPtr) Unused);
        }

        /// <summary>Set extra ascent for each line (Scintilla feature 2525)</summary>
        public void SetExtraAscent(int extraAscent)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETEXTRAASCENT, (IntPtr) extraAscent, (IntPtr) Unused);
        }

        /// <summary>Get extra ascent for each line (Scintilla feature 2526)</summary>
        public int GetExtraAscent()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETEXTRAASCENT, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set extra descent for each line (Scintilla feature 2527)</summary>
        public void SetExtraDescent(int extraDescent)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETEXTRADESCENT, (IntPtr) extraDescent, (IntPtr) Unused);
        }

        /// <summary>Get extra descent for each line (Scintilla feature 2528)</summary>
        public int GetExtraDescent()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETEXTRADESCENT, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Which symbol was defined for markerNumber with MarkerDefine (Scintilla feature 2529)</summary>
        public int MarkerSymbolDefined(int markerNumber)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_MARKERSYMBOLDEFINED, (IntPtr) markerNumber, (IntPtr) Unused);
        }

        /// <summary>Set the text in the text margin for a line (Scintilla feature 2530)</summary>
        public unsafe void MarginSetText(int line, string text)
        {
            fixed (byte* textPtr = Encoding.UTF8.GetBytes(text))
            {
                Win32.SendMessage(scintilla, SciMsg.SCI_MARGINSETTEXT, (IntPtr) line, (IntPtr) textPtr);
            }
        }

        /// <summary>Get the text in the text margin for a line (Scintilla feature 2531)</summary>
        public unsafe string MarginGetText(int line)
        {
            return GetNullStrippedStringFromMessageThatReturnsLength(SciMsg.SCI_MARGINGETTEXT, (IntPtr)line);
        }

        /// <summary>Set the style number for the text margin for a line (Scintilla feature 2532)</summary>
        public void MarginSetStyle(int line, int style)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_MARGINSETSTYLE, (IntPtr) line, (IntPtr) style);
        }

        /// <summary>Get the style number for the text margin for a line (Scintilla feature 2533)</summary>
        public int MarginGetStyle(int line)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_MARGINGETSTYLE, (IntPtr) line, (IntPtr) Unused);
        }

        /// <summary>Set the style in the text margin for a line (Scintilla feature 2534)</summary>
        public unsafe void MarginSetStyles(int line, string styles)
        {
            fixed (byte* stylesPtr = Encoding.UTF8.GetBytes(styles))
            {
                Win32.SendMessage(scintilla, SciMsg.SCI_MARGINSETSTYLES, (IntPtr) line, (IntPtr) stylesPtr);
            }
        }

        /// <summary>Get the styles in the text margin for a line (Scintilla feature 2535)</summary>
        public unsafe string MarginGetStyles(int line)
        {
            return GetNullStrippedStringFromMessageThatReturnsLength(SciMsg.SCI_MARGINGETSTYLES, (IntPtr)line);
        }

        /// <summary>Clear the margin text on all lines (Scintilla feature 2536)</summary>
        public void MarginTextClearAll()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_MARGINTEXTCLEARALL, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Get the start of the range of style numbers used for margin text (Scintilla feature 2537)</summary>
        public void MarginSetStyleOffset(int style)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_MARGINSETSTYLEOFFSET, (IntPtr) style, (IntPtr) Unused);
        }

        /// <summary>Get the start of the range of style numbers used for margin text (Scintilla feature 2538)</summary>
        public int MarginGetStyleOffset()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_MARGINGETSTYLEOFFSET, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set the margin options. (Scintilla feature 2539)</summary>
        public void SetMarginOptions(MarginOption marginOptions)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETMARGINOPTIONS, (IntPtr) marginOptions, (IntPtr) Unused);
        }

        /// <summary>Get the margin options. (Scintilla feature 2557)</summary>
        public MarginOption GetMarginOptions()
        {
            return (MarginOption)Win32.SendMessage(scintilla, SciMsg.SCI_GETMARGINOPTIONS, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set the annotation text for a line (Scintilla feature 2540)</summary>
        public unsafe void AnnotationSetText(int line, string text)
        {
            fixed (byte* textPtr = Encoding.UTF8.GetBytes(text))
            {
                Win32.SendMessage(scintilla, SciMsg.SCI_ANNOTATIONSETTEXT, (IntPtr) line, (IntPtr) textPtr);
            }
        }

        /// <summary>Get the annotation text for a line (Scintilla feature 2541)</summary>
        public unsafe string AnnotationGetText(int line)
        {
            return GetNullStrippedStringFromMessageThatReturnsLength(SciMsg.SCI_ANNOTATIONGETTEXT, (IntPtr)line);
        }

        /// <summary>Set the style number for the annotations for a line (Scintilla feature 2542)</summary>
        public void AnnotationSetStyle(int line, int style)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_ANNOTATIONSETSTYLE, (IntPtr) line, (IntPtr) style);
        }

        /// <summary>Get the style number for the annotations for a line (Scintilla feature 2543)</summary>
        public int AnnotationGetStyle(int line)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_ANNOTATIONGETSTYLE, (IntPtr) line, (IntPtr) Unused);
        }

        /// <summary>Set the annotation styles for a line (Scintilla feature 2544)</summary>
        public unsafe void AnnotationSetStyles(int line, string styles)
        {
            fixed (byte* stylesPtr = Encoding.UTF8.GetBytes(styles))
            {
                Win32.SendMessage(scintilla, SciMsg.SCI_ANNOTATIONSETSTYLES, (IntPtr) line, (IntPtr) stylesPtr);
            }
        }

        /// <summary>Get the annotation styles for a line (Scintilla feature 2545)</summary>
        public unsafe string AnnotationGetStyles(int line)
        {
            return GetNullStrippedStringFromMessageThatReturnsLength(SciMsg.SCI_ANNOTATIONGETSTYLES, (IntPtr)line);
        }

        /// <summary>Get the number of annotation lines for a line (Scintilla feature 2546)</summary>
        public int AnnotationGetLines(int line)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_ANNOTATIONGETLINES, (IntPtr) line, (IntPtr) Unused);
        }

        /// <summary>Clear the annotations from all lines (Scintilla feature 2547)</summary>
        public void AnnotationClearAll()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_ANNOTATIONCLEARALL, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set the visibility for the annotations for a view (Scintilla feature 2548)</summary>
        public void AnnotationSetVisible(AnnotationVisible visible)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_ANNOTATIONSETVISIBLE, (IntPtr) visible, (IntPtr) Unused);
        }

        /// <summary>Get the visibility for the annotations for a view (Scintilla feature 2549)</summary>
        public AnnotationVisible AnnotationGetVisible()
        {
            return (AnnotationVisible)Win32.SendMessage(scintilla, SciMsg.SCI_ANNOTATIONGETVISIBLE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Get the start of the range of style numbers used for annotations (Scintilla feature 2550)</summary>
        public void AnnotationSetStyleOffset(int style)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_ANNOTATIONSETSTYLEOFFSET, (IntPtr) style, (IntPtr) Unused);
        }

        /// <summary>Get the start of the range of style numbers used for annotations (Scintilla feature 2551)</summary>
        public int AnnotationGetStyleOffset()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_ANNOTATIONGETSTYLEOFFSET, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Release all extended (>255) style numbers (Scintilla feature 2552)</summary>
        public void ReleaseAllExtendedStyles()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_RELEASEALLEXTENDEDSTYLES, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Allocate some extended (>255) style numbers and return the start of the range (Scintilla feature 2553)</summary>
        public int AllocateExtendedStyles(int numberStyles)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_ALLOCATEEXTENDEDSTYLES, (IntPtr) numberStyles, (IntPtr) Unused);
        }

        /// <summary>Add a container action to the undo stack (Scintilla feature 2560)</summary>
        public void AddUndoAction(int token, UndoFlags flags)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_ADDUNDOACTION, (IntPtr) token, (IntPtr) flags);
        }

        /// <summary>Find the position of a character from a point within the window. (Scintilla feature 2561)</summary>
        public int CharPositionFromPoint(int x, int y)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_CHARPOSITIONFROMPOINT, (IntPtr) x, (IntPtr) y);
        }

        /// <summary>
        /// Find the position of a character from a point within the window.
        /// Return INVALID_POSITION if not close to text.
        /// (Scintilla feature 2562)
        /// </summary>
        public int CharPositionFromPointClose(int x, int y)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_CHARPOSITIONFROMPOINTCLOSE, (IntPtr) x, (IntPtr) y);
        }

        /// <summary>Set whether switching to rectangular mode while selecting with the mouse is allowed. (Scintilla feature 2668)</summary>
        public void SetMouseSelectionRectangularSwitch(bool mouseSelectionRectangularSwitch)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETMOUSESELECTIONRECTANGULARSWITCH, new IntPtr(mouseSelectionRectangularSwitch ? 1 : 0), (IntPtr) Unused);
        }

        /// <summary>Whether switching to rectangular mode while selecting with the mouse is allowed. (Scintilla feature 2669)</summary>
        public bool GetMouseSelectionRectangularSwitch()
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETMOUSESELECTIONRECTANGULARSWITCH, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set whether multiple selections can be made (Scintilla feature 2563)</summary>
        public void SetMultipleSelection(bool multipleSelection)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETMULTIPLESELECTION, new IntPtr(multipleSelection ? 1 : 0), (IntPtr) Unused);
        }

        /// <summary>Whether multiple selections can be made (Scintilla feature 2564)</summary>
        public bool GetMultipleSelection()
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETMULTIPLESELECTION, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set whether typing can be performed into multiple selections (Scintilla feature 2565)</summary>
        public void SetAdditionalSelectionTyping(bool additionalSelectionTyping)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETADDITIONALSELECTIONTYPING, new IntPtr(additionalSelectionTyping ? 1 : 0), (IntPtr) Unused);
        }

        /// <summary>Whether typing can be performed into multiple selections (Scintilla feature 2566)</summary>
        public bool GetAdditionalSelectionTyping()
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETADDITIONALSELECTIONTYPING, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set whether additional carets will blink (Scintilla feature 2567)</summary>
        public void SetAdditionalCaretsBlink(bool additionalCaretsBlink)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETADDITIONALCARETSBLINK, new IntPtr(additionalCaretsBlink ? 1 : 0), (IntPtr) Unused);
        }

        /// <summary>Whether additional carets will blink (Scintilla feature 2568)</summary>
        public bool GetAdditionalCaretsBlink()
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETADDITIONALCARETSBLINK, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set whether additional carets are visible (Scintilla feature 2608)</summary>
        public void SetAdditionalCaretsVisible(bool additionalCaretsVisible)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETADDITIONALCARETSVISIBLE, new IntPtr(additionalCaretsVisible ? 1 : 0), (IntPtr) Unused);
        }

        /// <summary>Whether additional carets are visible (Scintilla feature 2609)</summary>
        public bool GetAdditionalCaretsVisible()
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETADDITIONALCARETSVISIBLE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>How many selections are there? (Scintilla feature 2570)</summary>
        public int GetSelections()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETSELECTIONS, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Is every selected range empty? (Scintilla feature 2650)</summary>
        public bool GetSelectionEmpty()
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETSELECTIONEMPTY, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Clear selections to a single empty stream selection (Scintilla feature 2571)</summary>
        public void ClearSelections()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_CLEARSELECTIONS, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set a simple selection (Scintilla feature 2572)</summary>
        public void SetSelection(int caret, int anchor)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETSELECTION, (IntPtr) caret, (IntPtr) anchor);
        }

        /// <summary>Add a selection (Scintilla feature 2573)</summary>
        public void AddSelection(int caret, int anchor)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_ADDSELECTION, (IntPtr) caret, (IntPtr) anchor);
        }

        /// <summary>Drop one selection (Scintilla feature 2671)</summary>
        public void DropSelectionN(int selection)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_DROPSELECTIONN, (IntPtr) selection, (IntPtr) Unused);
        }

        /// <summary>Set the main selection (Scintilla feature 2574)</summary>
        public void SetMainSelection(int selection)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETMAINSELECTION, (IntPtr) selection, (IntPtr) Unused);
        }

        /// <summary>Which selection is the main selection (Scintilla feature 2575)</summary>
        public int GetMainSelection()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETMAINSELECTION, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set the caret position of the nth selection. (Scintilla feature 2576)</summary>
        public void SetSelectionNCaret(int selection, int caret)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETSELECTIONNCARET, (IntPtr) selection, (IntPtr) caret);
        }

        /// <summary>Return the caret position of the nth selection. (Scintilla feature 2577)</summary>
        public int GetSelectionNCaret(int selection)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETSELECTIONNCARET, (IntPtr) selection, (IntPtr) Unused);
        }

        /// <summary>Set the anchor position of the nth selection. (Scintilla feature 2578)</summary>
        public void SetSelectionNAnchor(int selection, int anchor)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETSELECTIONNANCHOR, (IntPtr) selection, (IntPtr) anchor);
        }

        /// <summary>Return the anchor position of the nth selection. (Scintilla feature 2579)</summary>
        public int GetSelectionNAnchor(int selection)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETSELECTIONNANCHOR, (IntPtr) selection, (IntPtr) Unused);
        }

        /// <summary>Set the virtual space of the caret of the nth selection. (Scintilla feature 2580)</summary>
        public void SetSelectionNCaretVirtualSpace(int selection, int space)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETSELECTIONNCARETVIRTUALSPACE, (IntPtr) selection, (IntPtr) space);
        }

        /// <summary>Return the virtual space of the caret of the nth selection. (Scintilla feature 2581)</summary>
        public int GetSelectionNCaretVirtualSpace(int selection)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETSELECTIONNCARETVIRTUALSPACE, (IntPtr) selection, (IntPtr) Unused);
        }

        /// <summary>Set the virtual space of the anchor of the nth selection. (Scintilla feature 2582)</summary>
        public void SetSelectionNAnchorVirtualSpace(int selection, int space)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETSELECTIONNANCHORVIRTUALSPACE, (IntPtr) selection, (IntPtr) space);
        }

        /// <summary>Return the virtual space of the anchor of the nth selection. (Scintilla feature 2583)</summary>
        public int GetSelectionNAnchorVirtualSpace(int selection)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETSELECTIONNANCHORVIRTUALSPACE, (IntPtr) selection, (IntPtr) Unused);
        }

        /// <summary>Sets the position that starts the selection - this becomes the anchor. (Scintilla feature 2584)</summary>
        public void SetSelectionNStart(int selection, int anchor)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETSELECTIONNSTART, (IntPtr) selection, (IntPtr) anchor);
        }

        /// <summary>Returns the position at the start of the selection. (Scintilla feature 2585)</summary>
        public int GetSelectionNStart(int selection)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETSELECTIONNSTART, (IntPtr) selection, (IntPtr) Unused);
        }

        /// <summary>Sets the position that ends the selection - this becomes the currentPosition. (Scintilla feature 2586)</summary>
        public void SetSelectionNEnd(int selection, int caret)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETSELECTIONNEND, (IntPtr) selection, (IntPtr) caret);
        }

        /// <summary>Returns the position at the end of the selection. (Scintilla feature 2587)</summary>
        public int GetSelectionNEnd(int selection)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETSELECTIONNEND, (IntPtr) selection, (IntPtr) Unused);
        }

        /// <summary>Set the caret position of the rectangular selection. (Scintilla feature 2588)</summary>
        public void SetRectangularSelectionCaret(int caret)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETRECTANGULARSELECTIONCARET, (IntPtr) caret, (IntPtr) Unused);
        }

        /// <summary>Return the caret position of the rectangular selection. (Scintilla feature 2589)</summary>
        public int GetRectangularSelectionCaret()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETRECTANGULARSELECTIONCARET, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set the anchor position of the rectangular selection. (Scintilla feature 2590)</summary>
        public void SetRectangularSelectionAnchor(int anchor)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETRECTANGULARSELECTIONANCHOR, (IntPtr) anchor, (IntPtr) Unused);
        }

        /// <summary>Return the anchor position of the rectangular selection. (Scintilla feature 2591)</summary>
        public int GetRectangularSelectionAnchor()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETRECTANGULARSELECTIONANCHOR, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set the virtual space of the caret of the rectangular selection. (Scintilla feature 2592)</summary>
        public void SetRectangularSelectionCaretVirtualSpace(int space)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETRECTANGULARSELECTIONCARETVIRTUALSPACE, (IntPtr) space, (IntPtr) Unused);
        }

        /// <summary>Return the virtual space of the caret of the rectangular selection. (Scintilla feature 2593)</summary>
        public int GetRectangularSelectionCaretVirtualSpace()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETRECTANGULARSELECTIONCARETVIRTUALSPACE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set the virtual space of the anchor of the rectangular selection. (Scintilla feature 2594)</summary>
        public void SetRectangularSelectionAnchorVirtualSpace(int space)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETRECTANGULARSELECTIONANCHORVIRTUALSPACE, (IntPtr) space, (IntPtr) Unused);
        }

        /// <summary>Return the virtual space of the anchor of the rectangular selection. (Scintilla feature 2595)</summary>
        public int GetRectangularSelectionAnchorVirtualSpace()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETRECTANGULARSELECTIONANCHORVIRTUALSPACE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set options for virtual space behaviour. (Scintilla feature 2596)</summary>
        public void SetVirtualSpaceOptions(VirtualSpace virtualSpaceOptions)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETVIRTUALSPACEOPTIONS, (IntPtr) virtualSpaceOptions, (IntPtr) Unused);
        }

        /// <summary>Return options for virtual space behaviour. (Scintilla feature 2597)</summary>
        public VirtualSpace GetVirtualSpaceOptions()
        {
            return (VirtualSpace)Win32.SendMessage(scintilla, SciMsg.SCI_GETVIRTUALSPACEOPTIONS, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// On GTK, allow selecting the modifier key to use for mouse-based
        /// rectangular selection. Often the window manager requires Alt+Mouse Drag
        /// for moving windows.
        /// Valid values are SCMOD_CTRL(default), SCMOD_ALT, or SCMOD_SUPER.
        /// (Scintilla feature 2598)
        /// </summary>
        public void SetRectangularSelectionModifier(int modifier)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETRECTANGULARSELECTIONMODIFIER, (IntPtr) modifier, (IntPtr) Unused);
        }

        /// <summary>Get the modifier key used for rectangular selection. (Scintilla feature 2599)</summary>
        public int GetRectangularSelectionModifier()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETRECTANGULARSELECTIONMODIFIER, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Set the foreground colour of additional selections.
        /// Must have previously called SetSelFore with non-zero first argument for this to have an effect.
        /// (Scintilla feature 2600)
        /// </summary>
        public void SetAdditionalSelFore(Colour fore)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETADDITIONALSELFORE, fore.Value, (IntPtr) Unused);
        }

        /// <summary>
        /// Set the background colour of additional selections.
        /// Must have previously called SetSelBack with non-zero first argument for this to have an effect.
        /// (Scintilla feature 2601)
        /// </summary>
        public void SetAdditionalSelBack(Colour back)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETADDITIONALSELBACK, back.Value, (IntPtr) Unused);
        }

        /// <summary>Set the alpha of the selection. (Scintilla feature 2602)</summary>
        public void SetAdditionalSelAlpha(Alpha alpha)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETADDITIONALSELALPHA, (IntPtr) alpha, (IntPtr) Unused);
        }

        /// <summary>Get the alpha of the selection. (Scintilla feature 2603)</summary>
        public Alpha GetAdditionalSelAlpha()
        {
            return (Alpha)Win32.SendMessage(scintilla, SciMsg.SCI_GETADDITIONALSELALPHA, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set the foreground colour of additional carets. (Scintilla feature 2604)</summary>
        public void SetAdditionalCaretFore(Colour fore)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETADDITIONALCARETFORE, fore.Value, (IntPtr) Unused);
        }

        /// <summary>Get the foreground colour of additional carets. (Scintilla feature 2605)</summary>
        public Colour GetAdditionalCaretFore()
        {
            return new Colour((int) Win32.SendMessage(scintilla, SciMsg.SCI_GETADDITIONALCARETFORE, (IntPtr) Unused, (IntPtr) Unused));
        }

        /// <summary>Set the main selection to the next selection. (Scintilla feature 2606)</summary>
        public void RotateSelection()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_ROTATESELECTION, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Swap that caret and anchor of the main selection. (Scintilla feature 2607)</summary>
        public void SwapMainAnchorCaret()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SWAPMAINANCHORCARET, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Add the next occurrence of the main selection to the set of selections as main.
        /// If the current selection is empty then select word around caret.
        /// (Scintilla feature 2688)
        /// </summary>
        public void MultipleSelectAddNext()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_MULTIPLESELECTADDNEXT, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Add each occurrence of the main selection in the target to the set of selections.
        /// If the current selection is empty then select word around caret.
        /// (Scintilla feature 2689)
        /// </summary>
        public void MultipleSelectAddEach()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_MULTIPLESELECTADDEACH, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Indicate that the internal state of a lexer has changed over a range and therefore
        /// there may be a need to redraw.
        /// (Scintilla feature 2617)
        /// </summary>
        public int ChangeLexerState(int start, int end)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_CHANGELEXERSTATE, (IntPtr) start, (IntPtr) end);
        }

        /// <summary>
        /// Find the next line at or after lineStart that is a contracted fold header line.
        /// Return -1 when no more lines.
        /// (Scintilla feature 2618)
        /// </summary>
        public int ContractedFoldNext(int lineStart)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_CONTRACTEDFOLDNEXT, (IntPtr) lineStart, (IntPtr) Unused);
        }

        /// <summary>Centre current line in window. (Scintilla feature 2619)</summary>
        public void VerticalCentreCaret()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_VERTICALCENTRECARET, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Move the selected lines up one line, shifting the line above after the selection (Scintilla feature 2620)</summary>
        public void MoveSelectedLinesUp()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_MOVESELECTEDLINESUP, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Move the selected lines down one line, shifting the line below before the selection (Scintilla feature 2621)</summary>
        public void MoveSelectedLinesDown()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_MOVESELECTEDLINESDOWN, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set the identifier reported as idFrom in notification messages. (Scintilla feature 2622)</summary>
        public void SetIdentifier(int identifier)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETIDENTIFIER, (IntPtr) identifier, (IntPtr) Unused);
        }

        /// <summary>Get the identifier. (Scintilla feature 2623)</summary>
        public int GetIdentifier()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETIDENTIFIER, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set the width for future RGBA image data. (Scintilla feature 2624)</summary>
        public void RGBAImageSetWidth(int width)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_RGBAIMAGESETWIDTH, (IntPtr) width, (IntPtr) Unused);
        }

        /// <summary>Set the height for future RGBA image data. (Scintilla feature 2625)</summary>
        public void RGBAImageSetHeight(int height)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_RGBAIMAGESETHEIGHT, (IntPtr) height, (IntPtr) Unused);
        }

        /// <summary>Set the scale factor in percent for future RGBA image data. (Scintilla feature 2651)</summary>
        public void RGBAImageSetScale(int scalePercent)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_RGBAIMAGESETSCALE, (IntPtr) scalePercent, (IntPtr) Unused);
        }

        /// <summary>
        /// Define a marker from RGBA data.
        /// It has the width and height from RGBAImageSetWidth/Height
        /// (Scintilla feature 2626)
        /// </summary>
        public unsafe void MarkerDefineRGBAImage(int markerNumber, string pixels)
        {
            fixed (byte* pixelsPtr = Encoding.UTF8.GetBytes(pixels))
            {
                Win32.SendMessage(scintilla, SciMsg.SCI_MARKERDEFINERGBAIMAGE, (IntPtr) markerNumber, (IntPtr) pixelsPtr);
            }
        }

        /// <summary>
        /// Register an RGBA image for use in autocompletion lists.
        /// It has the width and height from RGBAImageSetWidth/Height
        /// (Scintilla feature 2627)
        /// </summary>
        public unsafe void RegisterRGBAImage(int type, string pixels)
        {
            fixed (byte* pixelsPtr = Encoding.UTF8.GetBytes(pixels))
            {
                Win32.SendMessage(scintilla, SciMsg.SCI_REGISTERRGBAIMAGE, (IntPtr) type, (IntPtr) pixelsPtr);
            }
        }

        /// <summary>Scroll to start of document. (Scintilla feature 2628)</summary>
        public void ScrollToStart()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SCROLLTOSTART, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Scroll to end of document. (Scintilla feature 2629)</summary>
        public void ScrollToEnd()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SCROLLTOEND, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set the technology used. (Scintilla feature 2630)</summary>
        public void SetTechnology(Technology technology)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETTECHNOLOGY, (IntPtr) technology, (IntPtr) Unused);
        }

        /// <summary>Get the tech. (Scintilla feature 2631)</summary>
        public Technology GetTechnology()
        {
            return (Technology)Win32.SendMessage(scintilla, SciMsg.SCI_GETTECHNOLOGY, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Create an ILoader*. (Scintilla feature 2632)</summary>
        public IntPtr CreateLoader(int bytes, DocumentOption documentOptions)
        {
            return Win32.SendMessage(scintilla, SciMsg.SCI_CREATELOADER, (IntPtr) bytes, (IntPtr) documentOptions);
        }

        /// <summary>On OS X, show a find indicator. (Scintilla feature 2640)</summary>
        public void FindIndicatorShow(int start, int end)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_FINDINDICATORSHOW, (IntPtr) start, (IntPtr) end);
        }

        /// <summary>On OS X, flash a find indicator, then fade out. (Scintilla feature 2641)</summary>
        public void FindIndicatorFlash(int start, int end)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_FINDINDICATORFLASH, (IntPtr) start, (IntPtr) end);
        }

        /// <summary>On OS X, hide the find indicator. (Scintilla feature 2642)</summary>
        public void FindIndicatorHide()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_FINDINDICATORHIDE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Move caret to before first visible character on display line.
        /// If already there move to first character on display line.
        /// (Scintilla feature 2652)
        /// </summary>
        public void VCHomeDisplay()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_VCHOMEDISPLAY, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Like VCHomeDisplay but extending selection to new caret position. (Scintilla feature 2653)</summary>
        public void VCHomeDisplayExtend()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_VCHOMEDISPLAYEXTEND, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Is the caret line always visible? (Scintilla feature 2654)</summary>
        public bool GetCaretLineVisibleAlways()
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETCARETLINEVISIBLEALWAYS, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Sets the caret line to always visible. (Scintilla feature 2655)</summary>
        public void SetCaretLineVisibleAlways(bool alwaysVisible)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETCARETLINEVISIBLEALWAYS, new IntPtr(alwaysVisible ? 1 : 0), (IntPtr) Unused);
        }

        /// <summary>Set the line end types that the application wants to use. May not be used if incompatible with lexer or encoding. (Scintilla feature 2656)</summary>
        public void SetLineEndTypesAllowed(LineEndType lineEndBitSet)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETLINEENDTYPESALLOWED, (IntPtr) lineEndBitSet, (IntPtr) Unused);
        }

        /// <summary>Get the line end types currently allowed. (Scintilla feature 2657)</summary>
        public LineEndType GetLineEndTypesAllowed()
        {
            return (LineEndType)Win32.SendMessage(scintilla, SciMsg.SCI_GETLINEENDTYPESALLOWED, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Get the line end types currently recognised. May be a subset of the allowed types due to lexer limitation. (Scintilla feature 2658)</summary>
        public LineEndType GetLineEndTypesActive()
        {
            return (LineEndType)Win32.SendMessage(scintilla, SciMsg.SCI_GETLINEENDTYPESACTIVE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set the way a character is drawn. (Scintilla feature 2665)</summary>
        public unsafe void SetRepresentation(string encodedCharacter, string representation)
        {
            fixed (byte* encodedCharacterPtr = Encoding.UTF8.GetBytes(encodedCharacter))
            {
                fixed (byte* representationPtr = Encoding.UTF8.GetBytes(representation))
                {
                    Win32.SendMessage(scintilla, SciMsg.SCI_SETREPRESENTATION, (IntPtr) encodedCharacterPtr, (IntPtr) representationPtr);
                }
            }
        }

        /// <summary>
        /// Set the way a character is drawn.
        /// Result is NUL-terminated.
        /// (Scintilla feature 2666)
        /// </summary>
        public unsafe string GetRepresentation(string encodedCharacter)
        {
            fixed (byte* encodedCharacterPtr = Encoding.UTF8.GetBytes(encodedCharacter))
            {
                return GetNullStrippedStringFromMessageThatReturnsLength(SciMsg.SCI_GETREPRESENTATION, (IntPtr)encodedCharacterPtr);
            }
        }

        /// <summary>Remove a character representation. (Scintilla feature 2667)</summary>
        public unsafe void ClearRepresentation(string encodedCharacter)
        {
            fixed (byte* encodedCharacterPtr = Encoding.UTF8.GetBytes(encodedCharacter))
            {
                Win32.SendMessage(scintilla, SciMsg.SCI_CLEARREPRESENTATION, (IntPtr) encodedCharacterPtr, (IntPtr) Unused);
            }
        }

        /// <summary>Start notifying the container of all key presses and commands. (Scintilla feature 3001)</summary>
        public void StartRecord()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_STARTRECORD, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Stop notifying the container of all key presses and commands. (Scintilla feature 3002)</summary>
        public void StopRecord()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_STOPRECORD, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set the lexing language of the document. (Scintilla feature 4001)</summary>
        public void SetLexer(int lexer)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETLEXER, (IntPtr) lexer, (IntPtr) Unused);
        }

        /// <summary>Retrieve the lexing language of the document. (Scintilla feature 4002)</summary>
        public int GetLexer()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETLEXER, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Colourise a segment of the document using the current lexing language. (Scintilla feature 4003)</summary>
        public void Colourise(int start, int end)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_COLOURISE, (IntPtr) start, (IntPtr) end);
        }

        /// <summary>Set up a value that may be used by a lexer for some optional feature. (Scintilla feature 4004)</summary>
        public unsafe void SetProperty(string key, string value)
        {
            fixed (byte* keyPtr = Encoding.UTF8.GetBytes(key))
            {
                fixed (byte* valuePtr = Encoding.UTF8.GetBytes(value))
                {
                    Win32.SendMessage(scintilla, SciMsg.SCI_SETPROPERTY, (IntPtr) keyPtr, (IntPtr) valuePtr);
                }
            }
        }

        /// <summary>Set up the key words used by the lexer. (Scintilla feature 4005)</summary>
        public unsafe void SetKeyWords(int keyWordSet, string keyWords)
        {
            fixed (byte* keyWordsPtr = Encoding.UTF8.GetBytes(keyWords))
            {
                Win32.SendMessage(scintilla, SciMsg.SCI_SETKEYWORDS, (IntPtr) keyWordSet, (IntPtr) keyWordsPtr);
            }
        }

        /// <summary>Set the lexing language of the document based on string name. (Scintilla feature 4006)</summary>
        public unsafe void SetLexerLanguage(string language)
        {
            fixed (byte* languagePtr = Encoding.UTF8.GetBytes(language))
            {
                Win32.SendMessage(scintilla, SciMsg.SCI_SETLEXERLANGUAGE, (IntPtr) Unused, (IntPtr) languagePtr);
            }
        }

        /// <summary>Load a lexer library (dll / so). (Scintilla feature 4007)</summary>
        public unsafe void LoadLexerLibrary(string path)
        {
            fixed (byte* pathPtr = Encoding.UTF8.GetBytes(path))
            {
                Win32.SendMessage(scintilla, SciMsg.SCI_LOADLEXERLIBRARY, (IntPtr) Unused, (IntPtr) pathPtr);
            }
        }

        /// <summary>
        /// Retrieve a "property" value previously set with SetProperty.
        /// Result is NUL-terminated.
        /// (Scintilla feature 4008)
        /// </summary>
        public unsafe string GetProperty(string key)
        {
            fixed (byte* keyPtr = Encoding.UTF8.GetBytes(key))
            {
                return GetNullStrippedStringFromMessageThatReturnsLength(SciMsg.SCI_GETPROPERTY, (IntPtr)keyPtr);
            }
        }

        /// <summary>
        /// Retrieve a "property" value previously set with SetProperty,
        /// with "$()" variable replacement on returned buffer.
        /// Result is NUL-terminated.
        /// (Scintilla feature 4009)
        /// </summary>
        public unsafe string GetPropertyExpanded(string key)
        {
            fixed (byte* keyPtr = Encoding.UTF8.GetBytes(key))
            {
                return GetNullStrippedStringFromMessageThatReturnsLength(SciMsg.SCI_GETPROPERTYEXPANDED, (IntPtr)keyPtr);
            }
        }

        /// <summary>
        /// Retrieve a "property" value previously set with SetProperty,
        /// interpreted as an int AFTER any "$()" variable replacement.
        /// (Scintilla feature 4010)
        /// </summary>
        public unsafe int GetPropertyInt(string key, int defaultValue)
        {
            fixed (byte* keyPtr = Encoding.UTF8.GetBytes(key))
            {
                return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETPROPERTYINT, (IntPtr) keyPtr, (IntPtr) defaultValue);
            }
        }

        /// <summary>
        /// Retrieve the name of the lexer.
        /// Return the length of the text.
        /// Result is NUL-terminated.
        /// (Scintilla feature 4012)
        /// </summary>
        public unsafe string GetLexerLanguage()
        {
            return GetNullStrippedStringFromMessageThatReturnsLength(SciMsg.SCI_GETLEXERLANGUAGE);
        }

        /// <summary>For private communication between an application and a known lexer. (Scintilla feature 4013)</summary>
        public IntPtr PrivateLexerCall(int operation, IntPtr pointer)
        {
            return Win32.SendMessage(scintilla, SciMsg.SCI_PRIVATELEXERCALL, (IntPtr) operation, (IntPtr) pointer);
        }

        /// <summary>
        /// Retrieve a '\n' separated list of properties understood by the current lexer.
        /// Result is NUL-terminated.
        /// (Scintilla feature 4014)
        /// </summary>
        public unsafe string PropertyNames()
        {
            return GetNullStrippedStringFromMessageThatReturnsLength(SciMsg.SCI_PROPERTYNAMES);
        }

        /// <summary>Retrieve the type of a property. (Scintilla feature 4015)</summary>
        public unsafe TypeProperty PropertyType(string name)
        {
            fixed (byte* namePtr = Encoding.UTF8.GetBytes(name))
            {
                return (TypeProperty)Win32.SendMessage(scintilla, SciMsg.SCI_PROPERTYTYPE, (IntPtr) namePtr, (IntPtr) Unused);
            }
        }

        /// <summary>
        /// Describe a property.
        /// Result is NUL-terminated.
        /// (Scintilla feature 4016)
        /// </summary>
        public unsafe string DescribeProperty(string name)
        {
            fixed (byte* namePtr = Encoding.UTF8.GetBytes(name))
            {
                return GetNullStrippedStringFromMessageThatReturnsLength(SciMsg.SCI_DESCRIBEPROPERTY, (IntPtr)namePtr);
            }
        }

        /// <summary>
        /// Retrieve a '\n' separated list of descriptions of the keyword sets understood by the current lexer.
        /// Result is NUL-terminated.
        /// (Scintilla feature 4017)
        /// </summary>
        public unsafe string DescribeKeyWordSets()
        {
            return GetNullStrippedStringFromMessageThatReturnsLength(SciMsg.SCI_DESCRIBEKEYWORDSETS);
        }

        /// <summary>
        /// Bit set of LineEndType enumertion for which line ends beyond the standard
        /// LF, CR, and CRLF are supported by the lexer.
        /// (Scintilla feature 4018)
        /// </summary>
        public int GetLineEndTypesSupported()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETLINEENDTYPESSUPPORTED, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Allocate a set of sub styles for a particular base style, returning start of range (Scintilla feature 4020)</summary>
        public int AllocateSubStyles(int styleBase, int numberStyles)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_ALLOCATESUBSTYLES, (IntPtr) styleBase, (IntPtr) numberStyles);
        }

        /// <summary>The starting style number for the sub styles associated with a base style (Scintilla feature 4021)</summary>
        public int GetSubStylesStart(int styleBase)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETSUBSTYLESSTART, (IntPtr) styleBase, (IntPtr) Unused);
        }

        /// <summary>The number of sub styles associated with a base style (Scintilla feature 4022)</summary>
        public int GetSubStylesLength(int styleBase)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETSUBSTYLESLENGTH, (IntPtr) styleBase, (IntPtr) Unused);
        }

        /// <summary>For a sub style, return the base style, else return the argument. (Scintilla feature 4027)</summary>
        public int GetStyleFromSubStyle(int subStyle)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETSTYLEFROMSUBSTYLE, (IntPtr) subStyle, (IntPtr) Unused);
        }

        /// <summary>For a secondary style, return the primary style, else return the argument. (Scintilla feature 4028)</summary>
        public int GetPrimaryStyleFromStyle(int style)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETPRIMARYSTYLEFROMSTYLE, (IntPtr) style, (IntPtr) Unused);
        }

        /// <summary>Free allocated sub styles (Scintilla feature 4023)</summary>
        public void FreeSubStyles()
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_FREESUBSTYLES, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set the identifiers that are shown in a particular style (Scintilla feature 4024)</summary>
        public unsafe void SetIdentifiers(int style, string identifiers)
        {
            fixed (byte* identifiersPtr = Encoding.UTF8.GetBytes(identifiers))
            {
                Win32.SendMessage(scintilla, SciMsg.SCI_SETIDENTIFIERS, (IntPtr) style, (IntPtr) identifiersPtr);
            }
        }

        /// <summary>
        /// Where styles are duplicated by a feature such as active/inactive code
        /// return the distance between the two types.
        /// (Scintilla feature 4025)
        /// </summary>
        public int DistanceToSecondaryStyles()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_DISTANCETOSECONDARYSTYLES, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Get the set of base styles that can be extended with sub styles
        /// Result is NUL-terminated.
        /// (Scintilla feature 4026)
        /// </summary>
        public unsafe string GetSubStyleBases()
        {
            return GetNullStrippedStringFromMessageThatReturnsLength(SciMsg.SCI_GETSUBSTYLEBASES);
        }

        /// <summary>Retrieve the number of named styles for the lexer. (Scintilla feature 4029)</summary>
        public int GetNamedStyles()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETNAMEDSTYLES, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Retrieve the name of a style.
        /// Result is NUL-terminated.
        /// (Scintilla feature 4030)
        /// </summary>
        public unsafe string NameOfStyle(int style)
        {
            return GetNullStrippedStringFromMessageThatReturnsLength(SciMsg.SCI_NAMEOFSTYLE, (IntPtr)style);
        }

        /// <summary>
        /// Retrieve a ' ' separated list of style tags like "literal quoted string".
        /// Result is NUL-terminated.
        /// (Scintilla feature 4031)
        /// </summary>
        public unsafe string TagsOfStyle(int style)
        {
            return GetNullStrippedStringFromMessageThatReturnsLength(SciMsg.SCI_TAGSOFSTYLE, (IntPtr)style);
        }

        /// <summary>
        /// Retrieve a description of a style.
        /// Result is NUL-terminated.
        /// (Scintilla feature 4032)
        /// </summary>
        public unsafe string DescriptionOfStyle(int style)
        {
            return GetNullStrippedStringFromMessageThatReturnsLength(SciMsg.SCI_DESCRIPTIONOFSTYLE, (IntPtr)style);
        }

        /// <summary>Retrieve bidirectional text display state. (Scintilla feature 2708)</summary>
        public Bidirectional GetBidirectional()
        {
            return (Bidirectional)Win32.SendMessage(scintilla, SciMsg.SCI_GETBIDIRECTIONAL, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Set bidirectional text display state. (Scintilla feature 2709)</summary>
        public void SetBidirectional(Bidirectional bidirectional)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETBIDIRECTIONAL, (IntPtr) bidirectional, (IntPtr) Unused);
        }

        /// <summary>Retrieve line character index state. (Scintilla feature 2710)</summary>
        public LineCharacterIndexType GetLineCharacterIndex()
        {
            return (LineCharacterIndexType)Win32.SendMessage(scintilla, SciMsg.SCI_GETLINECHARACTERINDEX, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Request line character index be created or its use count increased. (Scintilla feature 2711)</summary>
        public void AllocateLineCharacterIndex(LineCharacterIndexType lineCharacterIndex)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_ALLOCATELINECHARACTERINDEX, (IntPtr) lineCharacterIndex, (IntPtr) Unused);
        }

        /// <summary>Decrease use count of line character index and remove if 0. (Scintilla feature 2712)</summary>
        public void ReleaseLineCharacterIndex(LineCharacterIndexType lineCharacterIndex)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_RELEASELINECHARACTERINDEX, (IntPtr) lineCharacterIndex, (IntPtr) Unused);
        }

        /// <summary>Retrieve the document line containing a position measured in index units. (Scintilla feature 2713)</summary>
        public int LineFromIndexPosition(int pos, LineCharacterIndexType lineCharacterIndex)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_LINEFROMINDEXPOSITION, (IntPtr) pos, (IntPtr) lineCharacterIndex);
        }

        /// <summary>Retrieve the position measured in index units at the start of a document line. (Scintilla feature 2714)</summary>
        public int IndexPositionFromLine(int line, LineCharacterIndexType lineCharacterIndex)
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_INDEXPOSITIONFROMLINE, (IntPtr) line, (IntPtr) lineCharacterIndex);
        }

        /// <summary>
        /// Divide each styling byte into lexical class bits (default: 5) and indicator
        /// bits (default: 3). If a lexer requires more than 32 lexical states, then this
        /// is used to expand the possible states.
        /// (Scintilla feature 2090)
        /// </summary>
        public void SetStyleBits(int bits)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETSTYLEBITS, (IntPtr) bits, (IntPtr) Unused);
        }

        /// <summary>Retrieve number of bits in style bytes used to hold the lexical state. (Scintilla feature 2091)</summary>
        public int GetStyleBits()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETSTYLEBITS, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Retrieve the number of bits the current lexer needs for styling. (Scintilla feature 4011)</summary>
        public int GetStyleBitsNeeded()
        {
            return (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETSTYLEBITSNEEDED, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// Deprecated in 3.5.5
        /// Always interpret keyboard input as Unicode
        /// (Scintilla feature 2521)
        /// </summary>
        public void SetKeysUnicode(bool keysUnicode)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETKEYSUNICODE, new IntPtr(keysUnicode ? 1 : 0), (IntPtr) Unused);
        }

        /// <summary>Are keys always interpreted as Unicode? (Scintilla feature 2522)</summary>
        public bool GetKeysUnicode()
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETKEYSUNICODE, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>Is drawing done in two phases with backgrounds drawn before foregrounds? (Scintilla feature 2283)</summary>
        public bool GetTwoPhaseDraw()
        {
            return 1 == (int)Win32.SendMessage(scintilla, SciMsg.SCI_GETTWOPHASEDRAW, (IntPtr) Unused, (IntPtr) Unused);
        }

        /// <summary>
        /// In twoPhaseDraw mode, drawing is performed in two phases, first the background
        /// and then the foreground. This avoids chopping off characters that overlap the next run.
        /// (Scintilla feature 2284)
        /// </summary>
        public void SetTwoPhaseDraw(bool twoPhase)
        {
            Win32.SendMessage(scintilla, SciMsg.SCI_SETTWOPHASEDRAW, new IntPtr(twoPhase ? 1 : 0), (IntPtr) Unused);
        }

        /* --Autogenerated -- end of section automatically generated from Scintilla.iface */
    }
}
