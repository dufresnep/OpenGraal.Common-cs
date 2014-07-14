using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;

namespace OpenGraal.Common.Scripting
{
	public static class V8Instance
	{
		private static V8ScriptEngine _instance;
		//private string script = "";
		public static V8ScriptEngine GetInstance()
		{
			if (_instance == null)
			{
				_instance = new V8ScriptEngine();
				_instance.AddHostType("Console", typeof(Console));
				_instance.AddHostType("Animation", typeof(OpenGraal.Common.Animations.Animation));
			}

			return _instance;
		}
		/*
		private V8Instance()
		{
			//
		}
		*/
		public static object InvokeFunction(ScriptEngine engine, object script, string name, object[] args)
		{
			var host = new HostFunctions();
			((IScriptableObject)host).OnExposedToScriptCode(engine);
			int argsLength;

			if (args != null)
				argsLength = args.Length;
			else
				argsLength = 0;

			var del = (Delegate)host.func<object>(argsLength, ((dynamic)script)[name]);
			return del.DynamicInvoke(args);
		}

		public static object InvokeFunction(ScriptEngine engine, object script, string name, object args)
		{
			var host = new HostFunctions();
			((IScriptableObject)host).OnExposedToScriptCode(engine);
			int argsLength = 0;

			if (args != null)
				argsLength = 1;


			var del = (Delegate)host.func<object>(argsLength, ((dynamic)script)[name]);
			return del.DynamicInvoke(args);
		}

		public static dynamic hasMethod = V8Instance.GetInstance().Evaluate(@"
			(function (obj, name, paramCount) {
				return (typeof obj[name] === 'function') && (obj[name].length === paramCount);
			})
		");
		public static dynamic forEachProp = V8Instance.GetInstance().Evaluate(@"
			(function (obj, callback)
			{
				for (var propName in obj)
				{
					if (obj[propName] !== undefined)
						callback(propName + ' ' + obj[propName].length);
				}
			})
		");
		public static dynamic execMethod = V8Instance.GetInstance().Evaluate(@"
			(function (obj, name, paramCount) {
				return (typeof obj[name] === 'function') && (obj[name].length === paramCount);
			})
		");
	}
}
