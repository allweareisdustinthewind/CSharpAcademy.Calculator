
using Newtonsoft.Json;

namespace CalculatorLibrary;

// Date of one calcualtion to store in log 
public class CalcData
{ 
   public string Input { get; set; } // Input in format "operand1 opeartion operand2", i.e. "2 x 3"
   public string Result { get; set; }

   public CalcData (string input, string result)
   {
      Input = input;
      Result = result;
   }
}

public class Log
{
   public int UsedCount { get; set; } // Count of launched times

   public List<CalcData> Calculations { get; set; } = new ();

   readonly string nameLogFile = "log.json";

   public void Load ()
   {
      try
      {
         using (StreamReader reader = new (nameLogFile))
         {
            string json = reader.ReadToEnd ();

            Log? log = JsonConvert.DeserializeObject<Log> (json);
            if (log != null)
            {
               UsedCount = log.UsedCount;
               Calculations = log.Calculations;
            }
         }
      }
      catch (Exception)
      {
         // No information about previous calculations available
      }
   }

   public void Save ()
   {
      try
      {
         string json = JsonConvert.SerializeObject (this, formatting: Formatting.Indented);
         using (StreamWriter writer = File.CreateText (nameLogFile))
         {
            writer.Write (json);
         }
      }
      catch (Exception)
      {
      }
   }

   public void AddInfo (string input, string result)
   {
      Calculations.Add (new (input, result));
      Save ();
   }

   public void Delete (int index)
   {
      Calculations.RemoveAt (index);
      Save ();
   }

   public void Reset ()
   {
      Calculations.Clear ();
      Save ();
   }
}
