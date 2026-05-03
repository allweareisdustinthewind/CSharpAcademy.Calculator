namespace CalculatorLibrary;
public class Gui
{
   public static void WriteColorText (string text, ConsoleColor colorFoderground, int colorBackground = -1)
   {
      ConsoleColor curColorForeground = Console.ForegroundColor;
      ConsoleColor curColorBackground = Console.BackgroundColor;

      Console.ForegroundColor = colorFoderground;
      if (colorBackground >= 0)
         Console.BackgroundColor = (ConsoleColor) colorBackground;

      Console.Write (text);

      Console.ForegroundColor = curColorForeground;
      Console.BackgroundColor = curColorBackground;
   }

   // Notify about some exeption or give some feedback on console. The text will be dissapeared after 2 seconds
   public static void Notify (string notification, ConsoleColor colorFoderground = ConsoleColor.White, ConsoleColor colorBackground = ConsoleColor.DarkRed)
   {
      var (x, y) = Console.GetCursorPosition ();
      Console.CursorVisible = false;

      WriteColorText (notification, colorFoderground, (int) colorBackground);
      System.Threading.Thread.Sleep (2000);

      Console.SetCursorPosition (x, y);
      Console.Write (new string (' ', notification.Length));
      Console.SetCursorPosition (x, y);
      Console.CursorVisible = true;
   }
}
