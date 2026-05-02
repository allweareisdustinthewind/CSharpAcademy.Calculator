using System.ComponentModel.DataAnnotations;
using System.Data;

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

      public void Display ()
      {
         Console.SetCursorPosition (PosX, PosY);
         Console.Write (Text);
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
      int _indexFrom;
      int _posYMin;
      int _posYMax;

      readonly int _maxLineCount = 8;
      readonly int _maxWidth = 40;

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
         SelectItem ();

         Console.CursorVisible = false;
         string command = DoMenu (out val);

         Console.CursorVisible = true;
         return command;
      }

      void Fill ()
      {
         if (_lines.Count <= 0)
            return;

         int count = 0;
         int x = _lines [_indexFrom].PosX;
         int y = _lines [_indexFrom].PosY;

         for (int i = _indexFrom; i < _lines.Count; ++i, ++y)
         {
            _lines [i].Display ();
            if (++count >= _maxLineCount)
               break;
         }

         if (count >= _maxLineCount)
            return;

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

               case ConsoleKey.B:
                  Console.Clear ();
                  return "b";

               case ConsoleKey.E:
                  return "e";

               case ConsoleKey.Enter:
                  val = double.Parse (_log.Calculations [_selectedItem].Result);
                  return "u";

               case ConsoleKey.Delete:
                  Delete ();
                  if (_lines.Count <= 0)
                     return "r";

                  break;

               case ConsoleKey.R:
                  Reset ();
                  return "r";
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

         if (_lines [--_selectedItem].PosY < _posYMin)
            ScrollDown ();

         SelectItem ();

      }

      void SelectNextItem ()
      {
         if (_selectedItem == _lines.Count - 1)
            return;

         DeselectItem ();

         if (_lines [++_selectedItem].PosY >= _posYMax)
            ScrollUp ();

         SelectItem ();
      }

      void SelectFirstItem ()
      {
         if (_lines.Count > _maxLineCount)
         {
            int y = _posYMin;

            foreach (var data in _lines)
               data.PosY = y++;
         }

         _selectedItem = 0;
         _indexFrom = 0;

         Fill ();
         SelectItem ();
      }

      void SelectLastItem ()
      {
         if (_lines.Count > _maxLineCount)
         {
            int y = _posYMax - 1;
            for (int i = _lines.Count - 1; i >= 0; --i)
               _lines [i].PosY = y--;

            _indexFrom = _lines.Count - _maxLineCount;
         }

         _selectedItem = _lines.Count - 1;

         Fill ();
         SelectItem ();
      }

      void ScrollUp ()
      {
         ++_indexFrom;
         foreach (var data in _lines)
            --data.PosY;

         Fill ();
      }

      void ScrollDown ()
      {
         --_indexFrom;

         foreach (var data in _lines)
            ++data.PosY;

         Fill ();
      }

      void Delete ()
      {
         for (int i = _selectedItem + 1; i < _lines.Count; ++i)
            --_lines [i].PosY;

         _lines.RemoveAt (_selectedItem);
         _log.Delete (_selectedItem);

         if (_selectedItem >= _lines.Count)
            _selectedItem = _lines.Count - 1;

         Fill ();
         SelectItem ();
      }

      void Reset ()
      {
         _lines.Clear ();
         _log.Reset ();
      }
   }
}
