﻿/* The File Commander - plugin API
 * Bookmark parser
 * (C) The File Commander Team - https://github.com/atauenis/fcmd
 * (C) 2014, Alexander Tauenis (atauenis@yandex.ru)
 * Contributors should place own signs here.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace pluginner
{
	/// <summary>Bookmark menu tools</summary>
	public class BookmarkTools
	{
		List<Bookmark> bookmarks = new List<Bookmark>();
		/// <summary>Initialize bookmark menu toolkit</summary>
		/// <param name="BookmarkXML">The bookmark database (in XML format)</param>
		/// <param name="Category">"QuickAccessBar", "BookmarksMenu" or "UserMenu"</param>
		public BookmarkTools(string BookmarkXML = null, string Category = "BookmarksMenu")
		{
			if (BookmarkXML == null)
			{
				BookmarkXML = Utilities.GetEmbeddedResource("DefaultBookmarks.xml");
				if (BookmarkXML == null) throw new Exception("Cannot load pluginner.dll::DefaultBookmarks.xml");
			}

			XmlDocument bmDoc = new XmlDocument();
			bmDoc.LoadXml(BookmarkXML);
			XmlNodeList items = bmDoc.GetElementsByTagName("SpeedDial");
			foreach (XmlNode x in items)
			{//parsing speed dials
				if (
					x.Attributes.GetNamedItem("type") != null
					&&
					x.Attributes.GetNamedItem("type").Value == Category
				)
				{
					foreach (XmlNode xc in x.ChildNodes)
					{//parsing bookmark list
						if (xc.Name == "AutoBookmarks")//автозакладка
						{
							switch (xc.Attributes.GetNamedItem("type").Value)
							{
								case "System.IO.DriveInfo.GetDrives":
									bookmarks.AddRange(AddSysDrives());
									break;
								case "LinuxMounts":
									bookmarks.AddRange(AddLinuxMounts());
									break;
								//todo: LinuxSystemDirs (/), LinuxUserMounts
							}
						}
						else if (xc.Name == "Bookmark")//простая закладка
						{
							try { 
							Bookmark bm = new Bookmark();
							bm.title=xc.Attributes.GetNamedItem("title").Value;
							bm.url=xc.Attributes.GetNamedItem("url").Value;
							if(xc.OuterXml.IndexOf("icon=") > 0)
							bm.Icon=xc.Attributes.GetNamedItem("icon").Value;
							bookmarks.Add(bm);
							}
							catch { 
								Console.WriteLine("WARNING: Invalid bookmark declaration: " + xc.OuterXml);
							};
						}
						//todo: bookmark folders
					}
				}
			}
		}

		/// <summary>Display bookmark list to the XWT Box as an array of Buttons</summary>
		/// <param name="box">The XWT box</param>
		/// <param name="OnClick">What should happen if user clicks the bookmark</param>
		/// <param name="s">The Stylist that should apply usertheme to the button (or null)</param>
		public void DisplayBookmarks(Xwt.Box box, Action<string> OnClick, Stylist s = null)
		{
			if(s==null) s = new Stylist();
			box.Clear();
			foreach (Bookmark b in bookmarks)
			{
				string url = b.url;
				Xwt.Button NewBtn = new Xwt.Button(null, b.title);
				NewBtn.Clicked += (o, ea) => { OnClick(url); };
				NewBtn.CanGetFocus = false;
				NewBtn.Style = Xwt.ButtonStyle.Flat;
				NewBtn.Margin = -3;
				NewBtn.Cursor = Xwt.CursorType.Hand;
				NewBtn.Image = b.GetIcon();
				s.Stylize(NewBtn);
				box.PackStart(NewBtn);
			}
		}

		/// <summary>Display bookmark list to the specifed XWT Menu</summary>
		/// <param name="mnu">The XWT menu</param>
		/// <param name="OnClick">What should happen if user clicks the bookmark</param>
		public void DisplayBookmarks(Xwt.Menu mnu, Action<string> OnClick)
		{
			if(mnu == null)  mnu = new Xwt.Menu();
			mnu.Items.Clear();
			foreach (Bookmark b in bookmarks)
			{
				string url = b.url;
				Xwt.MenuItem mi = new Xwt.MenuItem();
				mi.Clicked += (o, ea) => { OnClick(url); };
				mi.Label = b.title;
				mi.Image = b.GetIcon();
				mnu.Items.Add(mi);
			}
		}

		/// <summary>Add bookmarks of mounted medias (*nix)</summary>
		private List<Bookmark> AddLinuxMounts()
		{
			List<Bookmark> bms = new List<Bookmark>();

			if (Directory.Exists(@"/mnt"))
			{
				foreach (string dir in Directory.GetDirectories(@"/mnt/"))
				{
					Bookmark bm = new Bookmark();
					bm.url="file://" + dir;
					bm.title=dir.Replace(@"/mnt/", "");
					bm.Icon = "(internal)drive-removable-media.png";
					bms.Add(bm);
				}
			}
			else return AddSysDrives(); //fallback for Windows

			return bms;
		}

		/// <summary>Add bookmarks of mounted medias (Windows)</summary>
		private List<Bookmark> AddSysDrives()
		{
			List<Bookmark> bms = new List<Bookmark>();
			foreach (System.IO.DriveInfo di in System.IO.DriveInfo.GetDrives())
			{
				Bookmark bm = new Bookmark();
				bm.title=di.Name;
				bm.url = "file://" + di.Name;
				/*if (di.IsReady)
				{
					NewBtn.TooltipText = di.VolumeLabel + " (" + di.DriveFormat + ")";
				}*/

				switch (di.DriveType)
				{
					case System.IO.DriveType.Fixed:
						bm.Icon = "(internal)drive-harddisk.png";
						break;
					case System.IO.DriveType.CDRom:
						bm.Icon = "(internal)drive-optical.png";
						break;
					case System.IO.DriveType.Removable:
						bm.Icon = "(internal)drive-removable-media.png";
						break;
					case System.IO.DriveType.Network:
						bm.Icon = "(internal)network-server.png";
						break;
					case System.IO.DriveType.Ram:
						bm.Icon = "(internal)emblem-system.png";
						break;
					case System.IO.DriveType.Unknown:
						bm.Icon = "(internal)image-missing.png";
						break;
				}
				
				string d = di.Name;
				//OS-specific icons
				if (d.StartsWith("A:")) bm.Icon = "(internal)media-floppy.png";
				if (d.StartsWith("B:")) bm.Icon = "(internal)media-floppy.png";
				if (d.StartsWith("/dev")) bm.Icon = "(internal)preferences-desktop-peripherals.png";
				if (d.StartsWith("/proc")) bm.Icon = "(internal)emblem-system.png";
				if (d == "/") bm.Icon = "(internal)root-folder.png";
				bms.Add(bm);
			}
			return bms;
		}
	}

	/// <summary>Represents a item in the bookmark DB</summary>
	public class Bookmark
	{
		private Xwt.Drawing.Image i = Xwt.Drawing.Image.FromResource("pluginner.Resources.folder.png");

		/// <summary>The URL of the bookmark</summary>
		public string url { get;set;}
		/// <summary>The label (caption, title, mark) of the bookmark</summary>
		public string title { get;set;}
		/// <summary>Get the icon of the bookmark (or default icon if the bookmark hasn't an icon)</summary>
		public Xwt.Drawing.Image GetIcon() { return i; }
		/// <summary>Set the icon of the bookmark</summary>
		public string Icon{
			set{
				try {
					if(value == null || value == "") throw new Exception("Please catch me! Catch me! Catch me!"); //to leave 'try' and go to 'catch' block.

					if (value.StartsWith("(internal)"))
						i = Xwt.Drawing.Image.FromResource(value.Replace("(internal)","pluginner.Resources."));
					else
						i = Xwt.Drawing.Image.FromFile(value);
				}
				catch (Exception ex)
				{
					Console.WriteLine("ERROR: Can't load bookmark image " + value + " because of " + ex.Message);
					i = Xwt.Drawing.Image.FromResource("pluginner.Resources.image-missing.png");
				}
			}
		}

		//todo: public SpeedDial SubMenu;
		//todo: public string Description;
	}
}
