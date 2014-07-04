using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;

namespace OpenGraal.Common.Scripting
{
	public class V8EvaluationInstance
	{
		private static V8ScriptEngine _instance;

		public static V8ScriptEngine GetInstance()
		{
			if (_instance == null)
				_instance = new V8ScriptEngine();
			return _instance;
		}

		private V8EvaluationInstance()
		{
			//
		}
	}
}
