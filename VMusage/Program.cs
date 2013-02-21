using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;

namespace VMusage
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [MTAThread]
        static void Main(string[] args)
        {
            if (args.Length == 0)
                Application.Run(new Form1());
            else if (args.Length == 1)
            {
                int iTimeout = Convert.ToUInt16(args[0]);
                if(iTimeout>0)
                    Application.Run(new Form1(iTimeout));
                else
                    Application.Run(new Form1());
            }
            else if (args.Length == 2)
            {
                int iTimeout = Convert.ToUInt16(args[0]);
                if (iTimeout <= 0)
                    iTimeout = 3;
                if(args[1].EndsWith("hide",StringComparison.CurrentCultureIgnoreCase))
                    Application.Run(new Form1(iTimeout, true));
                else
                    Application.Run(new Form1(iTimeout, false));
            }
        }
    }
}