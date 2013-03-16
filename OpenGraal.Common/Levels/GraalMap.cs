using System;
using System.IO;
using System.Linq;
using System.Text;
using OpenGraal;
using OpenGraal.Core;
using OpenGraal.Common;
using OpenGraal.Common.Players;

namespace OpenGraal.Common.Levels
{
	public class GraalMap
	{
		#region Member Variables
		/// <summary>
		/// Member Variables
		/// </summary>
		public int MapWidth, MapHeight;
		public bool AutoMapping, LoadFullMap;
		public string MapName, MapImage, MiniMapImage;
		private GraalLevel[] MapLevels = new GraalLevel[] { };
		#endregion

		#region Constructors / Destructors
		/// <summary>
		/// Constructor
		/// </summary>
		public GraalMap(String MapName)
		{
			this.MapName = MapName;
		}

		/// <summary>
		/// Constructor (load by file)
		/// </summary>
		public GraalMap(String MapName, String FilePath)
		{
			this.MapName = MapName;
			this.ParseFile(FilePath);
		}
		#endregion

		#region Public functions
		/// <summary>
		/// Get Level from Map X / Map Y
		/// </summary>
		public string GetLevelAt(int MapX, int MapY)
		{
			int pos = MapX + MapY * MapWidth;
			return (pos < MapLevels.Length ? MapLevels[pos].Name : "");
		}

		/// <summary>
		/// Level Exists
		/// </summary>
		public bool LevelExists(String LevelName)
		{
			bool exist = false;
			foreach (GraalLevel level in this.MapLevels)
			{
				if (level.Name == LevelName)
					exist = true;
			}
			
			return exist;
		}

		/// <summary>
		/// Parse GMap by File
		/// </summary>
		public void ParseFile(String File)
		{
			using (TextReader reader = new StreamReader(File))
			{
				this.Parse((StreamReader)reader);
			}
		}

		public int Generate(string TemplateFileName = "")
		{
			int GmapLevelArea = (this.MapWidth * this.MapHeight);

			// Create MapLevels Array
			this.MapLevels = new GraalLevel[GmapLevelArea];

			int gx = 0, gy = 0;
			String GeneratedLevelName;

			for (int i = 0; i < GmapLevelArea; i++)
			{
				GeneratedLevelName = (this.MapName + "_" + gx + "_" + gy + ".nw");

				for (gx = 0; gx < this.MapWidth; gx++)
				{
					int pos = gx + gy * MapWidth;
					if (pos < MapLevels.Length)
					{
						this.MapLevels[pos] = new GraalLevel(GeneratedLevelName, new object());
						if (TemplateFileName.Length != 0)
							if (!this.MapLevels[pos].Load(new CString() + TemplateFileName))
								return 2;
					}
				}

				gy++;
			}

			System.Console.WriteLine("File: " + MapName + " | Level Count: " + MapLevels.Length);
			return 1;
		}

		/// <summary>
		/// Parse GMAP by Stream
		/// </summary>
		public void Parse(StreamReader Stream)
		{
			String line = String.Empty;
			while ((line = Stream.ReadLine()) != null)
			{
				String[] tokens = line.Split(' ');
				if (tokens.Length < 1)
					continue;

				switch (tokens[0])
				{
					// Map Width
					case "WIDTH":
						if (tokens.Length > 1)
							this.MapWidth = Convert.ToInt32(tokens[1]);
						break;

					// Map Height
					case "HEIGHT":
						if (tokens.Length > 1)
							this.MapHeight = Convert.ToInt32(tokens[1]);
						break;

					// Map Image
					case "MAPIMG":
						if (tokens.Length > 1)
							this.MapImage = tokens[1];
						break;

					// Map Image
					case "MINIMAPIMG":
						if (tokens.Length > 1)
							this.MiniMapImage = tokens[1];
						break;

					// Option -> Load Full Map
					case "LOADFULLMAP":
						break;

					// Option -> Disable Automapping
					case "NOAUTOMAPPING":
						break;

					// GENERATED (unknown??)
					case "GENERATED":
						break;

					// Option -> Load Map at Start
					case "LOADATSTART":
						while ((line = Stream.ReadLine()) != null)
						{
							if (line == "LOADATSTARTEND")
								break;
						}
						break;

					// Map Levels
					case "LEVELNAMES":
					{
						// Create MapLevels Array
						this.MapLevels = new GraalLevel[this.MapWidth * this.MapHeight];

						int gx = 0, gy = 0;
						while ((line = Stream.ReadLine()) != null)
						{
							if (line == "LEVELNAMESEND")
								break;

							if (line.Length > 0)
							{
								string[] levels = CString.untokenize(line).Replace("\r", "").Split('\n');
								foreach (string level in levels)
								{
									int pos = gx + gy * MapWidth;
									if (pos < MapLevels.Length)
									{
										this.MapLevels[pos] = new GraalLevel(level, new object());
										this.MapLevels[pos].Load(new CString() + level);
									}

									gx++;
								}
							}

							gx = 0;
							gy++;
						}

						break;
					}
				}
			}

			System.Console.WriteLine("File: " + MapName + " | Level Count: " + MapLevels.Length);
		}

		public int Save(string GmapDirectory)
		{
			//Create the new directory
			if (System.IO.Directory.Exists(GmapDirectory))
				return 0;

			System.IO.Directory.CreateDirectory(GmapDirectory);


			StringBuilder GmapFile = new StringBuilder();

			GmapFile.AppendLine("GRMAP001");
			GmapFile.AppendLine("WIDTH " + this.MapWidth);
			GmapFile.AppendLine("HEIGHT " + this.MapHeight);

			if (this.AutoMapping == true)
			{
				GmapFile.AppendLine("NOAUTOMAPPING");
			}

			if (this.LoadFullMap == true)
			{
				GmapFile.AppendLine("LOADFULLMAP");
			}

			GmapFile.AppendLine("LEVELNAMES");


			GmapFile.AppendLine("LEVELNAMESEND");

			File.WriteAllText(GmapDirectory + this.MapName, GmapFile.ToString());

			String[] Lines = File.ReadAllLines(GmapDirectory + this.MapName + ".gmap");
			Boolean IsMapLine = false;
			GmapFile.Clear();

			foreach (String line in Lines)
			{
				if (line.StartsWith("LEVELNAMES"))
				{
					IsMapLine = true;
				}
				if (line.StartsWith("LEVELNAMESEND"))
				{
					IsMapLine = false;
				}
				if (IsMapLine == true && line.StartsWith("LEVELNAMES") != true)
				{
					GmapFile.AppendLine(line.Substring(0, line.Length - 1));
				}
				else
				{
					GmapFile.AppendLine(line);
				}
			}

			File.Delete(GmapDirectory + this.MapName + ".gmap");
			File.WriteAllText(GmapDirectory + this.MapName + ".gmap", GmapFile.ToString());

			return 1;
		}
		#endregion
	}
}
