using System;
using System.Text;
using OpenGraal;
using OpenGraal.Core;
using OpenGraal.Common;
using OpenGraal.Common.Scripting;
using OpenGraal.Common.Players;
using OpenGraal.Common.Animations;
using System.Runtime.InteropServices;

namespace OpenGraal.Common.Levels
{
	public class GraalLevelNPC : IRefObject, IGaniObj
	{
		/// <summary>
		/// NPC Properties Enum
		/// </summary>
		public enum Properties
		{
			IMAGE = 0,
			SCRIPT = 1,
			NPCX = 2,
			NPCY = 3,
			NPCPOWER = 4,
			NPCRUPEES = 5,
			ARROWS = 6,
			BOMBS = 7,
			GLOVEPOWER = 8,
			BOMBPOWER = 9,
			SWORDIMG = 10,
			SHIELDIMG = 11,
			GANI = 12,
			VISFLAGS = 13,
			BLOCKFLAGS = 14,
			MESSAGE = 15,
			HURTDXDY = 16,
			NPCID = 17,
			SPRITE = 18,
			COLORS = 19,
			NICKNAME = 20,
			HORSEIMG = 21,
			HEADIMG = 22,
			SAVE0 = 23,
			SAVE1 = 24,
			SAVE2 = 25,
			SAVE3 = 26,
			SAVE4 = 27,
			SAVE5 = 28,
			SAVE6 = 29,
			SAVE7 = 30,
			SAVE8 = 31,
			SAVE9 = 32,
			ALIGNMENT = 33,
			IMAGEPART = 34,
			BODYIMG = 35,
			GATTRIB1 = 36,
			GATTRIB2 = 37,
			GATTRIB3 = 38,
			GATTRIB4 = 39,
			GATTRIB5 = 40,
			GMAPLVLX = 41,
			GMAPLVLY = 42,
			EMPTY43 = 43,
			GATTRIB6 = 44,
			GATTRIB7 = 45,
			GATTRIB8 = 46,
			GATTRIB9 = 47,
			UNKNOWN48 = 48,
			UNKNOWN49 = 49,
			UNKNOWN50 = 50,
			UNKNOWN51 = 51,
			UNKNOWN52 = 52,
			GATTRIB10 = 53,
			GATTRIB11 = 54,
			GATTRIB12 = 55,
			GATTRIB13 = 56,
			GATTRIB14 = 57,
			GATTRIB15 = 58,
			GATTRIB16 = 59,
			GATTRIB17 = 60,
			GATTRIB18 = 61,
			GATTRIB19 = 62,
			GATTRIB20 = 63,
			GATTRIB21 = 64,
			GATTRIB22 = 65,
			GATTRIB23 = 66,
			GATTRIB24 = 67,
			GATTRIB25 = 68,
			GATTRIB26 = 69,
			GATTRIB27 = 70,
			GATTRIB28 = 71,
			GATTRIB29 = 72,
			GATTRIB30 = 73,
			CLASS = 74,
			// NPC-Server class.  Possibly also join scripts.
			PIXELX = 75,
			PIXELY = 76,
		};

		/// <summary>
		/// Enumerator -> Packet Out
		/// </summary>
		public enum PacketOut
		{
			NPCLOG = 0,
			GETWEAPONS = 1,
			GETLEVELS = 2,
			SENDPM = 3,
			SENDTORC = 4,
			WEAPONADD = 5,
			WEAPONDEL = 6,
			PLAYERPROPSSET = 7,
			PLAYERWEAPONSGET = 8,
			PLAYERPACKET = 9,
			PLAYERWEAPONADD = 10,
			PLAYERWEAPONDEL = 11,
			LEVELGET = 12,
			NPCPROPSSET = 13,
			NPCWARP = 14,
			SENDRPGMESSAGE = 15,
			PLAYERFLAGSET = 16,
			SAY2SIGN = 17,
			PLAYERSTATUSSET = 18,
			NPCMOVE = 19,
			PLAYERPROPS = 2,
			RCCHAT = 79,
			NCQUERY = 103,
		};

		/// <summary>
		/// Enumerator -> NCQuery Packets
		/// </summary>
		public enum NCREQ
		{
			NPCLOG = 0,
			GETWEAPONS = 1,
			GETLEVELS = 2,
			SENDPM = 3,
			SENDTORC = 4,
			WEAPONADD = 5,
			WEAPONDEL = 6,
			PLSETPROPS = 7,
			PLGETWEPS = 8,
			PLSNDPACKET = 9,
			PLADDWEP = 10,
			PLDELWEP = 11,
			LEVELGET = 12,
			NPCPROPSET = 13,
			NPCWARP = 14,
			PLRPGMSG = 15,
			PLSETFLAG = 16,
			PLMSGSIGN = 17,
			PLSETSTATUS = 18,
			NPCMOVE = 19,
			FORWARDTOPLAYER = 20,
		};

		public enum NCI
		{
			PLAYERWEAPONS = 0,
			PLAYERWEAPONADD = 1,
			PLAYERWEAPONDEL = 2,
			GMAPLIST = 3,
		};

		public bool CompileScript = false;
		public bool npcserver = false;
		/// <summary>
		/// Member Variables
		/// </summary>
		public byte GMapX = 0, GMapY = 0;
		public sbyte BlockFlags = 0, VisFlags = 17;
		public double
			Hearts = 3.0;
		public int
			Ap = 50,
			Arrows = 10,
			Bombs = 5,
			BombPower = 1,
			_dir = 2,
			FullHearts = 3,
			GlovePower = 1,
			Gralats = 0,
			Id = 0,
			_pixelX = 480,
			_pixelY = 488,
			ShieldPower = 1,
			SwordPower = 1,
			Width = 2,
			Height = 2;
		public string
			Animation = "idle",
			_bodyImage = "body.png",
			Chat = String.Empty,
			_headImage = "head0.png",
			_swordImage = "",
			_shieldImage = "",
			Image = String.Empty,
			Nickname = "unknown",
			prevGani = "";

		public string HeadImage
		{
			get { return _headImage; }
			set { _headImage = value; }
		}

		public string BodyImage
		{
			get { return _bodyImage; }
			set { _bodyImage = value; }
		}

		public string ShieldImage
		{
			get { return _shieldImage; }
			set { _shieldImage = value; }
		}

		public string SwordImage
		{
			get { return _swordImage; }
			set { _swordImage = value; }
		}

		public int PixelX
		{
			get { return _pixelX; }
			set { _pixelX = value; }
		}

		public int PixelY
		{
			get { return _pixelY; }
			set { _pixelY = value; }
		}

		public int Dir
		{
			get { return _dir; }
			set { _dir = value; }
		}

		public Animations.Animation _currentGani;
		public float 
			OldX = 0.00f,
			OldY = 0.00f;
		public CString ImagePart;
		public GraalLevel Level = null;
		public SaveIndex Save = null;
		internal CSocket Server;

		/// <summary>
		/// Override -> Error Text
		/// </summary>
		public override string GetErrorText()
		{
			return new StringBuilder(Level.name).Append(" (").Append(PixelX / 16).Append(',').Append(PixelY / 16).Append(')').ToString();
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public GraalLevelNPC(GraalLevel Level, int Id)
			: base(ScriptType.LEVELNPC)
		{
			this.Server = null;
			this.Level = Level;
			this.Id = Id;
			this.Save = new SaveIndex(this, 10);
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public GraalLevelNPC(CSocket Server, GraalLevel Level, int Id)
			: base(ScriptType.LEVELNPC)
		{
			this.Server = Server;
			this.Level = Level;
			this.Id = Id;
			this.Save = new SaveIndex(this, 10);
		}

		/// <summary>
		/// Send Prop to GServer
		/// </summary>
		public void SendProp(Properties PropId)
		{
			//Console.WriteLine (this.Server.Connected);
			//Console.WriteLine("Debug: " + (byte)3 + (int)this.Id + (byte)PropId + GetProp(PropId));
			if (this.npcserver)
				this.Server.SendPacket(new CString() + (byte)Levels.GraalLevelNPC.PacketOut.NCQUERY + (byte)Levels.GraalLevelNPC.NCREQ.NPCPROPSET + (int)this.Id + (byte)PropId + GetProp(PropId));
		}

		/// <summary>
		/// Get Property
		/// </summary>
		public CString GetProp(Properties PropId)
		{
			switch (PropId)
			{
				case Properties.IMAGE: // 0
					return new CString() + (byte)this.Image.Length + this.Image;

				case Properties.SCRIPT: // 1
					return new CString() + (long)this.Script.Length + this.Script;

				case Properties.VISFLAGS: // 13
					return new CString() + (byte)VisFlags;

				case Properties.BLOCKFLAGS: // 14
					return new CString() + (byte)BlockFlags;

				case Properties.MESSAGE: // 15
					return new CString() + (byte)Chat.Length + Chat;

				case Properties.SAVE0: // 23
					return new CString() + (byte)Save[0];

				case Properties.SAVE1: // 24
					return new CString() + (byte)Save[1];

				case Properties.SAVE2: // 25
					return new CString() + (byte)Save[2];

				case Properties.SAVE3: // 26
					return new CString() + (byte)Save[3];

				case Properties.SAVE4: // 27
					return new CString() + (byte)Save[4];

				case Properties.SAVE5: // 28
					return new CString() + (byte)Save[5];

				case Properties.SAVE6: // 29
					return new CString() + (byte)Save[6];

				case Properties.SAVE7: // 30
					return new CString() + (byte)Save[7];

				case Properties.SAVE8: // 31
					return new CString() + (byte)Save[8];

				case Properties.SAVE9: // 32
					return new CString() + (byte)Save[9];

				case Properties.IMAGEPART: // 34
					return ImagePart;

				case Properties.GMAPLVLX: // 41
					return new CString() + (byte)GMapX;

				case Properties.GMAPLVLY: // 42
					return new CString() + (byte)GMapY;

				case Properties.PIXELX: // 75
					{
						int res = (PixelX < 0 ? -PixelX : PixelX) << 1;
						if (PixelX < 0)
							res |= 0x0001;
						return new CString() + (short)res;
					}

				case Properties.PIXELY: // 76
					{
						int res = (PixelY < 0 ? -PixelY : PixelY) << 1;
						if (PixelY < 0)
							res |= 0x0001;
						return new CString() + (short)res;
					}

				default:
					return new CString();
			
			}


		}

		/// <summary>
		/// Set Properties
		/// </summary>
		/// <param name="Packet"></param>
		public void SetProps(CString Packet)
		{
			while (Packet.BytesLeft > 0)
			{
				Int32 PropId = Packet.readGUChar();
				//Console.Write("[prop]: " + PropId.ToString());
				switch ((Properties)PropId)
				{
					case Properties.IMAGE: // 0
						this.Image = Packet.readChars(Packet.readGUChar()).ToString();
						break;

					case Properties.SCRIPT: // 1
						int length;
						if (!this.npcserver)
							length = Packet.readGUShort();
						else
							length = (int)Packet.ReadGUByte5();
						this.Script = Packet.readChars(length).ToString();
						this.Script = this.Script.Replace("\xa7", "\n");
						this.Script = this.Script.Replace("�", "\n");
						this.Script = this.Script.Replace("ï¿½", "\n");

						//if (this.Script.IndexOf("void") > 0 || this.Script.IndexOf("join(") > 0)
						this.CompileScript = true;

						break;

					case Properties.NPCX: // 2 - obsolete
					//Packet.ReadGByte1 ();
						this.OldX = (float)(Packet.readGChar()) / 2.0f;
						this.PixelX = (int)(this.OldX * 16);
						break;

					case Properties.NPCY: // 3 - obsolete
					//Packet.ReadGByte1 ();
						this.OldY = (float)(Packet.readGChar()) / 2.0f;
						this.PixelY = (int)(this.OldY * 16);
						break;

					case Properties.NPCPOWER: // 4
						Packet.readGUChar();
						break;

					case Properties.NPCRUPEES: // 5
						this.Gralats = (int)Packet.readGUInt();
						break;

					case Properties.ARROWS: // 6
						this.Arrows = Packet.readGUChar();
						break;

					case Properties.BOMBS: // 7
						this.Bombs = Packet.readGUChar();
						break;

					case Properties.GLOVEPOWER: // 8
						Packet.readGUChar();
						break;

					case Properties.BOMBPOWER: // 9
						Packet.readGUChar();
						break;
					case Properties.SWORDIMG: // 10
						{
							int sp = Packet.readGUChar();
							if (sp <= 4)
								_swordImage = "sword" + sp.ToString() + ".png";
							else
							{
								sp -= 30;
								int len = Packet.readGUChar();
								if (len > 0)
								{
									_swordImage = Packet.readChars(len).ToString();
									if (_swordImage.ToString().Trim().Length != 0)// && clientVersion < CLVER_2_1 && getExtension(swordImage).isEmpty())
									_swordImage += ".gif";
								}
								else
									_swordImage = "";
								//swordPower = clip(sp, ((settings->getBool("healswords", false) == true) ? -(settings->getInt("swordlimit", 3)) : 0), settings->getInt("swordlimit", 3));
							}
							//_swordPower = (char)sp;
						}
						break;
					case Properties.SHIELDIMG: // 11
						{
							int sp = Packet.readGUChar();
							CString shieldImage = new CString();
							int len;
							if (sp <= 3)
							{
								shieldImage = new CString() + "shield" + new CString(sp.ToString()) + ".png";
							}
							else
							{
								sp -= 10;
								len = Packet.readGUChar();
								if (len > 0)
								{
									shieldImage = Packet.readChars(len);
									if (shieldImage.ToString().Trim().Length != 0)// && clientVersion < CLVER_2_1 && getExtension(shieldImage).isEmpty())
									shieldImage += new CString() + ".gif";
								}
								else
									shieldImage = new CString() + "";
							}
							//shieldPower = (char)sp;
							_shieldImage = shieldImage.ToString();
						}
						break;
					case Properties.GANI: // 12
						this.Animation = Packet.readChars(Packet.readGUChar()).ToString();
						break;

					case Properties.VISFLAGS: // 13
						this.VisFlags = (sbyte)Packet.readGUChar();
						break;

					case Properties.BLOCKFLAGS: // 14
						this.BlockFlags = (sbyte)Packet.readGUChar();
						break;

					case Properties.MESSAGE: // 15
						this.Chat = Packet.readChars(Packet.readGUChar()).ToString();
						break;
					case Properties.HURTDXDY: // 16
						float hurtX = ((float)(Packet.readGUChar() - 32)) / 32;
						float hurtY = ((float)(Packet.readGUChar() - 32)) / 32;
						break;
					case Properties.NPCID: // 17
						this.Id = (int)Packet.readGUInt();
						break;

					case Properties.SPRITE: // 18
						this.Dir = Packet.readGUChar();
						break;

					case Properties.COLORS: // 19
						for (int i = 0; i < 5; i++)
							Packet.readGUChar();
						break;

					case Properties.NICKNAME: // 20
						this.Nickname = Packet.readChars(Packet.readGUChar()).ToString();
						break;

					case Properties.HORSEIMG: // 21
						Packet.readChars(Packet.readGUChar());
						break;

					case Properties.HEADIMG: // 22
						{
							Int32 len = Packet.readGUChar();
							this.HeadImage = (len < 100 ? "head" + len + ".png" : Packet.readChars(len - 100).ToString());
							break;
						}

					case Properties.SAVE0: // 23
						this.Save[0] = (byte)Packet.readGUChar();
						break;

					case Properties.SAVE1: // 24
						this.Save[1] = (byte)Packet.readGUChar();
						break;

					case Properties.SAVE2: // 25
						this.Save[2] = (byte)Packet.readGUChar();
						break;

					case Properties.SAVE3: // 26
						this.Save[3] = (byte)Packet.readGUChar();
						break;

					case Properties.SAVE4: // 27
						this.Save[4] = (byte)Packet.readGUChar();
						break;

					case Properties.SAVE5: // 28
						this.Save[5] = (byte)Packet.readGUChar();
						break;

					case Properties.SAVE6: // 29
						this.Save[6] = (byte)Packet.readGUChar();
						break;

					case Properties.SAVE7: // 30
						this.Save[7] = (byte)Packet.readGUChar();
						break;

					case Properties.SAVE8: // 31
						this.Save[8] = (byte)Packet.readGUChar();
						break;

					case Properties.SAVE9: // 32
						this.Save[9] = (byte)Packet.readGUChar();
						break;

					case Properties.ALIGNMENT: // 33
						Packet.readGUChar();
						break;

					case Properties.IMAGEPART: // 34
						this.ImagePart = Packet.readChars(6);
						break;

					case Properties.BODYIMG: // 35
						this.BodyImage = Packet.readChars(Packet.readGUChar()).ToString();
						break;

					case Properties.GMAPLVLX: // 41
						this.GMapX = (byte)Packet.readGUChar();
						break;

					case Properties.GMAPLVLY: // 42
						this.GMapY = (byte)Packet.readGUChar();
						break;
					case Properties.GATTRIB6: // 44
					case Properties.GATTRIB7: // 45
					case Properties.GATTRIB8: // 46
					case Properties.GATTRIB9: // 47
					case Properties.GATTRIB10: // 53,
					case Properties.GATTRIB11: // 54,
					case Properties.GATTRIB12: // 55,
					case Properties.GATTRIB13: // 56,
					case Properties.GATTRIB14: // 57,
					case Properties.GATTRIB15: // 58,
					case Properties.GATTRIB16: // 59,
					case Properties.GATTRIB17: // 60,
					case Properties.GATTRIB18: // 61,
					case Properties.GATTRIB19: // 62,
					case Properties.GATTRIB20: // 63,
					case Properties.GATTRIB21: // 64,
					case Properties.GATTRIB22: // 65,
					case Properties.GATTRIB23: // 66,
					case Properties.GATTRIB24: // 67,
					case Properties.GATTRIB25: // 68,
					case Properties.GATTRIB26: // 69,
					case Properties.GATTRIB27: // 70,
					case Properties.GATTRIB28: // 71,
					case Properties.GATTRIB29: // 72,
					case Properties.GATTRIB30: // 73,
						Packet.readChars(Packet.readGUChar());
						break;

					case Properties.CLASS: // 74
						Packet.readChars(Packet.readGShort());
						break;

					case Properties.PIXELX: // 75
						{
							int tmp = this.PixelX = Packet.readGUShort();
						
							// If the first bit is 1, our position is negative.
							this.PixelX >>= 1;
							if ((tmp & 0x0001) != 0)
								this.PixelX = -this.PixelX;
							break;
						}

					case Properties.PIXELY: // 76
						{
							int tmp = this.PixelY = Packet.readGUShort();

							// If the first bit is 1, our position is negative.
							this.PixelY >>= 1;
							if ((tmp & 0x0001) != 0)
								this.PixelY = -this.PixelY;
							break;
						}

					default:
						System.Console.WriteLine("Unknown NPC Prop: " + PropId + " Data: " + Packet.ReadString());
					//Packet.ReadGUByte1 ();
						return;
				
				}

				//Console.WriteLine(": " + this.GetProp((Properties)PropId).ToString());
			}

			// Compile script if script changed.
			//if (CompileScript)
			//	Server.Compiler.CompileAdd(this);
		}
	}

	public class SaveIndex
	{
		private byte[] SaveData;
		private GraalLevelNPC LevelNpc = null;

		public SaveIndex(GraalLevelNPC LevelNpc, int size)
		{
			this.LevelNpc = LevelNpc;

			SaveData = new byte[size];
			for (int i = 0; i < size; i++)
				SaveData[i] = 0;
		}

		public byte this[int pos]
		{
			get
			{
				return SaveData[pos];
			}
			set
			{
				SaveData[pos] = value;
				LevelNpc.SendProp(GraalLevelNPC.Properties.SAVE0 + pos);
			}
		}
	}

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
				Ref.SendProp(GraalLevelNPC.Properties.PIXELX);
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
				Ref.SendProp(GraalLevelNPC.Properties.PIXELY);
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
		public GraalLevel level
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
			Ref.SendProp(GraalLevelNPC.Properties.BLOCKFLAGS);
		}

		/// <summary>
		/// Dont Block
		/// </summary>
		public void dontblock()
		{
			Ref.BlockFlags = 1;
			Ref.SendProp(GraalLevelNPC.Properties.BLOCKFLAGS);
		}

		/// <summary>
		/// Draw Under Player
		/// </summary>
		public void drawoverplayer()
		{
			Ref.VisFlags |= 2;
			Ref.SendProp(GraalLevelNPC.Properties.VISFLAGS);
		}

		/// <summary>
		/// Draw Under Player
		/// </summary>
		public void drawunderplayer()
		{
			Ref.VisFlags &= ~2;
			Ref.VisFlags |= 4;
			Ref.SendProp(GraalLevelNPC.Properties.VISFLAGS);
		}

		/// <summary>
		/// Message / NPC Chat
		/// </summary>
		public void message(string msg)
		{
			Ref.Chat = msg;
			Ref.SendProp(GraalLevelNPC.Properties.MESSAGE);
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
			Ref.SendProp(GraalLevelNPC.Properties.IMAGE);
			Ref.SendProp(GraalLevelNPC.Properties.IMAGEPART);
		}

		/// <summary>
		/// Set Image Part (Image, X, Y, W, H)
		/// </summary>
		public void setimgpart(string image, int x, int y, int w, int h)
		{
			Ref.Image = image;
			Ref.ImagePart = new CString() + (short)x + (short)y + (byte)w + (byte)h;
			Ref.SendProp(GraalLevelNPC.Properties.IMAGE);
			Ref.SendProp(GraalLevelNPC.Properties.IMAGEPART);
		}

		/// <summary>
		/// NPC -> Hide
		/// </summary>
		public void hide()
		{
			Ref.VisFlags &= ~1;
			Ref.SendProp(GraalLevelNPC.Properties.VISFLAGS);
		}

		/// <summary>
		/// NPC -> Show
		/// </summary>
		public void show()
		{
			Ref.VisFlags |= 1;
			Ref.SendProp(GraalLevelNPC.Properties.VISFLAGS);
		}

		/// <summary>
		/// Library Function -> Find Player by Account
		/// </summary>
		public override GraalPlayer FindPlayer(string Account)
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

		public ScriptLevelNpc(CSocket socket, IRefObject Ref) : base(socket)
		{
			this.Ref = (GraalLevelNPC)Ref;
			this.Server = this.Ref.Server;

		}
	};
}
