using ICSharpCode.TextEditor.Util;
using System;
using System.Text;

namespace ICSharpCode.TextEditor.Document
{
	public class DocumentFactory
	{
		public DocumentFactory()
		{
		}

		public IDocument CreateDocument()
		{
			DefaultDocument defaultDocument = new DefaultDocument()
			{
				TextBufferStrategy = new GapTextBufferStrategy(),
				FormattingStrategy = new DefaultFormattingStrategy()
			};
            defaultDocument.LineManager = new LineManager(defaultDocument, null);
            defaultDocument.FoldingManager = new FoldingManager(defaultDocument, defaultDocument.LineManager)
            {
                FoldingStrategy = null
            };
            defaultDocument.MarkerStrategy = new MarkerStrategy(defaultDocument);
            defaultDocument.BookmarkManager = new BookmarkManager(defaultDocument, defaultDocument.LineManager);
            return defaultDocument;
		}

		public IDocument CreateFromFile(string fileName)
		{
			IDocument document = this.CreateDocument();
			document.TextContent = FileReader.ReadFileContent(fileName, Encoding.Default);
			return document;
		}

		public IDocument CreateFromTextBuffer(ITextBufferStrategy textBuffer)
		{
			DefaultDocument text = (DefaultDocument)this.CreateDocument();
			text.TextContent = textBuffer.GetText(0, textBuffer.Length);
			text.TextBufferStrategy = textBuffer;
			return text;
		}
	}
}