using System;
using System.Collections.Generic;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.CSharp;
using OpenGraal;
using OpenGraal.Core;
using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;

using System.IO;

namespace OpenGraal.Common.Scripting
{
	public class GameCompiler : IDisposable
	{
		/// <summary>
		/// Compile Thread
		/// </summary>
		public void CompileThread()
		{
			while (CompileList.Count > 0)
			{
				// Grab from Queue
				IRefObject cur;
				lock (CompileList)
					cur = CompileList.Dequeue();

				// Compile
				this.Compile(cur);

				// Add to Run List
				lock (RunList)
					RunList.Enqueue(cur);
			}
		}

		public void RunnerThread()
		{
			if (RunList2.Count > 0)
			{
				while (RunList2.Count > 0)
				{
					// Grab from Queue
					KeyValuePair<IRefObject, KeyValuePair<string, object[]>> cur;
					lock (RunList2)
					{
						try
						{
							cur = RunList2.Dequeue();
						}
						catch (Exception e)
						{
							cur = new KeyValuePair<IRefObject, KeyValuePair<string, object[]>>(null, new KeyValuePair<string,object[]>(null,null));
						}
					}
					if (cur.Key != null)
					{
						if (cur.Key.ScriptObject != null)
						{
							if (cur.Value.Key == "RunEvents")
								cur.Key.ScriptObject.RunEvents();
							else
								cur.Key.Call(cur.Value.Key, cur.Value.Value);
						}
					}

				}
			}
		}
		/// <summary>
		/// Member Variables
		/// </summary>
		protected List<CSocket> _sockets;
		protected Int32[] NextId = new Int32[] { 0, 0, 0 };
		protected Int32 ActiveCompilers = 0;
		protected Int32 ActiveRunners = 0;
		protected Thread[] Compilers, Runners;

		
		protected Queue<IRefObject> CompileList = new Queue<IRefObject>();
		//protected IFramework Server;
		public Queue<IRefObject> RunList = new Queue<IRefObject>();
		public Queue<KeyValuePair<IRefObject, KeyValuePair<string, object[]>>> RunList2 = new Queue<KeyValuePair<IRefObject, KeyValuePair<string, object[]>>>();

		/// <summary>
		/// Constructor -> Create Compiler, pass NPCServer reference
		/// </summary>
		public GameCompiler()
		{
			//this.Server = Server;

			// Two compilers / processor
			Compilers = new Thread[Environment.ProcessorCount * 4];
			Runners = new Thread[Environment.ProcessorCount * 4];
		}

		public void Dispose()
		{
			CompileList.Clear();
			RunList.Clear();
			foreach (Thread t in Compilers)
			{
				if (t != null)
				{
					t.Join();
					t.Abort();
				}
			}

			foreach (Thread t in Runners)
			{
				if (t != null)
				{
					t.Join();
					t.Abort();
				}
			}

			
		}
		/// <summary>
		/// Manage active compilers
		/// </summary>
		public void ManageCompilers()
		{
			// No scripts to compile, or all compilers are running
			if (CompileList.Count == ActiveCompilers || (CompileList.Count > ActiveCompilers && ActiveCompilers == Compilers.Length))
				return;
			
			// Iterate compilers
			for (int i = 0; i < Compilers.Length; i++)
			{
				// Remove Compilers
				if (Compilers[i] != null)
				{
					if (!Compilers[i].IsAlive)
					{
						ActiveCompilers--;
						Compilers[i] = null;
					}
				}

				// No need to create another compiler, continue
				if (CompileList.Count > i && Compilers[i] == null)
				{
					Compilers[i] = new Thread(CompileThread);
					Compilers[i].IsBackground = true;
					Compilers[i].Start();
					ActiveCompilers++;
				}
			}
		}

		public void ManageRunners()
		{
			// No scripts to compile, or all compilers are running
			if (RunList2.Count == ActiveRunners || (RunList2.Count > ActiveRunners && ActiveRunners == Runners.Length))
				return;

			// Iterate compilers
			for (int i = 0; i < Runners.Length; i++)
			{
				// Remove Compilers
				if (Runners[i] != null)
				{
					if (!Runners[i].IsAlive)
					{
						ActiveRunners--;
						Runners[i] = null;
					}
				}

				// No need to create another compiler, continue
				if (RunList2.Count > i && Runners[i] == null)
				{
					Runners[i] = new Thread(RunnerThread);
					Runners[i].IsBackground = true;
					Runners[i].Start();
					ActiveRunners++;
				}
			}
		}

		/// <summary>
		/// Add to Compile List
		/// </summary>
		public void CompileAdd(IRefObject ScriptObj)
		{
			lock (CompileList)
				CompileList.Enqueue(ScriptObj);
		}

		public void RunnerAdd(KeyValuePair<IRefObject,KeyValuePair<string,object[]>> ScriptObj)
		{
			lock (RunList2)
				RunList2.Enqueue(ScriptObj);
		}

		public void Compile(IRefObject ScriptObj)
		{
			String[] Script = ParseJoins(ScriptObj.Script);
			V8Script scrpt = null;
			
			//Serverside script
			try
			{

				if (Script[0].Trim().Length != 0)
				{
					scrpt = V8EvaluationInstance.GetInstance().Compile(Script[0]);
				}


				if (scrpt != null)
				{
					if (ScriptObj.V8ScriptName == null)
						ScriptObj.V8ScriptName = ScriptObj.Type.ToString().ToLower() + NextId[(int)ScriptObj.Type]++;
					// var " + ScriptObj.V8ScriptName + " = 
					ScriptObj.AttachToGlobalScriptInstance = "(function(engine){\nvar self = this; var player;\nvar ArrayProto = Array.prototype, ObjProto = Object.prototype, FuncProto = Function.prototype;\nvar\n    push             = ArrayProto.push,\n    slice            = ArrayProto.slice,\n    concat           = ArrayProto.concat,\n    toString         = ObjProto.toString,\n    hasOwnProperty   = ObjProto.hasOwnProperty;\n      var\n    nativeForEach      = ArrayProto.forEach,\n    nativeMap          = ArrayProto.map,\n    nativeReduce       = ArrayProto.reduce,\n    nativeReduceRight  = ArrayProto.reduceRight,\n    nativeFilter       = ArrayProto.filter,\n    nativeEvery        = ArrayProto.every,\n    nativeSome         = ArrayProto.some,\n    nativeIndexOf      = ArrayProto.indexOf,\n    nativeLastIndexOf  = ArrayProto.lastIndexOf,\n    nativeIsArray      = Array.isArray,\n    nativeKeys         = Object.keys,\n    nativeBind         = FuncProto.bind;\n      var each = this.each = this.forEach = function(obj, iterator, context) {\n    if (obj == null) return obj;\n    if (nativeForEach && obj.forEach === nativeForEach) {\n      obj.forEach(iterator, context);\n    } else if (obj.length === +obj.length) {\n      for (var i = 0, length = obj.length; i < length; i++) {\n        if (iterator.call(context, obj[i], i, obj) === breaker) return;\n      }\n    } else {\n      var keys = _.keys(obj);\n      for (var i = 0, length = keys.length; i < length; i++) {\n        if (iterator.call(context, obj[keys[i]], keys[i], obj) === breaker) return;\n      }\n    }\n    return obj;\n  };\nthis.extend = function(obj) {\n    each(slice.call(arguments, 1), function(source) {\n      if (source) {\n        for (var prop in source) {\n          obj[prop] = source[prop];\n        }\n      }\n    });\n    return obj;\n};\nthis.extend(this, engine);\nthis.extend(this, system); \n\n" + Script[0] + "\nreturn self;\n});";
					ScriptObj.scriptobj = V8Instance.GetInstance().Evaluate(@ScriptObj.AttachToGlobalScriptInstance);
					if (ScriptObj.Type != IRefObject.ScriptType.CLASS)
						ScriptObj.ScriptObject = this.RunScript(ScriptObj, scrpt);

					
				}
			}
			catch (ScriptEngineException e)
			{
				HandleErrors((ScriptObj.Type == IRefObject.ScriptType.WEAPON ? "weapon" : "levelnpc_") + ScriptObj.GetErrorText(), e.ErrorDetails.Replace('\n',' '));
			}
			/*
			//Clientside script
			Assembly Asm2 = CompileScript(ScriptObj, out Results, Script[1], true);
			if (Asm2 == null)
				HandleErrors((ScriptObj.Type == IRefObject.ScriptType.WEAPON ? "weapon" : "levelnpc_") + ScriptObj.GetErrorText(), Results, true);
			Asm2 = null;
			*/ 
			
		}

		public void HandleErrors(String Name, string Results, bool ClientSide = false)
		{
			if (Results != null)
			{
				if (ClientSide)
					this.OutputError("//#CLIENTSIDE:");

				this.OutputError("Script compiler output for " + Name + ":");
				this.OutputError(Results);
			}
			else
			{
				this.OutputError("error: Result is null.");
			}
		}

		public virtual void OutputError(string errorText)
		{
			Console.WriteLine(errorText);
		}

		public String[] ParseJoins(String Script)
		{
			MatchCollection col = Regex.Matches(Script, "join\\(\"(?<class>[A-Za-z0-9]*)\"\\);", RegexOptions.IgnoreCase);
			String NewScript = Regex.Replace(Script, "join\\(\"(?<class>[A-Za-z0-9]*)\"\\);", "", RegexOptions.IgnoreCase);
			NewScript = NewScript.Replace("\0", "");
			String Serverside, Clientside;

			foreach (Match x in col)
			{
				ServerClass Class = this.FindClass(x.Groups["class"].Value);
				if (Class != null)
					NewScript += "\n" + Class.Script;
			}

			int pos = NewScript.IndexOf("//#CLIENTSIDE");
			if (pos >= 0)
			{
				Serverside = NewScript.Substring(0, pos);
				Clientside = NewScript.Substring(pos + 13);
			}
			else
			{
				Serverside = NewScript;
				Clientside = "";
			}

			String[] scripts = new String[2];
			scripts.SetValue(Serverside, 0);
			scripts.SetValue(Clientside, 1);
			return scripts;
		}

		public virtual ServerClass FindClass(string Name)
		{
			throw new NotImplementedException();
		}

		public virtual Dictionary<string,ServerClass> GetClasses()
		{
			return null;
		}

		public ScriptObj RunScript(IRefObject Reference,V8Script script)
		{
			ScriptObj obj = null;
					
			obj = (ScriptObj)InvokeConstruct(Reference);

			//obj.scriptobj = script;
					
			if (obj != null)
				obj.RunEvents();

			return obj;
		}

		public virtual ScriptObj InvokeConstruct(IRefObject Reference)
		{
			ScriptObj obj = null;

			if (Reference.Type == IRefObject.ScriptType.WEAPON)
				obj = (ScriptObj)(new ScriptWeapon(Reference));// (ScriptObj)Reference;//
			else if (Reference.Type == IRefObject.ScriptType.LEVELNPC)
				obj = (ScriptObj)(new ScriptLevelNpc(Reference));

			return obj;
		}
	}
}
