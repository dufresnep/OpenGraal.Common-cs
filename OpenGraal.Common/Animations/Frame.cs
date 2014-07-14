using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenGraal.Common.Animations
{
	public class Frame
	{
		public DirFrame[] dirFrames = new DirFrame[4];
		public string PlaySound = "";
		public double x = 0.0;
		public double y = 0.0;
	}

	public class DirFrame
	{
		public List<StageSprite> sprites = new List<StageSprite>();
	}
}
