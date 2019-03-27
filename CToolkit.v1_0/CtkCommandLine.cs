using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToolkit.v1_0
{
    public class CtkCommandLine
    {



        public void CmdWrite(string msg, params object[] obj)
        {
            if (msg != null)
            {
                Console.WriteLine();
                Console.WriteLine(msg, obj);
            }
            Console.Write(">");
        }

        public void CommandLine(Action<string> act = null)
        {
            CmdWrite(this.GetType().Name);
            var cmd = "";
            do
            {
                cmd = Console.ReadLine();
                if (act != null) act(cmd);

            } while (string.Compare(cmd, "exit", true) != 0);


        }

    }
}
