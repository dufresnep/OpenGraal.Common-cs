using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
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
		private bool _isLoop, _isContinuous, _isSingleDir = false;
		private CString _setBackTo;
		private int _max;
		private List<int> _waits = new List<int> ();
		private List<Frame> _frames = new List<Frame> ();
		private List<Sprite> _spriteList = new List<Sprite> ();
		public bool playing = true;
		public int currentWait;
		public int currentFrame;
		public bool errord = false;

		#endregion

		#region Get/set value functions

		public List<Sprite> SpriteList
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

		public List<Frame> Frames
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

		public List<int> Waits
		{
			get
			{
				return this._waits;
			}
			set
			{
				this._waits = value;
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

		public Animation ()
		{
			
		}

		~Animation ()
		{
			_spriteList.Clear ();
			_frames.Clear ();
		}

		#endregion

		#region Public functions

		public static String nextLine (Stack<string> lines)
		{
			if (lines.Count > 0)
			{
				String line = lines.Pop ();
				line.Replace ("\n", "");
				line.Replace ("\r", "");

				return line;
			}
			else
			{
				return "";
			}
		}

		public Sprite getSpriteDef (int id)
		{
			foreach (Sprite s in _spriteList)
			{
				if (s.SpriteId == id)
					return s;
			}

			return null;
		}

		public void loadFromFile (String path)
		{
			if (!path.Contains (".gani"))
				path = path + "idle.gani";

			//Console.WriteLine("Loading animation from " + path + "...");

			Stack<string> revLines = new Stack<string> (System.IO.File.ReadAllLines (path));
			Stack<string> lines = new Stack<string> ();
			foreach (string l in revLines)
			{
				lines.Push (l);
			}

			String line = nextLine (lines);
			while (lines.Count > 0)
			{
				line = line.Trim ();
				line = Regex.Replace (line, @"[ ]{2,}", @" ");

				string[] words = line.Split (new char[] { ' ' });
				if (words.Count () == 0)
				{
					continue;
				}

				switch (words [0])
				{
				case "SINGLEDIRECTION":
					{
						_isSingleDir = true;
						break;
					}

				case "SPRITE":
					{
						Sprite s = new Sprite ();
						s.SpriteId = int.Parse (words [1]);
						s.Type = new CString (words [2]);
						s.X = int.Parse (words [3]);
						s.Y = int.Parse (words [4]);
						s.Width = int.Parse (words [5]);
						s.Height = int.Parse (words [6]);
						_spriteList.Add (s);
						break;
					}

				case "SCRIPT":
					{
						while (line != "SCRIPTEND")
						{
							line = nextLine (lines);
						}
						break;
					}

				case "ANI":
					{
						line = nextLine (lines);

						while (line != "ANIEND")
						{
							if (line.Length == 0)
							{
								line = nextLine (lines);
								continue;
							}

							if (line.Split (' ') [0] == "PLAYSOUND")
							{
								line = nextLine (lines);
								continue;
							}

							Frame newFrame = new Frame ();
							for (int dir = 0; dir < ((_isSingleDir) ? 1 : 4); dir++)
							{
								string[] offsets = line.Split (new char[] { ',' });

								newFrame.dirFrames [dir] = new DirFrame ();
								foreach (string offsetO in offsets)
								{
									string offset = offsetO.Trim ();
									offset = Regex.Replace (offset, @"[ ]{2,}", @" ");

									string[] partsO = offset.Split (new char[] { ' ' });
									string[] parts = new string[3];
									int c = 0;
									foreach (string p in partsO)
									{
										if (IsNumeric (p))
										{
											parts [c++] = p;
										}
									}

									StageSprite newStageSprite = new StageSprite ();

									newStageSprite.SpriteId = int.Parse (parts [0]);
									newStageSprite.X = int.Parse (parts [1]);
									newStageSprite.Y = int.Parse (parts [2]);

									newFrame.dirFrames [dir].sprites.Add (newStageSprite);
								}

								if (dir < ((_isSingleDir) ? 0 : 3))
								{
									line = nextLine (lines);
								}
							}

							_frames.Add (newFrame);
							_waits.Add (0);

							line = nextLine (lines);
							line = line.Trim ();
							line = Regex.Replace (line, @"[ ]{2,}", @" ");

							while (true)
							{
								string[] toks = line.Split (' ');
								if (toks [0] == "WAIT")
								{
									_waits [_frames.Count - 1] = int.Parse (toks [1]);
								}
								if (line == "ANIEND")
								{
									break;
								}
								else if (line.Length == 0 || line == "\n" || line == "\r" || !IsNumeric (toks [0]))
								{
									line = nextLine (lines);
									line = line.Trim ();
									line = Regex.Replace (line, @"[ ]{2,}", @" ");
								}
								else
								{
									break;
								}
							}
						}
						break;
					}
				}

				line = nextLine (lines);
			}

			//Console.WriteLine("Success.");
		}

		public bool isAbsolutePath (string p)
		{
			if (p.IndexOf (':') != -1)
			{
				return true;
			}

			return false;
		}

		public string MakePath (string p, string top)
		{
			return (isAbsolutePath (p)) ? p : top + "/" + p;
		}

		public virtual void Draw (int x, int y, int dir, object targetWindow, bool dontIncrement = false)
		{
			// OVERRIDE THIS
		}

		public virtual void Draw (int x, int y, int dir, object targetWindow, object spriteBatch, bool dontIncrement = false)
		{
			// OVERRIDE THIS
		}

		public static System.Boolean IsNumeric (System.Object Expression)
		{
			if (Expression == null || Expression is DateTime)
				return false;

			if (Expression is Int16 || Expression is Int32 || Expression is Int64 || Expression is Decimal || Expression is Single || Expression is Double || Expression is Boolean)
				return true;

			try
			{
				if (Expression is string)
					Double.Parse (Expression as string);
				else
					Double.Parse (Expression.ToString ());
				return true;
			}
			catch
			{ // just dismiss errors but return false
				return false;
			}


		}

		#endregion

	}
}
