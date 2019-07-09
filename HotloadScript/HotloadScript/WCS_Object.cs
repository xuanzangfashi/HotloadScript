using System;
using System.Collections.Generic;
using System.Text;

namespace WCS
{
    class WCS_Object
    {
        public string[] source_in_line;
        public Dictionary<string, int[]> FunctionMap = new Dictionary<string, int[]>();
        public Dictionary<string, string[]> FunctionParams = new Dictionary<string, string[]>();
        public List<string[]> ImportArray = new List<string[]>();
        public Dictionary<string, WCS_ValueData> ValueMap = new Dictionary<string, WCS_ValueData>();
        
        public WCS_Object(string[] src)
        {
            this.source_in_line = src;
        }

        public void CallFunction(string function_name)
        {

        }
    }
}
