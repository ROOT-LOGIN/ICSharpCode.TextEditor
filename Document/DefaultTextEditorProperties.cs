using System;
using System.Drawing;
using System.Drawing.Text;
using System.Text;

namespace ICSharpCode.TextEditor.Document
{
	public class DefaultTextEditorProperties : ITextEditorProperties
	{
		private int tabIndent = 4;

		private int indentationSize = 4;

		private ICSharpCode.TextEditor.Document.IndentStyle indentStyle = ICSharpCode.TextEditor.Document.IndentStyle.Smart;

		private ICSharpCode.TextEditor.Document.DocumentSelectionMode documentSelectionMode;

		private System.Text.Encoding encoding = System.Text.Encoding.UTF8;

		private ICSharpCode.TextEditor.Document.BracketMatchingStyle bracketMatchingStyle = ICSharpCode.TextEditor.Document.BracketMatchingStyle.After;

		private ICSharpCode.TextEditor.Document.FontContainer fontContainer;

		private static System.Drawing.Font DefaultFont;

		private bool allowCaretBeyondEOL;

		private bool caretLine;

		private bool showMatchingBracket = true;

		private bool showLineNumbers = true;

		private bool showSpaces;

		private bool showTabs;

		private bool showEOLMarker;

		private bool showInvalidLines;

		private bool isIconBarVisible;

		private bool enableFolding = true;

		private bool showHorizontalRuler;

		private bool showVerticalRuler = true;

		private bool convertTabsToSpaces;

		private System.Drawing.Text.TextRenderingHint textRenderingHint;

		private bool mouseWheelScrollDown = true;

		private bool mouseWheelTextZoom = true;

		private bool hideMouseCursor;

		private bool cutCopyWholeLine = true;

		private int verticalRulerRow = 80;

		private ICSharpCode.TextEditor.Document.LineViewerStyle lineViewerStyle;

		private string lineTerminator = "\r\n";

		private bool autoInsertCurlyBracket = true;

		private bool supportReadOnlySegments;

		public bool AllowCaretBeyondEOL
		{
			get
			{
				return this.allowCaretBeyondEOL;
			}
			set
			{
				this.allowCaretBeyondEOL = value;
			}
		}

		public bool AutoInsertCurlyBracket
		{
			get
			{
				return this.autoInsertCurlyBracket;
			}
			set
			{
				this.autoInsertCurlyBracket = value;
			}
		}

		public ICSharpCode.TextEditor.Document.BracketMatchingStyle BracketMatchingStyle
		{
			get
			{
				return this.bracketMatchingStyle;
			}
			set
			{
				this.bracketMatchingStyle = value;
			}
		}

		public bool CaretLine
		{
			get
			{
				return this.caretLine;
			}
			set
			{
				this.caretLine = value;
			}
		}

		public bool ConvertTabsToSpaces
		{
			get
			{
				return this.convertTabsToSpaces;
			}
			set
			{
				this.convertTabsToSpaces = value;
			}
		}

		public bool CutCopyWholeLine
		{
			get
			{
				return this.cutCopyWholeLine;
			}
			set
			{
				this.cutCopyWholeLine = value;
			}
		}

		public ICSharpCode.TextEditor.Document.DocumentSelectionMode DocumentSelectionMode
		{
			get
			{
				return this.documentSelectionMode;
			}
			set
			{
				this.documentSelectionMode = value;
			}
		}

		public bool EnableFolding
		{
			get
			{
				return this.enableFolding;
			}
			set
			{
				this.enableFolding = value;
			}
		}

		public System.Text.Encoding Encoding
		{
			get
			{
				return this.encoding;
			}
			set
			{
				this.encoding = value;
			}
		}

		public System.Drawing.Font Font
		{
			get
			{
				return this.fontContainer.DefaultFont;
			}
			set
			{
				this.fontContainer.DefaultFont = value;
			}
		}

		public ICSharpCode.TextEditor.Document.FontContainer FontContainer
		{
			get
			{
				return this.fontContainer;
			}
		}

		public bool HideMouseCursor
		{
			get
			{
				return this.hideMouseCursor;
			}
			set
			{
				this.hideMouseCursor = value;
			}
		}

		public int IndentationSize
		{
			get
			{
				return this.indentationSize;
			}
			set
			{
				this.indentationSize = value;
			}
		}

		public ICSharpCode.TextEditor.Document.IndentStyle IndentStyle
		{
			get
			{
				return this.indentStyle;
			}
			set
			{
				this.indentStyle = value;
			}
		}

		public bool IsIconBarVisible
		{
			get
			{
				return this.isIconBarVisible;
			}
			set
			{
				this.isIconBarVisible = value;
			}
		}

		public string LineTerminator
		{
			get
			{
				return this.lineTerminator;
			}
			set
			{
				this.lineTerminator = value;
			}
		}

		public ICSharpCode.TextEditor.Document.LineViewerStyle LineViewerStyle
		{
			get
			{
				return this.lineViewerStyle;
			}
			set
			{
				this.lineViewerStyle = value;
			}
		}

		public bool MouseWheelScrollDown
		{
			get
			{
				return this.mouseWheelScrollDown;
			}
			set
			{
				this.mouseWheelScrollDown = value;
			}
		}

		public bool MouseWheelTextZoom
		{
			get
			{
				return this.mouseWheelTextZoom;
			}
			set
			{
				this.mouseWheelTextZoom = value;
			}
		}

		public bool ShowEOLMarker
		{
			get
			{
				return this.showEOLMarker;
			}
			set
			{
				this.showEOLMarker = value;
			}
		}

		public bool ShowHorizontalRuler
		{
			get
			{
				return this.showHorizontalRuler;
			}
			set
			{
				this.showHorizontalRuler = value;
			}
		}

		public bool ShowInvalidLines
		{
			get
			{
				return this.showInvalidLines;
			}
			set
			{
				this.showInvalidLines = value;
			}
		}

		public bool ShowLineNumbers
		{
			get
			{
				return this.showLineNumbers;
			}
			set
			{
				this.showLineNumbers = value;
			}
		}

		public bool ShowMatchingBracket
		{
			get
			{
				return this.showMatchingBracket;
			}
			set
			{
				this.showMatchingBracket = value;
			}
		}

		public bool ShowSpaces
		{
			get
			{
				return this.showSpaces;
			}
			set
			{
				this.showSpaces = value;
			}
		}

		public bool ShowTabs
		{
			get
			{
				return this.showTabs;
			}
			set
			{
				this.showTabs = value;
			}
		}

		public bool ShowVerticalRuler
		{
			get
			{
				return this.showVerticalRuler;
			}
			set
			{
				this.showVerticalRuler = value;
			}
		}

		public bool SupportReadOnlySegments
		{
			get
			{
				return this.supportReadOnlySegments;
			}
			set
			{
				this.supportReadOnlySegments = value;
			}
		}

		public int TabIndent
		{
			get
			{
				return this.tabIndent;
			}
			set
			{
				this.tabIndent = value;
			}
		}

		public System.Drawing.Text.TextRenderingHint TextRenderingHint
		{
			get
			{
				return this.textRenderingHint;
			}
			set
			{
				this.textRenderingHint = value;
			}
		}

		public int VerticalRulerRow
		{
			get
			{
				return this.verticalRulerRow;
			}
			set
			{
				this.verticalRulerRow = value;
			}
		}

		public DefaultTextEditorProperties()
		{
			if (DefaultTextEditorProperties.DefaultFont == null)
			{
				DefaultTextEditorProperties.DefaultFont = new System.Drawing.Font("Courier New", 10f);
			}
			this.fontContainer = new ICSharpCode.TextEditor.Document.FontContainer(DefaultTextEditorProperties.DefaultFont);
		}
	}
}