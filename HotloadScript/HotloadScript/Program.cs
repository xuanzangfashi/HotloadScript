using System;
using System.Text.RegularExpressions;

namespace HotloadScript
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            Regex function_rx = new Regex(@"[^\d]+[\w]+[(]+[\S]+[)]");
            var str = function_rx.Match("garfwef0console.log(str.toString(),98,111.2,avc)afcvwedqwesd").Value;
            Console.WriteLine(str);


            //WCS.WCS_Parser.InitWCS_Obj("./test.js");
        }
    }
}
