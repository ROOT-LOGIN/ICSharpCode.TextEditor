using System;
using System.Drawing;
using System.Drawing.Text;
using System.Text;

namespace ICSharpCode.TextEditor.Document
{
	public interface ITextEditorProperties
	{
		bool AllowCaretBeyondEOL
		{
			get;
			set;
		}

		bool AutoInsertCurlyBracket
		{
			get;
			set;
		}

		ICSharpCode.TextEditor.Document.BracketMatchingStyle BracketMatchingStyle
		{
			get;
			set;
		}

		bool CaretLine
		{
			get;
			set;
		}

		bool ConvertTabsToSpaces
		{
			get;
			set;
		}

		bool CutCopyWholeLine
		{
			get;
			set;
		}

		ICSharpCode.TextEditor.Document.DocumentSelectionMode DocumentSelectionMode
		{
			get;
			set;
		}

		bool EnableFolding
		{
			get;
			set;
		}

		System.Text.Encoding Encoding
		{
			get;
			set;
		}

		System.Drawing.Font Font
		{
			get;
			set;
		}

		ICSharpCode.TextEditor.Document.FontContainer FontContainer
		{
			get;
		}

		bool HideMouseCursor
		{
			get;
			set;
		}

		int IndentationSize
		{
			get;
			set;
		}

		ICSharpCode.TextEditor.Document.IndentStyle IndentStyle
		{
			get;
			set;
		}

		bool IsIconBarVisible
		{
			get;
			set;
		}

		string LineTerminator
		{
			get;
			set;
		}

		ICSharpCode.TextEditor.Document.LineViewerStyle LineViewerStyle
		{
			get;
			set;
		}

		bool MouseWheelScrollDown
		{
			get;
			set;
		}

		bool MouseWheelTextZoom
		{
			get;
			set;
		}

		bool ShowEOLMarker
		{
			get;
			set;
		}

		bool ShowHorizontalRuler
		{
			get;
			set;
		}

		bool ShowInvalidLines
		{
			get;
			set;
		}

		bool ShowLineNumbers
		{
			get;
			set;
		}

		bool ShowMatchingBracket
		{
			get;
			set;
		}

		bool ShowSpaces
		{
			get;
			set;
		}

		bool ShowTabs
		{
			get;
			set;
		}

		bool ShowVerticalRuler
		{
			get;
			set;
		}

		bool SupportReadOnlySegments
		{
			get;
			set;
		}

		int TabIndent
		{
			get;
			set;
		}

		System.Drawing.Text.TextRenderingHint TextRenderingHint
		{
			get;
			set;
		}

		int VerticalRulerRow
		{
			get;
			set;
		}
	}
}