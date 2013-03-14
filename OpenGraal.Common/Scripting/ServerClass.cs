using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenGraal;
using OpenGraal.Core;
using OpenGraal.Common;
using OpenGraal.Common.Players;

namespace OpenGraal.Common.Scripting
{
	public class ServerClass
	{
		/// <summary>
		/// Member Variables
		/// </summary>
		public String Name, Script;

		/// <summary>
		/// Constructor
		/// </summary>
		public ServerClass(String ClassName, String ClassScript)
		{
			this.UpdateClass(ClassName, ClassScript);
		}

		/// <summary>
		/// Update Weapon Properties
		/// </summary>
		public void UpdateClass(String ClassName, String ClassScript)
		{
			this.Name = ClassName;
			this.Script = ClassScript;
		}
	}
}
