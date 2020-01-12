using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml;

namespace ICSharpCode.TextEditor.Document
{
	public class HighlightingManager
	{
		private ArrayList syntaxModeFileProviders = new ArrayList();

		private static HighlightingManager highlightingManager;

		private Hashtable highlightingDefs = new Hashtable();

		private Hashtable extensionsToName = new Hashtable();

		public DefaultHighlightingStrategy DefaultHighlighting
		{
			get
			{
				return (DefaultHighlightingStrategy)this.highlightingDefs["Default"];
			}
		}

		public Hashtable HighlightingDefinitions
		{
			get
			{
				return this.highlightingDefs;
			}
		}

		public static HighlightingManager Manager
		{
			get
			{
				return HighlightingManager.highlightingManager;
			}
		}

		static HighlightingManager()
		{
			HighlightingManager.highlightingManager = new HighlightingManager();
			HighlightingManager.highlightingManager.AddSyntaxModeFileProvider(new ResourceSyntaxModeProvider());
		}

		public HighlightingManager()
		{
			this.CreateDefaultHighlightingStrategy();
		}

		public void AddHighlightingStrategy(IHighlightingStrategy highlightingStrategy)
		{
			this.highlightingDefs[highlightingStrategy.Name] = highlightingStrategy;
			string[] extensions = highlightingStrategy.Extensions;
			for (int i = 0; i < (int)extensions.Length; i++)
			{
				string name = extensions[i];
				this.extensionsToName[name.ToUpperInvariant()] = highlightingStrategy.Name;
			}
		}

		public void AddSyntaxModeFileProvider(ISyntaxModeFileProvider syntaxModeFileProvider)
		{
			foreach (SyntaxMode syntaxMode in syntaxModeFileProvider.SyntaxModes)
			{
				this.highlightingDefs[syntaxMode.Name] = new DictionaryEntry(syntaxMode, syntaxModeFileProvider);
				string[] extensions = syntaxMode.Extensions;
				for (int i = 0; i < (int)extensions.Length; i++)
				{
					string name = extensions[i];
					this.extensionsToName[name.ToUpperInvariant()] = syntaxMode.Name;
				}
			}
			if (!this.syntaxModeFileProviders.Contains(syntaxModeFileProvider))
			{
				this.syntaxModeFileProviders.Add(syntaxModeFileProvider);
			}
		}

		private void CreateDefaultHighlightingStrategy()
		{
			DefaultHighlightingStrategy defaultHighlightingStrategy = new DefaultHighlightingStrategy()
			{
				Extensions = new string[0]
			};
			defaultHighlightingStrategy.Rules.Add(new HighlightRuleSet());
			this.highlightingDefs["Default"] = defaultHighlightingStrategy;
		}

		public IHighlightingStrategy FindHighlighter(string name)
		{
			object item = this.highlightingDefs[name];
			if (item is DictionaryEntry)
			{
				return this.LoadDefinition((DictionaryEntry)item);
			}
			if (item != null)
			{
				return (IHighlightingStrategy)item;
			}
			return this.DefaultHighlighting;
		}

		internal KeyValuePair<SyntaxMode, ISyntaxModeFileProvider> FindHighlighterEntry(string name)
		{
			KeyValuePair<SyntaxMode, ISyntaxModeFileProvider> keyValuePair;
			IEnumerator enumerator = this.syntaxModeFileProviders.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					ISyntaxModeFileProvider current = (ISyntaxModeFileProvider)enumerator.Current;
					using (IEnumerator<SyntaxMode> enumerator1 = current.SyntaxModes.GetEnumerator())
					{
						while (enumerator1.MoveNext())
						{
							SyntaxMode syntaxMode = enumerator1.Current;
							if (syntaxMode.Name != name)
							{
								continue;
							}
							keyValuePair = new KeyValuePair<SyntaxMode, ISyntaxModeFileProvider>(syntaxMode, current);
							return keyValuePair;
						}
					}
				}
				return new KeyValuePair<SyntaxMode, ISyntaxModeFileProvider>(null, null);
			}
			finally
			{
				IDisposable disposable = enumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
			return keyValuePair;
		}

		public IHighlightingStrategy FindHighlighterForFile(string fileName)
		{
			string item = (string)this.extensionsToName[Path.GetExtension(fileName).ToUpperInvariant()];
			if (item == null)
			{
				return this.DefaultHighlighting;
			}
			object obj = this.highlightingDefs[item];
			if (obj is DictionaryEntry)
			{
				return this.LoadDefinition((DictionaryEntry)obj);
			}
			if (obj != null)
			{
				return (IHighlightingStrategy)obj;
			}
			return this.DefaultHighlighting;
		}

		private IHighlightingStrategy LoadDefinition(DictionaryEntry entry)
		{
			SyntaxMode key = (SyntaxMode)entry.Key;
			ISyntaxModeFileProvider value = (ISyntaxModeFileProvider)entry.Value;
			DefaultHighlightingStrategy defaultHighlighting = null;
			try
			{
				XmlTextReader syntaxModeFile = value.GetSyntaxModeFile(key);
				if (syntaxModeFile == null)
				{
					throw new HighlightingDefinitionInvalidException(string.Concat("Could not get syntax mode file for ", key.Name));
				}
				defaultHighlighting = HighlightingDefinitionParser.Parse(key, syntaxModeFile);
				if (defaultHighlighting.Name != key.Name)
				{
					string[] name = new string[] { "The name specified in the .xshd '", defaultHighlighting.Name, "' must be equal the syntax mode name '", key.Name, "'" };
					throw new HighlightingDefinitionInvalidException(string.Concat(name));
				}
			}
			finally
			{
				if (defaultHighlighting == null)
				{
					defaultHighlighting = this.DefaultHighlighting;
				}
				this.highlightingDefs[key.Name] = defaultHighlighting;
				defaultHighlighting.ResolveReferences();
			}
			return defaultHighlighting;
		}

		protected virtual void OnReloadSyntaxHighlighting(EventArgs e)
		{
			if (this.ReloadSyntaxHighlighting != null)
			{
				this.ReloadSyntaxHighlighting(this, e);
			}
		}

		public void ReloadSyntaxModes()
		{
			this.highlightingDefs.Clear();
			this.extensionsToName.Clear();
			this.CreateDefaultHighlightingStrategy();
			foreach (ISyntaxModeFileProvider syntaxModeFileProvider in this.syntaxModeFileProviders)
			{
				syntaxModeFileProvider.UpdateSyntaxModeList();
				this.AddSyntaxModeFileProvider(syntaxModeFileProvider);
			}
			this.OnReloadSyntaxHighlighting(EventArgs.Empty);
		}

		public event EventHandler ReloadSyntaxHighlighting;
	}
}