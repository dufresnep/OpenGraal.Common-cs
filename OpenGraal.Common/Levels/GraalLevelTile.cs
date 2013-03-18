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
		private int TileId, TileType;
		public int transparent_index = -2;
		#endregion

		#region Constructor / Destructor
		public GraalLevelTile()
		{
			this.TileId = 0;
		}

		~GraalLevelTile()
		{
			// Do something
		}
		#endregion

		#region Public functions
		public void Render(int pX, int pY)
		{
			if (this.TileId < 0 || this.TileId > 4096)
				return;

			int Tilex, Tiley;
			float TileTexx, TileTexy, sixteenpixx, sixteenpixy;
			Tilex = ((TileId / 512) * 16 + TileId % 16) * 16;
			Tiley = ((TileId % 512) / 16) * 16;
			TileTexx = (float)Tilex / 2048.0f;
			TileTexy = (float)Tiley / 512.0f;
			sixteenpixx = 16.0f/2048.0f;
			sixteenpixy = 16.0f/512.0f;
			/*
			glBegin(GL_TRIANGLE_STRIP);
				glTexCoord2f(TileTexx, TileTexy);
				glVertex2f(pX, pY);

				glTexCoord2f(TileTexx + sixteenpixx, TileTexy);
				glVertex2f(pX + 16, pY);

				glTexCoord2f(TileTexx, TileTexy + sixteenpixy);
				glVertex2f(pX, pY + 16);

				glTexCoord2f(TileTexx + sixteenpixx, TileTexy + sixteenpixy);
				glVertex2f(pX + 16, pY + 16);
			glEnd();
			*/
		}
		
		public int GetIndex()
		{
			return this.TileId;
		}
		
		public void SetTile(int pTileId)
		{
			this.TileId = pTileId;
			this.TileType = (this.TileType == 511 ? 1 : 0);
		}
		#endregion
	}
}
