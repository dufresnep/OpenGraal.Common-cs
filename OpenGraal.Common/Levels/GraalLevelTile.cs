using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenGraal;
using OpenGraal.Core;
using OpenGraal.Common;
using OpenGraal.Common.Players;

namespace OpenGraal.Common.Levels
{
	public class GraalLevelTile
	{
		#region Member Variables
		private int _tileId, _tileType;
		public int transparent_index = -2;
		#endregion

		#region Constructor / Destructor
		public GraalLevelTile()
		{
			this._tileId = 0;
		}

		~GraalLevelTile()
		{
			// Do something
		}
		#endregion

		#region Public functions
		public int GetIndex()
		{
			return this._tileId;
		}
		
		public void SetTile(int pTileId)
		{
			this._tileId = pTileId;
			this._tileType = (this._tileType == 511 ? 1 : 0);
		}
		#endregion

		#region Get value functions
		public int TileId
		{
			get
			{
				return this._tileId;
			}
			set
			{
				this._tileId = value;
			}
		}
		public int TileType
		{
			get
			{
				return this._tileType;
			}
			set
			{
				this._tileType = value;
			}
		}
		#endregion
	}
}
