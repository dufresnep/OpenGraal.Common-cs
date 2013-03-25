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
		private CString _img;
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
				return this._img;
			}
			set
			{
				this._img = value;
			}
		}
		#endregion

		#region Constructor / Destructor
		public Sprite(int pSprite, CString pImage, int pX, int pY, int pW, int pH)
		{
			_sprite = pSprite;
			_img = pImage;
			_x = pX;
			_y = pY;
			_w = pW;
			_h = pH;
		}

		~Sprite()
		{
			if (_img == null)
				return;

			//img.countChange(-1);
		}
		#endregion

		#region Public functions
		public void Render(int pX, int pY)
		{
			if (_img == null)
				return;

			//img.Render(pX, pY, x, y, w, h);
		}
		#endregion
	}
}
