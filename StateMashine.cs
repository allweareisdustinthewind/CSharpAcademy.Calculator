
using CalculatorLibrary;
using System.Text.RegularExpressions;

namespace ProgramLogic;

//
//  State-mashine for calculator. It has all logic to move the programm  
//  from one determined state to another
//

public class StateMashine
{
   delegate void State ();
   State? _nextState;

   // Position in console to show result of calculation
   int _resPosX;
   int _resPosY;

   // Position of current input from user
   int _inputPosX;
   int _inputPosY;

   string? _result;

   double _num1 = double.NaN;
   double _num2 = double.NaN;
   string _operation = string.Empty;
   string? _resCalculation; 

   Calculator _calculator = new ();

   public StateMashine ()
   {
      _nextState = GetFirstOperand;
   }

   // Return "false" means exit from program
   public bool ProcessCurrentState ()
   {
      if (_nextState == null)
         return false;

      _nextState ();
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

      // Convert possible "-0" to "0"
      if (Math.Abs (_num1) == 0)
         _num1 = 0;

      FormatResult ();

      _nextState = GetOperation;
   }

   void GetOperation ()
   {
      _operation = string.Empty;

      ShowTitle ();
      ShowOperations ();
      FormatResult ();

      _operation = GetUserInput (["a", "s", "m", "d", "sqrt", "pow", "10", "e", "sin", "cos", "tg", "ctg"], out double val);
      FormatResult ();

      if (IsOperationNeedSecondOperand ())
         _nextState = GetSecondOperand;
      else
         _nextState = DoCalculation;
   }

   void GetSecondOperand ()
   {
      _num2 = double.NaN;
      ShowTitle ();
      ShowOperationsForGetOperand (false);

      string op = GetUserInput (["v", "e"], out _num2);
      if (!ProcessGetOperand (op, false))
         return;

      // Convert possible "-0" to "0"
      if (Math.Abs (_num2) == 0)
         _num2 = 0;

      FormatResult ();

      _nextState = DoCalculation;
   }

   void DoCalculation ()
   {
      _resCalculation = _calculator.DoOperation (_num1, _num2, _operation);
      if (string.IsNullOrEmpty (_resCalculation))
      {
         Gui.Notify ("This operation will result in a mathematical error.");

         if (IsOperationNeedSecondOperand ())
            _nextState = GetSecondOperand;
         else
            _nextState = GetOperation;

         return;
      }

      _calculator.CalcLog.AddInfo (_result!, _resCalculation);

      FormatResult ();

      _nextState = AskToContinue;
   }

   void AskToContinue ()
   {
      ShowTitle ();

      Console.CursorVisible = false;
      Console.WriteLine ("\nPress 'e' to exit the application, or press any other key to continue.");
      if (Console.ReadKey (true).Key == ConsoleKey.E)
         _nextState = null;
      else
      {
         Console.CursorVisible = true;
         _nextState = GetFirstOperand;

         _num1 = double.NaN;
         _num2 = double.NaN;
         _operation = string.Empty;
         _resCalculation = string.Empty;
      }
   }

   void ShowTitle (bool showResult = true)
   {
      Console.SetCursorPosition (0, 0);
      Console.Clear ();

      Gui.WriteColorText ("\n     Calculator\n", ConsoleColor.Blue);

      string text = $"  (launched {_calculator.CalcLog.UsedCount - 1} times)";
      Console.WriteLine (text);

      string line = new ('─', text.Length - 2);
      Console.WriteLine ($"  {line}\n");

      (_resPosX, _resPosY) = Console.GetCursorPosition ();

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
      (_inputPosX, _inputPosY) = Console.GetCursorPosition ();
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
      (_inputPosX, _inputPosY) = Console.GetCursorPosition ();
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

         // Clear inputed data on console (we have it in "userInput" parameter and won't see anymore)
         var (x, y) = Console.GetCursorPosition ();
         Console.SetCursorPosition (_inputPosX, _inputPosY);
         Console.Write (new string (' ', userInput.Length));
         Console.SetCursorPosition (x, y);

         userInput = userInput.Trim ().ToLower ();
         string regex = string.Join ("|", operations);

         // Regex.Match can throw exeptions for instance by input of symbols of unsupported codepage,
         // therefore keep it i ntry / catch
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
            Console.SetCursorPosition (_inputPosX, _inputPosY);

            continue;
         }

         break;
      }

      return string.Empty;
   }

   // Process commands "e" (exit app) and "v" (view list of previous calculations)
   bool ProcessGetOperand (string op, bool isFirstOperand = true)
   {
      // Exit program?
      if (op == "e")
      {
         _nextState = null;
         return false;
      }

      // View list of previous calculations?
      if (op == "v")
      {
         ShowTitle (false);

         LogList list = new (_calculator.CalcLog);

         double val = 0;
         string command = list.Show (out val);

         switch (command)
         {
            case "b": // Back to calculations (return "false" means "don't change actual program's state)
               return false;

            case "e": // Exit app
               _nextState = null;
               return false;

            case "u": // Use result of previous calculation
               _result += string.Format ("{0:0.######}", val);
               if (isFirstOperand)
                  _num1 = val;
               else
                  _num2 = val;

               UpdateResult ();
               break;

            default:
               return false;
         }
      }

      return true;
   }

   // Creates full string of actual calculation and display it to console
   void FormatResult ()
   {
      // Clear pervious calculation on console
      if (_result?.Length > 0)
      {
         _result = new string (' ', _result.Length);
         UpdateResult ();
      }

      // Append first operand
      _result = string.Format ("{0:0.######}", _num1);

      // Append operation
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
            if (_num1 < 0)
               _result = $"10 ^ ({_result})";
            else
               _result = $"10 ^ {_result}";
            break;

         case "e":
            if (_num1 < 0)
               _result = $"e ^ ({_result})";
            else
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

      // Append second operand if present
      if (!double.IsNaN (_num2))
      {
         if (_num2 < 0)
            _result += string.Format ("({0:0.######})", _num2);
         else
            _result += string.Format ("{0:0.######}", _num2);
      }

      // Append result of calculation
      if (!string.IsNullOrEmpty (_resCalculation))
         _result += $" = {_resCalculation}";

      UpdateResult ();
   }

   void UpdateResult ()
   {
      var (x, y) = Console.GetCursorPosition ();
      Console.SetCursorPosition (_resPosX, _resPosY);

      Gui.WriteColorText ("Result: ", ConsoleColor.Green);
      Console.Write (_result);

      Console.SetCursorPosition (x, y);
   }

   bool IsOperationNeedSecondOperand ()
   {
      return _operation == "a" || _operation == "s" || _operation == "m" || _operation == "d" || _operation == "pow";
   }
}
