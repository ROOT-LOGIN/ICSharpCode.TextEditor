using ICSharpCode.TextEditor;
using System;

namespace ICSharpCode.TextEditor.Gui.CompletionWindow
{
	public class DefaultCompletionData : ICompletionData
	{
		private string text;

		private string description;

		private int imageIndex;

		private double priority;

		public virtual string Description
		{
			get
			{
				return this.description;
			}
		}

		public int ImageIndex
		{
			get
			{
				return this.imageIndex;
			}
		}

		public double Priority
		{
			get
			{
				return JustDecompileGenerated_get_Priority();
			}
			set
			{
				JustDecompileGenerated_set_Priority(value);
			}
		}

		public double JustDecompileGenerated_get_Priority()
		{
			return this.priority;
		}

		public void JustDecompileGenerated_set_Priority(double value)
		{
			this.priority = value;
		}

		public string Text
		{
			get
			{
				return this.text;
			}
			set
			{
				this.text = value;
			}
		}

		public DefaultCompletionData(string text, int imageIndex)
		{
			this.text = text;
			this.imageIndex = imageIndex;
		}

		public DefaultCompletionData(string text, string description, int imageIndex)
		{
			this.text = text;
			this.description = description;
			this.imageIndex = imageIndex;
		}

		public static int Compare(ICompletionData a, ICompletionData b)
		{
			if (a == null)
			{
				throw new ArgumentNullException("a");
			}
			if (b == null)
			{
				throw new ArgumentNullException("b");
			}
			return string.Compare(a.Text, b.Text, StringComparison.InvariantCultureIgnoreCase);
		}

		public virtual bool InsertAction(TextArea textArea, char ch)
		{
			textArea.InsertString(this.text);
			return false;
		}
	}
}