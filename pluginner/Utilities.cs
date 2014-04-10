﻿/* The File Commander - plugin API
 * FC & FC plugin utility set
 * (C) The File Commander Team - https://github.com/atauenis/fcmd
 * (C) 2013-14, Alexander Tauenis (atauenis@yandex.ru)
 * Contributors should place own signs here.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Windows.Forms;

namespace pluginner
{
	public static class Utilities
	{
		public static int Hex2Dec(string hex)
		{
			return Convert.ToInt32(hex, 16);
		}

		public static Xwt.Drawing.Color Rgb2XwtColor(string RGBhex)
		{
			if (RGBhex.Length != 6) throw new ArgumentOutOfRangeException("RGBhex", "The color should be in the following format: HHHHHH (where H is any hexacedimal number)");
			string[] colors16 = new string[3];
			colors16[0] = RGBhex.Substring(0, 1) + RGBhex.Substring(1, 1);
			colors16[1] = RGBhex.Substring(2, 1) + RGBhex.Substring(3, 1);
			colors16[2] = RGBhex.Substring(4, 1) + RGBhex.Substring(5, 1);
			int[] colors10 = new int[3];
			for (int i = 0; i < 3; i++)
			{
				colors10[i] = Hex2Dec(colors16[i]);
			}
			return new Xwt.Drawing.Color(colors10[0], colors10[1], colors10[2]);
		}

		/// <summary>Loads embedded resource</summary>
		/// <param name="resourceName">The name of the resource</param>
		/// <param name="assembly">The assembly, which contains the resource</param>
		public static string GetEmbeddedResource(string resourceName, Assembly assembly)
		{
			resourceName = FormatResourceName(assembly, resourceName);
			using (Stream resourceStream = assembly.GetManifestResourceStream(resourceName))
			{
				if (resourceStream == null)
					return null;

				using (StreamReader reader = new StreamReader(resourceStream))
				{
					return reader.ReadToEnd();
				}
			}
		}

		private static string FormatResourceName(Assembly assembly, string resourceName)
		{
			return assembly.GetName().Name + "." + resourceName.Replace(" ", "_")
															   .Replace("\\", ".")
															   .Replace("/", ".");
		}

		/// <summary>Loads embedded resource from current program</summary>
		/// <param name="resourceName">The name of the resource</param>
		public static string GetEmbeddedResource(string resourceName)
		{
			return GetEmbeddedResource(resourceName, Assembly.GetExecutingAssembly());
		}

		/// <summary>Search for icon of the selected MIME type</summary>
		/// <param name="MIME">The MIME type (i.e. application/msword)</param>
		/// <returns></returns>
		public static Xwt.Drawing.Image GetIconForMIME(string MIME)
		{
			if (MIME == "x-fcmd/directory")
				return Xwt.Drawing.Image.FromResource("pluginner.Resources.x-fcmd-directory.png");
			
			if (MIME == "x-fcmd/up")
				return Xwt.Drawing.Image.FromResource("pluginner.Resources.x-fcmd-up.png");

			if (System.IO.File.Exists(Application.StartupPath + "/icons/" + MIME.Replace("/","-")+".png"))
			{
				return Xwt.Drawing.Image.FromStream(System.IO.File.OpenRead(Application.StartupPath + Path.DirectorySeparatorChar + "icons" + Path.DirectorySeparatorChar + MIME.Replace("/", "-") + ".png"));
			}

			//UNDONE: забубенить выдирание иконок из катАлагав /etc/mime; ~/.mime
			//UNDONE: забубенить выдирание иконок из реестра Win32 (с выдиранием иконок из exeшников и захреначиванием в кэш)
			//TODO: сделать поддержку MacOS X.

#if DEBUG
			Console.WriteLine("utilities: Can't find an icon for " + MIME);
#endif
			return Xwt.Drawing.Image.FromResource("pluginner.Resources.application-octet-stream.png");
		}
	}
}
