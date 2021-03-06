﻿/* The File Commander Settings window tabs
 * Tab "Main window layout"
 * (C) The File Commander Team - https://github.com/atauenis/fcmd
 * (C) 2013-14, Alexander Tauenis (atauenis@yandex.ru)
 * Contributors should place own signs here.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace fcmd.SettingsWindowTabs
{
	/// <summary>
	/// "Main window" tab
	/// </summary>
	class swtMainWindow : ISettingsWindowTab
	{
		Xwt.VBox box = new Xwt.VBox();
		Localizator Locale = new Localizator();
		
		Xwt.Frame fraMain = new Xwt.Frame();
		Xwt.VBox fraMainBox = new Xwt.VBox();
		Xwt.CheckBox chkShowToolBar = new Xwt.CheckBox(){Sensitive = false};
		Xwt.CheckBox chkDiskButtons = new Xwt.CheckBox(); //ok
		Xwt.CheckBox chkDiskListBox = new Xwt.CheckBox(){ Sensitive = false };
		Xwt.CheckBox chkPanelTitle = new Xwt.CheckBox(); //ok
		Xwt.CheckBox chkTableCollumns = new Xwt.CheckBox();//ok
		Xwt.CheckBox chkInfoBar = new Xwt.CheckBox();//ok
		Xwt.CheckBox chkCmdLine = new Xwt.CheckBox(){ Sensitive = false };
		Xwt.CheckBox chkKeybHelp = new Xwt.CheckBox();//ok
		Xwt.Label lblBookmarks = new Xwt.Label();
		Xwt.TextEntry txtBookmarks = new Xwt.TextEntry();

		public swtMainWindow()
		{
			box.PackStart(fraMain);
			fraMain.Content = fraMainBox;
			fraMain.Label = Locale.GetString("swtMainWindow");

			chkShowToolBar.Label = Locale.GetString("SWTMWtoolbar");
			chkDiskButtons.Label = Locale.GetString("SWTMWdiskbuttons");
			chkDiskListBox.Label = Locale.GetString("SWTMWdisklistbox");
			chkPanelTitle.Label = Locale.GetString("SWTMWpaneltitle");
			chkTableCollumns.Label = Locale.GetString("SWTMWtablecollumns");
			chkInfoBar.Label = Locale.GetString("SWTMWinfobar");
			chkCmdLine.Label = Locale.GetString("SWTMWcmdline");
			chkKeybHelp.Label = Locale.GetString("SWTMWkeybhelp");
			lblBookmarks.Text = Locale.GetString("SWTMWbookmars");

			chkDiskButtons.State = CBSfromBool(fcmd.Properties.Settings.Default.ShowDiskList);
			chkPanelTitle.State = CBSfromBool(fcmd.Properties.Settings.Default.ShowPanelUrlbox);
			chkTableCollumns.State = CBSfromBool(fcmd.Properties.Settings.Default.ShowPanelTableCaptions);
			chkInfoBar.State = CBSfromBool(fcmd.Properties.Settings.Default.ShowFileInfo);
			chkKeybHelp.State = CBSfromBool(fcmd.Properties.Settings.Default.ShowKeybrdHelp);
			txtBookmarks.Text = fcmd.Properties.Settings.Default.BookmarksFile ?? "";

			fraMainBox.PackStart(chkShowToolBar);
			fraMainBox.PackStart(chkDiskButtons);
			fraMainBox.PackStart(chkDiskListBox);
			fraMainBox.PackStart(chkPanelTitle);
			fraMainBox.PackStart(chkTableCollumns);
			fraMainBox.PackStart(chkInfoBar);
			fraMainBox.PackStart(chkCmdLine);
			fraMainBox.PackStart(chkKeybHelp);
			fraMainBox.PackStart(lblBookmarks);
			fraMainBox.PackStart(txtBookmarks);
		}

		public bool SaveSettings() {
			try
			{
				fcmd.Properties.Settings.Default.ShowDiskList = BoolFromCBX(chkDiskButtons);
				fcmd.Properties.Settings.Default.ShowPanelUrlbox = BoolFromCBX(chkPanelTitle);
				fcmd.Properties.Settings.Default.ShowPanelTableCaptions = BoolFromCBX(chkTableCollumns);
				fcmd.Properties.Settings.Default.ShowFileInfo = BoolFromCBX(chkInfoBar);
				fcmd.Properties.Settings.Default.ShowKeybrdHelp = BoolFromCBX(chkKeybHelp);
				fcmd.Properties.Settings.Default.BookmarksFile = txtBookmarks.Text;
				return true;
			}
			catch(Exception ex) { Xwt.MessageDialog.ShowError(ex.Message) ;  return false; }
		}

		public Xwt.Widget Content
		{
			get { return box; }
		}

		/// <summary>Converts boolean values into Xwt.CheckBoxState</summary>
		private Xwt.CheckBoxState CBSfromBool (bool Bulevo)
		{
			switch (Bulevo){
				case true: return Xwt.CheckBoxState.On;
				case false: return Xwt.CheckBoxState.Off;
			}
			return Xwt.CheckBoxState.Mixed; //fallback
		}

		/// <summary>Converts Xwt.CheckBox selection status into boolean value</summary>
		private bool BoolFromCBX(Xwt.CheckBox CBX)
		{
			switch (CBX.State){
				case Xwt.CheckBoxState.On: return true;
				case Xwt.CheckBoxState.Off: return false;
			}
			return false; //fallback
		}
	}
}
