using LibCLCC.NET.TextProcessing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

namespace TerminalEmulator
{
    public class shell : TEApp
    {
        CommandLineParser CommandLineParser = new CommandLineParser();
        public override void Init(TECore core)
        {
            base.Init(core);
            __cd = Environment.CurrentDirectory;
            WriteLine("esh, Example SHell");
            WritePrompt();
        }
        public string __cd;
        public void WritePrompt()
        {
            Write(__cd + " $");
        }
        public override void OnGetLine(string s)
        {
            var seg = CommandLineParser.Parse(s , false);
            switch (seg.content)
            {
                case "ls":
                    {
                        DirectoryInfo directoryInfo = new DirectoryInfo(__cd);
                        var dirs = directoryInfo.EnumerateDirectories();
                        foreach (var item in dirs)
                        {
                            WriteLine("d\t" + item.Name);
                        }
                        var files = directoryInfo.EnumerateFiles();
                        foreach (var item in files)
                        {
                            WriteLine("f\t" + item.Name);
                        }
                    }
                    break;
                case "echo":
                    {
                        WriteLine(seg.Next.content);
                    }
                    break;
                case "version":
                    {
                        WriteLine("Version: \x1b[32m1.00\x1b[0m");
                    }
                    break;
                case "clear":
                    {
                        Write("\x1b[J\x1b[h");
                    }
                    break;
                case "cd":
                    {
                        var arg1 = seg.Next.content;
                        var p = Path.Combine(__cd , arg1);
                        if (Directory.Exists(p))
                        {

                            DirectoryInfo directoryInfo = new DirectoryInfo(p);
                            __cd = directoryInfo.FullName;
                        }
                        else
                        {
                            if (Directory.Exists(arg1))
                            {
                                DirectoryInfo absDir = new DirectoryInfo(arg1);
                                __cd = absDir.FullName;

                            }
                            else
                            {
                                WriteLine("Error: No such a directory!");
                            }

                        }
                    }
                    break;
                default:
                    break;
            }
            WritePrompt();
        }
    }

}
