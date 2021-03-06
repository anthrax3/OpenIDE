using System;
using System.IO;
using System.Collections.Generic;
using OpenIDE.Core.Logging;

namespace OpenIDE.CodeEngine.Core.ReactiveScripts
{
	public enum ScriptTouchEvents
	{
		None,
		Added,
		Changed,
		Removed,
		Pause,
		Resume,
		Restart
	}

	public class ScriptTouchHandler
	{
		private List<string> _rscriptPaths = new List<string>();

		public ScriptTouchHandler(List<string> rscriptPaths) {
			_rscriptPaths = rscriptPaths;
		}

		public ScriptTouchEvents Handle(string message) {
			var result = parseMessage(
				message,
				"'codemodel' 'raw-filesystem-change-filecreated' '",
				ScriptTouchEvents.Added);
			if (result != ScriptTouchEvents.None)
				return result;
			
			result = parseMessage(
				message,
				"'codemodel' 'raw-filesystem-change-filechanged' '",
				ScriptTouchEvents.Changed);
			if (result != ScriptTouchEvents.None)
				return result;
			
			result = parseMessage(
				message,
				"'codemodel' 'raw-filesystem-change-filedeleted' '",
				ScriptTouchEvents.Removed);
			if (result != ScriptTouchEvents.None)
				return result;

			result = parseMessage(
				message,
				"'reactive-script-pause' '",
				ScriptTouchEvents.Pause);
			if (result != ScriptTouchEvents.None)
				return result;

			result = parseMessage(
				message,
				"'reactive-script-resume' '",
				ScriptTouchEvents.Resume);
			if (result != ScriptTouchEvents.None)
				return result;

			result = parseMessage(
				message,
				"'reactive-script-restart' '",
				ScriptTouchEvents.Restart);
			if (result != ScriptTouchEvents.None)
				return result;

			return ScriptTouchEvents.None;
		}

		public string GetPath(string message) {
			var start = message.LastIndexOf("'", message.Length - 2) + 1;
			return message
				.Substring(
					start,
					message.Length - 1 - start);
		}

		private ScriptTouchEvents parseMessage(string message, string addedPattern, ScriptTouchEvents type) {
			foreach (var path in _rscriptPaths) {
				if (message.StartsWith(addedPattern + path)) {
					var scriptRoot = Path.GetDirectoryName(GetPath(message));
					// Only accept scripts in rscript root folder
					if (scriptRoot != path)
						continue;
					return type;
				}
			}
			return ScriptTouchEvents.None;
		}
	}
}
