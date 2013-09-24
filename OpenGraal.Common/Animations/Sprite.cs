using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenGraal.Core;
using OpenGraal.Common;

namespace OpenGraal.Common.Animations
{
	public class Sprite
	{
		#region Member Variables
		private int _sprite, _x, _y, _w, _h;
		private CString _type, _file;
		#endregion

		#region Get/set value functions
		public int SpriteId
		{
			get
			{
				return this._sprite;
			}
			set
			{
				this._sprite = value;
			}
		}

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
				return this._w;
			}
			set
			{
				this._w = value;
			}
		}

		public int Height
		{
			get
			{
				return this._h;
			}
			set
			{
				this._h = value;
			}
		}

		public CString Img
		{
			get
			{
				return this._file;
			}
			set
			{
				this._file = value;
			}
		}

		public CString Type
		{
			get
			{
				return this._type;
			}
			set
			{
				this._type = value;
			}
		}

		public CString File
		{
			get
			{
				return this._file;
			}
			set
			{
				this._file = value;
			}
		}
		#endregion

		#region Constructor / Destructor
		public Sprite()
		{

		}

		public Sprite(int pSprite, CString pImage, int pX, int pY, int pW, int pH)
		{
			_sprite = pSprite;
			_file = pImage;
			_x = pX;
			_y = pY;
			_w = pW;
			_h = pH;
		}

		~Sprite()
		{
			if (_file == null)
				return;

			//img.countChange(-1);
		}
		#endregion

		#region Public functions
		public void Render(int pX, int pY)
		{
			if (_file == null)
				return;

			//img.Render(pX, pY, x, y, w, h);
		}
		#endregion
	}

	public class StageSprite
	{
		#region Member Variables
		private int _sprite, _x, _y;
		#endregion

		#region Get/set value functions
		public int SpriteId
		{
			get
			{
				return this._sprite;
			}
			set
			{
				this._sprite = value;
			}
		}

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
		#endregion
	}
}
