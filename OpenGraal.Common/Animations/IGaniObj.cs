using System;

namespace OpenGraal.Common.Animations
{
	public interface IGaniObj
	{
		string HeadImage
		{
			get;
			set;
		}

		string BodyImage
		{
			get;
			set;
		}

		string SwordImage
		{
			get;
			set;
		}

		string ShieldImage
		{
			get;
			set;
		}

		int PixelX
		{
			get;
			set;
		}

		int PixelY
		{
			get;
			set;
		}

		int Dir
		{
			get;
			set;
		}
	}
}

