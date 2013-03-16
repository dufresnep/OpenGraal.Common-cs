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
	public class GraalLevel
	{
		#region Member Variables
		/// <summary>
		/// Member Variables
		/// </summary>
		public Dictionary<int, GraalLevelNPC> NpcList = new Dictionary<int, GraalLevelNPC>();
		public FlagManager FlagManager = null;
		public List<GraalPlayer> Players = new List<GraalPlayer>();
		public short[] Tiles = new short[4096];
		public uint ModTime;
		public string Name;
		public object TimerLock = new object();
		public CString base64 = new CString() + "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
		private GraalLevelTile[] tiles = new GraalLevelTile[4096];
		#endregion

		#region	Constructor /  Destructor
		/// <summary>
		/// Constructor
		/// </summary>
		public GraalLevel(String LevelName, object TimerLock)
		{
			this.Name = LevelName;
			this.TimerLock = TimerLock;
			this.FlagManager = new FlagManager(null);
		}

		~GraalLevel()
		{
			this.Clear();
		}
		#endregion

		#region Public functions
		/// <summary>
		/// Clear Level (test)
		/// </summary>
		public void Clear()
		{
			// Reset Mod Time
			this.ModTime = 0;

			// Clear NPCS
			this.Players.Clear();
			lock (this.TimerLock)
			{
				this.NpcList.Clear();
			}
		}

		/// <summary>
		/// Add Player to Level
		/// </summary>
		/// <param name="Player"></param>
		public void AddPlayer(GraalPlayer Player)
		{
			if (!Players.Contains(Player))
			{
				Players.Add(Player);
				this.CallNPCs("onPlayerEnters", new object[] { Player });
			}
		}

		/// <summary>
		/// Delete Player from Level
		/// </summary>
		/// <param name="Player"></param>
		public void DeletePlayer(GraalPlayer Player)
		{
			if (Players.Contains(Player))
			{
				Players.Remove(Player);
				this.CallNPCs("onPlayerLeaves", new object[] { Player });
			}
		}

		/// <summary>
		/// Call NPC Events
		/// </summary>
		public void CallNPCs(String Event, object[] Args)
		{
			foreach (KeyValuePair<int, GraalLevelNPC> e in NpcList)
				e.Value.Call(Event, Args);
		}

		/// <summary>
		/// Delete NPC from List
		/// </summary>
		public bool DeleteNPC(int Id)
		{
			lock (this.TimerLock)
			{
				return NpcList.Remove(Id);
			}
		}

		/// <summary>
		/// Gets a npc from the level with the specified level id.
		/// </summary>
		public GraalLevelNPC GetNPC(int Id)
		{
			GraalLevelNPC npc = null;
			if (!NpcList.TryGetValue(Id, out npc))
			{
				npc = new GraalLevelNPC(this, Id);
				lock (this.TimerLock)
				{
					NpcList[Id] = npc;
				}
			}
			return npc;
		}

		/// <summary>
		/// Set Mod Time
		/// </summary>
		public void SetModTime(uint NewTime)
		{
			this.ModTime = NewTime;
		}

		/// <summary>
		/// Player is on npc
		/// </summary>
		public GraalLevelNPC isOnNPC(int x, int y)
		{
			foreach (KeyValuePair<int, GraalLevelNPC> v in NpcList)
			{
				GraalLevelNPC npc = v.Value;
				if (npc.Image != String.Empty)
				{
					if ((npc.VisFlags & 1) != 0) // && (npc.BlockFlags & 1) == 0
					{
						if (x >= npc.PixelX && x <= npc.PixelX + npc.Width && y >= npc.PixelY && y < npc.PixelY + npc.Height)
							return npc;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Check if X/Y is on a wall
		/// </summary>
		public bool isOnWall(double x, double y)
		{
			if (x < 0 || x >= 64 || y < 0 || y >= 64)
				return true;
			return IsTileWall(Tiles[(int)x + ((int)y) * 64]);
		}

		/// <summary>
		/// Check if x/y is on water
		/// </summary>
		public bool IsOnWater(double x, double y)
		{
			if (x < 0 || x >= 64 || y < 0 || y >= 64)
				return false;
			return IsTileWater(Tiles[(int)x + ((int)y) * 64]);
		}

		/// <summary>
		/// Check if a tile is blocking
		/// </summary>
		public bool IsTileWall(int TileId)
		{
			int TileX = TileId % 16;
			int TileY = TileId / 16;
			return (TileId == 32) || // black tile
				((TileX >= 2 && TileY >= 26 && TileY < 28) || // lift objects
				(TileY >= 30 && TileY < 48) || // chest, movestone, jumpstone, throughthrough
				(TileY >= 84 && TileY < 96) || // lower 12 lines of foreground
				(TileY >= 116 && TileY < 128) || // lower 12 lines of foreground
				(TileY >= 128 && ((TileY / 16) & 1) != 0)); // lower half of normal tiles
		}

		/// <summary>
		/// Check if a tile is water
		/// </summary>
		public bool IsTileWater(int TileId)
		{
			return (TileId >= 64 && TileId < 1152);
		}

		/// <summary>
		/// Trigger Event
		/// </summary>
		public void trigger(double x, double y, string Event, params string[] Params)
		{
			GraalLevelNPC npc = this.isOnNPC((int)(x * 16), (int)(y * 16));
			if (npc != null)
				npc.Call(Event, Params);
		}

		public bool Load(CString pFileName)
		{
			CStringList levelData = new CStringList();
			if (!levelData.Load(pFileName.Text))
				return false;

			if (levelData.Count < 1)
				return false;

			for (int i = 1; i < levelData.Count; i++)
			{
				CStringList words = new CStringList();
				words.Load(levelData.Get(i).Text, ' ');
				if (words.Count <= 0)
					continue;

				if (words.Get(i).Text == "BOARD")
				{
					if (words.Count <= 5)
						continue;

					int x, y, w;
					int.TryParse(words.Get(1).Text, out x);
					int.TryParse(words.Get(2).Text, out y);
					int.TryParse(words.Get(3).Text, out w);
					CString data = words.Get(5);

					if (x >= 0 && x <= 64 && y >= 0 && y <= 64 && w > 0 && x + w <= 64)
					{
						if (data.Length >= w * 2)
						{
							for (int ii = x; ii < x + w; ii++)
							{
								byte left = data.ReadGUByte1();
								byte top = data.ReadGUByte1();
								int tile = base64.IndexOf((char)left) << 6;
								tile += base64.IndexOf((char)top);
								tiles[ii + y*64].SetTile(tile);
							}
						}
					}
				}
			}

			return true;
		}

		public void Render()
		{
			/*
			glBindTexture(GL_TEXTURE_2D, tileset->getTexture());
			glAlphaFunc(GL_GREATER, 0);
			glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
			glColor4f(1.0f, 1.0f, 1.0f, 1.0f);
			glEnable(GL_BLEND);
			glEnable(GL_ALPHA_TEST);
			*/
			/* Render Tiles */
			for (int i = 0; i < 4096; i++)
			{
				tiles[i].Render((i % 64) * 16, (i / 64) * 16);
			}

			/*
			glDisable(GL_ALPHA_TEST);
			glDisable(GL_BLEND);
			*/
		}
		#endregion

		#region	Get-Value Functions
		/// <summary>
		/// Grab Local NPCS
		/// </summary>
		public List<dynamic> npcs
		{
			get
			{
				List<dynamic> obj = new List<dynamic>();
				foreach (KeyValuePair<int, GraalLevelNPC> l in NpcList)
				{
					if (l.Value.ScriptObject != null)
						obj.Add((dynamic)l.Value.ScriptObject);
				}
				return obj;
			}
		}

		/// <summary>
		/// Grab Local Players
		/// </summary>
		public List<GraalPlayer> players
		{
			get { return this.Players; }
		}

		/// <summary>
		/// Script Variable -> flags
		/// </summary>
		public FlagManager flags
		{
			get { return FlagManager; }
		}

		/// <summary>
		/// Script Variable -> name
		/// </summary>
		public string name
		{
			get { return this.Name; }
		}
		#endregion
	}
}
