using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace ICSharpCode.TextEditor.Document
{
	public class BookmarkManagerMemento
	{
		private List<int> bookmarks = new List<int>();

		public List<int> Bookmarks
		{
			get
			{
				return this.bookmarks;
			}
			set
			{
				this.bookmarks = value;
			}
		}

		public BookmarkManagerMemento()
		{
		}

		public BookmarkManagerMemento(XmlElement element)
		{
			foreach (XmlElement childNode in element.ChildNodes)
			{
				this.bookmarks.Add(int.Parse(childNode.Attributes["line"].InnerText));
			}
		}

		public BookmarkManagerMemento(List<int> bookmarks)
		{
			this.bookmarks = bookmarks;
		}

		public void CheckMemento(IDocument document)
		{
			for (int i = 0; i < this.bookmarks.Count; i++)
			{
				int item = this.bookmarks[i];
				if (item < 0 || item >= document.TotalNumberOfLines)
				{
					this.bookmarks.RemoveAt(i);
					i--;
				}
			}
		}

		public object FromXmlElement(XmlElement element)
		{
			return new BookmarkManagerMemento(element);
		}

		public XmlElement ToXmlElement(XmlDocument doc)
		{
			XmlElement xmlElement = doc.CreateElement("Bookmarks");
			foreach (int bookmark in this.bookmarks)
			{
				XmlElement xmlElement1 = doc.CreateElement("Mark");
				XmlAttribute str = doc.CreateAttribute("line");
				str.InnerText = bookmark.ToString();
				xmlElement1.Attributes.Append(str);
				xmlElement.AppendChild(xmlElement1);
			}
			return xmlElement;
		}
	}
}