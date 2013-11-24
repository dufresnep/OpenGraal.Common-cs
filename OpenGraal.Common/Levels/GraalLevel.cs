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
		public Dictionary<int, GraalLevelSign> SignList = new Dictionary<int, GraalLevelSign>();
		public Dictionary<int, GraalLevelLink> LinkList = new Dictionary<int, GraalLevelLink>();
		public FlagManager FlagManager = null;
		public List<GraalPlayer> Players = new List<GraalPlayer>();
		//public short[] Tiles = new short[4096];
		public uint ModTime;
		public string Name;
		public object TimerLock = new object();
		public CString base64 = new CString() + "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
		public GraalLevelTile[] Tiles = new GraalLevelTile[4096];
		public Dictionary<int, GraalLevelTileList> layers = new Dictionary<int, GraalLevelTileList>();

		public bool isOnMap { get; set; }

		public int MapPositionX { get; set; }

		public int MapPositionY { get; set; }

		#endregion

		enum TileType
		{
			TILE_TYPE_NONBLOCK = 0,
			TILE_TYPE_HURT_UNDERGROUND = 2,
			TILE_TYPE_CHAIR = 3,
			TILE_TYPE_BED_UPPER = 4,
			TILE_TYPE_BED_LOWER = 5,
			TILE_TYPE_SWAMP = 6,
			TILE_TYPE_LAVA_SWAMP = 7,
			TILE_TYPE_NEAR_WATER = 8,
			TILE_TYPE_WATER = 11,
			TILE_TYPE_LAVA = 12,
			TILE_TYPE_THROW_THROUGH = 20,
			TILE_TYPE_JUMP_STONE = 21,
			TILE_TYPE_BLOCKING = 22}
		;

		Dictionary<int,int> TileTypeDefinitions = new Dictionary<int, int>();

		#region	Constructor /  Destructor

		/// <summary>
		/// Constructor
		/// </summary>
		public GraalLevel(String LevelName, object TimerLock)
		{
			this.Name = LevelName;
			this.TimerLock = TimerLock;
			this.FlagManager = new FlagManager(null);
			this.layers[0] = new GraalLevelTileList();

			for (int i = 0; i < 4096; i++)
			{
				this.layers[0].AddTile(i, 632);
			}

			this.LoadTypeDefinitions(new CString() + "tiletypes1.dat");
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
				Player.Level = this;
				this.CallNPCs("onPlayerEnters", new object[] { Player });
				Player.CallNPCs("onPlayerEnters", new object[] { Player });
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
				Player.CallNPCs("onPlayerLeaves", new object[] { Player });
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
		/// Gets a npc from the level with the specified level id.
		/// </summary>
		public GraalLevelNPC GetNPC(CSocket socket, int Id)
		{
			GraalLevelNPC npc = null;
			if (!NpcList.TryGetValue(Id, out npc))
			{
				npc = new GraalLevelNPC(socket, this, Id);
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

			int tileId = this.layers[0].FindTile((int)x + ((int)y) * 64).TileId;

			//Console.WriteLine("Testing tile2 x: " + x.ToString() + " - y: " + y.ToString() + " - TileId: " + tileId.ToString() + " - Type: " + Enum.GetName(typeof(TileType), this.TileTypeDefinitions[tileId]));
			if (this.TileTypeDefinitions[tileId] == (int)TileType.TILE_TYPE_BLOCKING)
				return true;
			else if (this.TileTypeDefinitions[tileId] == (int)TileType.TILE_TYPE_JUMP_STONE)
				return true;
			else if (this.TileTypeDefinitions[tileId] == (int)TileType.TILE_TYPE_THROW_THROUGH)
				return true;
			else
				return false;

		}

		/// <summary>
		/// Check if x/y is on water
		/// </summary>
		public bool IsOnWater(double x, double y)
		{
			if (x < 0 || x >= 64 || y < 0 || y >= 64)
				return false;

			int tileId = this.layers[0].FindTile((int)x + ((int)y) * 64).TileId;

			if (this.TileTypeDefinitions[tileId] == (int)TileType.TILE_TYPE_WATER)
				return true;
			else
				return false;
		}

		/// <summary>
		/// Check if a tile is blocking
		/// </summary>
		public bool IsTileWall(int TileId)
		{


			int TileX = TileId % 16;
			int TileY = TileId / 16;
			Console.WriteLine("TileX: " + TileX.ToString() + " - TileY: " + TileY.ToString() + " - TileId: " + TileId.ToString());
			return (TileId >= 4 && TileId <= 7) || (TileId >= 9 && TileId <= 14) || (TileId >= 20 && TileId <= 23);
			/*
			return (TileId == 32) || // black tile
			((TileX >= 2 && TileY >= 26 && TileY < 28) || // lift objects
			(TileY >= 30 && TileY < 48) || // chest, movestone, jumpstone, throughthrough
			(TileY >= 84 && TileY < 96) || // lower 12 lines of foreground
			(TileY >= 116 && TileY < 128) || // lower 12 lines of foreground
			(TileY >= 128 && ((TileY / 16) & 1) != 0)); // lower half of normal tiles
			*/
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
		/*
		 * Thanks to the Gonstruct source for some help with the Load and Save functions!
		 */
		public bool Load(string pFileName)
		{
			return this.Load(new CString() + pFileName);
		}

		public bool LoadTypeDefinitions(CString pFileName)
		{
			CStringList typeDefs = new CStringList();
			if (!typeDefs.Load(pFileName.Text))
				return false;

			if (typeDefs.Count < 1)
				return false;

			foreach (CString typeDefLine in typeDefs)
			{
				for (var i = 0; i < typeDefLine.Text.Length; i++)
				{
					//Console.WriteLine("TileId: " + i.ToString() + " - TileType: " + Enum.GetName(typeof(TileType), typeDefLine.ReadByte()).ToString());
					this.TileTypeDefinitions.Add(i, typeDefLine.ReadByte());
				}
			}
			return true;
		}

		public bool Load(CString pFileName)
		{
			CStringList levelData = new CStringList();
			if (!levelData.Load(pFileName.Text))
				return false;

			if (levelData.Count < 1)
				return false;

			// Grab file version.
			string fileVersion = levelData.Get(0).ReadChars(8);

			// Determine the level type.
			int v = -1;
			if (fileVersion == "GLEVNW01")
				v = 0;
			else if (fileVersion == "GR-V1.03" || fileVersion == "GR-V1.02" || fileVersion == "GR-V1.01")
				v = 1;
			else if (fileVersion == "Z3-V1.04" || fileVersion == "Z3-V1.03")
				v = 2;

			// Not a level.
			if (v == -1)
				return false;

			// Load the correct level.
			if (v == 0)
				return this.LoadNW(levelData);
			if (v == 1)
				return this.LoadGraal(levelData.Join("\n"), fileVersion);
			if (v == 2)
				return this.LoadZelda(levelData.Join("\n"), fileVersion);
			return false;

		}

		#region LoadGraal

		private bool LoadGraal(CString levelData, string fileVersion)
		{
			return false;
		}

		#endregion

		#region LoadZelda

		private bool LoadZelda(CString levelData, string fileVersion)
		{
			// Get the appropriate filesystem.
			//CFileSystem* fileSystem = server->getFileSystem();
			//if (server->getSettings()->getBool("nofoldersconfig", false) == false)
			//	fileSystem = server->getFileSystem(FS_LEVEL);

			// Path-To-File
			//actualLevelName = levelName = pLevelName;
			//fileName = fileSystem->find(pLevelName);
			//modTime = fileSystem->getModTime(pLevelName);

			// Check if it is actually a .graal level.  The 1.39-1.41r1 client actually
			// saved .zelda as .graal.
			if (fileVersion.Substring(0, 2) == "GR")
				return this.LoadGraal(levelData, fileVersion);

			int v = -1;
			if (fileVersion == "Z3-V1.03")
				v = 3;
			else if (fileVersion == "Z3-V1.04")
				v = 4;
			if (v == -1)
				return false;

			#region Load tiles.
			bool layerExists = false;

			foreach (KeyValuePair<int, GraalLevelTileList> l in this.layers)
				if (l.Key == 0)
					layerExists = true;

			if (!layerExists)
				this.layers[0] = new GraalLevelTileList();
			{
				int bits = (v > 4 ? 13 : 12);
				int read = 0;
				uint buffer = 0;
				long code = 0;
				int[] tiles = new int[2] { -1, -1 };
				int boardIndex = 0;
				int count = 1;
				bool doubleMode = false;

				// Read the tiles.
				while (boardIndex < 64 * 64 && levelData.BytesLeft != 0)
				{
					// Every control code/tile is either 12 or 13 bits.  WTF.
					// Read in the bits.
					while (read < bits)
					{
						buffer += ((uint)levelData.ReadGByte1()) << read;
						read += 8;
					}

					// Pull out a single 12/13 bit code from the buffer.
					code = buffer & (bits == 12 ? 0xFFF : 0x1FFF);
					buffer >>= bits;
					read -= bits;

					Console.WriteLine("Code&bits: " + (code & (bits == 12 ? 0x800 : 0x1000)));
					// See if we have an RLE control code.
					// Control codes determine how the RLE scheme works.
					if ((code & (bits == 12 ? 0x800 : 0x1000)) != 0)
					{
						
						// If the 0x100 bit is set, we are in a double repeat mode.
						// {double 4}56 = 56565656
						if ((code & 0x100) != 0)
							doubleMode = true;

						// How many tiles do we count?
						count = (int)(code & 0xFF);
						continue;
					}

					// If our count is 1, just read in a tile.  This is the default mode.
					if (count == 1)
					{
						GraalLevelTile tile = this.layers[0].AddTile(boardIndex++, (int)code);
						continue;
					}

					// If we reach here, we have an RLE scheme.
					// See if we are in double repeat mode or not.
					if (doubleMode)
					{
						// Read in our first tile.
						if (tiles[0] == -1)
						{
							tiles[0] = (int)code;
							continue;
						}

						// Read in our second tile.
						tiles[1] = (int)code;

						// Add the tiles now.
						for (int i = 0; i < count && boardIndex < 64*64-1; ++i)
						{
							GraalLevelTile tile = this.layers[0].AddTile(boardIndex++, tiles[0]);
							GraalLevelTile tile2 = this.layers[0].AddTile(boardIndex++, tiles[1]);
						}

						// Clean up.
						tiles[0] = tiles[1] = -1;
						doubleMode = false;
						count = 1;
					}
					// Regular RLE scheme.
					else
					{
						GraalLevelTile tile = null;
						for (int i = 0; i < count && boardIndex < 64*64; ++i)
							tile = this.layers[0].AddTile(boardIndex++, (int)code);

						count = 1;
					}
				}
			}
			#endregion

			#region Load the links.
			{
				while (levelData.BytesLeft != 0)
				{
					CString line = levelData.ReadString('\n');
					if (line.Length == 0 || line.Text == "#")
						break;

					// Assemble the level string.

					CStringList vline = new CStringList();
					vline.Load(line.Text, ' ');
					CString level = vline.Get(0);
					if (vline.Count > 7)
					{
						for (int i = 0; i < vline.Count - 7; ++i)
							level += " " + vline.Get(1 + i);
					}

					int offset = vline.Count - 7;

					//GraalLevelLink link = new GraalLevelLink(vline.Get(0).Text, vline.Get(1).ToInt(), vline.Get(2).ToInt(), vline.Get(3).ToInt(), vline.Get(4).ToInt(),vline.Get(5).Text, vline.Get(6).Text);
					
					//this.LinkList.Add(this.LinkList.Count + 1, link);
				}
			}
			#endregion

			#region Load the baddies.
			{
				while (levelData.BytesLeft != 0)
				{
					int x = levelData.ReadGUByte1();
					int y = levelData.ReadGUByte1();
					int type = levelData.ReadGUByte1();

					// Ends with an invalid baddy.
					if (x == -1 && y == -1 && type == -1)
					{
						levelData.ReadString('\n');	// Empty verses.
						break;
					}
					/*
					// Add the baddy.
					TLevelBaddy* baddy = addBaddy((float)x, (float)y, type);
					if (baddy == 0)
						continue;

					// Only v1.04+ baddies have verses.
					if (v > 3)
					{
						// Load the verses.
						std::vector<CString> bverse = fileData.readString("\n").tokenize("\\");
						CString props;
						for (char j = 0; j < (char)bverse.size(); ++j)
							props >> (char)(BDPROP_VERSESIGHT + j) >> (char)bverse[j].length() << bverse[j];
						if (props.length() != 0) baddy->setProps(props);
					}
					*/
				}
			}
			#endregion

			#region Load signs.
			{
				while (levelData.BytesLeft != 0)
				{
					CString line = levelData.ReadString('\n');
					if (line.Length == 0)
						break;

					byte x = line.ReadGUByte1();
					byte y = line.ReadGUByte1();
					CString text = line.ReadString();

					this.SignList.Add(this.SignList.Count + 1, new GraalLevelSign(x, y, text.Text));
				}
			}
			#endregion

			return true;
		}

		#endregion

		#region LoadNW

		private bool LoadNW(CStringList levelData)
		{
			bool isNpcCodeLine = false, isSignCodeLine = false;
			int line = 0;
			foreach (CString lvlDat in levelData)
			{
				line++;

				CStringList words = new CStringList();
				words.Load(lvlDat.Text, ' ');
				if (words.Count <= 0)
					continue;

				string type = words.Get(0).Text;
				if (type != NW_LEVEL_VERSION && line == 1)
				{
					throw new Exception("load_nw_level() failed: Version mismatch (" + type + " != " + NW_LEVEL_VERSION + ")");
				}
				if (type == "BOARD")
				{
					#region BOARD code
					if (words.Count <= 5)
						continue;

					int start_x, start_y, width, layer;
					int.TryParse(words.Get(1).Text, out start_x);
					int.TryParse(words.Get(2).Text, out start_y);
					int.TryParse(words.Get(3).Text, out width);
					int.TryParse(words.Get(4).Text, out layer);
					CString data = words.Get(5);

					bool layerExists = false;

					foreach (KeyValuePair<int, GraalLevelTileList> l in this.layers)
						if (l.Key == layer)
							layerExists = true;

					if (!layerExists)
						this.layers[layer] = new GraalLevelTileList();

					for (int i = 0; i < width * 2; i += 2)
					{
						int tile_index = this.Base64Decode(data.Text.Substring(i, 2));
						int x = start_x + i / 2;

						GraalLevelTile tile = this.layers[layer].AddTile(x, start_y, width, tile_index);
					}
					#endregion
				} else if (type == "LINK")
				{
					#region LINK code
					GraalLevelLink link = new GraalLevelLink();
					int linkX, linkY, width, height;
					link.Destination = words.Get(1).Text;
					int.TryParse(words.Get(2).Text, out linkX);
					int.TryParse(words.Get(3).Text, out linkY);
					link.X = linkX;
					link.Y = linkY;

					int.TryParse(words.Get(4).Text, out width);
					int.TryParse(words.Get(5).Text, out height);
					link.Width = width;
					link.Height = width;

					link.NewX = words.Get(6).Text;
					link.NewY = words.Get(7).Text;

					this.LinkList.Add(this.LinkList.Count + 1, link);
					#endregion
				} else if (type == "SIGN" || isSignCodeLine) // read npcs
				{
					#region SIGN code
					if (isSignCodeLine)
					{
						if (type == "SIGNEND")
						{
							isSignCodeLine = false;
						} else
						{
							GraalLevelSign sign = this.SignList[this.SignList.Count];
							sign.Text += lvlDat.Text;
							sign.Text += "\n";

							this.SignList[this.SignList.Count] = sign;
						}
					} else
					{
						GraalLevelSign sign = new GraalLevelSign();
						int signX, signY;
						int.TryParse(words.Get(1).Text, out signX);
						int.TryParse(words.Get(2).Text, out signY);
						sign.X = signX;
						sign.Y = signY;

						isSignCodeLine = true;

						this.SignList.Add(this.SignList.Count + 1, sign);
					}
					#endregion
				} else if (type == "NPC" || isNpcCodeLine) // read npcs
				{
					#region NPC code
					isNpcCodeLine = this.AddNPC(isNpcCodeLine, lvlDat, words, type);
					#endregion
				}
			}

			return true;
		}

		public virtual bool AddNPC(bool isNpcCodeLine, CString lvlDat, CStringList words, string type)
		{
			if (isNpcCodeLine)
			{
				if (type == "NPCEND")
				{
					isNpcCodeLine = false;
				} else
				{
					GraalLevelNPC npc = this.NpcList[this.NpcList.Count];
					npc.Script += lvlDat.Text;
					npc.Script += "\n";

					this.NpcList[this.NpcList.Count] = npc;
				}
			} else
			{
				GraalLevelNPC npc = new GraalLevelNPC(this, this.NpcList.Count + 1);
				npc.Image = words.Get(1).Text;

				if (npc.Image == "-")
					npc.Image = "";

				float rx, ry;
				float.TryParse(words.Get(2).Text, out rx);
				float.TryParse(words.Get(3).Text, out ry);
				npc.GMapX = (byte)rx;
				npc.GMapY = (byte)ry;
				npc.PixelX = Convert.ToInt32(npc.GMapX * 16.0);
				npc.PixelY = Convert.ToInt32(npc.GMapY * 16.0);
				isNpcCodeLine = true;

				this.NpcList.Add(this.NpcList.Count + 1, npc);
			}
			return isNpcCodeLine;
		}

		#endregion

		public void Save(CString pFileName)
		{
			CString stream = new CString();

			stream += NW_LEVEL_VERSION + "\n";

			// white space separator
			string s = " ";

			#region TILE code
			// write tiles
			for (int layer = 0; layer < this.layers.Count; layer++)
			{
				GraalLevelTileList tiles = this.layers[layer];
				for (int y = 0; y < tiles.get_height(); y++)
				{
					string data = "";
					// chunk start, chunk data pairs
					List<Dictionary<int, string>> chunks = new List<Dictionary<int, string>>();

					/* Separate each row into chunks of actually non-transparent tiles.
					* Every time we encounter a transparent tile, flush the current data
					* into the chunk list and clear it. If we never encounter a transparent
					* tile, flush the entire data after the loop */
					int current_start = 0;
					for (int x = 0; x < tiles.get_width(); x++)
					{
						//Console.WriteLine("Save x-y: " + x + "-" + y);
						GraalLevelTile tile = tiles.FindTile(x, y);

						if (tile.GetIndex() == tile.transparent_index)
						{
							if (data != null)
							{
								Dictionary<int, string> chunk = new Dictionary<int, string>();
								current_start = x;
								data = null;
								chunk.Add(current_start, data);
								chunks.Add(chunk);
							}

							// Skip transparent tile
							current_start++;

							continue;
						}

						data += this.Base64Encode(tile.GetIndex());
					}

					if (data != "")
					{
						Dictionary<int, string> chunk = new Dictionary<int, string>();
						chunk.Add(current_start, data);
						;
						chunks.Add(chunk);
					}

					/* Draw one BOARD entry for each chunk so transparent tile-data is culled */
					foreach (Dictionary<int, string> chunk in chunks)
					{
						stream += "BOARD" + s + 0 + s + y + s + tiles.get_width() + s + layer + s; // x, y, width, layer
						foreach (string td in chunk.Values)
							stream += td;

						stream += "\n";
					}
				}
			}
			#endregion

			#region LINK code
			foreach (KeyValuePair<int,GraalLevelLink> link in this.LinkList)
			{
				stream += "LINK" + s + link.Value.Destination + s + link.Value.X + s + link.Value.Y
				+ s + link.Value.Width + s + link.Value.Height + s + link.Value.NewX
				+ s + link.Value.NewY + "\n";
			}
			#endregion

			#region SIGN code
			foreach (KeyValuePair<int,GraalLevelSign> sign in this.SignList)
			{
				stream += "SIGN" + s + sign.Value.X + s + sign.Value.Y + "\n";
				stream += sign.Value.Text + "\n";
				stream += "SIGNEND" + "\n";
			}
			#endregion

			#region NPC code
			// Output the npc's from the NpcList
			foreach (KeyValuePair<int, GraalLevelNPC> npc in this.NpcList)
			{
				string image = npc.Value.Image;

				// If the npc does not have an image, use "-"
				if (image.Trim() == "")
					image = "-";

				stream += "NPC" + s + image + s + (int)npc.Value.GMapX + s + (int)npc.Value.GMapY + "\n";
				stream += npc.Value.Script + "\n";
				stream += "NPCEND" + "\n";
			}
			#endregion

			System.IO.StreamWriter nwFile = new System.IO.StreamWriter(pFileName.Text);
			nwFile.Write(stream.Text);
			nwFile.Flush();
			nwFile.Close();
		}

		static string NW_LEVEL_VERSION = "GLEVNW01";
		static string BASE64 = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

		public int Base64Decode(string str)
		{
			int num = 0;
			for (int i = 0; i < str.Length; ++i)
			{
				int pos = BASE64.IndexOf(str[i]);

				/*
				if (pos == string.N) {
					throw new Exception("BASE64: invalid format");
				}
				*/
				num += (pos) << (str.Length - i - 1) * 6;
			}
			return num;
		}

		public string Base64Encode(int num, int len = 2)
		{
			string str = "";

			for (int i = 0; i < len; ++i)
			{
				int index = (num >> (len - i - 1) * 6) & 0x3F; // 6 bit per character
				
				str += BASE64[index];
			}

			return str;
		}

		public void Render()
		{
			
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

		public Dictionary<int, GraalLevelTileList> Layers
		{
			get
			{
				return this.layers;
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
