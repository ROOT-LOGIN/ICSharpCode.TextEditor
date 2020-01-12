using ICSharpCode.TextEditor.Actions;
using ICSharpCode.TextEditor.Document;
using ICSharpCode.TextEditor.Undo;
using ICSharpCode.TextEditor.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Text;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;

namespace ICSharpCode.TextEditor
{
	[ToolboxItem(false)]
	public abstract class TextEditorControlBase : UserControl
	{
		private string currentFileName;

		private int updateLevel;

		private IDocument document;

		protected Dictionary<Keys, IEditAction> editactions = new Dictionary<Keys, IEditAction>();

		private System.Text.Encoding encoding;

		public abstract TextAreaControl ActiveTextAreaControl
		{
			get;
		}

		[Category("Behavior")]
		[DefaultValue(false)]
		[Description("Allows the caret to be placed beyond the end of line")]
		public bool AllowCaretBeyondEOL
		{
			get
			{
				return this.document.TextEditorProperties.AllowCaretBeyondEOL;
			}
			set
			{
				this.document.TextEditorProperties.AllowCaretBeyondEOL = value;
				this.OptionsChanged();
			}
		}

		[Category("Behavior")]
		[DefaultValue(ICSharpCode.TextEditor.Document.BracketMatchingStyle.After)]
		[Description("Specifies if the bracket matching should match the bracket before or after the caret.")]
		public ICSharpCode.TextEditor.Document.BracketMatchingStyle BracketMatchingStyle
		{
			get
			{
				return this.document.TextEditorProperties.BracketMatchingStyle;
			}
			set
			{
				this.document.TextEditorProperties.BracketMatchingStyle = value;
				this.OptionsChanged();
			}
		}

		[Category("Behavior")]
		[DefaultValue(false)]
		[Description("Converts tabs to spaces while typing")]
		public bool ConvertTabsToSpaces
		{
			get
			{
				return this.document.TextEditorProperties.ConvertTabsToSpaces;
			}
			set
			{
				this.document.TextEditorProperties.ConvertTabsToSpaces = value;
				this.OptionsChanged();
			}
		}

		protected override System.Drawing.Size DefaultSize
		{
			get
			{
				return new System.Drawing.Size(100, 100);
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public IDocument Document
		{
			get
			{
				return this.document;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				if (this.document != null)
				{
					this.document.DocumentChanged -= new DocumentEventHandler(this.OnDocumentChanged);
				}
				this.document = value;
				this.document.UndoStack.TextEditorControl = this;
				this.document.DocumentChanged += new DocumentEventHandler(this.OnDocumentChanged);
			}
		}

		[Category("Appearance")]
		[DefaultValue(true)]
		[Description("If true folding is enabled in the textarea")]
		public bool EnableFolding
		{
			get
			{
				return this.document.TextEditorProperties.EnableFolding;
			}
			set
			{
				this.document.TextEditorProperties.EnableFolding = value;
				this.OptionsChanged();
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public System.Text.Encoding Encoding
		{
			get
			{
				if (this.encoding != null)
				{
					return this.encoding;
				}
				return this.TextEditorProperties.Encoding;
			}
			set
			{
				this.encoding = value;
			}
		}

		[Browsable(false)]
		[ReadOnly(true)]
		public string FileName
		{
			get
			{
				return this.currentFileName;
			}
			set
			{
				if (this.currentFileName != value)
				{
					this.currentFileName = value;
					this.OnFileNameChanged(EventArgs.Empty);
				}
			}
		}

		[Browsable(true)]
		[Description("The base font of the text area. No bold or italic fonts can be used because bold/italic is reserved for highlighting purposes.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public override System.Drawing.Font Font
		{
			get
			{
				return this.document.TextEditorProperties.Font;
			}
			set
			{
				this.document.TextEditorProperties.Font = value;
				this.OptionsChanged();
			}
		}

		[Category("Behavior")]
		[DefaultValue(false)]
		[Description("Hide the mouse cursor while typing")]
		public bool HideMouseCursor
		{
			get
			{
				return this.document.TextEditorProperties.HideMouseCursor;
			}
			set
			{
				this.document.TextEditorProperties.HideMouseCursor = value;
				this.OptionsChanged();
			}
		}

		[Category("Behavior")]
		[DefaultValue(ICSharpCode.TextEditor.Document.IndentStyle.Smart)]
		[Description("The indent style")]
		public ICSharpCode.TextEditor.Document.IndentStyle IndentStyle
		{
			get
			{
				return this.document.TextEditorProperties.IndentStyle;
			}
			set
			{
				this.document.TextEditorProperties.IndentStyle = value;
				this.OptionsChanged();
			}
		}

		[Category("Appearance")]
		[DefaultValue(false)]
		[Description("If true the icon bar is displayed")]
		public bool IsIconBarVisible
		{
			get
			{
				return this.document.TextEditorProperties.IsIconBarVisible;
			}
			set
			{
				this.document.TextEditorProperties.IsIconBarVisible = value;
				this.OptionsChanged();
			}
		}

		[Browsable(false)]
		public bool IsInUpdate
		{
			get
			{
				return this.updateLevel > 0;
			}
		}

		[Browsable(false)]
		public bool IsReadOnly
		{
			get
			{
				return this.Document.ReadOnly;
			}
			set
			{
				this.Document.ReadOnly = value;
			}
		}

		[Category("Appearance")]
		[DefaultValue(ICSharpCode.TextEditor.Document.LineViewerStyle.None)]
		[Description("The line viewer style")]
		public ICSharpCode.TextEditor.Document.LineViewerStyle LineViewerStyle
		{
			get
			{
				return this.document.TextEditorProperties.LineViewerStyle;
			}
			set
			{
				this.document.TextEditorProperties.LineViewerStyle = value;
				this.OptionsChanged();
			}
		}

		[Category("Appearance")]
		[DefaultValue(false)]
		[Description("If true EOL markers are shown in the textarea")]
		public bool ShowEOLMarkers
		{
			get
			{
				return this.document.TextEditorProperties.ShowEOLMarker;
			}
			set
			{
				this.document.TextEditorProperties.ShowEOLMarker = value;
				this.OptionsChanged();
			}
		}

		[Category("Appearance")]
		[DefaultValue(false)]
		[Description("If true the horizontal ruler is shown in the textarea")]
		public bool ShowHRuler
		{
			get
			{
				return this.document.TextEditorProperties.ShowHorizontalRuler;
			}
			set
			{
				this.document.TextEditorProperties.ShowHorizontalRuler = value;
				this.OptionsChanged();
			}
		}

		[Category("Appearance")]
		[DefaultValue(false)]
		[Description("If true invalid lines are marked in the textarea")]
		public bool ShowInvalidLines
		{
			get
			{
				return this.document.TextEditorProperties.ShowInvalidLines;
			}
			set
			{
				this.document.TextEditorProperties.ShowInvalidLines = value;
				this.OptionsChanged();
			}
		}

		[Category("Appearance")]
		[DefaultValue(true)]
		[Description("If true line numbers are shown in the textarea")]
		public bool ShowLineNumbers
		{
			get
			{
				return this.document.TextEditorProperties.ShowLineNumbers;
			}
			set
			{
				this.document.TextEditorProperties.ShowLineNumbers = value;
				this.OptionsChanged();
			}
		}

		[Category("Appearance")]
		[DefaultValue(true)]
		[Description("If true matching brackets are highlighted")]
		public bool ShowMatchingBracket
		{
			get
			{
				return this.document.TextEditorProperties.ShowMatchingBracket;
			}
			set
			{
				this.document.TextEditorProperties.ShowMatchingBracket = value;
				this.OptionsChanged();
			}
		}

		[Category("Appearance")]
		[DefaultValue(false)]
		[Description("If true spaces are shown in the textarea")]
		public bool ShowSpaces
		{
			get
			{
				return this.document.TextEditorProperties.ShowSpaces;
			}
			set
			{
				this.document.TextEditorProperties.ShowSpaces = value;
				this.OptionsChanged();
			}
		}

		[Category("Appearance")]
		[DefaultValue(false)]
		[Description("If true tabs are shown in the textarea")]
		public bool ShowTabs
		{
			get
			{
				return this.document.TextEditorProperties.ShowTabs;
			}
			set
			{
				this.document.TextEditorProperties.ShowTabs = value;
				this.OptionsChanged();
			}
		}

		[Category("Appearance")]
		[DefaultValue(true)]
		[Description("If true the vertical ruler is shown in the textarea")]
		public bool ShowVRuler
		{
			get
			{
				return this.document.TextEditorProperties.ShowVerticalRuler;
			}
			set
			{
				this.document.TextEditorProperties.ShowVerticalRuler = value;
				this.OptionsChanged();
			}
		}

		[Category("Appearance")]
		[DefaultValue(4)]
		[Description("The width in spaces of a tab character")]
		public int TabIndent
		{
			get
			{
				return this.document.TextEditorProperties.TabIndent;
			}
			set
			{
				this.document.TextEditorProperties.TabIndent = value;
				this.OptionsChanged();
			}
		}

		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
		[EditorBrowsable(EditorBrowsableState.Always)]
		public override string Text
		{
			get
			{
				return this.Document.TextContent;
			}
			set
			{
				this.Document.TextContent = value;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ITextEditorProperties TextEditorProperties
		{
			get
			{
				return this.document.TextEditorProperties;
			}
			set
			{
				this.document.TextEditorProperties = value;
				this.OptionsChanged();
			}
		}

		[Category("Appearance")]
		[DefaultValue(System.Drawing.Text.TextRenderingHint.SystemDefault)]
		[Description("Specifies the quality of text rendering (whether to use hinting and/or anti-aliasing).")]
		public System.Drawing.Text.TextRenderingHint TextRenderingHint
		{
			get
			{
				return this.document.TextEditorProperties.TextRenderingHint;
			}
			set
			{
				this.document.TextEditorProperties.TextRenderingHint = value;
				this.OptionsChanged();
			}
		}

		[Category("Appearance")]
		[DefaultValue(80)]
		[Description("The row in which the vertical ruler is displayed")]
		public int VRulerRow
		{
			get
			{
				return this.document.TextEditorProperties.VerticalRulerRow;
			}
			set
			{
				this.document.TextEditorProperties.VerticalRulerRow = value;
				this.OptionsChanged();
			}
		}

		protected TextEditorControlBase()
		{
			this.GenerateDefaultActions();
			TextEditorControlBase textEditorControlBase = this;
			HighlightingManager.Manager.ReloadSyntaxHighlighting += new EventHandler(textEditorControlBase.OnReloadHighlighting);
		}

		public virtual void BeginUpdate()
		{
			this.updateLevel++;
		}

		public bool CanSaveWithCurrentEncoding()
		{
			if (this.encoding == null || FileReader.IsUnicode(this.encoding))
			{
				return true;
			}
			string textContent = this.document.TextContent;
			return this.encoding.GetString(this.encoding.GetBytes(textContent)) == textContent;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				TextEditorControlBase textEditorControlBase = this;
				HighlightingManager.Manager.ReloadSyntaxHighlighting -= new EventHandler(textEditorControlBase.OnReloadHighlighting);
				this.document.HighlightingStrategy = null;
				this.document.UndoStack.TextEditorControl = null;
			}
			base.Dispose(disposing);
		}

		public virtual void EndUpdate()
		{
			this.updateLevel = Math.Max(0, this.updateLevel - 1);
		}

		private void GenerateDefaultActions()
		{
			this.editactions[Keys.Left] = new CaretLeft();
			this.editactions[Keys.LButton | Keys.MButton | Keys.XButton1 | Keys.Space | Keys.Prior | Keys.PageUp | Keys.Home | Keys.Left | Keys.Shift] = new ShiftCaretLeft();
			this.editactions[Keys.LButton | Keys.MButton | Keys.XButton1 | Keys.Space | Keys.Prior | Keys.PageUp | Keys.Home | Keys.Left | Keys.Control] = new WordLeft();
			this.editactions[Keys.LButton | Keys.MButton | Keys.XButton1 | Keys.Space | Keys.Prior | Keys.PageUp | Keys.Home | Keys.Left | Keys.Shift | Keys.Control] = new ShiftWordLeft();
			this.editactions[Keys.Right] = new CaretRight();
			this.editactions[Keys.LButton | Keys.RButton | Keys.Cancel | Keys.MButton | Keys.XButton1 | Keys.XButton2 | Keys.Space | Keys.Prior | Keys.PageUp | Keys.Next | Keys.PageDown | Keys.End | Keys.Home | Keys.Left | Keys.Up | Keys.Right | Keys.Shift] = new ShiftCaretRight();
			this.editactions[Keys.LButton | Keys.RButton | Keys.Cancel | Keys.MButton | Keys.XButton1 | Keys.XButton2 | Keys.Space | Keys.Prior | Keys.PageUp | Keys.Next | Keys.PageDown | Keys.End | Keys.Home | Keys.Left | Keys.Up | Keys.Right | Keys.Control] = new WordRight();
			this.editactions[Keys.LButton | Keys.RButton | Keys.Cancel | Keys.MButton | Keys.XButton1 | Keys.XButton2 | Keys.Space | Keys.Prior | Keys.PageUp | Keys.Next | Keys.PageDown | Keys.End | Keys.Home | Keys.Left | Keys.Up | Keys.Right | Keys.Shift | Keys.Control] = new ShiftWordRight();
			this.editactions[Keys.Up] = new CaretUp();
			this.editactions[Keys.RButton | Keys.MButton | Keys.XButton2 | Keys.Space | Keys.Next | Keys.PageDown | Keys.Home | Keys.Up | Keys.Shift] = new ShiftCaretUp();
			this.editactions[Keys.RButton | Keys.MButton | Keys.XButton2 | Keys.Space | Keys.Next | Keys.PageDown | Keys.Home | Keys.Up | Keys.Control] = new ScrollLineUp();
			this.editactions[Keys.Down] = new CaretDown();
			this.editactions[Keys.Back | Keys.Space | Keys.Down | Keys.Shift] = new ShiftCaretDown();
			this.editactions[Keys.Back | Keys.Space | Keys.Down | Keys.Control] = new ScrollLineDown();
			this.editactions[Keys.Insert] = new ToggleEditMode();
			this.editactions[Keys.LButton | Keys.MButton | Keys.XButton1 | Keys.Back | Keys.Tab | Keys.Clear | Keys.Return | Keys.Enter | Keys.Space | Keys.Prior | Keys.PageUp | Keys.Home | Keys.Left | Keys.Down | Keys.Select | Keys.Snapshot | Keys.PrintScreen | Keys.Insert | Keys.Control] = new Copy();
			this.editactions[Keys.LButton | Keys.MButton | Keys.XButton1 | Keys.Back | Keys.Tab | Keys.Clear | Keys.Return | Keys.Enter | Keys.Space | Keys.Prior | Keys.PageUp | Keys.Home | Keys.Left | Keys.Down | Keys.Select | Keys.Snapshot | Keys.PrintScreen | Keys.Insert | Keys.Shift] = new Paste();
			this.editactions[Keys.Delete] = new Delete();
			this.editactions[Keys.RButton | Keys.MButton | Keys.XButton2 | Keys.Back | Keys.LineFeed | Keys.Clear | Keys.Space | Keys.Next | Keys.PageDown | Keys.Home | Keys.Up | Keys.Down | Keys.Print | Keys.Snapshot | Keys.PrintScreen | Keys.Delete | Keys.Shift] = new Cut();
			this.editactions[Keys.Home] = new Home();
			this.editactions[Keys.MButton | Keys.Space | Keys.Home | Keys.Shift] = new ShiftHome();
			this.editactions[Keys.MButton | Keys.Space | Keys.Home | Keys.Control] = new MoveToStart();
			this.editactions[Keys.MButton | Keys.Space | Keys.Home | Keys.Shift | Keys.Control] = new ShiftMoveToStart();
			this.editactions[Keys.End] = new End();
			this.editactions[Keys.LButton | Keys.RButton | Keys.Cancel | Keys.Space | Keys.Prior | Keys.PageUp | Keys.Next | Keys.PageDown | Keys.End | Keys.Shift] = new ShiftEnd();
			this.editactions[Keys.LButton | Keys.RButton | Keys.Cancel | Keys.Space | Keys.Prior | Keys.PageUp | Keys.Next | Keys.PageDown | Keys.End | Keys.Control] = new MoveToEnd();
			this.editactions[Keys.LButton | Keys.RButton | Keys.Cancel | Keys.Space | Keys.Prior | Keys.PageUp | Keys.Next | Keys.PageDown | Keys.End | Keys.Shift | Keys.Control] = new ShiftMoveToEnd();
			this.editactions[Keys.Prior] = new MovePageUp();
			this.editactions[Keys.LButton | Keys.Space | Keys.Prior | Keys.PageUp | Keys.Shift] = new ShiftMovePageUp();
			this.editactions[Keys.Next] = new MovePageDown();
			this.editactions[Keys.RButton | Keys.Space | Keys.Next | Keys.PageDown | Keys.Shift] = new ShiftMovePageDown();
			this.editactions[Keys.Return] = new Return();
			this.editactions[Keys.Tab] = new Tab();
			this.editactions[Keys.LButton | Keys.Back | Keys.Tab | Keys.Shift] = new ShiftTab();
			this.editactions[Keys.Back] = new Backspace();
			this.editactions[Keys.Back | Keys.Shift] = new Backspace();
			this.editactions[Keys.Back | Keys.ShiftKey | Keys.FinalMode | Keys.H | Keys.P | Keys.X | Keys.Control] = new Cut();
			this.editactions[Keys.LButton | Keys.RButton | Keys.Cancel | Keys.A | Keys.B | Keys.C | Keys.Control] = new Copy();
			this.editactions[Keys.RButton | Keys.MButton | Keys.XButton2 | Keys.ShiftKey | Keys.Menu | Keys.Capital | Keys.CapsLock | Keys.B | Keys.D | Keys.F | Keys.P | Keys.R | Keys.T | Keys.V | Keys.Control] = new Paste();
			this.editactions[Keys.LButton | Keys.A | Keys.Control] = new SelectWholeDocument();
			this.editactions[Keys.Escape] = new ClearAllSelections();
			this.editactions[Keys.LButton | Keys.RButton | Keys.Cancel | Keys.MButton | Keys.XButton1 | Keys.XButton2 | Keys.Back | Keys.Tab | Keys.LineFeed | Keys.Clear | Keys.Return | Keys.Enter | Keys.Space | Keys.Prior | Keys.PageUp | Keys.Next | Keys.PageDown | Keys.End | Keys.Home | Keys.Left | Keys.Up | Keys.Right | Keys.Down | Keys.Select | Keys.Print | Keys.Execute | Keys.Snapshot | Keys.PrintScreen | Keys.Insert | Keys.Delete | Keys.Help | Keys.A | Keys.B | Keys.C | Keys.D | Keys.E | Keys.F | Keys.G | Keys.H | Keys.I | Keys.J | Keys.K | Keys.L | Keys.M | Keys.N | Keys.O | Keys.NumPad0 | Keys.NumPad1 | Keys.NumPad2 | Keys.NumPad3 | Keys.NumPad4 | Keys.NumPad5 | Keys.NumPad6 | Keys.NumPad7 | Keys.NumPad8 | Keys.NumPad9 | Keys.Multiply | Keys.Add | Keys.Separator | Keys.Subtract | Keys.Decimal | Keys.Divide | Keys.Control] = new ToggleComment();
			this.editactions[Keys.LButton | Keys.RButton | Keys.Cancel | Keys.MButton | Keys.XButton1 | Keys.XButton2 | Keys.Back | Keys.Tab | Keys.LineFeed | Keys.Clear | Keys.Return | Keys.Enter | Keys.ShiftKey | Keys.ControlKey | Keys.Menu | Keys.Pause | Keys.Capital | Keys.CapsLock | Keys.KanaMode | Keys.HanguelMode | Keys.HangulMode | Keys.JunjaMode | Keys.FinalMode | Keys.HanjaMode | Keys.KanjiMode | Keys.Escape | Keys.IMEConvert | Keys.IMENonconvert | Keys.IMEAccept | Keys.IMEAceept | Keys.IMEModeChange | Keys.Space | Keys.Prior | Keys.PageUp | Keys.Next | Keys.PageDown | Keys.End | Keys.Home | Keys.Left | Keys.Up | Keys.Right | Keys.Down | Keys.Select | Keys.Print | Keys.Execute | Keys.Snapshot | Keys.PrintScreen | Keys.Insert | Keys.Delete | Keys.Help | Keys.D0 | Keys.D1 | Keys.D2 | Keys.D3 | Keys.D4 | Keys.D5 | Keys.D6 | Keys.D7 | Keys.D8 | Keys.D9 | Keys.F17 | Keys.F18 | Keys.F19 | Keys.F20 | Keys.F21 | Keys.F22 | Keys.F23 | Keys.F24 | Keys.NumLock | Keys.Scroll | Keys.LShiftKey | Keys.RShiftKey | Keys.LControlKey | Keys.RControlKey | Keys.LMenu | Keys.RMenu | Keys.BrowserBack | Keys.BrowserForward | Keys.BrowserRefresh | Keys.BrowserStop | Keys.BrowserSearch | Keys.BrowserFavorites | Keys.BrowserHome | Keys.VolumeMute | Keys.VolumeDown | Keys.VolumeUp | Keys.MediaNextTrack | Keys.MediaPreviousTrack | Keys.MediaStop | Keys.MediaPlayPause | Keys.LaunchMail | Keys.SelectMedia | Keys.LaunchApplication1 | Keys.LaunchApplication2 | Keys.OemSemicolon | Keys.Oem1 | Keys.Oemplus | Keys.Oemcomma | Keys.OemMinus | Keys.OemPeriod | Keys.OemQuestion | Keys.Oem2 | Keys.Control] = new ToggleComment();
			this.editactions[Keys.Back | Keys.Alt] = new ICSharpCode.TextEditor.Actions.Undo();
			this.editactions[Keys.RButton | Keys.Back | Keys.LineFeed | Keys.ShiftKey | Keys.Menu | Keys.FinalMode | Keys.B | Keys.H | Keys.J | Keys.P | Keys.R | Keys.X | Keys.Z | Keys.Control] = new ICSharpCode.TextEditor.Actions.Undo();
			this.editactions[Keys.LButton | Keys.Back | Keys.Tab | Keys.ShiftKey | Keys.ControlKey | Keys.FinalMode | Keys.HanjaMode | Keys.KanjiMode | Keys.A | Keys.H | Keys.I | Keys.P | Keys.Q | Keys.X | Keys.Y | Keys.Control] = new Redo();
			this.editactions[Keys.RButton | Keys.MButton | Keys.XButton2 | Keys.Back | Keys.LineFeed | Keys.Clear | Keys.Space | Keys.Next | Keys.PageDown | Keys.Home | Keys.Up | Keys.Down | Keys.Print | Keys.Snapshot | Keys.PrintScreen | Keys.Delete | Keys.Control] = new DeleteWord();
			this.editactions[Keys.Back | Keys.Control] = new WordBackspace();
			this.editactions[Keys.MButton | Keys.D | Keys.Control] = new DeleteLine();
			this.editactions[Keys.MButton | Keys.D | Keys.Shift | Keys.Control] = new DeleteToLineEnd();
			this.editactions[Keys.RButton | Keys.B | Keys.Control] = new GotoMatchingBrace();
		}

		internal IEditAction GetEditAction(Keys keyData)
		{
			if (!this.IsEditAction(keyData))
			{
				return null;
			}
			return this.editactions[keyData];
		}

		public virtual string GetRangeDescription(int selectedItem, int itemCount)
		{
			StringBuilder stringBuilder = new StringBuilder(selectedItem.ToString());
			stringBuilder.Append(" from ");
			stringBuilder.Append(itemCount.ToString());
			return stringBuilder.ToString();
		}

		public bool IsEditAction(Keys keyData)
		{
			return this.editactions.ContainsKey(keyData);
		}

		public void LoadFile(string fileName)
		{
			this.LoadFile(fileName, true, true);
		}

		public void LoadFile(string fileName, bool autoLoadHighlighting, bool autodetectEncoding)
		{
			using (FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				this.LoadFile(fileName, fileStream, autoLoadHighlighting, autodetectEncoding);
			}
		}

		public void LoadFile(string fileName, Stream stream, bool autoLoadHighlighting, bool autodetectEncoding)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			this.BeginUpdate();
			this.document.TextContent = string.Empty;
			this.document.UndoStack.ClearAll();
			this.document.BookmarkManager.Clear();
			if (autoLoadHighlighting)
			{
				try
				{
					this.document.HighlightingStrategy = HighlightingStrategyFactory.CreateHighlightingStrategyForFile(fileName);
				}
				catch (HighlightingDefinitionInvalidException highlightingDefinitionInvalidException1)
				{
					HighlightingDefinitionInvalidException highlightingDefinitionInvalidException = highlightingDefinitionInvalidException1;
					MessageBox.Show(highlightingDefinitionInvalidException.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			}
			if (!autodetectEncoding)
			{
				using (StreamReader streamReader = new StreamReader(fileName, this.Encoding))
				{
					this.Document.TextContent = streamReader.ReadToEnd();
				}
			}
			else
			{
				System.Text.Encoding encoding = this.Encoding;
				this.Document.TextContent = FileReader.ReadFileContent(stream, ref encoding);
				this.Encoding = encoding;
			}
			this.FileName = fileName;
			this.Document.UpdateQueue.Clear();
			this.EndUpdate();
			this.OptionsChanged();
			this.Refresh();
		}

		private void OnDocumentChanged(object sender, EventArgs e)
		{
			this.OnTextChanged(e);
		}

		protected virtual void OnFileNameChanged(EventArgs e)
		{
			if (this.FileNameChanged != null)
			{
				this.FileNameChanged(this, e);
			}
		}

		protected virtual void OnReloadHighlighting(object sender, EventArgs e)
		{
			if (this.Document.HighlightingStrategy != null)
			{
				try
				{
					this.Document.HighlightingStrategy = HighlightingStrategyFactory.CreateHighlightingStrategy(this.Document.HighlightingStrategy.Name);
				}
				catch (HighlightingDefinitionInvalidException highlightingDefinitionInvalidException1)
				{
					HighlightingDefinitionInvalidException highlightingDefinitionInvalidException = highlightingDefinitionInvalidException1;
					MessageBox.Show(highlightingDefinitionInvalidException.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
				this.OptionsChanged();
			}
		}

		public abstract void OptionsChanged();

		private static System.Drawing.Font ParseFont(string font)
		{
			char[] chrArray = new char[] { ',', '=' };
			string[] strArrays = font.Split(chrArray);
			return new System.Drawing.Font(strArrays[1], float.Parse(strArrays[3]));
		}

		public override void Refresh()
		{
			if (this.IsInUpdate)
			{
				return;
			}
			base.Refresh();
		}

		public void SaveFile(string fileName)
		{
			using (FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
			{
				this.SaveFile(fileStream);
			}
			this.FileName = fileName;
		}

		public void SaveFile(Stream stream)
		{
			StreamWriter streamWriter = new StreamWriter(stream, this.Encoding ?? System.Text.Encoding.UTF8);
			foreach (LineSegment lineSegmentCollection in this.Document.LineSegmentCollection)
			{
				streamWriter.Write(this.Document.GetText(lineSegmentCollection.Offset, lineSegmentCollection.Length));
				if (lineSegmentCollection.DelimiterLength <= 0)
				{
					continue;
				}
				char charAt = this.Document.GetCharAt(lineSegmentCollection.Offset + lineSegmentCollection.Length);
				if (charAt != '\n' && charAt != '\r')
				{
					throw new InvalidOperationException("The document cannot be saved because it is corrupted.");
				}
				streamWriter.Write(this.document.TextEditorProperties.LineTerminator);
			}
			streamWriter.Flush();
		}

		public event EventHandler FileNameChanged;

		[Browsable(true)]
		[EditorBrowsable(EditorBrowsableState.Always)]
		public event EventHandler TextChanged
		{
			add
			{
				base.TextChanged += value;
			}
			remove
			{
				base.TextChanged -= value;
			}
		}
	}
}