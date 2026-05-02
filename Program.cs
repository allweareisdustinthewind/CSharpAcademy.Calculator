
using ProgramLogic;

StateMashine mashine = new ();

for (; ; )
{
   if (!mashine.ProcessCurrentState ())
      break;
}
