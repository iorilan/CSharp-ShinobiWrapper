using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace Sample.ShinobiVideo.Lib
{
	public class ImageChecker
	{

		private static ILog _log = LogManager.GetLogger(typeof(ImageChecker));

		private static bool SameColor(Color color1, Color color2)
		{
			var delta = 3;
			return Math.Abs(color1.A - color2.A) < delta &&
				   Math.Abs(color1.R - color2.R) < delta &&
				   Math.Abs(color1.G - color2.G) < delta &&
				   Math.Abs(color1.B - color2.B) < delta;
		}
		public static bool IsGoodImage(byte[] imageData)
		{
			//shinobi blank image code :
			// [A=255, R=196, G=154, B=106]
			var colorBlank = Color.FromArgb(196, 154, 106);
			using (var ms = new MemoryStream(imageData))
			{
				var image = Image.FromStream(ms);
				var bmp = new Bitmap(image);
				var color1 = bmp.GetPixel(0, 1);
				var color2 = bmp.GetPixel(bmp.Width / 2, bmp.Height / 2);
				var color3 = bmp.GetPixel(bmp.Width / 3, bmp.Height / 3);
				var color4 = bmp.GetPixel(bmp.Width / 4, bmp.Height / 4);
				var color5 = bmp.GetPixel(bmp.Width / 5, bmp.Height / 5);

				//_log.Info($"images colors :{color1} {color2} {color3} {colorBlank}");
				if (SameColor(color1, colorBlank) &&
					SameColor(color2, colorBlank) &&
					SameColor(color3, colorBlank) &&
					SameColor(color4, colorBlank) &&
					SameColor(color5, colorBlank))
				{
					return false;
				}

				return true;
			}
		}
	}
}
