using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenGraal;
using OpenGraal.Core;

namespace OpenGraal.Common.Players
{
	public class GraalPlayerList : IEnumerable
	{
		/// <summary>
		/// Player-List
		/// </summary>
		protected Dictionary<Int16, GraalPlayer> PlayerList = new Dictionary<Int16, GraalPlayer>();
		protected Int32 Position = -1;

		/// <summary>
		/// Constructor
		/// </summary>
		public GraalPlayerList()
		{
		}

		/// <summary>
		/// Iterate Playerlist Manager
		/// </summary>
		public IEnumerator<GraalPlayer> GetEnumerator()
		{
			foreach (KeyValuePair<Int16, GraalPlayer> Player in PlayerList)
				yield return Player.Value;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <summary>
		/// Add Player to Playerlist (or return player)
		/// </summary>
		public GraalPlayer AddPlayer(Int16 Id)
		{
			GraalPlayer pl = FindPlayer(Id);
			if (pl == null)
			{
				GraalPlayer Player = new GraalPlayer(Id);
				PlayerList[Id] = Player;
				return Player;
			}

			return pl;
		}

		/// <summary>
		/// Delete Player from Playerlist
		/// </summary>
		public bool DeletePlayer(Int16 pId)
		{
			return PlayerList.Remove(pId);
		}

		/// <summary>
		/// Find Player by Id
		/// </summary>
		public GraalPlayer FindPlayer(Int16 Id)
		{
			GraalPlayer player;
			if (PlayerList.TryGetValue(Id, out player))
				return player;
			return null;
		}

		/// <summary>
		/// Find Player by Account
		/// </summary>
		public GraalPlayer FindPlayer(String Account)
		{
			GraalPlayer rc = null;
			foreach (KeyValuePair<Int16, GraalPlayer> Player in PlayerList)
			{
				if (Player.Value.account == Account)
				{
					if (Player.Value.level != null)
						return Player.Value;
					else
						rc = Player.Value;
				}
			}

			return rc;
		}

		/// <summary>
		/// Find Player by Id/Account
		/// </summary>
		public GraalPlayer FindPlayer(String pAccount, Int16 pId)
		{
			foreach (KeyValuePair<Int16, GraalPlayer> Player in PlayerList)
			{
				if (Player.Value.account == pAccount && Player.Value.id == pId)
					return Player.Value;
			}

			return null;
		}

		public int count()
		{
			return PlayerList.Count;
		}
	}
}
