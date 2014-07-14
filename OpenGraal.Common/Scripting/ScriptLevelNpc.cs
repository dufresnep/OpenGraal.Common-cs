using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenGraal;
using OpenGraal.Core;
using OpenGraal.Common;
using OpenGraal.Common.Players;
using OpenGraal.Common.Levels;
using OpenGraal.Common.Scripting;

namespace OpenGraal.Common.Scripting
{
	/// <summary>
	/// Class: ScriptLevelNpc Object
	/// </summary>
	public class ScriptLevelNpc : ScriptObj
	{
		// -- Member Variables -- //
		public readonly GraalLevelNPC Ref;
		public readonly bool isweapon = true;

		/// <summary>
		/// NPC -> Visible
		/// </summary>
		public bool visible
		{
			get { return ((Ref.VisFlags & 1) != 0); }
		}

		/// <summary>
		/// NPC -> X
		/// </summary>
		public double x
		{
			get { return Convert.ToDouble(Ref.PixelX / 16.0); }
			set
			{
				Ref.PixelX = Convert.ToInt32(value * 16.0);
				Ref.SendProp(Common.Interfaces.NpcProperties.PIXELX);
			}
		}

		/// <summary>
		/// NPC -> Y
		/// </summary>
		public double y
		{
			get { return Ref.PixelY / 16; }
			set
			{
				Ref.PixelY = Convert.ToInt32(value * 16.0);
				Ref.SendProp(Common.Interfaces.NpcProperties.PIXELY);
			}
		}

		/// <summary>
		/// Name -> Read Only
		/// </summary>
		public int id
		{
			get { return Ref.Id; }
		}

		/// <summary>
		/// Image -> Read Only
		/// </summary>
		public string image
		{
			get { return Ref.Image; }
		}

		/// <summary>
		/// Level -> Read Only
		/// </summary>
		public Common.Interfaces.ILevel level
		{
			get { return Ref.Level; }
		}

		/// <summary>
		/// Save[0] - Save[9]
		/// </summary>
		public SaveIndex save
		{
			get { return Ref.Save; }
		}

		/// <summary>
		/// Block Again
		/// </summary>
		public void blockagain()
		{
			Ref.BlockFlags = 0;
			Ref.SendProp(Common.Interfaces.NpcProperties.BLOCKFLAGS);
		}

		/// <summary>
		/// Dont Block
		/// </summary>
		public void dontblock()
		{
			Ref.BlockFlags = 1;
			Ref.SendProp(Common.Interfaces.NpcProperties.BLOCKFLAGS);
		}

		/// <summary>
		/// Draw Under Player
		/// </summary>
		public void drawoverplayer()
		{
			Ref.VisFlags |= 2;
			Ref.SendProp(Common.Interfaces.NpcProperties.VISFLAGS);
		}

		/// <summary>
		/// Draw Under Player
		/// </summary>
		public void drawunderplayer()
		{
			Ref.VisFlags &= ~2;
			Ref.VisFlags |= 4;
			Ref.SendProp(Common.Interfaces.NpcProperties.VISFLAGS);
		}

		/// <summary>
		/// Message / NPC Chat
		/// </summary>
		public void message(string msg)
		{
			Ref.Chat = msg;
			Ref.SendProp(Common.Interfaces.NpcProperties.MESSAGE);
		}

		/// <summary>
		/// Attach Player to Object
		/// </summary>
		public void attachplayer(GraalPlayer player)
		{
			if (player != null && Ref.npcserver)
				this.SendGSPacket(new CString() + (byte)OpenGraal.Common.Levels.GraalLevelNPC.PacketOut.NCQUERY + (byte)OpenGraal.Common.Levels.GraalLevelNPC.NCREQ.PLSETPROPS + (short)player.Id + (byte)GraalPlayer.Properties.PLATTACHNPC + (byte)0 + (int)this.id);
		}

		/// <summary>
		/// Attach Player to Object
		/// </summary>
		public void detachplayer(GraalPlayer player)
		{
			if (player != null && Ref.npcserver)
				this.SendGSPacket(new CString() + (byte)OpenGraal.Common.Levels.GraalLevelNPC.PacketOut.NCQUERY + (byte)OpenGraal.Common.Levels.GraalLevelNPC.NCREQ.PLSETPROPS + (short)player.Id + (byte)GraalPlayer.Properties.PLATTACHNPC + (byte)0 + (int)0);
		}

		/// <summary>
		/// Move NPC
		/// </summary>
		public void move(double dx, double dy, double time, int options)
		{
			int start_x = (Math.Abs(Ref.PixelX) << 1) | (Ref.PixelX < 0 ? 0x0001 : 0x0000);
			int start_y = (Math.Abs(Ref.PixelY) << 1) | (Ref.PixelY < 0 ? 0x0001 : 0x0000);
			int pdx = (((short)Math.Abs(dx) * 16) << 1) | (dx < 0 ? 0x0001 : 0x0000);
			int pdy = (((short)Math.Abs(dy) * 16) << 1) | (dy < 0 ? 0x0001 : 0x0000);
			int itime = (short)(time / 0.05);
			this.SendGSPacket(new CString() + (byte)OpenGraal.Common.Levels.GraalLevelNPC.PacketOut.NCQUERY + (byte)OpenGraal.Common.Levels.GraalLevelNPC.NCREQ.NPCMOVE + (int)this.id + (short)start_x + (short)start_y + (short)pdx + (short)pdy + (short)itime + (byte)options);

			Ref.PixelX = Ref.PixelX + Convert.ToInt32(dx * 16);
			Ref.PixelY = Ref.PixelY + Convert.ToInt32(dy * 16);
			if ((options & 8) != 0)
				this.ScheduleEvent(time, "onMovementFinished");
		}

		/// <summary>
		/// Setshape
		/// </summary>
		public void setshape(int type, int width, int height)
		{
			Ref.Width = width;
			Ref.Height = height;
		}

		/// <summary>
		/// Send Packet to GServer
		/// </summary>
		public void SendGSPacket(CString Packet)
		{
			this.Server.SendPacket(Packet);
		}

		/// <summary>
		/// Set Image
		/// </summary>
		public void setimg(string image)
		{
			Ref.Image = image;
			Ref.ImagePart = new CString() + (short)0 + (short)0 + (byte)0 + (byte)0;
			Ref.SendProp(Common.Interfaces.NpcProperties.IMAGE);
			Ref.SendProp(Common.Interfaces.NpcProperties.IMAGEPART);
		}

		/// <summary>
		/// Set Image Part (Image, X, Y, W, H)
		/// </summary>
		public void setimgpart(string image, int x, int y, int w, int h)
		{
			Ref.Image = image;
			Ref.ImagePart = new CString() + (short)x + (short)y + (byte)w + (byte)h;
			Ref.SendProp(Common.Interfaces.NpcProperties.IMAGE);
			Ref.SendProp(Common.Interfaces.NpcProperties.IMAGEPART);
		}

		/// <summary>
		/// NPC -> Hide
		/// </summary>
		public void hide()
		{
			Ref.VisFlags &= ~1;
			Ref.SendProp(Common.Interfaces.NpcProperties.VISFLAGS);
		}

		/// <summary>
		/// NPC -> Show
		/// </summary>
		public void show()
		{
			Ref.VisFlags |= 1;
			Ref.SendProp(Common.Interfaces.NpcProperties.VISFLAGS);
		}

		/// <summary>
		/// Library Function -> Find Player by Account
		/// </summary>
		public IPlayer FindPlayer(string Account)
		{
			//throw new NotImplementedException("requires override!");
			return Ref.Level.FindPlayer(Account);
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public ScriptLevelNpc()
		{
		}

		public ScriptLevelNpc(IRefObject Ref)
		{
			this.Ref = (GraalLevelNPC)Ref;
			this.Server = this.Ref.Server;
		}

		public ScriptLevelNpc(CSocket socket, IRefObject Ref)
			: base(socket)
		{
			this.Ref = (GraalLevelNPC)Ref;
			this.Server = this.Ref.Server;

		}
	};
}
