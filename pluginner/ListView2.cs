﻿/* The File Commander - plugin API - ListView2
 * The enhanced ListView widget
 * (C) The File Commander Team - https://github.com/atauenis/fcmd
 * (C) 2014, Alexander Tauenis (atauenis@yandex.ru)
 * Contributors should place own signs here.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xwt;

namespace pluginner
{
	/// <summary>Modern listview widget</summary>
	public class ListView2 : Widget
	{
		private VBox Layout = new VBox();
		private HBox CollumnRow = new HBox();
		public HeavyScroller ScrollerIn = new HeavyScroller(); //vertical scroller
		private ScrollView ScrollerOut = new ScrollView(); //horizontal scroller
		private List<Label> CollumnTitles = new List<Label>();
		private Table Grid = new Table();
		private int LastRow = 0;
		private int LastCol = 0;
		private Views _View = Views.Details;
		//todo: int MaxRow (для переноса при режиме Small Icons)
		private List<CollumnInfo> _Collumns = new List<CollumnInfo>();
		private int Through10Counter = 0; //для устранения зависания UI при загрузке длинных списков
		private bool Color2 = false; //для обеспечения чередования цветов строк

		//Color sheme
		public Xwt.Drawing.Color NormalBgColor1 = Xwt.Drawing.Colors.White;
		public Xwt.Drawing.Color NormalBgColor2 = Xwt.Drawing.Colors.WhiteSmoke;
		public Xwt.Drawing.Color NormalFgColor1 = Xwt.Drawing.Colors.Black;
		public Xwt.Drawing.Color NormalFgColor2 = Xwt.Drawing.Colors.Black;
		public Xwt.Drawing.Color PointedBgColor = Xwt.Drawing.Colors.LightGray;
		public Xwt.Drawing.Color PointedFgColor = Xwt.Drawing.Colors.Black;
		public Xwt.Drawing.Color SelectedBgColor = Xwt.Drawing.Colors.White;
		public Xwt.Drawing.Color SelectedFgColor = Xwt.Drawing.Colors.Red;
		/// <summary>List of items. Please do not edit directly! Please use the AddItem and RemoveItem functions.</summary>
		public List<ListView2Item> Items = new List<ListView2Item>();
		/// <summary>The pointed item</summary>
		public ListView2Item PointedItem;
		/// <summary>The list of selected items</summary>
		public List<ListView2Item> SelectedItems = new List<ListView2Item>();
		/// <summary>The rows that are allowed to be pointed by keyboard OR null if all rows are allowed</summary>
		public List<int> AllowedToPoint = new List<int>();

		public ListView2()
		{
			base.Content = ScrollerOut;
			Layout.Spacing = 0;
			Grid.DefaultRowSpacing = 0;

			ScrollerOut.Content = Layout;
			ScrollerOut.VerticalScrollPolicy = ScrollPolicy.Never;
			ScrollerIn.Content = Grid;
			ScrollerIn.CanScrollByX = false;// ScrollPolicy.Never;
			Layout.PackStart(CollumnRow);
			Layout.PackStart(ScrollerIn,true,true);

			Layout.KeyPressed += Layout_KeyPressed;
			Layout.CanGetFocus = true;
			base.CanGetFocus = true;
			this.KeyPressed += Layout_KeyPressed;

			this.ScrollerIn.BackgroundColor = Xwt.Drawing.Colors.White;

			//tests for custom pointing edge setup
			/*AllowedToPoint.Add(5);
			AllowedToPoint.Add(6);
			AllowedToPoint.Add(9);*/
		}


		//EVENT HANDLERS

		private void Item_ButtonPressed(object sender, ButtonEventArgs e)
		{
			this.SetFocus();
			ListView2Item lvi = Items[(sender as ListView2Item).RowNo];//вырезание гланд через жопу автогеном? уточнить лучший способ, sender не работает
			//currently, the mouse click policy is same as in Total and Norton Commander
			if (e.Button == PointerButton.Right)//right click - select & do nothing
			{
				_SelectItem(lvi);
				return;
			}
			if (e.Button == PointerButton.Left)//left click - point & don't touch selection
			{
				_SetPoint(lvi);
			}
		}

		void Layout_KeyPressed(object sender, KeyEventArgs e)
		{
		    //See GH issue #10
			Console.WriteLine("LV2 DEBUG: pressed {0}, repeat={1}, handled={2}",e.Key,e.IsRepeat,e.Handled); //УБРАТЬ!!!
			//currently, the keyboard feel is same as in Norton & Total Commanders
			switch (e.Key)
			{
				case Key.Up: //[↑] - move cursor up
					_SetPointerByCondition(-1);
					e.Handled = true;
					return;
				case Key.Down: //[↓] - move cursor bottom
					_SetPointerByCondition(+1);
					e.Handled = true;
					return;
				case Key.Insert: //[Ins] - set selection & move pointer bottom
					_SelectItem(PointedItem);
					_SetPointerByCondition(+1);
					e.Handled = true;
					return;
				case Key.Return: //[↵] - same as double click
					PointedItem.OnDblClick();
					e.Handled = true;
					return;
				case Key.NumPadMultiply: //gray [*] - invert selection
					InvertSelection();
					e.Handled = true;
					return;
				case Key.Home:
					_SetPoint(Items[0]);
					return;
				case Key.End:
					_SetPoint(Items[Items.Count-1]);
					return;
				}
		}


		//SUB-PROGRAMS

		/// <summary>
		/// Sets the pointer to an item by defined condition.
		/// </summary>
		/// <param name='Condition'>
		/// Условие (на сколько строк переместиться)
		/// </param>
		private void _SetPointerByCondition(int Condition){
			/*ОПИСАНИЕ: Перенос курсора выше или ниже.
			  ПРИНЦИП: При наличии списка допущенных к выбору строк (массив номеров строк AllowedToPoint),
			  курсор прыгает в ближайшую допущенную строку в прямом направлении. При выходе из сего списка,
			  курсор может идти в том же направлении дальше без ограничений.
			  */
			int NewRow;
			if(Condition > 0){
				//move bottom
				NewRow = PointedItem.RowNo + Condition;
				foreach(int r in AllowedToPoint){
					if(r > NewRow-1){
						NewRow = r; break;
					}
				}

				if(NewRow < LastRow)
					_SetPoint(Items[NewRow]);
			}else if(Condition < 0){
				//move up
				NewRow = PointedItem.RowNo - -Condition;
				for(int i = AllowedToPoint.Count-1; i > 0; i--){
					int r = AllowedToPoint[i];
					if(r < NewRow){
						NewRow = r; break;
					}
				}
				if(NewRow >= 0)
					_SetPoint(Items[NewRow]);
			}
		}

		/// <summary>Inverts selection of an item</summary>
		/// <param name="lvi">The requested ListView2Item</param>
		private void _SelectItem(ListView2Item lvi)
		{
			SelectedItems.Add(lvi);
			switch (lvi.State)
			{
				case ItemStates.Default:
					lvi.State = ItemStates.Selected;
					break;
				case ItemStates.Pointed:
					lvi.State = ItemStates.PointedAndSelected;
					break;

				case ItemStates.Selected:
				case ItemStates.PointedAndSelected:
					_UnselectItem(lvi);
					break;
			}
			if (SelectionChanged != null)
				SelectionChanged(SelectedItems);
		}

		/// <summary>Removes selection of an item</summary>
		/// <param name="lvi">The requested ListView2Item</param>
		private void _UnselectItem(ListView2Item lvi)
		{
			SelectedItems.Remove(lvi);
			if (lvi.State == ItemStates.PointedAndSelected)
				lvi.State = ItemStates.Pointed;
			else
				lvi.State = ItemStates.Default;
			if (SelectionChanged != null)
				SelectionChanged(SelectedItems);
		}

		/// <summary>Sets the pointer to an item</summary>
		/// <param name="lvi">The requested ListView2Item</param>
		private void _SetPoint(ListView2Item lvi)
		{
			//unpoint current
			if (PointedItem != null)
			{
				if ((int)PointedItem.State > 1)
					PointedItem.State = ItemStates.Selected;
				else
					PointedItem.State = ItemStates.Default;
			}

			//point new
			if ((int)lvi.State > 1)
				lvi.State = ItemStates.PointedAndSelected;
			else
				lvi.State = ItemStates.Pointed;
			PointedItem = lvi;

			if (PointerMoved != null)
				PointerMoved(lvi);

			//if need, scroll the view
			double top = -ScrollerIn.PosY;
			double down = ScrollerIn.Size.Height;
			double newpos = lvi.Size.Height * lvi.RowNo;

			if (top > down){
				//если прокручено далее первой страницы
				down = top + ScrollerIn.Size.Height;
			}

			if (newpos > down || newpos < top)
			{
				ScrollerIn.ScrollTo(-(lvi.Size.Height * lvi.RowNo));
			}

			//todo: add smooth scrolling
		}


		//PUBLIC MEMBERS

		/// <summary>Imitates a press of a keyboard key</summary>
		/// <param name="kea">The key to be "pressed"</param>
		public new void OnKeyPressed(Xwt.KeyEventArgs kea)
		{
			base.OnKeyPressed(kea);
		}

		/// <summary>Add a new item</summary>
		/// <param name="Data">The item's content</param>
		/// <param name="Tag">The tag for the new item (optional)</param>
		public void AddItem(List<Object> Data, string Tag = null)
		{
			ListView2Item lvi = new ListView2Item(
				LastRow,
				LastCol,
				Tag,
				_Collumns.ToArray(),
				Data);
			lvi.Font = Xwt.Drawing.Font.SystemSansSerifFont.WithWeight(Xwt.Drawing.FontWeight.Heavy);
			lvi.PointerBgColor = PointedBgColor;
			lvi.PointerFgColor = PointedFgColor;
			lvi.SelectionBgColor = SelectedBgColor;
			lvi.SelectionFgColor = SelectedFgColor;
			lvi.State = ItemStates.Default;
			AddItem(lvi);
		}

		/// <summary>Add a new ListView2Item into this ListView2</summary>
		/// <param name="Item">The new ListView2Item</param>
		public void AddItem(ListView2Item Item)
		{
			if (Color2){
				Item.NormalBgColor = NormalBgColor2;
				Item.NormalFgColor = NormalFgColor1;
			}
			else{
				Item.NormalBgColor = NormalBgColor1;
				Item.NormalFgColor = NormalFgColor1;
			}

			Color2 = !Color2;
			Items.Add(Item);
			Grid.Add(Item, LastCol, LastRow,1,1,true);
			Item.ButtonPressed += new EventHandler<ButtonEventArgs>(Item_ButtonPressed);
			Item.CanGetFocus = true;
			if (LastRow == 0) _SetPoint(Item);
			LastRow++;

			Through10Counter++;
			if (Through10Counter == 250)
			{
				Xwt.Application.MainLoop.DispatchPendingEvents();
				Through10Counter = 0;
			}
		}

		/// <summary>Removes the specifed item from the list</summary>
		/// <param name="Item">The item</param>
		public void RemoveItem(ListView2Item Item)
		{
			//Note that the removing item is simply hided.
			//To remove it completely, call Clear() sub-programm. But all other rows will be also removed.
			Item.Visible = false;
		}

		/// <summary>Gets pointer for the ListView2Item at specifed row №</summary>
		/// <param name="Row">The row's number</param>
		/// <returns>A pointer to the ListView2 Item</returns>
		public ListView2Item GetItem(int Row)
		{
			return Items[Row];
		}

		/// <summary>Purges the ListView2 (deletes all items from display and memory). Useful when memory leaks are happen.</summary>
		public void Clear()
		{
			Grid.Clear();
			Items.Clear();
			LastRow = LastCol = 0;
			PointedItem = null;
		}

		/// <summary>Clear selection of row</summary>
		/// <param name="Item">The row or null if need to unselect all</param>
		public void Unselect(ListView2Item Item = null)
		{
			if (Item == null)
			{
				foreach (ListView2Item lvi in SelectedItems)
				{
					lvi.State = ItemStates.Default;
				}
				SelectedItems.Clear();
			}
			else
			{
				Item.State = ItemStates.Default;
				SelectedItems.Remove(Item);
			}

			if (SelectionChanged != null)
				SelectionChanged(SelectedItems);
		}

		/// <summary>Selects an row</summary>
		/// <param name="Item">The row or null if need to select all rows</param>
		public void Select(ListView2Item Item = null)
		{
			if (Item != null){
				_SelectItem(Item);
				return;
			}

			SelectedItems.Clear();
			foreach (ListView2Item lvi in Items)
			{
				if (lvi.State == ItemStates.Pointed || lvi.State == ItemStates.PointedAndSelected)
					lvi.State = ItemStates.PointedAndSelected;
				else
					lvi.State = ItemStates.Selected;

				SelectedItems.Add(lvi);
			}

			if (SelectionChanged != null)
				SelectionChanged(SelectedItems);
		}

		/// <summary>Inverts selection of items (like the "[*] gray" key)</summary>
		public void InvertSelection()
		{
			foreach (ListView2Item lvi in Items)
			{
				if ((int)lvi.State >= 2)
				{
					_UnselectItem(lvi);
				}
				else
				{
					_SelectItem(lvi);
				}
			}
			if (SelectionChanged != null)
				SelectionChanged(SelectedItems);
		}

		/// <summary>Scrolls the internal scroll view to the specifed row</summary>
		/// <param name="rowno">The row's number</param>
		public void ScrollToRow(int rowno)
		{
			double Y = Items[0].Surface.GetPreferredSize().Height * rowno;
			ScrollerIn.ScrollTo(Y);
		}

        //PUBLIC EVENTS

		public event TypedEvent<pluginner.ListView2Item> PointerMoved;
		public event TypedEvent<List<pluginner.ListView2Item>> SelectionChanged;

        //PUBLIC PROPERTIES

		/// <summary>Sets collumn configuration</summary>
		public CollumnInfo[] Collumns
		{
			get { return _Collumns.ToArray(); }
			set {
				_Collumns.Clear();
				CollumnTitles.Clear();
				foreach (CollumnInfo ci in value)
				{
					_Collumns.Add(ci);
					CollumnTitles.Add(new Xwt.Label(ci.Title) { WidthRequest = ci.Width, Visible = ci.Visible});
					CollumnRow.PackStart(CollumnTitles[CollumnTitles.Count-1]);
				}
			}
		}

		/// <summary>Defines visiblity of the widget's border</summary>
		public bool BorderVisible
		{
			get { return ScrollerOut.BorderVisible; }
			set { ScrollerOut.BorderVisible = value; }
		}

		/// <summary>Selected row's number</summary>
		public int SelectedRow
		{
			get { return PointedItem.RowNo; }
			set { _SetPoint(Items[value]); }
		}

        /// <summary>Gets the list of the rows that currently are choosed by the user</summary>
        public List<ListView2Item> ChoosedRows
        {
            get
            {
                if (SelectedItems.Count == 0){
                    List<ListView2Item> list_one = new List<ListView2Item>();
                    list_one.Add(PointedItem);
                    return list_one;
                }
                else{
                    return SelectedItems;
                }
            }
        }


		//ENUMS & STRUCTS

		/// <summary>
		/// Defines how the items are displayed in the control.
		/// </summary>
		public enum Views
		{
			SmallIcons, LargeIcons, Details
		}


		/// <summary>
		/// Enumeration of items' selection statuses
		/// </summary>
		public enum ItemStates
		{
			/// <summary>Default item state (not selected nor pointed)</summary>
			Default = 0,
			/// <summary>The item is pointed, but not selected</summary>
			Pointed = 1,
			/// <summary>The item is selected</summary>
			Selected = 2,
			/// <summary>The item is pointed and selected</summary>
			PointedAndSelected = 3
		}

		/// <summary>
		/// Structure, that contains information about collumns
		/// </summary>
		public struct CollumnInfo
		{
			public string Title;
			public object Tag;
			public double Width;
			public bool Visible;
		}
	}
}
