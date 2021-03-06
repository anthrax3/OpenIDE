using System;
using System.IO;
using System.Linq;
using System.Reflection;
using OpenIDE.Core.Language;
using OpenIDE.Core.RScripts;

namespace OpenIDE.Arguments.Handlers
{
	class EditReactiveScriptHandler : ICommandHandler
	{
		private string _keyPath;
		private Action<string> _dispatch;
		private Func<PluginLocator> _pluginLocator;

		public CommandHandlerParameter Usage {
			get {
					var usage = new CommandHandlerParameter(
						"All",
						CommandType.FileCommand,
						Command,
						"Opens an existing reactive script for editor");
					usage.Add("SCRIPT-NAME", "Reactive script name. Local are picked over global");
					return usage;
			}
		}
	
		public string Command { get { return "edit"; } }
		
		public EditReactiveScriptHandler(Action<string> dispatch, Func<PluginLocator> locator, string keyPath)
		{
			_dispatch = dispatch;
			_pluginLocator = locator;
			_keyPath = keyPath;
		}
	
		public void Execute(string[] arguments)
		{
			if (arguments.Length < 1)
				return;
			var scripts = new ReactiveScriptReader(
				_keyPath,
				_pluginLocator,
				(p, m) => {},
				(m) => {})
				.Read();
			var script = scripts.FirstOrDefault(x => x.Name.Equals(arguments[0]));
			if (script == null)
				return;
			_dispatch(string.Format("command|editor goto \"{0}|0|0\"", script.File));
		}
	}
}
