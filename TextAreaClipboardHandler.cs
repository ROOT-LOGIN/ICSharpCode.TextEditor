using ICSharpCode.TextEditor.Actions;
using ICSharpCode.TextEditor.Document;
using ICSharpCode.TextEditor.Undo;
using ICSharpCode.TextEditor.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ICSharpCode.TextEditor
{
	public class TextAreaClipboardHandler
	{
		private const string LineSelectedType = "MSDEVLineSelect";

		private TextArea textArea;

		public static TextAreaClipboardHandler.ClipboardContainsTextDelegate GetClipboardContainsText;

		[ThreadStatic]
		private static int SafeSetClipboardDataVersion;

		public bool EnableCopy
		{
			get
			{
				return true;
			}
		}

		public bool EnableCut
		{
			get
			{
				return this.textArea.EnableCutOrPaste;
			}
		}

		public bool EnableDelete
		{
			get
			{
				if (!this.textArea.SelectionManager.HasSomethingSelected)
				{
					return false;
				}
				return !this.textArea.SelectionManager.SelectionIsReadonly;
			}
		}

		public bool EnablePaste
		{
			get
			{
				bool flag;
				if (!this.textArea.EnableCutOrPaste)
				{
					return false;
				}
				TextAreaClipboardHandler.ClipboardContainsTextDelegate getClipboardContainsText = TextAreaClipboardHandler.GetClipboardContainsText;
				if (getClipboardContainsText != null)
				{
					return getClipboardContainsText();
				}
				try
				{
					flag = Clipboard.ContainsText();
				}
				catch (ExternalException externalException)
				{
					flag = false;
				}
				return flag;
			}
		}

		public bool EnableSelectAll
		{
			get
			{
				return true;
			}
		}

		public TextAreaClipboardHandler(TextArea textArea)
		{
			this.textArea = textArea;
			textArea.SelectionManager.SelectionChanged += new EventHandler(this.DocumentSelectionChanged);
		}

		public void Copy(object sender, EventArgs e)
		{
			if (!this.CopyTextToClipboard(this.textArea.SelectionManager.SelectedText) && this.textArea.Document.TextEditorProperties.CutCopyWholeLine)
			{
				int lineNumberForOffset = this.textArea.Document.GetLineNumberForOffset(this.textArea.Caret.Offset);
				LineSegment lineSegment = this.textArea.Document.GetLineSegment(lineNumberForOffset);
				string text = this.textArea.Document.GetText(lineSegment.Offset, lineSegment.TotalLength);
				this.CopyTextToClipboard(text, true);
			}
		}

		private bool CopyTextToClipboard(string stringToCopy, bool asLine)
		{
			if (stringToCopy.Length <= 0)
			{
				return false;
			}
			DataObject dataObject = new DataObject();
			dataObject.SetData(DataFormats.UnicodeText, true, stringToCopy);
			if (asLine)
			{
				MemoryStream memoryStream = new MemoryStream(1);
				memoryStream.WriteByte(1);
				dataObject.SetData("MSDEVLineSelect", false, memoryStream);
			}
			if (this.textArea.Document.HighlightingStrategy.Name != "Default")
			{
				dataObject.SetData(DataFormats.Rtf, RtfWriter.GenerateRtf(this.textArea));
			}
			this.OnCopyText(new CopyTextEventArgs(stringToCopy));
			TextAreaClipboardHandler.SafeSetClipboard(dataObject);
			return true;
		}

		private bool CopyTextToClipboard(string stringToCopy)
		{
			return this.CopyTextToClipboard(stringToCopy, false);
		}

		public void Cut(object sender, EventArgs e)
		{
			if (this.textArea.SelectionManager.HasSomethingSelected)
			{
				if (this.CopyTextToClipboard(this.textArea.SelectionManager.SelectedText))
				{
					if (this.textArea.SelectionManager.SelectionIsReadonly)
					{
						return;
					}
					this.textArea.BeginUpdate();
					this.textArea.Caret.Position = this.textArea.SelectionManager.SelectionCollection[0].StartPosition;
					this.textArea.SelectionManager.RemoveSelectedText();
					this.textArea.EndUpdate();
					return;
				}
			}
			else if (this.textArea.Document.TextEditorProperties.CutCopyWholeLine)
			{
				int lineNumberForOffset = this.textArea.Document.GetLineNumberForOffset(this.textArea.Caret.Offset);
				LineSegment lineSegment = this.textArea.Document.GetLineSegment(lineNumberForOffset);
				string text = this.textArea.Document.GetText(lineSegment.Offset, lineSegment.TotalLength);
				this.textArea.SelectionManager.SetSelection(this.textArea.Document.OffsetToPosition(lineSegment.Offset), this.textArea.Document.OffsetToPosition(lineSegment.Offset + lineSegment.TotalLength));
				if (this.CopyTextToClipboard(text, true))
				{
					if (this.textArea.SelectionManager.SelectionIsReadonly)
					{
						return;
					}
					this.textArea.BeginUpdate();
					this.textArea.Caret.Position = this.textArea.Document.OffsetToPosition(lineSegment.Offset);
					this.textArea.SelectionManager.RemoveSelectedText();
					this.textArea.Document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.PositionToEnd, new TextLocation(0, lineNumberForOffset)));
					this.textArea.EndUpdate();
				}
			}
		}

		public void Delete(object sender, EventArgs e)
		{
			(new Delete()).Execute(this.textArea);
		}

		private void DocumentSelectionChanged(object sender, EventArgs e)
		{
		}

		protected virtual void OnCopyText(CopyTextEventArgs e)
		{
			if (this.CopyText != null)
			{
				this.CopyText(this, e);
			}
		}

		public void Paste(object sender, EventArgs e)
		{
			if (!this.textArea.EnableCutOrPaste)
			{
				return;
			}
			int num = 0;
			while (true)
			{
				try
				{
					IDataObject dataObject = Clipboard.GetDataObject();
					if (dataObject != null)
					{
						bool dataPresent = dataObject.GetDataPresent("MSDEVLineSelect");
						if (dataObject.GetDataPresent(DataFormats.UnicodeText))
						{
							string data = (string)dataObject.GetData(DataFormats.UnicodeText);
							if (!string.IsNullOrEmpty(data))
							{
								this.textArea.Document.UndoStack.StartUndoGroup();
								try
								{
									if (this.textArea.SelectionManager.HasSomethingSelected)
									{
										this.textArea.Caret.Position = this.textArea.SelectionManager.SelectionCollection[0].StartPosition;
										this.textArea.SelectionManager.RemoveSelectedText();
									}
									if (!dataPresent)
									{
										this.textArea.InsertString(data);
									}
									else
									{
										int column = this.textArea.Caret.Column;
										this.textArea.Caret.Column = 0;
										if (!this.textArea.IsReadOnly(this.textArea.Caret.Offset))
										{
											this.textArea.InsertString(data);
										}
										this.textArea.Caret.Column = column;
									}
								}
								finally
								{
									this.textArea.Document.UndoStack.EndUndoGroup();
								}
							}
						}
						break;
					}
					else
					{
						break;
					}
				}
				catch (ExternalException externalException)
				{
					if (num > 5)
					{
						throw;
					}
				}
				num++;
			}
		}

		private static void SafeSetClipboard(object dataObject)
		{
			int safeSetClipboardDataVersion = TextAreaClipboardHandler.SafeSetClipboardDataVersion + 1;
			TextAreaClipboardHandler.SafeSetClipboardDataVersion = safeSetClipboardDataVersion;
			int num = safeSetClipboardDataVersion;
			try
			{
				Clipboard.SetDataObject(dataObject, true);
			}
			catch (ExternalException externalException1)
			{
				Timer timer = new Timer()
				{
					Interval = 100
				};
				timer.Tick += new EventHandler((object argument0, EventArgs argument1) => {
					timer.Stop();
					timer.Dispose();
					if (TextAreaClipboardHandler.SafeSetClipboardDataVersion == num)
					{
						try
						{
							Clipboard.SetDataObject(dataObject, true, 10, 50);
						}
						catch (ExternalException externalException)
						{
						}
					}
				});
				timer.Start();
			}
		}

		public void SelectAll(object sender, EventArgs e)
		{
			(new SelectWholeDocument()).Execute(this.textArea);
		}

		public event CopyTextEventHandler CopyText;

		public delegate bool ClipboardContainsTextDelegate();
	}
}