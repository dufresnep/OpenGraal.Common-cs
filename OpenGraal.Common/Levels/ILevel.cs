using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenGraal.Core;
using OpenGraal.Common;
using OpenGraal.Common.Players;
using OpenGraal.Common.Levels;

namespace OpenGraal.Common.Interfaces
{
	public abstract class ILevel
	{

		public ILevel(string name)
		{
			this.Name = name;
		}

		public string Name;
		public uint ModTime;
		public ConcurrentDictionary<int, GraalPlayer> Players = new ConcurrentDictionary<int, GraalPlayer>();
		public ConcurrentDictionary<int, GraalLevelNPC> NpcList = new ConcurrentDictionary<int, GraalLevelNPC>();

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

		public void CallNPCs(String Event, object[] Args)
		{
			foreach (KeyValuePair<int, GraalLevelNPC> e in NpcList)
				e.Value.Call(Event, Args);
		}

		public void Clear()
		{
			// Reset Mod Time
			this.ModTime = 0;

			// Clear NPCS
			this.Players.Clear();

			{
				this.NpcList.Clear();
			}

			this.ClearAll();
		}

		public virtual void ClearAll()
		{
		}

		public void AddPlayer(GraalPlayer Player)
		{
			if (!Players.ContainsKey(Player.Id) && Player != null)
			{
				Players.GetOrAdd(Player.Id, Player);
				try
				{
					Player.Level = this;
					this.CallNPCs("onPlayerEnters", new object[] { Player });
					Player.CallNPCs("onPlayerEnters", new object[] { Player });
				}
				catch (System.NullReferenceException e)
				{
					Console.WriteLine("error: " + e.Message);
				}

			}
		}

		public virtual GraalPlayer FindPlayer(Int16 Id)
		{
			foreach (KeyValuePair<int, GraalPlayer> Player in Players)
			{
				if (Player.Value.Id == Id)
					return Player.Value;
			}
			return null;
		}

		public virtual GraalPlayer FindPlayer(String Account)
		{
			GraalPlayer rc = null;
			foreach (KeyValuePair<int, GraalPlayer> Player in Players)
			{
				if (Player.Value.Account == Account)
				{
					return Player.Value;
				}
			}

			return rc;
		}

		public virtual GraalPlayer FindPlayer(String pAccount, Int16 pId)
		{
			foreach (KeyValuePair<int, GraalPlayer> Player in Players)
			{
				if (Player.Value.Account == pAccount && Player.Value.Id == pId)
					return Player.Value;
			}

			return null;
		}

		public void DeletePlayer(GraalPlayer Player)
		{
			if (Players.ContainsKey(Player.Id))
			{
				Player.CallNPCs("onPlayerLeaves", new object[] { Player });
				Players.TryRemove(Player.Id, out Player);

				this.CallNPCs("onPlayerLeaves", new object[] { Player });
			}
		}

		public bool DeleteNPC(int Id)
		{
			//lock (this.TimerLock)
			GraalLevelNPC val;
			{
				return NpcList.TryRemove(Id, out val);
			}
		}

		public GraalLevelNPC GetNPC(int Id)
		{
			GraalLevelNPC npc = null;
			if (!NpcList.TryGetValue(Id, out npc))
			{
				npc = new GraalLevelNPC(this, Id);
				//lock (this.TimerLock)
				{
					NpcList[Id] = npc;
				}
			}
			return npc;
		}

		public GraalLevelNPC GetNPC(CSocket socket, int Id)
		{
			GraalLevelNPC npc = null;
			if (!NpcList.TryGetValue(Id, out npc))
			{
				npc = new GraalLevelNPC(socket, this, Id);
				//lock (this.TimerLock)
				{
					NpcList[Id] = npc;
				}
			}
			return npc;
		}

		public virtual void Update(double TotalSeconds)
		{
		}

		public virtual void Draw(double TotalSeconds, int x, int y)
		{
		}

		public virtual bool	isOnMap
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public virtual int MapPositionX
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		public virtual bool Load(string pFileName)
		{
			return false;
		}

		public virtual bool Load(CString pFileName)
		{
			return false;
		}

		public virtual int MapPositionY
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		public void SetModTime(uint NewTime)
		{
			this.ModTime = NewTime;
		}

		public virtual Dictionary<int, GraalLevelTileList> Layers
		{
			get
			{
				return null;// this.layers;
			}
		}
	}
}
