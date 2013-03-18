using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenGraal.Common.Levels
{
	public class GraalLevelLink
	{
		#region Member Variables
		private int _x, _y, _width, _height;
		private string _destination, _newX, _newY;
		#endregion

		#region Constructor / Destructor
		public GraalLevelLink()
		{
		}

		public GraalLevelLink(string destination, int x, int y, int width, int height, string newX, string newY)
		{
			this.X = x;
			this.Y = y;
			this.Destination = destination;
			this.Width = width;
			this.Height = height;
			this.NewX = newX;
			this.NewY = newY;
		}

		~GraalLevelLink()
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

		public int Width
		{
			get
			{
				return this._width;
			}
			set
			{
				this._width = value;
			}
		}
		public int Height
		{
			get
			{
				return this._height;
			}
			set
			{
				this._height = value;
			}
		}

		public string NewX
		{
			get
			{
				return this._newX;
			}
			set
			{
				this._newX = value;
			}
		}
		public string NewY
		{
			get
			{
				return this._newY;
			}
			set
			{
				this._newY = value;
			}
		}

		public string Destination
		{
			get
			{
				return this._destination;
			}
			set
			{
				this._destination = value;
			}
		}
		#endregion

	}
}
