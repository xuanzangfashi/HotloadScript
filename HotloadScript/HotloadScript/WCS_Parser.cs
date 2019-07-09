using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace WCS
{
    public struct WCS_ValueData
    {
        public object data;
        public Type type;
        public string assembly;
    }
    static class WCS_Parser
    {
        public static WCS_Object InitWCS_Obj(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open);
            int read_count = 0;
            List<byte> filebytes = new List<byte>();
            byte[] buffer = null;
            do
            {
                if (buffer != null)
                {
                    filebytes.AddRange(buffer);
                }
                buffer = new byte[255];
                read_count = fs.Read(buffer, 0, 255);

            } while (read_count > 0);

            fs.Close();
            fs = null;

            byte[] file_byte_array = filebytes.ToArray();
            string src = Encoding.UTF8.GetString(file_byte_array);
            string[] src_lines = src.Split("\r\n");


            bool headerDone = false;

            Dictionary<string, int[]> tmp_functionMap = new Dictionary<string, int[]>();
            Dictionary<string, string[]> tmp_functionParams = new Dictionary<string, string[]>();
            List<string[]> tmp_importArray = new List<string[]>();
            Dictionary<string, WCS_ValueData> tmp_valueMap = new Dictionary<string, WCS_ValueData>();

            for (int i = 0; i < src_lines.Length; i++)
            {
                src_lines[i] = src_lines[i].Replace("\t", "");
                src_lines[i] = src_lines[i].Replace("\0", "");

                //find functions
                int functionStartIndex = src_lines[i].IndexOf("function");
                if (functionStartIndex != -1)
                {
                    headerDone = true;
                    string[] functionNameAndParams = src_lines[i].Substring(functionStartIndex + 9).Split("(");
                    string functionNameStr = functionNameAndParams[0];
                    string tmpParamsLine = functionNameAndParams[1];
                    tmpParamsLine = tmpParamsLine.Remove(tmpParamsLine.Length - 1);
                    string[] param = tmpParamsLine.Split(",");
                    tmp_functionParams.Add(functionNameStr, param);
                    int functionEndIndex = 0;
                    int bracesBgein = 0;
                    for (int j = i; j < src_lines.Length; j++)
                    {
                        src_lines[j] = src_lines[j].Replace("\t", "");
                        src_lines[j] = src_lines[j].Replace("\0", "");
                        if (src_lines[j] == "}")
                        {
                            bracesBgein--;
                            if (bracesBgein == 0)
                            {
                                functionEndIndex = j;
                                break;
                            }
                        }
                        else if(src_lines[j] == "{")
                        {
                            bracesBgein++;
                        }
                    }
                    int[] seArray = new int[2];
                    seArray[0] = i + 2;
                    seArray[1] = functionEndIndex - 1;
                    tmp_functionMap.Add(functionNameStr, seArray);
                }

                if(!headerDone)
                {
                    bool importsDone = false;
                    //find imports
                    
                    int importIndex = src_lines[i].IndexOf("import");
                    if (importIndex != -1)
                    {
                        importsDone = true;
                        string importStr = src_lines[i].Split(" ")[1];
                        tmp_importArray.Add(importStr.Split("."));
                    }

                    if (!importsDone)
                    {
                        //find vars
                        int varIndex = src_lines[i].IndexOf("var");
                        if(varIndex != -1)
                        {
                            string[] varStrs = src_lines[i].Split(" ");
                            string varSubStr = "";

                            int varStrsIndex = 0;
                            foreach(string j in varStrs)
                            {
                                if (varStrsIndex == 0)
                                {
                                    varStrsIndex++;
                                    continue;
                                }
                                varSubStr = varSubStr + j;
                                varStrsIndex++;
                            }
                            string[] varSubStrs = varSubStr.Split("=");
                            string varName = varSubStrs[0];
                            string tmpVarValue = varSubStrs[1];
                            Type varType = null;
                            object varValue = null;
                            if(tmpVarValue.IndexOf("0") != -1 || 
                                tmpVarValue.IndexOf("1") != -1 || 
                                tmpVarValue.IndexOf("2") != -1 || 
                                tmpVarValue.IndexOf("3") != -1 || 
                                tmpVarValue.IndexOf("4") != -1 || 
                                tmpVarValue.IndexOf("5") != -1 || 
                                tmpVarValue.IndexOf("6") != -1 ||
                                tmpVarValue.IndexOf("7") != -1 ||
                                tmpVarValue.IndexOf("8") != -1 ||
                                tmpVarValue.IndexOf("9") != -1)
                            {
                                if (tmpVarValue.IndexOf(".") != -1)
                                {
                                    varType = typeof(float);
                                    varValue = float.Parse(tmpVarValue);
                                    goto Out;
                                }
                                varType = typeof(int);
                                varValue = int.Parse(tmpVarValue);
                            }
                            else if(tmpVarValue.IndexOf("\"") != -1)
                            {
                                varType = typeof(string);
                                tmpVarValue = tmpVarValue.Remove(0, 1);
                                tmpVarValue = tmpVarValue.Remove(tmpVarValue.Length - 1);
                                varValue = tmpVarValue;
                            }
                            else if (tmpVarValue.IndexOf("objectOf") != -1)
                            {
                                tmpVarValue = tmpVarValue.Remove(tmpVarValue.Length - 1);
                                tmpVarValue = tmpVarValue.Remove(0, 1);
                                string[] typeValue = tmpVarValue.Split(",");
                                string vType = typeValue[0];
                                string vValue = typeValue[1];

                                int typeStartIndex = vType.LastIndexOf(".");
                                char[] vSubType = new char[50];
                                vType.CopyTo(typeStartIndex+1, vSubType, 0, vType.Length - (typeStartIndex +2));
                                vType.Remove(typeStartIndex);
                                string vSubTypeStr = new string(vSubType);
                                Assembly assembly = Assembly.Load(vType);
                                Type type = assembly.GetType(vSubTypeStr);
                                varValue = type.Assembly.CreateInstance(type.ToString());
                                varType = type;
                            }
                            Out:;

                            WCS_ValueData tmpdata = new WCS_ValueData();
                            tmpdata.data = varValue;
                            tmpdata.type = varType;
                            tmp_valueMap.Add(varName, tmpdata);
                        }
                    }
                }
            }

            WCS_Object obj = new WCS_Object(src_lines);
            return obj;
        }

        static void ParserAndRunFunction(WCS_Object obj, string name,object[] paramInputs)
        {
            int[] function_start_end = obj.FunctionMap[name];
            string[] function_params = obj.FunctionParams[name];
            for(int i = function_start_end[0]; i <= function_start_end[1]; i++)
            {
                //call functions
                Regex function_rx = new Regex(@"[^\d]+[\w]+[(]+[\S]+[)]");

                MatchCollection function_mc = function_rx.Matches(obj.source_in_line[i]);
            }
        }
    }
}
