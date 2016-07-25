﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Roslyn.Compilers;
using Roslyn.Compilers.Common;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;
using System.IO;

namespace ExceptionAnalysis
{
    /// <summary>
    /// Set up Logger class
    /// </summary>
    public static class Logger
    {
        static string LogFileName;
        static StreamWriter LogWriter;
        public static void Initialize()
        {
            LogFileName = IOFile.CompleteFileName(
                DateTime.Today.Date.ToShortDateString().Replace("/", "") + ".log");
            LogWriter = File.AppendText(LogFileName);
            Log("-------------------------------------------------------");
            Log("-------------------------------------------------------");
            Log("New Task.");
        }
        public static void Log(String message)
        {
            Console.WriteLine("[{0}] {1}", DateTime.Now.ToString(), message);
            LogWriter.WriteLine("[{0}] {1}", DateTime.Now.ToString(), message);
            LogWriter.Flush();
        }
        public static void Log(int number)
        {
            Log(number.ToString());
        }
        public static void Log(Exception e)
        {
            Console.WriteLine("[{0}]", DateTime.Now.ToString());
            LogWriter.WriteLine("[{0}]", DateTime.Now.ToString());
            Console.WriteLine(e);
            LogWriter.WriteLine(e);
            LogWriter.Flush();
        }
        public static void Close()
        {
            LogWriter.Close();
        }
    }

    /// <summary>
    /// Set up Config class to process the config file
    /// </summary>
    static class Config
    {
        static public String[] LogMethods { get; private set; } // "WriteError"
        static public String[] NotLogMethods { get; private set; } // "TraceUtil.If"
        static public int LogLevelArgPos { get; private set; } // ="2"
        static public int AssertConditionIndex { get; private set; }
        static public bool Orthogonal;

        static public void Load(String FileName)
        {
            StreamReader Input = new StreamReader(FileName);

            try
            {
                LogMethods = GetOneParameter(Input).Split(',');
                NotLogMethods = GetOneParameter(Input).Split(',');
                LogLevelArgPos = Convert.ToInt32(GetOneParameter(Input));
                String temp = GetOneParameter(Input);
                if (temp == "O") Orthogonal = true;
                else if (temp == "N") Orthogonal = false;
                else throw new IOException();
                AssertConditionIndex = Convert.ToInt32(GetOneParameter(Input));
            }
            catch
            {
                Logger.Log("Illegal Configure File Format.");
            }
            finally
            {
                Input.Close();
            }
        }

        static private String GetOneParameter(StreamReader Input)
        {
            String Parameter = Input.ReadLine();
            Parameter = Parameter.Split('%')[0];
            return Parameter;
        }
    }

    /// <summary>
    /// File name processing
    /// </summary>
    static class IOFile
    {
        public static String FolderPath;
       
        public static String CompleteFileName(String tail)
        {
            return (FolderPath + "\\" + FolderPath.Split('\\').Last() + "_" + tail);
        }

        static public String DeleteSpace(String str)
        {
            if (str == null || str == "") return str;

            String updatedStr = str.Replace("\n", "").Replace("\r", "").Replace("\t", "")
            .Replace("    ", " ").Replace("    ", " ").Replace("   ", " ")
            .Replace("  ", " ");

            return updatedStr;
        }

        static public String MethodNameExtraction(String str)
        {
            try
            {
                String methodName = str;
                try
                {                  
                    methodName = Regex.Replace(methodName, "<.*>", "");
                    methodName = Regex.Replace(methodName, "{.*}", "");
                    methodName = Regex.Replace(methodName, "\\(.*\\)", "");
                    if (methodName.IndexOf('(') != -1)
                    {
                        methodName = methodName.Split('(').First();
                    }
                    methodName = DeleteSpace(methodName);
                    methodName = methodName.Replace(" ", "");
                }
                catch { }
                return methodName;
            }
            catch
            {
                return null;
            }
        }

        static public String ShortMethodNameExtraction(String str)
        {
            try
            {
                String methodName = null;
                MatchCollection allMatches = Regex.Matches(str, "\\.[a-zA-Z0-9\\s]+\\(");
                if (allMatches.Count > 1)
                {
                    methodName = Regex.Replace(allMatches[allMatches.Count - 1].ToString(), "[\\.(\\s]", "");
                }
                else
                {
                    methodName = MethodNameExtraction(str);
                }
                if (methodName.IndexOf('.') != -1)
                {
                    methodName = methodName.Split('.').Last();
                }
                else if (methodName == null)
                {
                    Logger.Log("An API call cannot be extracted by the ShortMethodNameExtraction function:\n" + str);
                }

                return methodName;
            }
            catch
            {
                return null;
            }
        }
    }

}