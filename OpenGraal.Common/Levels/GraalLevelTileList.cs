using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenGraal;
using OpenGraal.Core;

namespace OpenGraal.Common.Levels
{
	public class GraalLevelTileList : IEnumerable
	{
		/// <summary>
		/// Tile-List
		/// </summary>
		protected Dictionary<int, GraalLevelTile> TileList = new Dictionary<int, GraalLevelTile>();
		protected Int32 Position = -1;

		/// <summary>
		/// Constructor
		/// </summary>
		public GraalLevelTileList()
		{
		}

		/// <summary>
		/// Iterate Tilelist Manager
		/// </summary>
		public IEnumerator<GraalLevelTile> GetEnumerator()
		{
			foreach (KeyValuePair<int, GraalLevelTile> Tile in TileList)
				yield return Tile.Value;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public Dictionary<int, GraalLevelTile> GetTileList()
		{
			return TileList;
		}

		/// <summary>
		/// Add Tile to Tilelist (or return Tile)
		/// </summary>
		public GraalLevelTile AddTile(int x, int start_y, int width, int tile_index)
		{

			GraalLevelTile pl = FindTile(x, start_y);
			if (pl == null)
			{
				GraalLevelTile Tile = new GraalLevelTile();
				Tile.SetTile(tile_index);
				TileList[x + start_y * width] = Tile;
				return Tile;
			}

			return pl;
		}

		/// <summary>
		/// Delete Tile from Tilelist
		/// </summary>
		public bool DeleteTile(Int16 pId)
		{
			return TileList.Remove(pId);
		}


		/// <summary>
		/// Find Tile by Id/Account
		/// </summary>
		public GraalLevelTile FindTile(int x, int start_y)
		{
			int width = this.get_width();
			foreach (KeyValuePair<int, GraalLevelTile> t in TileList)
				if (t.Key == (x + start_y * width))
					return TileList[x + start_y * width];

			return null;
		}

		public int Count()
		{
			return TileList.Count;
		}

		internal int get_height()
		{
			return 64;
		}

		internal int get_width()
		{
			return 64;
		}
	}
}
