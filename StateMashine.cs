
using CalculatorLibrary;
using System.Text.RegularExpressions;

namespace ProgramLogic
{
   public class StateMashine
   {
      delegate void State ();
      State? _currentState;

      int _calcPosX;
      int _calcPosY;

      int _curPosX;
      int _curPosY;

      string? _result;

      double _num1 = double.NaN;
      double _num2 = double.NaN;
      string _operation = string.Empty;

      Calculator _calculator = new ();

      public StateMashine ()
      {
         _currentState = GetFirstOperand;
      }

      public bool ProcessCurrentState ()
      {
         if (_currentState == null)
            return false;

         _currentState ();
         return true;
      }

      void GetFirstOperand ()
      {
         _num1 = double.NaN;
         _result = string.Empty;

         ShowTitle ();
         ShowOperationsForGetOperand ();

         string op = GetUserInput (["v", "e"], out _num1);
         if (!ProcessGetOperand (op))
            return;

         FormatResult ();

         _currentState = GetOperation;
      }

      void GetOperation ()
      {
         _operation = string.Empty;

         ShowTitle ();
         ShowOperations ();

         _operation = GetUserInput (["a", "s", "m", "d", "sqrt", "pow", "10", "e", "sin", "cos", "tg", "ctg"], out double val);
         FormatResult ();

         if (IsOperationNeedSecondOperand ())
            _currentState = GetSecondOperand;
         else
            _currentState = DoCalculation;
      }

      void GetSecondOperand ()
      {
         _num2 = double.NaN;
         ShowTitle ();
         ShowOperationsForGetOperand (false);

         string op = GetUserInput (["v", "e"], out _num2);
         if (!ProcessGetOperand (op, false))
            return;

         FormatResult ();

         _currentState = DoCalculation;
      }

      void DoCalculation ()
      {
         string res = _calculator.DoOperation (_num1, _num2, _operation);
         if (string.IsNullOrEmpty (res))
         {
            Gui.Notify ("This operation will result in a mathematical error.");

            if (IsOperationNeedSecondOperand ())
               _currentState = GetSecondOperand;
            else
               _currentState = GetOperation;

            return;
         }

         _calculator.CalcLog.AddInfo (_result!, res);
         _result += $" = {res}";

         UpdateResult ();

         _currentState = AskToContinue;
      }

      void AskToContinue ()
      {
         Console.CursorVisible = false;
         Console.WriteLine ("\n  Press 'e' to exit the application, or press any other key to continue.");
         if (Console.ReadKey (true).Key == ConsoleKey.E)
            _currentState = null;
         else
         {
            Console.CursorVisible = true;
            _currentState = GetFirstOperand;
         }
      }

      void ShowTitle (bool showResult = true)
      {
         Console.SetCursorPosition (0, 0);
         Console.Clear ();

         // Display title as the C# console calculator app.
         Gui.WriteColorText ("\n     Calculator\n", ConsoleColor.Blue);

         string text = $"  (launched {_calculator.CalcLog.UsedCount - 1} times)";
         Console.WriteLine (text);

         string line = new ('─', text.Length - 2);
         Console.WriteLine ($"  {line}\n");

         (_calcPosX, _calcPosY) = Console.GetCursorPosition ();

         if (showResult)
         {
            Gui.WriteColorText ("Result: ", ConsoleColor.Green);
            Console.WriteLine (_result);
            Console.WriteLine ();
         }
      }

      void ShowOperationsForGetOperand (bool isFirstOperand = true)
      {
         string text = isFirstOperand ? "first" : "second";
         Console.WriteLine ($"Choose an operator from the following list or type {text} number to start calculation:");

         Gui.WriteColorText ("\tv", ConsoleColor.Cyan);
         Console.WriteLine (" - view results of previous calculations");

         Gui.WriteColorText ("\te", ConsoleColor.Cyan);
         Console.WriteLine (" - exit program");

         Gui.WriteColorText (">> ", ConsoleColor.Yellow);
         (_curPosX, _curPosY) = Console.GetCursorPosition ();
      }

      void ShowOperations ()
      {
         Tuple<string, string> [] operations =
            [
               Tuple.Create ("\ta", " - Add"),
               Tuple.Create ("       sqrt", " - Square root"),
               Tuple.Create ("  sin", " - Sine"),
               Tuple.Create ("\ts", " - Subtract"),
               Tuple.Create ("  pow", "  - x^y"),
               Tuple.Create ("          cos", " - Cosine"),
               Tuple.Create ("\tm", " - Multiply"),
               Tuple.Create ("  10", "   - 10^x"),
               Tuple.Create ("         tg", "  - Tangent"),
               Tuple.Create ("\td", " - Divide"),
               Tuple.Create ("    e", "    - e^x"),
               Tuple.Create ("          ctg", " - Cotangent")
            ];

         Console.WriteLine ("Choose an operator from the following list:");
         for (int i = 0; i < operations.Length; ++i)
         {
            var (operation, description) = operations [i];
            Gui.WriteColorText (operation, ConsoleColor.Cyan);
            Console.Write (description);

            if ((i + 1) % 3 == 0)
               Console.WriteLine ();
         }

         Gui.WriteColorText (">> ", ConsoleColor.Yellow);
         (_curPosX, _curPosY) = Console.GetCursorPosition ();
      }

      string GetUserInput (string [] operations, out double number)
      {
         for (; ; )
         {
            number = double.NaN;
            if (operations == null || operations.Length <= 0)
               return string.Empty;

            string? userInput = Console.ReadLine ();
            if (string.IsNullOrEmpty (userInput))
               return string.Empty;

            var (x, y) = Console.GetCursorPosition ();
            Console.SetCursorPosition (_curPosX, _curPosY);
            Console.Write (new string (' ', userInput.Length));
            Console.SetCursorPosition (x, y);

            userInput = userInput.Trim ().ToLower ();
            string regex = string.Join ("|", operations);

            try
            {
               var match = Regex.Match ($"[{regex}]", userInput);
               if (!string.IsNullOrEmpty (match.Value))
                  return match.Value;

               number = double.Parse (userInput);
            }
            catch
            {
               Gui.Notify (" Error: Unrecognized input. ");

               Console.SetCursorPosition (_curPosX, _curPosY);
               Console.Write (new string (' ', userInput.Length));
               Console.SetCursorPosition (_curPosX, _curPosY);

               continue;
            }

            break;
         }

         return string.Empty;
      }

      bool ProcessGetOperand (string op, bool isFirstOperand = true)
      {
         if (op == "e")
         {
            _currentState = null;
            return false;
         }

         if (op == "v")
         {
            ShowTitle (false);

            LogList list = new (_calculator.CalcLog);

            double val = 0;
            string command = list.Show (out val);

            switch (command)
            {
               case "b":
                  return false;

               case "e":
                  _currentState = null;
                  return false;

               case "u":
                  _result += string.Format ("{0:0.##}", val);
                  if (isFirstOperand)
                     _num1 = val;
                  else
                     _num2 = val;

                  UpdateResult ();

                  break;
            }
         }

         return true;
      }

      void FormatResult ()
      {
         _result = string.Format ("{0:0.##}", _num1);

         switch (_operation)
         {
            case "a":
               _result += " + ";
               break;

            case "s":
               _result += " - ";
               break;

            case "m":
               _result += " x ";
               break;

            case "d":
               _result += " / ";
               break;

            case "sqrt":
               _result = $"sqrt ({_result})";
               break;

            case "pow":
               _result += " ^ ";
               break;

            case "10":
               _result = $"10 ^ {_result}";
               break;

            case "e":
               _result = $"e ^ {_result}";
               break;

            case "sin":
               _result = $"sin ({_result}°)";
               break;

            case "cos":
               _result = $"cos ({_result}°)";
               break;

            case "tg":
               _result = $"tg ({_result}°)";
               break;

            case "ctg":
               _result = $"ctg ({_result}°)";
               break;
         }

         if (!double.IsNaN (_num2))
            _result += string.Format ("{0:0.##}", _num2);

         UpdateResult ();
      }

      void UpdateResult ()
      {
         var (x, y) = Console.GetCursorPosition ();
         Console.SetCursorPosition (_calcPosX, _calcPosY);

         Gui.WriteColorText ("Result: ", ConsoleColor.Green);
         Console.Write (_result);

         Console.SetCursorPosition (x, y);
      }

      bool IsOperationNeedSecondOperand ()
      {
         return _operation == "a" || _operation == "s" || _operation == "m" || _operation == "d" || _operation == "pow";
      }
   }
}
