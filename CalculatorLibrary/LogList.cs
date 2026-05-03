namespace CalculatorLibrary;

// Represent one line in calculations list
public class CalcLine
{
   public int PosX { get; set; }

   public int PosY { get; set; }

   public string Text { get; set; }

   public CalcLine (int posX, int posY, string text)
   {
      PosX = posX;
      PosY = posY;
      Text = text;
   }

   public void Display (bool selected = false)
   {
      var colorForeground = Console.ForegroundColor;
      var colorBackground = Console.BackgroundColor;
      if (selected)
      {
         Console.ForegroundColor = ConsoleColor.Black;
         Console.BackgroundColor = ConsoleColor.White;
      }

      Console.SetCursorPosition (PosX, PosY);
      Console.Write (Text);

      Console.ForegroundColor = colorForeground;
      Console.BackgroundColor = colorBackground;
   }
}

// List with previous calculations
public class LogList
{
   Log _log;
   List<CalcLine> _lines = new ();

   int _selectedIndex; // Index of selected line
   int _viewIndex;     // Index of first visible list's item
   int _posYMin; // Y-position of first viisble item in list
   int _posYMax; // Y-position of last visible item in list

   readonly int _maxLineCount = 8; // Maximal count of lines in list
   readonly int _maxWidth = 40; // Maximal width of list in symbols

   public LogList (Log log)
   {
      _log = log;
   }

   public string Show (out double val)
   {
      val = double.NaN;

      if (_log.Calculations.Count <= 0)
      {
         Gui.Notify (" There is no calculations yet ", ConsoleColor.Black, ConsoleColor.White);
         return string.Empty;
      }

      _lines.Clear ();

      string line = new ('─', _maxWidth);
      string indent = "       ";
      Console.WriteLine ("                   Previous calculations");
      Console.WriteLine ("{0}┌{1}┐", indent, line);

      var (x, y) = Console.GetCursorPosition ();
      x += indent.Length + 1;
      _posYMin = y;
      _posYMax = y + _maxLineCount;

      // Prepare view of list's items to display
      string text = string.Empty;
      foreach (var data in _log.Calculations)
      { 
         text = $" {data.Input} = {data.Result}";
         if (text.Length > _maxWidth)
         {
            string subText = text.Substring (0, _maxWidth - 3);
            text = subText + "...";
         }
         else
            text = text.PadRight (_maxWidth, ' ');

         _lines.Add (new (x, y++, text));
      }

      // Show empty list
      text = new (' ', _maxWidth);
      for (int i = 0; i < _maxLineCount; ++i)
         Console.WriteLine ($"{indent}│{text}│");

      Console.WriteLine ("{0}└{1}┘\n", indent, line);

      Tuple<string, string> [] commands =
         [
            Tuple.Create ("\tUp", "    go one line up         "),
            Tuple.Create ("Down", "    go one line down"),
            Tuple.Create ("\tPgUp", "  go to first record     "),
            Tuple.Create ("PgDown", "  go to last record"),
            Tuple.Create ("\tDel", "   delete current record  "),
            Tuple.Create ("R", "       reset list content"),
            Tuple.Create ("\tB", "     back to calculation    "),
            Tuple.Create ("E", "       exit program"),
            Tuple.Create ("\tEnter", " use result of current record for callculation")
         ];

      for (int i = 0; i < commands.Length; ++i)
      {
         var (key, description) = commands [i];
         Gui.WriteColorText (key, ConsoleColor.Cyan);
         Console.Write (description);

         if (i % 2 != 0)
            Console.WriteLine ();
      }

      Fill ();
      RefreshCurrentItem (selected: true);

      Console.CursorVisible = false;
      string command = DoMenu (out val);

      Console.CursorVisible = true;
      return command;
   }

   // Display list's items starting from first element, which is currently visible
   void Fill ()
   {
      if (_lines.Count <= 0)
         return;

      int count = 0;
      int x = _lines [_viewIndex].PosX;
      int y = _lines [_viewIndex].PosY;

      for (int i = _viewIndex; i < _lines.Count; ++i, ++y)
      {
         _lines [i].Display ();
         if (++count >= _maxLineCount)
            break;
      }

      // All items fit in console's view
      if (count >= _maxLineCount)
         return;

      // Erase all items on console from last visible item until end of list
      string text = new (' ', _maxWidth);
      for (;  count <= _maxLineCount - 1; ++count, ++y)
      {
         Console.SetCursorPosition (x, y);
         Console.Write (text);
      }
   }


   string DoMenu (out double val)
   {
      val = double.NaN;

      for (; ; )
      {
         var key = Console.ReadKey (true).Key;
         switch (key)
         {
            case ConsoleKey.UpArrow:
               SelectPrevItem ();
               break;

            case ConsoleKey.DownArrow:
               SelectNextItem ();
               break;

            case ConsoleKey.PageUp:
               SelectFirstItem ();
               break;

            case ConsoleKey.PageDown:
               SelectLastItem ();
               break;

            case ConsoleKey.B: // Back to calculation
               Console.Clear ();
               return "b";

            case ConsoleKey.E: // Exit app
               return "e";

            case ConsoleKey.Enter: // Use result of previous calculation in new calculation
               val = double.Parse (_log.Calculations [_selectedIndex].Result);
               return "u";

            case ConsoleKey.Delete:
               Delete ();
               if (_lines.Count <= 0)
                  return "r";

               break;

            case ConsoleKey.R: // Delete all items in list
               Reset ();
               return "r";
         }
      }
   }

   void RefreshCurrentItem (bool selected = false)
   {
      if (_selectedIndex >= 0 && _selectedIndex < _lines.Count)
         _lines [_selectedIndex].Display (selected);
   }

   void SelectPrevItem ()
   {
      if (_selectedIndex == 0)
         return;

      // Deselect actual item
      RefreshCurrentItem ();

      // If previous item is not more visible, move all items in list up one line
      if (_lines [--_selectedIndex].PosY < _posYMin)
         ScrollDown ();

      // Select current item
      RefreshCurrentItem (selected: true);
   }

   void SelectNextItem ()
   {
      if (_selectedIndex == _lines.Count - 1)
         return;

      // Deselect current item
      RefreshCurrentItem ();

      // If mext item is not more visible, move all items in list down one line
      if (_lines [++_selectedIndex].PosY >= _posYMax)
         ScrollUp ();

      // Select current item
      RefreshCurrentItem (selected: true);
   }

   void SelectFirstItem ()
   {
      // If there are more items in a list as a view can contain, then set the first item
      // as a first visible item and then position all left items downd this item accordingly their order.
      if (_lines.Count > _maxLineCount)
      {
         int y = _posYMin;

         foreach (var data in _lines)
            data.PosY = y++;
      }

      _selectedIndex = 0;
      _viewIndex = 0;

      Fill ();
      RefreshCurrentItem (selected: true);
   }

   void SelectLastItem ()
   {
      // If there are more items in a list as a view can contain, then set the last item
      // as a last visible item and then position all left items above this item accordingly their order.
      if (_lines.Count > _maxLineCount)
      {
         int y = _posYMax - 1;
         for (int i = _lines.Count - 1; i >= 0; --i)
            _lines [i].PosY = y--;

         _viewIndex = _lines.Count - _maxLineCount;
      }

      _selectedIndex = _lines.Count - 1;

      Fill ();
      RefreshCurrentItem (selected: true);
   }

   void ScrollUp ()
   {
      ++_viewIndex;
      foreach (var data in _lines)
         --data.PosY;

      Fill ();
   }

   void ScrollDown ()
   {
      --_viewIndex;

      foreach (var data in _lines)
         ++data.PosY;

      Fill ();
   }

   // Deletes selected item from list
   void Delete ()
   {
      // Move all items after deleted item up one line
      for (int i = _selectedIndex + 1; i < _lines.Count; ++i)
         --_lines [i].PosY;

      _lines.RemoveAt (_selectedIndex);
      _log.Delete (_selectedIndex);

      // Last item in list was deleted? Select previous item.
      if (_selectedIndex >= _lines.Count)
         _selectedIndex = _lines.Count - 1;

      Fill ();
      RefreshCurrentItem (selected: true);
   }

   void Reset ()
   {
      _lines.Clear ();
      _log.Reset ();
   }
}
