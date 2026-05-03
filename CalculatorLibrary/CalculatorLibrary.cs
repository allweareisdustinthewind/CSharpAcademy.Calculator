namespace CalculatorLibrary;

public class Calculator
{
   public Log CalcLog { get; set; } = new ();

   public Calculator ()
   {
      CalcLog.Load ();

      ++CalcLog.UsedCount;
      CalcLog.Save ();
   }

   public string DoOperation (double num1, double num2, string op)
   {
      double val = double.NaN;

      switch (op)
      {
         case "a":
            val = num1 + num2;
            break;

         case "s":
            val = num1 - num2;
            break;

         case "m":
            val = num1 * num2;
            break;

         case "d":
            if (num2 != 0)
               val = num1 / num2;
            break;

         case "sqrt":
            val = Math.Sqrt (num1);
            break;

         case "pow":
            if (num1 != 0 || num2 != 0)
               val = Math.Pow (num1, num2);

            break;

         case "10":
            val = Math.Pow (10, num1);
            break;

         case "e":
            val = Math.Exp (num1);
            break;

         case "sin":
            val = Math.Sin (ToRadian (num1));
            break;

         case "cos":
            val = Math.Cos (ToRadian (num1));
            break;

         case "tg":
            num1 = Reduce (num1);
            if (Math.Abs (num1) != 90)
               val = Math.Tan (ToRadian (num1));
            break;

         case "ctg":
            num1 = Reduce (num1);
            if (Math.Abs (num1) != 0)
               val = 1.0 / Math.Tan (ToRadian (num1));
            break;
      }

      if (double.IsNaN (val) || val == double.NegativeInfinity || val == double.PositiveInfinity)
         return string.Empty;

      // To prevent '-0' in result
      if (Math.Abs (val) == 0)
         val = 0;

      return string.Format ("{0:0.######}", val);
   }

   double ToRadian (double degree)
   {
      return degree * Math.PI / 180.0;
   }

   // Converts degrees > 180 or < -180 in degrees from interval (-180, 180)
   double Reduce (double degree)
   {
      return degree % 180;
   }
}
