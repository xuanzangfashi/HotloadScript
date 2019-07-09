using System;

namespace HotloadScript
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            WCS.WCS_Parser.InitWCS_Obj("./test.js");
        }
    }
}
