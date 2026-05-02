namespace CalculatorLibrary
{
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

      public void Select ()
      {
         var colorForeground = Console.ForegroundColor;
         var colorBackground = Console.BackgroundColor;
         Console.ForegroundColor = ConsoleColor.Black;
         Console.BackgroundColor = ConsoleColor.White;

         Console.SetCursorPosition (PosX, PosY);
         Console.Write (Text);

         Console.ForegroundColor = colorForeground;
         Console.BackgroundColor = colorBackground;
      }

      public void Deselect ()
      {
         Console.SetCursorPosition (PosX, PosY);
         Console.Write (Text);
      }
   }

   public class LogList
   {
      Log _log;
      List<CalcLine> _lines = new ();

      int _selectedItem;

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

         const int maxLineCount = 8;
         const int maxWidth = 40;
         string line = new ('─', maxWidth);
         string indent = "       ";
         Console.WriteLine ("                   Previous calculations");
         Console.WriteLine ("{0}┌{1}┐", indent, line);

         for (int i = 0; i < maxLineCount; ++i)
         {
            string text;
            if (i < _log.Calculations.Count)
            {
               var data = _log.Calculations [i];
               text = $" {data.Input} = {data.Result}";
               text = text.PadRight (maxWidth, ' ');

               var (x, y) = Console.GetCursorPosition ();
               _lines.Add (new (x + indent.Length + 1, y, text));
            }
            else
               text = new (' ', maxWidth);

            Console.WriteLine ($"{indent}│{text}│");
         }

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

         SelectItem ();

         Console.CursorVisible = false;
         string command = DoMenu (out val);

         Console.CursorVisible = true;
         return command;
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

               case ConsoleKey.B:
                  Console.Clear ();
                  return "b";

               case ConsoleKey.E:
                  return "e";

               case ConsoleKey.Enter:
                  val = double.Parse (_log.Calculations [_selectedItem].Result);
                  return "u";
            }
         }
      }

      void SelectItem ()
      {
         _lines [_selectedItem].Select ();
      }

      void DeselectItem ()
      {
         _lines [_selectedItem].Deselect ();
      }

      void SelectPrevItem ()
      {
         if (_selectedItem == 0)
            return;

         DeselectItem ();

         --_selectedItem;

         SelectItem ();

      }

      void SelectNextItem ()
      {
         if (_selectedItem == _lines.Count - 1)
            return;

         DeselectItem ();

         ++_selectedItem;

         SelectItem ();
      }

      void SelectFirstItem ()
      {
      }

      void SelectLastItem ()
      {
      }
   }
}
