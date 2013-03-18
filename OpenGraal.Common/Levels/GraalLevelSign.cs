using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenGraal.Common.Levels
{
	public class GraalLevelSign
	{
		#region Member Variables
		private int _x, _y;
		private string _text;
		#endregion

		#region Constructor / Destructor
		public GraalLevelSign()
		{
		}

		public GraalLevelSign(int x, int y, string text)
		{
			this.X = x;
			this.Y = y;
			this.Text = text;
		}

		~GraalLevelSign()
		{
			//
		}
		#endregion

		#region Get value functions
		public int X
		{
			get
			{
				return this._x;
			}
			set
			{
				this._x = value;
			}
		}

		public int Y
		{
			get
			{
				return this._y;
			}
			set
			{
				this._y = value;
			}
		}

		public string Text
		{
			get
			{
				return this._text;
			}
			set
			{
				this._text = value;
			}
		}
		#endregion

	}
}
