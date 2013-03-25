using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenGraal.Common.Animations
{
	public class Frame
	{
		#region Member Variables
		private Sprite _sprite;
		private int _x, _y;
		#endregion

		#region Get/set value functions
		public Sprite Sprite
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

		#region Constructor
		public Frame(Sprite pSprite, int pX, int pY)
		{
			_sprite = pSprite;
			_x = pX;
			_y = pY;
		}
		#endregion

		#region Public functions
		public virtual void Render(int pX, int pY)
		{
			if (_sprite == null)
				return;

			//img.Render(pX + x, pY + y);
		}
		#endregion
	}
}
