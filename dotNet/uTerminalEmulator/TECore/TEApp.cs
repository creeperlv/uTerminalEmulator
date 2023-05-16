using UnityEditorInternal;
using UnityEngine;

namespace TerminalEmulator
{
    public class TEApp : MonoBehaviour, TEHost
    {
        int InputPos = 0;
        int InputLen = 0;
        string input = "";
        bool ANSI = false;
        TECore core;
        public virtual void Init(TECore core)
        {
            this.core = core;
        }
        public virtual void CtrlC()
        {

        }
        bool ANSI_SEQ = false;
        string ansi_seq = "";
        public void OnObtainChar(char item)
        {
            if (item == '\x1b')
            {
                ansi_seq = "";
                ANSI = true;
                ANSI_SEQ = false;
                return;
            }
            if (ANSI)
            {
                if (item == '[')
                {
                    ANSI_SEQ = true;
                    return;
                }
                switch (item)
                {
                    case '~':
                        {
                            ANSI = false;
                            var keys = ansi_seq.Split(';');
                            {
                                if (int.TryParse(keys [ 0 ] , out var k))
                                {
                                    int Mod = 1;
                                    if (keys.Length > 1)
                                    {
                                        if (!int.TryParse(keys [ 1 ] , out Mod)) Mod = 1;
                                    }
                                    switch (k)
                                    {
                                        case 3:
                                            {
                                                if (InputPos < InputLen)
                                                {
                                                    input = input.Remove(InputPos , 1);
                                                    InputLen--;
                                                    Write($"\x1b[C");
                                                    Write(input [ (InputPos-1).. ]);
                                                    Write("  ");
                                                    Write($"\x1b[{InputLen - InputPos+2}C");
                                                }
                                            }
                                            break;
                                        case 'c':
                                            {
                                                if (Mod == Constants.Ctrl)
                                                {
                                                    CtrlC();
                                                }
                                            }
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }
                            return;
                        }
                        break;
                    case 'A':
                    case 'B':
                    case 'C':
                        {
                            ANSI = false;
                            InputPos -= 1;
                            if (InputPos < 0)
                            {
                                InputPos = 0;
                            }
                            else
                            {
                                Write("\x1b[C");
                            }
                        }
                        break;
                    case 'D':
                        {
                            ANSI = false;
                            InputPos += 1;
                            if (InputPos > InputLen)
                            {
                                InputPos = InputLen;
                            }
                            else
                            {
                                Write("\x1b[D");
                            }
                        }
                        break;
                    default:
                        ansi_seq += item;
                        return;
                        break;
                }
                ANSI = false;
                return;
            }
            if (item == '\b')
            {
                if (InputPos > 0)
                {
                    Write("\x1b[C");

                    //Buffer [ CurrentPosition.x , CurrentPosition.y ] = ' ';
                    input = input.Remove(InputPos - 1 , 1);
                    InputPos--;
                    InputLen--;
                    //var cp = CurrentPosition;
                    //Redraw(CurrentPosition.x , CurrentPosition.y);
                    Write(input [ (InputPos).. ]);
                    Write(" ");
                    var L = InputLen - InputPos + 1;
                    Write($"\x1b[{L}C");

                    //CurrentPosition = cp;
                    //SubmitBuffer();
                }
            }
            else if (item == '\n' || item == '\r')
            {
                Write("\n");
                this.OnGetLine(input);
                input = "";
                InputLen = 0;
                InputPos = 0;
            }
            else
            {
                InputPos++;
                InputLen++;
                if (InputPos != InputLen)
                    input = input.Insert(InputPos - 1 , item + "");
                else input += item;
                Write(item + "");
                if (InputPos != InputLen)
                {
                    //var cp = CurrentPosition;
                    Write(input [ (InputPos).. ]);
                    Write($"\x1b[{InputLen - InputPos}C");
                    //CurrentPosition = cp;
                    //SubmitBuffer();
                }
            }


        }
        public virtual void OnGetLine(string str)
        {

        }

        public void Write(string str)
        {
            core.Write(str);
        }

        public void WriteLine(string str)
        {
            core.WriteLine(str);
        }
    }
}
