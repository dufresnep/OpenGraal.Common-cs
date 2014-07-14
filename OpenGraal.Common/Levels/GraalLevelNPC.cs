using System;
using System.Text;
using OpenGraal;
using OpenGraal.Core;
using OpenGraal.Common;
using OpenGraal.Common.Interfaces;
using OpenGraal.Common.Scripting;
using OpenGraal.Common.Players;
using OpenGraal.Common.Animations;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace OpenGraal.Common.Levels
{
	[Serializable]
	public class GraalLevelNPC : IRefObject, IGaniObj
	{

		#region enums
		/// <summary>
		/// NPC Properties Enum
		/// </summary>
		

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
		#endregion
		public bool CompileScript = false;
		public bool npcserver = false;
		/// <summary>
		/// Member Variables
		/// </summary>
		public byte GMapX = 0, GMapY = 0;
		private sbyte _blockFlags = 0, _visFlags = 17;
		public sbyte BlockFlags
		{
			get
			{
				return _blockFlags;
			}
			set
			{
				_blockFlags = value;
			}
		}

		public sbyte VisFlags
		{
			get
			{
				return _visFlags;
			}
			set
			{
				_visFlags = value;
			}
		}

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
			_width = 2,
			_height = 2;

		public int Width
		{
			get
			{
				return _width;
			}
			set
			{
				_width = value;
			}
		}

		public int Height
		{
			get
			{
				return _height;
			}
			set
			{
				_height = value;
			}
		}

		public string
			Animation = "idle",
			_bodyImage = "body.png",
			Chat = String.Empty,
			_headImage = "head0.png",
			_swordImage = "",
			_shieldImage = "",
			_image = String.Empty,
			Nickname = "unknown",
			prevGani = "";

		public string Image
		{
			get
			{
				return _image;
			}
			set
			{
				_image = value;
			}
		}

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
		public ILevel Level = null;
		public SaveIndex Save = null;
		internal CSocket Server;

		/// <summary>
		/// Override -> Error Text
		/// </summary>
		public override string GetErrorText()
		{
			return new StringBuilder(Level.Name).Append(" (").Append(PixelX / 16).Append(',').Append(PixelY / 16).Append(')').ToString();
		}

		public GraalLevelNPC(ILevel Level, int Id)
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
		public GraalLevelNPC(CSocket Server, ILevel Level, int Id)
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
		public void SendProp(NpcProperties PropId)
		{
			//Console.WriteLine (this.Server.Connected);
			//Console.WriteLine("Debug: " + (byte)3 + (int)this.Id + (byte)PropId + GetProp(PropId));
			if (this.npcserver)
				this.Server.SendPacket(new CString() + (byte)Levels.GraalLevelNPC.PacketOut.NCQUERY + (byte)Levels.GraalLevelNPC.NCREQ.NPCPROPSET + (int)this.Id + (byte)PropId + GetProp(PropId));
		}

		/// <summary>
		/// Get Property
		/// </summary>
		public CString GetProp(NpcProperties PropId)
		{
			switch (PropId)
			{
				case NpcProperties.IMAGE: // 0
					return new CString() + (byte)this.Image.Length + this.Image;

				case NpcProperties.SCRIPT: // 1
					return new CString() + (long)this.Script.Length + this.Script;

				case NpcProperties.VISFLAGS: // 13
					return new CString() + (byte)VisFlags;

				case NpcProperties.BLOCKFLAGS: // 14
					return new CString() + (byte)BlockFlags;

				case NpcProperties.MESSAGE: // 15
					return new CString() + (byte)Chat.Length + Chat;

				case NpcProperties.SAVE0: // 23
					return new CString() + (byte)Save[0];

				case NpcProperties.SAVE1: // 24
					return new CString() + (byte)Save[1];

				case NpcProperties.SAVE2: // 25
					return new CString() + (byte)Save[2];

				case NpcProperties.SAVE3: // 26
					return new CString() + (byte)Save[3];

				case NpcProperties.SAVE4: // 27
					return new CString() + (byte)Save[4];

				case NpcProperties.SAVE5: // 28
					return new CString() + (byte)Save[5];

				case NpcProperties.SAVE6: // 29
					return new CString() + (byte)Save[6];

				case NpcProperties.SAVE7: // 30
					return new CString() + (byte)Save[7];

				case NpcProperties.SAVE8: // 31
					return new CString() + (byte)Save[8];

				case NpcProperties.SAVE9: // 32
					return new CString() + (byte)Save[9];

				case NpcProperties.IMAGEPART: // 34
					return ImagePart;

				case NpcProperties.GMAPLVLX: // 41
					return new CString() + (byte)GMapX;

				case NpcProperties.GMAPLVLY: // 42
					return new CString() + (byte)GMapY;

				case NpcProperties.PIXELX: // 75
					{
						int res = (PixelX < 0 ? -PixelX : PixelX) << 1;
						if (PixelX < 0)
							res |= 0x0001;
						return new CString() + (short)res;
					}

				case NpcProperties.PIXELY: // 76
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
		/// Set NpcProperties
		/// </summary>
		/// <param name="Packet"></param>
		public void SetProps(CString Packet)
		{
			while (Packet.BytesLeft > 0)
			{
				Int32 PropId = Packet.readGUChar();
				//Console.Write("[prop]: " + PropId.ToString());
				switch ((NpcProperties)PropId)
				{
					case NpcProperties.IMAGE: // 0
						this.Image = Packet.readChars(Packet.readGUChar()).ToString();
						break;

					case NpcProperties.SCRIPT: // 1
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

					case NpcProperties.NPCX: // 2 - obsolete
					//Packet.ReadGByte1 ();
						this.OldX = (float)(Packet.readGChar()) / 2.0f;
						this.PixelX = (int)(this.OldX * 16);
						break;

					case NpcProperties.NPCY: // 3 - obsolete
					//Packet.ReadGByte1 ();
						this.OldY = (float)(Packet.readGChar()) / 2.0f;
						this.PixelY = (int)(this.OldY * 16);
						break;

					case NpcProperties.NPCPOWER: // 4
						Packet.readGUChar();
						break;

					case NpcProperties.NPCRUPEES: // 5
						this.Gralats = (int)Packet.readGUInt();
						break;

					case NpcProperties.ARROWS: // 6
						this.Arrows = Packet.readGUChar();
						break;

					case NpcProperties.BOMBS: // 7
						this.Bombs = Packet.readGUChar();
						break;

					case NpcProperties.GLOVEPOWER: // 8
						Packet.readGUChar();
						break;

					case NpcProperties.BOMBPOWER: // 9
						Packet.readGUChar();
						break;
					case NpcProperties.SWORDIMG: // 10
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
					case NpcProperties.SHIELDIMG: // 11
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
					case NpcProperties.GANI: // 12
						this.Animation = Packet.readChars(Packet.readGUChar()).ToString();
						break;

					case NpcProperties.VISFLAGS: // 13
						this.VisFlags = (sbyte)Packet.readGUChar();
						break;

					case NpcProperties.BLOCKFLAGS: // 14
						this.BlockFlags = (sbyte)Packet.readGUChar();
						break;

					case NpcProperties.MESSAGE: // 15
						this.Chat = Packet.readChars(Packet.readGUChar()).ToString();
						break;
					case NpcProperties.HURTDXDY: // 16
						float hurtX = ((float)(Packet.readGUChar() - 32)) / 32;
						float hurtY = ((float)(Packet.readGUChar() - 32)) / 32;
						break;
					case NpcProperties.NPCID: // 17
						this.Id = (int)Packet.readGUInt();
						break;

					case NpcProperties.SPRITE: // 18
						this.Dir = Packet.readGUChar();
						break;

					case NpcProperties.COLORS: // 19
						for (int i = 0; i < 5; i++)
							Packet.readGUChar();
						break;

					case NpcProperties.NICKNAME: // 20
						this.Nickname = Packet.readChars(Packet.readGUChar()).ToString();
						break;

					case NpcProperties.HORSEIMG: // 21
						Packet.readChars(Packet.readGUChar());
						break;

					case NpcProperties.HEADIMG: // 22
						{
							Int32 len = Packet.readGUChar();
							this.HeadImage = (len < 100 ? "head" + len + ".png" : Packet.readChars(len - 100).ToString());
							break;
						}

					case NpcProperties.SAVE0: // 23
						this.Save[0] = (byte)Packet.readGUChar();
						break;

					case NpcProperties.SAVE1: // 24
						this.Save[1] = (byte)Packet.readGUChar();
						break;

					case NpcProperties.SAVE2: // 25
						this.Save[2] = (byte)Packet.readGUChar();
						break;

					case NpcProperties.SAVE3: // 26
						this.Save[3] = (byte)Packet.readGUChar();
						break;

					case NpcProperties.SAVE4: // 27
						this.Save[4] = (byte)Packet.readGUChar();
						break;

					case NpcProperties.SAVE5: // 28
						this.Save[5] = (byte)Packet.readGUChar();
						break;

					case NpcProperties.SAVE6: // 29
						this.Save[6] = (byte)Packet.readGUChar();
						break;

					case NpcProperties.SAVE7: // 30
						this.Save[7] = (byte)Packet.readGUChar();
						break;

					case NpcProperties.SAVE8: // 31
						this.Save[8] = (byte)Packet.readGUChar();
						break;

					case NpcProperties.SAVE9: // 32
						this.Save[9] = (byte)Packet.readGUChar();
						break;

					case NpcProperties.ALIGNMENT: // 33
						Packet.readGUChar();
						break;

					case NpcProperties.IMAGEPART: // 34
						this.ImagePart = Packet.readChars(6);
						break;

					case NpcProperties.BODYIMG: // 35
						this.BodyImage = Packet.readChars(Packet.readGUChar()).ToString();
						break;

					case NpcProperties.GMAPLVLX: // 41
						this.GMapX = (byte)Packet.readGUChar();
						if (this.GMapX != 0)
						{
						}
						break;

					case NpcProperties.GMAPLVLY: // 42
						this.GMapY = (byte)Packet.readGUChar();

						if (this.GMapY != 0)
						{
						}
						break;
					case NpcProperties.GATTRIB6: // 44
					case NpcProperties.GATTRIB7: // 45
					case NpcProperties.GATTRIB8: // 46
					case NpcProperties.GATTRIB9: // 47
					case NpcProperties.GATTRIB10: // 53,
					case NpcProperties.GATTRIB11: // 54,
					case NpcProperties.GATTRIB12: // 55,
					case NpcProperties.GATTRIB13: // 56,
					case NpcProperties.GATTRIB14: // 57,
					case NpcProperties.GATTRIB15: // 58,
					case NpcProperties.GATTRIB16: // 59,
					case NpcProperties.GATTRIB17: // 60,
					case NpcProperties.GATTRIB18: // 61,
					case NpcProperties.GATTRIB19: // 62,
					case NpcProperties.GATTRIB20: // 63,
					case NpcProperties.GATTRIB21: // 64,
					case NpcProperties.GATTRIB22: // 65,
					case NpcProperties.GATTRIB23: // 66,
					case NpcProperties.GATTRIB24: // 67,
					case NpcProperties.GATTRIB25: // 68,
					case NpcProperties.GATTRIB26: // 69,
					case NpcProperties.GATTRIB27: // 70,
					case NpcProperties.GATTRIB28: // 71,
					case NpcProperties.GATTRIB29: // 72,
					case NpcProperties.GATTRIB30: // 73,
						Packet.readChars(Packet.readGUChar());
						break;

					case NpcProperties.CLASS: // 74
						Packet.readChars(Packet.readGShort());
						break;

					case NpcProperties.PIXELX: // 75
						{
							int tmp = this.PixelX = Packet.readGUShort();
						
							// If the first bit is 1, our position is negative.
							this.PixelX >>= 1;
							if ((tmp & 0x0001) != 0)
								this.PixelX = -this.PixelX;
							break;
						}

					case NpcProperties.PIXELY: // 76
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

				//Console.WriteLine(": " + this.GetProp((NpcProperties)PropId).ToString());
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
				LevelNpc.SendProp(Common.Interfaces.NpcProperties.SAVE0 + pos);
			}
		}
	}


}
