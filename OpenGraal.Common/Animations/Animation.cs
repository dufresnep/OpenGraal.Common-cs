using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenGraal;
using OpenGraal.Core;
using OpenGraal.Common;
using OpenGraal.Common.Levels;
using OpenGraal.Common.Players;

namespace OpenGraal.Common.Animations
{
	public class Animation
	{
		#region Member Variables
		public bool loaded;
		public CString name, real;
		private bool _isLoop, _isContinuous, _isSingleDir;
		private CString _setBackTo;
		private Dictionary<int, List<Frame>> _frames = new Dictionary<int, List<Frame>>();
		private Dictionary<int, Sprite> _spriteList = new Dictionary<int, Sprite>();

		//private Dictionary<int,TImage> reallist = new Dictionary<int,TImage>();
		private int _max;
		//private Sdl.thread thread;
		#endregion

		#region Get/set value functions
		public Dictionary<int, Sprite> SpriteList
		{
			get
			{
				return this._spriteList;
			}
			set
			{
				this._spriteList = value;
			}
		}

		public Dictionary<int, List<Frame>> Frames
		{
			get
			{
				return this._frames;
			}
			set
			{
				this._frames = value;
			}
		}

		public bool isLoop
		{
			get
			{
				return this._isLoop;
			}
			set
			{
				this._isLoop = value;
			}
		}

		public bool isContinuous
		{
			get
			{
				return this._isContinuous;
			}
			set
			{
				this._isContinuous = value;
			}
		}

		public bool isSingleDir
		{
			get
			{
				return this._isSingleDir;
			}
			set
			{
				this._isSingleDir = value;
			}
		}

		public CString SetBackTo
		{
			get
			{
				return this._setBackTo;
			}
			set
			{
				this._setBackTo = value;
			}
		}

		public int Max
		{
			get
			{
				return this._max;
			}
			set
			{
				this._max = value;
			}
		}
		#endregion

		#region Constructor / Destructor
		public Animation(CString pName)
		{
			name = pName;
			real = new CString() + pName.Text + pName.IndexOf('\\') + 1;
			load();

			//aniList.add(this);
		}

		~Animation()
		{
			//aniList.Remove(this);

			for (int i = 0; i < _spriteList.Count; i++)
				_spriteList.Remove(i);

			for (int i = 0; i < _frames.Count; i++)
			{
				List<Frame> list = _frames [i];
				for (int j = 0; j < list.Count; j++)
					list.Remove(list [j]);

				list.Clear();
			}

			_spriteList.Clear();
			_frames.Clear();
		}
		#endregion

		#region Public functions
		public bool load()
		{
			CStringList pFile = new CStringList();
			pFile.Load(this.name.Text);
			
			/*
			char[] buffer = new char[65535];
			ulong len = sizeof(buffer);
			int error = uncompress((Bytef *)buffer, (ulong)&en, (byte)pFile.Text, pFile.length());
			if (error != Z_OK)
			{
				printf("Error Decompressing\n");
				return false;
			}
			*/
			CStringList lines = pFile;

			if (lines.Get(0).Text == "GANI0001")
			{
				for (int i = 1; i < lines.Count; i++)
				{
					CStringList words = new CStringList();
					CStringList words2 = new CStringList();
					CStringList words3 = new CStringList();

					words.Load(lines.Get(i).Text, ' ');

					if (words.Count < 1)
						continue;

					if (words.Get(0).Text == "CONTINUOUS" && words.Count == 2)
					{
						_isContinuous = words.Get(1).ToBool();
					}
					else if (words.Get(0).Text == "LOOP" && words.Count == 2)
					{
						_isLoop = words.Get(1).ToBool();
					}
					else if (words.Get(0).Text == "SETBACKTO" && words.Count == 2)
					{
						_setBackTo = words.Get(1);
					}
					else if (words.Get(0).Text == "SINGLEDIRECTION" && words.Count == 2)
					{
						_isSingleDir = words.Get(1).ToBool();
					}
					else if (words.Get(0).Text == "SPRITE")// && words.Count == 7)
					{
						Console.WriteLine("Sprite Add - Id: " + words.Get(1).Text + " - Word Count: " + words.Count);

						this._spriteList.Add(words.Get(1).ToInt(), new Sprite(words.Get(1).ToInt(), words.Get(2), words.Get(3).ToInt(), words.Get(4).ToInt(), words.Get(5).ToInt(), words.Get(6).ToInt()));
					}
					else if (words.Get(0).Text == "ANI" && words.Count == 1)
					{
						i++;

						for (i++; i < lines.Count && lines.Get(i).Text != "ANIEND"; i++)
						{
							if (lines.Get(i).Text.IndexOf("PLAYSOUND") == 0)
								continue;

							List<Frame> list = new List<Frame>();
							Console.WriteLine("Test0: " + lines.Get(i).Text);
							words2.Load(lines.Get(i).Text, ',');

							for (int j = 0; j < words2.Count; j++)
							{
								words3.Load(words2.Get(j).Text.Replace("  ", " ").Trim(), ' ');
								/*
								foreach (CString bla in words3)
									Console.WriteLine("Bla: " + bla.Text);
								*/
								for (int k = 0; k < words3.Count; k++)
								{
									int sprite, x, y;
									Console.WriteLine("Test: " + words3.Get(k).Text);

									sprite = words3.Get(k).ToInt();
									k++;
									x = words3.Get(k).ToInt();
									k++;
									y = words3.Get(k).ToInt();
									foreach (KeyValuePair<int,Sprite> spr in this._spriteList)
										Console.WriteLine("Sprite test - Id1: " + spr.Key + " - Id2: " + spr.Value.SpriteId + " - Img: " + spr.Value.Img);
									Console.WriteLine("Img" + this._spriteList [sprite].Img.Text);
									list.Add(new Frame(this._spriteList [sprite], x, y));
								}
							}
							_frames.Add(_frames.Count + 1, list);
						}
					}
				}

				_max = (_isSingleDir ? _frames.Count : _frames.Count / 4);
			}
			else
			{
				return false;
			}

			return true;
		}

		public virtual void render(int pX, int pY, int pDir, int pStep)
		{
			if (_frames.Count < 1)
				return;

			pStep = (pStep + 1) % _max;

			//*pStep = (isloop ? (*pStep + 1) % max : (*pStep < max-1 ? *pStep + 1 : *pStep));
			List<Frame> list = _frames [(_isSingleDir ? pStep : pStep * 4 + pDir)];

			if (list == null)
				return;

			for (int i = 0; i < list.Count; i++)
			{
				Frame img = (Frame)list [i];
				if (img == null)
					continue;

				img.Render(pX, pY);
			}
		}

		public static Animation Find(CString pName)
		{
			/*
			for (int i = 0; i < aniList.Count; i++)
			{
				TAnimation ani = (TAnimation)aniList[i];
				if (ani.real == pName)
					return ani;
			}
			*/
			return new Animation((pName));
		}
		/*
		public TImage findImage(CString pName)
		{
			for (int i = 0; i < reallist.Count; i++)
			{
				TImage img = (TImage)reallist[i];
				if (img.Real == pName)
					return img;
			}

			reallist.Add(reallist.Count+1, TImage.Find(pName));
			return (TImage)reallist[reallist.Count];
		}
		*/
		#endregion
	}
}
