using System;
using System.Text;
using OpenGraal;
using OpenGraal.Core;
using OpenGraal.Common;
using OpenGraal.Common.Levels;
using OpenGraal.Common.Scripting;
using OpenGraal.Common.Players;
using System.Runtime.InteropServices;

namespace OpenGraal.Common.Levels
{
	public class TNPC : IRefObject
	{
		public enum NPCPROP
		{
			IMAGE = 0,
			SCRIPT = 1,
			X = 2,
			Y = 3,
			POWER = 4,
			RUPEES = 5,
			ARROWS = 6,
			BOMBS = 7,
			GLOVEPOWER = 8,
			BOMBPOWER = 9,
			SWORDIMAGE = 10,
			SHIELDIMAGE = 11,
			GANI = 12,
			// BOWGIF in pre-2.x
			VISFLAGS = 13,
			BLOCKFLAGS = 14,
			MESSAGE = 15,
			HURTDXDY = 16,
			ID = 17,
			SPRITE = 18,
			COLORS = 19,
			NICKNAME = 20,
			HORSEIMAGE = 21,
			HEADIMAGE = 22,
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
			BODYIMAGE = 35,
			GATTRIB1 = 36,
			GATTRIB2 = 37,
			GATTRIB3 = 38,
			GATTRIB4 = 39,
			GATTRIB5 = 40,
			GMAPLEVELX = 41,
			GMAPLEVELY = 42,
			UNKNOWN43 = 43,
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
			X2 = 75,
			Y2 = 76,
			COUNT
		}
		//! VISFLAGS values.
		public enum NPCVISFLAG
		{
			VISIBLE = 0x01,
			DRAWOVERPLAYER	= 0x02,
			DRAWUNDERPLAYER	= 0x04,
		}
		//! BLOCKFLAGS values.
		public enum NPCBLOCKFLAG
		{
			BLOCK = 0x00,
			NOBLOCK	= 0x01,
		}

		private bool blockPositionUpdates, levelNPC;
		private DateTime[] modTime = new DateTime[Convert.ToInt16(NPCPROP.COUNT)];
		private float x, y, hurtX, hurtY;
		private int x2, y2;
		private char gmaplevelx, gmaplevely;
		private uint id;
		private int rupees;
		private char darts, bombs, glovePower, bombPower, swordPower, shieldPower;
		private char visFlags, blockFlags, sprite, power, ap;
		private char[] colors = new char[5];
		private CString[] gAttribs = new CString[30];
		private CString image, swordImage, shieldImage, headImage, bodyImage, horseImage, bowImage, gani;
		private CString nickName, imagePart, chatMsg, weaponName;
		private CString serverScript, clientScript;
		private CString serverScriptFormatted, clientScriptFormatted;
		private char[] saves = new char[10];
		private GraalLevel level;
		private CSocket server;
		int[] __nSavePackets = { 23, 24, 25, 26, 27, 28, 29, 30, 31, 32 };
		int[] __nAttrPackets = { 36, 37, 38, 39, 40, 44, 45, 46, 47, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73 };
		//static CString toWeaponName(CString code);
		//static CString doJoins(CString code, CFileSystem* fs);
		public TNPC(GraalLevel level, uint Id)
			: base(ScriptType.LEVELNPC)/*
blockPositionUpdates(false),
levelNPC(pLevelNPC),
x(pX), y(pY), hurtX(32.0f), hurtY(32.0f),
x2((int)(pX*16)), y2((int)(pY*16)),
gmaplevelx(0), gmaplevely(0),
id(0), rupees(0),
darts(0), bombs(0), glovePower(0), bombPower(0), swordPower(0), shieldPower(0),
visFlags(1), blockFlags(0), sprite(2), power(0), ap(50),
image(pImage), gani("idle"),
level(pLevel), server(pServer)*/
		{
			/*
	memset((void*)colors, 0, sizeof(colors));
	memset((void*)saves, 0, sizeof(saves));
	memset((void*)modTime, 0, sizeof(modTime));
	*/
			// bowImage for pre-2.x clients.
			bowImage = new CString() + (char)0;
			this.server = null;
			this.level = level;
			this.id = Id;
			//this.saves = new SaveIndex (this, 10);
			// imagePart needs to be Graal-packed.
			for (int i = 0; i < 6; i++)
				imagePart.writeGChar(0);

			// Set the gmap levels.
			/*
	GraalMap gmap = level.getMap();
	if (gmap && gmap->getType() == MAPTYPE_GMAP)
	{
		gmaplevelx = (uchar)gmap->getLevelX(level->getLevelName());
		gmaplevely = (uchar)gmap->getLevelY(level->getLevelName());
	}
*
	// We need to alter the modTime of the following props as they should be always sent.
	// If we don't, they won't be sent until the prop gets modified.
	modTime[NPCPROP.IMAGE] = modTime[NPCPROP.SCRIPT] = modTime[NPCPROP.X] = modTime[NPCPROP.Y]
		= modTime[NPCPROP.VISFLAGS] = modTime[NPCPROP.ID] = modTime[NPCPROP.SPRITE] = modTime[NPCPROP.MESSAGE]
		= modTime[NPCPROP.GMAPLEVELX] = modTime[NPCPROP.GMAPLEVELY]
		= modTime[NPCPROP.X2] = modTime[NPCPROP.Y2] = time(0);

	bool levelModificationNPCHack = false;

	// See if the NPC sets the level as a sparring zone.
	if (pScript.subString(0, 12) == "sparringzone")
	{
		pLevel->setSparringZone(true);
		levelModificationNPCHack = true;
	}

	// See if the NPC sets the level as singleplayer.
	if (pScript.subString(0, 12) == "singleplayer")
	{
		pLevel->setSingleplayer(true);
		levelModificationNPCHack = true;
	}

	// Separate clientside and serverside scripts.
	if (server->hasNPCServer())
	{
		if (levelModificationNPCHack)
			serverScript = clientScript = pScript;
		else
		{
			CString s = pScript;
			serverScript = s.readString("//#CLIENTSIDE");
			clientScript = s.readString("");
		}
	}
	else clientScript = pScript;

	// Do joins.
	if (!serverScript.isEmpty()) serverScript = doJoins(serverScript, server->getFileSystem());
	if (!clientScript.isEmpty()) clientScript = doJoins(clientScript, server->getFileSystem());

	// See if the NPC should block position updates from the level leader.
	if (server->hasNPCServer())
	{
		if (serverScript.find("//#BLOCKPOSITIONUPDATES") != -1)
			blockPositionUpdates = true;
	}
	else
	{
		if (clientScript.find("//#BLOCKPOSITIONUPDATES") != -1)
			blockPositionUpdates = true;
	}

	// Remove comments and trim the code if specified.
	if (trimCode)
	{
		if (!serverScript.isEmpty())
		{
			serverScriptFormatted = removeComments(serverScript, "\xa7");
			std::vector<CString> code = serverScriptFormatted.tokenize("\xa7");
			serverScriptFormatted.clear();
			for (List<CString>::iterator i = code.begin(); i != code.end(); ++i)
				serverScriptFormatted + (*i).trim() + "\xa7";
		}
		if (!clientScript.isEmpty())
		{
			clientScriptFormatted = removeComments(clientScript, "\xa7");
			std::vector<CString> code = clientScriptFormatted.tokenize("\xa7");
			clientScriptFormatted.clear();
			for (std::vector<CString>::iterator i = code.begin(); i != code.end(); ++i)
				clientScriptFormatted + (*i).trim() + "\xa7";
		}
	}

	// Search for toweapons in the clientside code and extract the name of the weapon.
	weaponName = toWeaponName(clientScript);

	// Just a little warning for people who don't know.
	if (clientScriptFormatted.Length > 0x705F)
		printf("WARNING: Clientside script of NPC (%s) exceeds the limit of 28767 bytes.\n", (weaponName.Length != 0 ? weaponName.text() : image.text()));
			*/
			// TODO: Create plugin hook so NPCServer can acquire/format code.
		}

		public override string GetErrorText()
		{
			return new StringBuilder(level.name).Append(" (").Append(this.x2 / 16).Append(',').Append(this.y2 / 16).Append(')').ToString();
		}

		CString getProp(char pId, int clientVersion)
		{
			switch ((NPCPROP)pId)
			{
				case NPCPROP.IMAGE:
					return new CString() + (char)image.Text.Length + image;

				case NPCPROP.SCRIPT:
					{
						//if (clientVersion != NSVER_GENERIC)
						return new CString() + (short)(clientScriptFormatted.Length > 0x3FFF ? 0x3FFF : clientScriptFormatted.Length) + clientScriptFormatted.ToString().Substring(0, 0x3FFF);
						//else
						//	return new CString() + (long)serverScriptFormatted.Length + serverScriptFormatted;
					}

				case NPCPROP.X:
					return new CString() + (char)(x * 2);

				case NPCPROP.Y:
					return new CString() + (char)(y * 2);

				case NPCPROP.POWER:
					return new CString() + (char)power;

				case NPCPROP.RUPEES:
					return new CString() + (int)rupees;

				case NPCPROP.ARROWS:
					return new CString() + (char)darts;

				case NPCPROP.BOMBS:
					return new CString() + (char)bombs;

				case NPCPROP.GLOVEPOWER:
					return new CString() + (char)glovePower;

				case NPCPROP.BOMBPOWER:
					return new CString() + (char)bombPower;

				case NPCPROP.SWORDIMAGE:
					if (swordPower == 0)
						return new CString() + (char)0;
					else
						return new CString() + (char)(swordPower + 30) + (char)swordImage.Length + swordImage;

				case NPCPROP.SHIELDIMAGE:
					if (shieldPower + 10 > 10)
						return new CString() + (char)(shieldPower + 10) + (char)shieldImage.Length + shieldImage;
					else
						return new CString() + (char)0;

				case NPCPROP.GANI:
		//if (clientVersion < CLVER_2_1)
		//	return bowImage;
					return new CString() + (char)gani.Length + gani;

				case NPCPROP.VISFLAGS:
					return new CString() + (char)visFlags;

				case NPCPROP.BLOCKFLAGS:
					return new CString() + (char)blockFlags;

				case NPCPROP.MESSAGE:
					return new CString() + (char)chatMsg.ToString().Substring(0, 200).Length + chatMsg.ToString().Substring(0, 200);

				case NPCPROP.HURTDXDY:
					return new CString() + (char)((hurtX * 32) + 32) + (char)((hurtY * 32) + 32);

				case NPCPROP.ID:
					return new CString() + (int)id;

			// Sprite is deprecated and has been replaced by def.gani.
			// Sprite now holds the direction of the npc.  sprite % 4 gives backwards compatibility.
				case NPCPROP.SPRITE:
					{
						//if (clientVersion < CLVER_2_1) return new CString() + (char)sprite;
						//else 
						return new CString() + (char)(sprite % 4);
					}

				case NPCPROP.COLORS:
					return new CString() + (char)colors[0] + (char)colors[1] + (char)colors[2] + (char)colors[3] + (char)colors[4];

				case NPCPROP.NICKNAME:
					return new CString() + (char)nickName.Length + nickName;

				case NPCPROP.HORSEIMAGE:
					return new CString() + (char)horseImage.Length + horseImage;

				case NPCPROP.HEADIMAGE:
					return new CString() + (char)(headImage.Length + 100) + headImage;

				case NPCPROP.ALIGNMENT:
					return new CString() + (char)ap;

				case NPCPROP.IMAGEPART:
					return new CString() + imagePart;

				case NPCPROP.BODYIMAGE:
					return new CString() + (char)bodyImage.Length + bodyImage;

				case NPCPROP.GMAPLEVELX:
					return new CString() + (char)gmaplevelx;

				case NPCPROP.GMAPLEVELY:
					return new CString() + (char)gmaplevely;

				case NPCPROP.CLASS:
					return new CString() + (short)0;

				case NPCPROP.X2:
					{
						ushort val = (ushort)(x2 < 0 ? -x2 : x2);
						val += (ushort)1;
						if (x2 < 0)
						{
							val |= 0x0001;
						}

						CString ret = new CString();
						ret.writeGShort((short)val);
			
						return ret;
					}

				case NPCPROP.Y2:
					{
						ushort val = (ushort)(y2 < 0 ? -y2 : y2);
						val += 1;
						if (y2 < 0)
							val |= 0x0001;

						CString ret = new CString();

						return ret.writeGShort((short)val);
					}
			}

			// Saves.
			if (pId.InRange((char)23, (char)32))
			{
				for (uint i = 0; i < __nSavePackets.Length; i++)
				{
					if (__nSavePackets[i] == pId)
						return new CString() + (char)saves[i];
				}
			}

			// Gani attributes.
			if (pId.InRange((char)NPCPROP.GATTRIB1, (char)NPCPROP.GATTRIB5) || pId.InRange((char)NPCPROP.GATTRIB6, (char)NPCPROP.GATTRIB9) || pId.InRange((char)NPCPROP.GATTRIB10, (char)NPCPROP.GATTRIB30))
			{
				for (uint i = 0; i < __nAttrPackets.Length; i++)
				{
					if (__nAttrPackets[i] == pId)
						return new CString() + (char)gAttribs[i].Length + gAttribs[i];
				}
			}

			return new CString();
		}

		CString getProps(DateTime newTime, int clientVersion)
		{
			bool oldcreated = false;//server->getSettings()->getBool("oldcreated", "false");
			CString retVal = new CString();
			int pmax = (int)NPCPROP.COUNT;
			//if (clientVersion < CLVER_2_1) pmax = 36;

			for (int i = 0; i < pmax; i++)
			{
				//if (modTime[i]. != 0 && modTime[i] >= newTime)
				{
					if (oldcreated && i == (int)NPCPROP.VISFLAGS)// && newTime == 0)
						retVal += new CString() + (char)i + (char)(visFlags | (int)NPCVISFLAG.VISIBLE);
					else
						retVal += new CString() + (char)i + getProp((char)i, clientVersion).ToString();
				}
			}
//	if (clientVersion > CLVER_1_411)
			{
				//if (modTime[(int)NPCPROP.GANI] == 0 && image == "#c#")
				retVal += new CString() + (char)NPCPROP.GANI + (char)4 + "idle";
			}

			return retVal;
		}

		CString setProps(CString pProps, int clientVersion)
		{
			CString ret = new CString();
			int len = 0;
			while (pProps.BytesLeft > 0)
			{
				char propId = pProps.readGUChar();
				CString oldProp = getProp(propId, clientVersion);
				//printf( "propId: %d\n", propId );
				switch ((NPCPROP)propId)
				{
					case NPCPROP.IMAGE:
						image = pProps.readChars(pProps.readGUChar());
						if (image.ToString().Trim().Length != 0)// && clientVersion < CLVER_2_1 && getExtension(image).isEmpty())
						image += new CString() + ".gif";
						break;

					case NPCPROP.SCRIPT:
						clientScript = pProps.readChars(pProps.readGUShort());
						break;

					case NPCPROP.X:
						if (blockPositionUpdates)
						{
							pProps.readGChar();
							continue;
						}
						x = (float)(pProps.readGChar()) / 2.0f;
						x2 = (int)(x * 16);
						break;

					case NPCPROP.Y:
						if (blockPositionUpdates)
						{
							pProps.readGChar();
							continue;
						}
						y = (float)(pProps.readGChar()) / 2.0f;
						y2 = (int)(y * 16);
						break;

					case NPCPROP.POWER:
						power = pProps.readGUChar();
						break;

					case NPCPROP.RUPEES:
						rupees = (int)pProps.readGUInt();
						break;

					case NPCPROP.ARROWS:
						darts = pProps.readGUChar();
						break;

					case NPCPROP.BOMBS:
						bombs = pProps.readGUChar();
						break;

					case NPCPROP.GLOVEPOWER:
						glovePower = pProps.readGUChar();
						break;

					case NPCPROP.BOMBPOWER:
						bombPower = pProps.readGUChar();
						break;

					case NPCPROP.SWORDIMAGE:
						{
							int sp = pProps.readGUChar();
							if (sp <= 4)
								swordImage = new CString() + "sword" + new CString(sp.ToString()) + ".png";
							else
							{
								sp -= 30;
								len = pProps.readGUChar();
								if (len > 0)
								{
									swordImage = pProps.readChars(len);
									if (swordImage.ToString().Trim().Length != 0)// && clientVersion < CLVER_2_1 && getExtension(swordImage).isEmpty())
									swordImage += new CString() + ".gif";
								}
								else
									swordImage = new CString() + "";
								//swordPower = clip(sp, ((settings->getBool("healswords", false) == true) ? -(settings->getInt("swordlimit", 3)) : 0), settings->getInt("swordlimit", 3));
							}
							swordPower = (char)sp;
						}
						break;

					case NPCPROP.SHIELDIMAGE:
						{
							int sp = pProps.readGUChar();
							if (sp <= 3)
								shieldImage = new CString() + "shield" + new CString(sp.ToString()) + ".png";
							else
							{
								sp -= 10;
								len = pProps.readGUChar();
								if (len > 0)
								{
									shieldImage = pProps.readChars(len);
									if (shieldImage.ToString().Trim().Length != 0)// && clientVersion < CLVER_2_1 && getExtension(shieldImage).isEmpty())
									shieldImage += new CString() + ".gif";
								}
								else
									shieldImage = new CString() + "";
							}
							shieldPower = (char)sp;
						}
						break;

					case NPCPROP.GANI:
					/*
				if (clientVersion < CLVER_2_1)
				{
					// Older clients don't use ganis.  This is the bow power and image instead.
					int sp = pProps.readGUChar();
					if (sp < 10)
						bowImage = CString() + (char)sp;
					else
					{
						sp -= 10;
						if (sp < 0) break;
						bowImage = pProps.readChars(sp);
						if (!bowImage.isEmpty() && clientVersion < CLVER_2_1 && getExtension(bowImage).isEmpty())
							bowImage + ".gif";
						bowImage = CString() + (char)(10 + bowImage.Length) + bowImage;
					}
					break;
				}
					*/
						gani = pProps.readChars(pProps.readGUChar());
						break;

					case NPCPROP.VISFLAGS:
						visFlags = pProps.readGUChar();
						break;

					case NPCPROP.BLOCKFLAGS:
						blockFlags = pProps.readGUChar();
						break;

					case NPCPROP.MESSAGE:
						chatMsg = pProps.readChars(pProps.readGUChar());
						break;

					case NPCPROP.HURTDXDY:
						hurtX = ((float)(pProps.readGUChar() - 32)) / 32;
						hurtY = ((float)(pProps.readGUChar() - 32)) / 32;
						break;

					case NPCPROP.ID:
						pProps.readGUInt();
						break;

					case NPCPROP.SPRITE:
						sprite = pProps.readGUChar();
						break;

					case NPCPROP.COLORS:
						for (int i = 0; i < 5; i++)
							colors[i] = pProps.readGUChar();
						break;

					case NPCPROP.NICKNAME:
						nickName = pProps.readChars(pProps.readGUChar());
						break;

					case NPCPROP.HORSEIMAGE:
						horseImage = pProps.readChars(pProps.readGUChar());
						if (horseImage.ToString().Trim().Length != 0)// && clientVersion < CLVER_2_1 && getExtension(horseImage).isEmpty())
						horseImage += new CString() + ".gif";
						break;

					case NPCPROP.HEADIMAGE:
						len = pProps.readGUChar();
						if (len < 100)
							headImage = new CString() + "head" + new CString(len.ToString()) + ".png";
						else
						{
							headImage = pProps.readChars(len - 100);
							if (headImage.ToString().Trim().Length != 0)// && clientVersion < CLVER_2_1 && getExtension(headImage).isEmpty())
							headImage += new CString() + ".gif";
						}
						break;

					case NPCPROP.ALIGNMENT:
						ap = pProps.readGUChar();
						if (ap < 0)
							ap = (char)0;
						else if (ap > 100)
							ap = (char)100;
				//ap = clip(ap, 0, 100);
						break;

					case NPCPROP.IMAGEPART:
						imagePart = pProps.readChars(6);
						break;

					case NPCPROP.BODYIMAGE:
						bodyImage = pProps.readChars(pProps.readGUChar());
						break;

					case NPCPROP.GMAPLEVELX:
						gmaplevelx = pProps.readGUChar();
						break;

					case NPCPROP.GMAPLEVELY:
						gmaplevely = pProps.readGUChar();
						break;

					case NPCPROP.CLASS:
						pProps.readChars(pProps.readGShort());
						break;

				// Location, in pixels, of the npc on the level in 2.3+ clients.
				// Bit 0x0001 controls if it is negative or not.
				// Bits 0xFFFE are the actual value.
					case NPCPROP.X2:
						if (blockPositionUpdates)
						{
							pProps.readGUShort();
							continue;
						}

						x2 = len = pProps.readGUShort();

				// If the first bit is 1, our position is negative.
						x2 += 1;
						if (((short)len & 0x0001) != 0)
							x2 = -x2;

				// Let pre-2.3+ clients see 2.3+ movement.
						x = (float)x2 / 16.0f;
						break;

					case NPCPROP.Y2:
						if (blockPositionUpdates)
						{
							pProps.readGUShort();
							continue;
						}

						y2 = len = pProps.readGUShort();

				// If the first bit is 1, our position is negative.
						y2 += 1;
						if (((short)len & 0x0001) != 0)
							y2 = -y2;

				// Let pre-2.3+ clients see 2.3+ movement.
						y = (float)y2 / 16.0f;
						break;

					case NPCPROP.SAVE0:
						saves[0] = pProps.readGUChar();
						break;
					case NPCPROP.SAVE1:
						saves[1] = pProps.readGUChar();
						break;
					case NPCPROP.SAVE2:
						saves[2] = pProps.readGUChar();
						break;
					case NPCPROP.SAVE3:
						saves[3] = pProps.readGUChar();
						break;
					case NPCPROP.SAVE4:
						saves[4] = pProps.readGUChar();
						break;
					case NPCPROP.SAVE5:
						saves[5] = pProps.readGUChar();
						break;
					case NPCPROP.SAVE6:
						saves[6] = pProps.readGUChar();
						break;
					case NPCPROP.SAVE7:
						saves[7] = pProps.readGUChar();
						break;
					case NPCPROP.SAVE8:
						saves[8] = pProps.readGUChar();
						break;
					case NPCPROP.SAVE9:
						saves[9] = pProps.readGUChar();
						break;

					case NPCPROP.GATTRIB1:
						gAttribs[0] = pProps.readChars(pProps.readGUChar());
						break;
					case NPCPROP.GATTRIB2:
						gAttribs[1] = pProps.readChars(pProps.readGUChar());
						break;
					case NPCPROP.GATTRIB3:
						gAttribs[2] = pProps.readChars(pProps.readGUChar());
						break;
					case NPCPROP.GATTRIB4:
						gAttribs[3] = pProps.readChars(pProps.readGUChar());
						break;
					case NPCPROP.GATTRIB5:
						gAttribs[4] = pProps.readChars(pProps.readGUChar());
						break;
					case NPCPROP.GATTRIB6:
						gAttribs[5] = pProps.readChars(pProps.readGUChar());
						break;
					case NPCPROP.GATTRIB7:
						gAttribs[6] = pProps.readChars(pProps.readGUChar());
						break;
					case NPCPROP.GATTRIB8:
						gAttribs[7] = pProps.readChars(pProps.readGUChar());
						break;
					case NPCPROP.GATTRIB9:
						gAttribs[8] = pProps.readChars(pProps.readGUChar());
						break;
					case NPCPROP.GATTRIB10:
						gAttribs[9] = pProps.readChars(pProps.readGUChar());
						break;
					case NPCPROP.GATTRIB11:
						gAttribs[10] = pProps.readChars(pProps.readGUChar());
						break;
					case NPCPROP.GATTRIB12:
						gAttribs[11] = pProps.readChars(pProps.readGUChar());
						break;
					case NPCPROP.GATTRIB13:
						gAttribs[12] = pProps.readChars(pProps.readGUChar());
						break;
					case NPCPROP.GATTRIB14:
						gAttribs[13] = pProps.readChars(pProps.readGUChar());
						break;
					case NPCPROP.GATTRIB15:
						gAttribs[14] = pProps.readChars(pProps.readGUChar());
						break;
					case NPCPROP.GATTRIB16:
						gAttribs[15] = pProps.readChars(pProps.readGUChar());
						break;
					case NPCPROP.GATTRIB17:
						gAttribs[16] = pProps.readChars(pProps.readGUChar());
						break;
					case NPCPROP.GATTRIB18:
						gAttribs[17] = pProps.readChars(pProps.readGUChar());
						break;
					case NPCPROP.GATTRIB19:
						gAttribs[18] = pProps.readChars(pProps.readGUChar());
						break;
					case NPCPROP.GATTRIB20:
						gAttribs[19] = pProps.readChars(pProps.readGUChar());
						break;
					case NPCPROP.GATTRIB21:
						gAttribs[20] = pProps.readChars(pProps.readGUChar());
						break;
					case NPCPROP.GATTRIB22:
						gAttribs[21] = pProps.readChars(pProps.readGUChar());
						break;
					case NPCPROP.GATTRIB23:
						gAttribs[22] = pProps.readChars(pProps.readGUChar());
						break;
					case NPCPROP.GATTRIB24:
						gAttribs[23] = pProps.readChars(pProps.readGUChar());
						break;
					case NPCPROP.GATTRIB25:
						gAttribs[24] = pProps.readChars(pProps.readGUChar());
						break;
					case NPCPROP.GATTRIB26:
						gAttribs[25] = pProps.readChars(pProps.readGUChar());
						break;
					case NPCPROP.GATTRIB27:
						gAttribs[26] = pProps.readChars(pProps.readGUChar());
						break;
					case NPCPROP.GATTRIB28:
						gAttribs[27] = pProps.readChars(pProps.readGUChar());
						break;
					case NPCPROP.GATTRIB29:
						gAttribs[28] = pProps.readChars(pProps.readGUChar());
						break;
					case NPCPROP.GATTRIB30:
						gAttribs[29] = pProps.readChars(pProps.readGUChar());
						break;

					default:
						{
							Console.Write("NPC %d (%.2f, %.2f): ", id, x, y);
							Console.Write("Unknown prop: %ud, readPos: %d\n", propId, pProps.ReadCount);
							for (int i = 0; i < pProps.Length; ++i)
								Console.Write("%02x ", (char)pProps[i]);
							Console.Write("\n");
						}
						return ret;
				}

				// If a prop changed, adjust its mod time.
				if (propId < (int)NPCPROP.COUNT)
				{
					if (oldProp != getProp(propId, clientVersion))
						modTime[propId] = new DateTime();
				}

				// Add to ret.
				ret += new CString() + (char)propId + getProp(propId, clientVersion);
			}
			return ret;
		}

		CString toWeaponName(CString code)
		{
			int name_start = code.ToString().IndexOf("toweapons ");
			if (name_start == -1)
				return new CString();
			name_start += 10;	// 10 = strlen("toweapons ")

			int[] name_end = { code.ToString().IndexOf(";", name_start), code.ToString().IndexOf("}", name_start) };
			if (name_end[0] == -1 && name_end[1] == -1)
				return new CString();

			int name_pos = -1;
			if (name_end[0] == -1)
				name_pos = name_end[1];
			if (name_end[1] == -1)
				name_pos = name_end[0];
			if (name_pos == -1)
				name_pos = (name_end[0] < name_end[1]) ? name_end[0] : name_end[1];

			return new CString() + code.ToString().Substring(name_start, name_pos - name_start).Trim();
		}

		CString doJoins(CString code, dynamic fs)
		{
			CString ret = new CString();
			/*
	CString c = code;
	std::vector<CString> joinList;

	// Parse out all the joins.
	while (c.bytesLeft())
	{
		ret + c.readString("join ");

		int pos = c.readPos();
		int loc = c.find(";", pos);
		if (loc != -1)
		{
			CString spacecheck = c.subString(pos, loc - pos);
			if (!spacecheck.contains(" \t") && c.bytesLeft())
			{
				ret + ";\xa7";
				joinList.push_back(CString() + c.readString(";") + ".txt");
			}
		}
	}

	// Add the files now.
	for (std::vector<CString>::iterator i = joinList.begin(); i != joinList.end(); ++i)
	{
		//printf("file: %s\n", (*i).text());
		c = fs->load(*i);
		c.removeAllI("\r");
		c.replaceAllI("\n", "\xa7");
		ret + c;
		//ret + removeComments(c, "\xa7");
	}
			*/
			return ret;
		}
	}
}