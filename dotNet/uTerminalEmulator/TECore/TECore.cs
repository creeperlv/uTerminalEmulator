using LibCLCC.NET.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Xml;
using UnityEngine;
namespace TerminalEmulator
{
    public class TECore :
        MonoBehaviour
    {
        [Header("Terminal Width")]
        public int Width;
        [Header("Terminal Height")]
        public int Height;
        [Header("Character Width")]
        public int CW;
        [Header("Character Height")]
        public int CH;
        [Header("Texture Grid Width")]
        public int TGW;
        [Header("Texture Grid Left")]
        public int TGL;
        [Header("Tab Size")]
        public int TabSize = 4;
        int SWidth;
        int SHeight;
        public int CharMapBlockSize;
        public int CharMapBlockW;
        public int CharMapBlockH;
        public KVDict<int , Texture2D> CharMap;
        public Dictionary<int , Texture2D> CharMapDict;
        public char [ , ] Buffer;
        public Texture2D TEBuffer;
        [Header("Helper of applying the texture")]
        public List<TextureApplier> textureApplier;
        public int BufferUpdatePerFrame = 10;
        public int FG;
        public int DefaultFG;
        public float Blink;
        public int BG;
        public int DefaultBG;
        public Color CurrentBG;
        public Color CurrentFG;
        public Dictionary<int , Color> BGPlatte;
        public Dictionary<int , Color> FGPlatte;
        public KVDict<int , Color> FGColorPlatte;
        public KVDict<int , Color> BGColorPlatte;
        public TEApp PreattachedApp;
        public bool PauseKeyboardInput = true;
        public bool PauseCursorUpdate = true;
        Sprite SBuffer;
        Queue<string> Commits = new Queue<string>();
        // Start is called before the first frame update
        void Start()
        {
            Buffer = new char [ Width , Height ];
            SWidth = Width * CW;
            SHeight = Height * CH;
            FG = DefaultFG;
            BG = DefaultBG;
            if (TEBuffer == null)
                TEBuffer = new Texture2D(SWidth , SHeight);
            CharMapDict = CharMap.ToDictionary();
            if (textureApplier != null)
                foreach (var item in textureApplier)
                {
                    item.ApplyTexture(TEBuffer);
                }
            BGPlatte = BGColorPlatte.ToDictionary();
            FGPlatte = FGColorPlatte.ToDictionary();
            CurrentBG = BGPlatte [ BG ];
            CurrentFG = FGPlatte [ FG ];
            StartCoroutine(Listen());
            Clear();
            if (PreattachedApp != null)
            {
                SetHost(PreattachedApp);
            }
        }
        public void Clear()
        {

            for (int x = 0 ; x < Width ; x++)
            {
                for (int y = 0 ; y < Height ; y++)
                {
                    Buffer [ x , y ] = ' ';
                    DrawChar(' ' , x , y);
                }
            }
            SubmitBuffer();
        }
        bool WillListen = true;
        public void OnDestroy()
        {
            WillListen = false;
        }
        IEnumerator Listen()
        {
            while (WillListen)
            {
                yield return null;
                for (int i = 0 ; i < BufferUpdatePerFrame ; i++)
                {
                    if (Commits.Count > 0)
                    {
                        var L = Commits.Dequeue();
                        {
                            ReceiveString(L);
                        }
                    }
                }
                SubmitBuffer();
            }
        }
        public void Write(string str)
        {
            Commits.Enqueue(str);
        }
        public void WriteLine(string str)
        {
            Commits.Enqueue(str);
            Commits.Enqueue("\n");
        }
        public Vector2Int CurrentPosition = Vector2Int.zero;
        public void SubmitBuffer()
        {
            TEBuffer.Apply();
        }
        void DrawChar(char c , int X , int Y)
        {
            X *= CW;
            Y = Height - 1 - Y;
            Y *= CH;
            {
                int TH = 0;
                int CB = (int)c / CharMapBlockSize;
                int CP = c % CharMapBlockSize;
                int CY = CP / CharMapBlockW;
                int CX = CP % CharMapBlockW;
                Texture2D CMT;
                bool Hit = CharMapDict.TryGetValue(CB , out CMT);
                if (Hit)
                {
                    for (int x = 0 ; x < CW ; x++)
                    {
                        for (int y = 0 ; y < CH ; y++)
                        {
                            var cx = CX * TGW + TGL + x;
                            var cy = (TH - 1 - CY) * CH + y;
                            TEBuffer.SetPixel(X + TGL + x , Y + y , CurrentBG + CMT.GetPixel(cx , cy) * CurrentFG);
                        }
                    }

                }
                else
                {

                    for (int x = 0 ; x < CW ; x++)
                    {
                        for (int y = 0 ; y < CH ; y++)
                        {
                            TEBuffer.SetPixel(X + x , Y + y , CurrentBG);
                        }
                    }
                }
            }
        }
        void DrawCharOverlay(char c , int X , int Y)
        {
            X *= CW;
            Y = Height - 1 - Y;
            Y *= CH;
            {
                int TH = 0;
                int CB = (int)c / CharMapBlockSize;
                int CP = c % CharMapBlockSize;
                int CY = CP / CharMapBlockW;
                int CX = CP % CharMapBlockW;
                Texture2D CMT;
                bool Hit = CharMapDict.TryGetValue(CB , out CMT);
                if (Hit)
                {
                    for (int x = 0 ; x < CW ; x++)
                    {
                        for (int y = 0 ; y < CH ; y++)
                        {
                            var cx = CX * TGW + TGL + x;
                            var cy = (TH - 1 - CY) * CH + y;
                            TEBuffer.SetPixel(X + TGL + x ,
                                              Y + y ,
                                              TEBuffer.GetPixel(X + TGL + x , Y + y) + CMT.GetPixel(cx , cy) * CurrentFG);
                        }
                    }

                }
                else
                {

                }
            }
        }
        public void SetPos(int X , int Y)
        {
            CurrentPosition.Set(X , Y);
        }
        void Redraw(int X , int Y)
        {
            char c = Buffer [ X , Y ];
            DrawChar(c , X , Y);
        }
        void ReceiveString(string str)
        {
            foreach (var c in str)
            {
                ReceiveCharacter(c);
            }
            //SubmitBuffer();
        }

        void CheckAndAdjustBuffer()
        {
            if (CurrentPosition.y >= Height)
            {
                {

                    for (int x = 0 ; x < Width ; x++)
                    {
                        for (int y = 0 ; y < Height - 1 ; y++)
                        {
                            Buffer [ x , y ] = Buffer [ x , y + 1 ];
                            //Redraw(x , y);
                        }
                    }
                    TEBuffer.SetPixels(0 , CH , SWidth , SHeight - CH , TEBuffer.GetPixels(0 , 0 , SWidth , SHeight - CH));
                    for (int x = 0 ; x < Width ; x++)
                    {
                        Buffer [ x , Height - 1 ] = ' ';
                        Redraw(x , Height - 1);
                    }
                    LastCP.y -= 1;
                    if (LastCP.y < 0) LastCP.y = 0;
                    CurrentPosition.y -= 1;
                }
            }
        }
        bool ANSISequence = false;
        bool ANSI_LB = false;
        string ANSICtrlSeq = "";
        string AllowedControlString = "0123456789;,";
        public void MovePos(int x , int y)
        {
            CurrentPosition.x += x;
            CurrentPosition.y += y;
            if (CurrentPosition.x >= Width)
            {
                CurrentPosition.x = 0;
                CurrentPosition.y += CurrentPosition.x / Width;
            }
            else if (CurrentPosition.x < 0)
            {
                CurrentPosition.x = Width - 1;
                if (CurrentPosition.y > 0)
                {
                    CurrentPosition.y -= Math.Abs(CurrentPosition.x) / Width;
                    CurrentPosition.y -= 1;
                }
                else
                {
                    CurrentPosition.y = 0;
                    CurrentPosition.x = 0;
                }

            }
            if (CurrentPosition.y < 0) CurrentPosition.y = 0;
            if (CurrentPosition.y >= Height) CurrentPosition.y = Height - 1;
        }
        void ReceiveCharacter(char c)
        {
            if (c == '\x1b')
            {
                ANSISequence = true;
                ANSI_LB = false;
                return;
            }
            if (ANSISequence == true)
            {
                if (c == '[')
                {
                    ANSICtrlSeq = "";
                    ANSI_LB = true;
                    return;
                }
                else
                {
                    if (ANSI_LB == true)
                    {
                        switch (c)
                        {
                            case 'n':
                                {
                                    if (int.TryParse(ANSICtrlSeq , out int n))
                                    {
                                        if (n == 6)
                                        {
                                            host.OnObtainChar('\x1b');
                                            host.OnObtainChar('[');
                                            var x_str = CurrentPosition.x.ToString();
                                            var y_str = CurrentPosition.y.ToString();
                                            foreach (var str_c in x_str)
                                            {
                                                host.OnObtainChar(str_c);
                                            }
                                            host.OnObtainChar(';');
                                            foreach (var str_c in y_str)
                                            {
                                                host.OnObtainChar(str_c);
                                            }
                                            host.OnObtainChar('R');
                                        }
                                    }
                                    ANSISequence = false;
                                }
                                break;
                            case 'm':
                                {
                                    var controls =
                                    ANSICtrlSeq.Split(';');
                                    foreach (var item in controls)
                                    {
                                        if (item.Length >= 1)
                                            if (item [ 0 ] == '0')
                                            {
                                                FG = DefaultFG;
                                                BG = DefaultBG;
                                                CurrentFG = FGPlatte [ FG ];
                                                CurrentBG = BGPlatte [ BG ];
                                            }
                                            else
                                            if (item [ 0 ] == '1')
                                            {
                                                if (item.Length == 3)
                                                {
                                                    BG = item [ 2 ] - 48 + 10;

                                                    CurrentBG = BGPlatte [ FG ];
                                                }

                                            }
                                            else
                                            if (item [ 0 ] == '3')
                                            {
                                                FG = item [ 1 ] - 48;
                                                CurrentFG = FGPlatte [ FG ];
                                            }
                                            else
                                            if (item [ 0 ] == '9')
                                            {
                                                FG = item [ 1 ] - 48 + 10;
                                                CurrentFG = FGPlatte [ FG ];
                                            }
                                            else if (item [ 0 ] == '4')
                                            {
                                                BG = item [ 1 ] - 48;
                                                CurrentBG = BGPlatte [ BG ];

                                            }
                                    }
                                    ANSISequence = false;
                                }
                                break;
                            case 'j':
                            case 'J':
                                {
                                    if (ANSICtrlSeq == "")
                                    {
                                        Clear();
                                    }
                                    else
                                    {
                                        if (int.TryParse(ANSICtrlSeq , out var i))
                                        {
                                            switch (i)
                                            {
                                                case 0:
                                                    {
                                                        int x = CurrentPosition.x;
                                                        for (int y = CurrentPosition.y ; y < Height ; y++)
                                                        {
                                                            for (; x < Width ; x++)
                                                            {
                                                                Buffer [ x , y ] = ' ';
                                                                Redraw(x , y);
                                                            }
                                                            x = 0;
                                                        }
                                                        SubmitBuffer();
                                                    }
                                                    break;
                                                case 1:
                                                    {

                                                        int x = CurrentPosition.x;
                                                        for (int y = CurrentPosition.y ; y >= 0 ; y--)
                                                        {
                                                            for (; x >= 0 ; x--)
                                                            {
                                                                Buffer [ x , y ] = ' ';
                                                                Redraw(x , y);
                                                            }
                                                            x = Width - 1;
                                                        }
                                                        SubmitBuffer();
                                                    }
                                                    break;
                                                case 2:
                                                    Clear();
                                                    break;
                                                case 3: break;
                                                default:
                                                    break;
                                            }
                                        }
                                        else ANSISequence = false;
                                    }
                                    ANSISequence = false;
                                }
                                break;
                            case 'K':
                                {
                                    if (ANSICtrlSeq == "")
                                    {
                                        for (int x = CurrentPosition.x ; x < Width ; x++)
                                        {
                                            Buffer [ x , CurrentPosition.y ] = ' ';
                                            Redraw(x , CurrentPosition.y);
                                        }
                                        SubmitBuffer();
                                    }
                                    else
                                    {
                                        if (int.TryParse(ANSICtrlSeq , out var i))
                                        {
                                            switch (i)
                                            {
                                                case 0:
                                                    {
                                                        for (int x = CurrentPosition.x ; x < Width ; x++)
                                                        {
                                                            Buffer [ x , CurrentPosition.y ] = ' ';
                                                            Redraw(x , CurrentPosition.y);
                                                        }
                                                        SubmitBuffer();
                                                    }
                                                    break;
                                                case 1:
                                                    {

                                                        for (int x = CurrentPosition.x ; x >= 0 ; x--)
                                                        {
                                                            Buffer [ x , CurrentPosition.y ] = ' ';
                                                            Redraw(x , CurrentPosition.y);
                                                        }
                                                        SubmitBuffer();
                                                    }
                                                    break;
                                                case 2:
                                                    {

                                                        for (int x = 0 ; x < Width ; x++)
                                                        {
                                                            Buffer [ x , CurrentPosition.y ] = ' ';
                                                            Redraw(x , CurrentPosition.y);
                                                        }
                                                        SubmitBuffer();
                                                    }
                                                    break;
                                                case 3: break;
                                                default:
                                                    break;
                                            }
                                        }
                                        else ANSISequence = false;
                                    }
                                    ANSISequence = false;
                                }
                                break;
                            case 'H':
                            case 'h':
                            case 'f':
                                {
                                    if (ANSICtrlSeq.IndexOf(';') > 0)
                                    {
                                        var xy = ANSICtrlSeq.Split(";");
                                        if (xy.Length >= 2)
                                        {
                                            if (int.TryParse(xy [ 0 ] , out var __x))
                                                CurrentPosition.x = __x;
                                            if (int.TryParse(xy [ 1 ] , out var __y))
                                                CurrentPosition.y = __y;
                                        }
                                    }
                                    else
                                        CurrentPosition = Vector2Int.zero;
                                    ANSISequence = false;
                                }
                                break;
                            case 'A':
                                {
                                    if (ANSICtrlSeq.Length > 0)
                                    {
                                        if (int.TryParse(ANSICtrlSeq , out var v))
                                        {
                                            MovePos(0 , v);
                                        }
                                    }
                                    else
                                    {
                                        MovePos(0 , 1);

                                    }
                                    ANSISequence = false;
                                }
                                break;
                            case 'B':
                                {
                                    if (ANSICtrlSeq.Length > 0)
                                    {
                                        if (int.TryParse(ANSICtrlSeq , out var v))
                                        {
                                            MovePos(0 , -v);
                                        }
                                    }
                                    else
                                    {
                                        MovePos(0 , -1);

                                    }
                                    ANSISequence = false;
                                }
                                break;
                            case 'C':
                                {
                                    if (ANSICtrlSeq.Length > 0)
                                    {
                                        if (int.TryParse(ANSICtrlSeq , out var v))
                                        {
                                            MovePos(-v , 0);
                                        }
                                    }
                                    else
                                    {
                                        MovePos(-1 , 0);

                                    }
                                    ANSISequence = false;
                                }
                                break;
                            case 'D':
                                {
                                    if (ANSICtrlSeq.Length > 0)
                                    {
                                        if (int.TryParse(ANSICtrlSeq , out var v))
                                        {
                                            MovePos(v , 0);
                                        }
                                    }
                                    else
                                    {
                                        MovePos(1 , 0);

                                    }
                                    ANSISequence = false;
                                }
                                break;
                            default:
                                {
                                    if (AllowedControlString.Contains(c))
                                        ANSICtrlSeq += c;
                                    else ANSISequence = false;
                                }
                                break;
                        }
                        return;
                    }
                    else
                    {
                        ANSISequence = false;
                    }
                }
            }
            if (c == '\n')
            {
                CurrentPosition.y += 1;
                CurrentPosition.x = 0;
                CheckAndAdjustBuffer();
                return;
            }
            if (c == '\t')
            {
                var l = TabSize - CurrentPosition.x % TabSize;

                if (CurrentPosition.x + l <= Width)
                {
                    for (int i = 0 ; i < l ; i++)
                    {
                        Buffer [ CurrentPosition.x + i , CurrentPosition.y ] = ' ';
                        Redraw(CurrentPosition.x + i , CurrentPosition.y);
                    }
                    CurrentPosition.x += l;
                    if (CurrentPosition.x >= Width)
                    {
                        CurrentPosition.x = 0;
                        CurrentPosition.y += 1;
                        CheckAndAdjustBuffer();
                    }
                }
                else
                {

                    CurrentPosition.x = 0;
                    CurrentPosition.y += 1;
                    CheckAndAdjustBuffer();
                    for (int i = 0 ; i < l ; i++)
                    {
                        Buffer [ CurrentPosition.x + i , CurrentPosition.y ] = ' ';
                        Redraw(CurrentPosition.x + i , CurrentPosition.y);
                    }
                    CurrentPosition.x += l;
                    if (CurrentPosition.x >= Width)
                    {
                        CurrentPosition.x = 0;
                        CurrentPosition.y += 1;
                        CheckAndAdjustBuffer();
                    }
                }
            }
            else
            {

                Buffer [ CurrentPosition.x , CurrentPosition.y ] = c;
                Redraw(CurrentPosition.x , CurrentPosition.y);
                CurrentPosition.x += 1;
                if (CurrentPosition.x >= Width)
                {
                    CurrentPosition.x = 0;
                    CurrentPosition.y += 1;
                    CheckAndAdjustBuffer();
                }
            }
        }
        string input = "";
        float BlinkD;
        bool ShowCursor;
        Vector2Int LastCP;
        public void DrawCursor()
        {
            try
            {
                Redraw(CurrentPosition.x , CurrentPosition.y);
                if (ShowCursor)
                {
                    DrawCharOverlay('_' , CurrentPosition.x , CurrentPosition.y);
                }
                if (LastCP != CurrentPosition)
                {
                    Redraw(LastCP.x , LastCP.y);
                    LastCP = CurrentPosition;
                }
            }
            catch (Exception)
            {
            }
        }
        int InputPos = 0;
        int InputLen = 0;
        TEHost host;
        public void SetHost(TEHost con)
        {
            host = con;
            host.Init(this);
        }
        bool Ctrl;
        bool Shift;
        public void Update()
        {
            if (!PauseCursorUpdate)
            {
                BlinkD += Time.deltaTime;
                if (BlinkD > Blink)
                {
                    ShowCursor = !ShowCursor;
                    BlinkD = 0;
                }
                DrawCursor();

            }
            if (PauseKeyboardInput) return;
            var str = Input.inputString;
            foreach (var item in str)
            {
                host.OnObtainChar(item);
            }
            int Modifier = 1;
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) Modifier += 4;
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) Modifier += 1;
            if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) Modifier += 2;
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                host.OnObtainChar('\x1B');
                host.OnObtainChar('[');
                host.OnObtainChar('3');
                //if (Modifier != 1)
                {
                    host.OnObtainChar(';');
                    host.OnObtainChar((char)Modifier);
                }
                host.OnObtainChar('~');
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                host.OnObtainChar('\x1B');
                host.OnObtainChar('[');
                //if (Modifier != 1)
                {
                    host.OnObtainChar((char)Modifier);
                }
                host.OnObtainChar('C');
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                host.OnObtainChar('\x1B');
                host.OnObtainChar('[');
                //if (Modifier != 1)
                {
                    host.OnObtainChar((char)Modifier);
                }
                host.OnObtainChar('D');
            }
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                host.OnObtainChar('\x1B');
                host.OnObtainChar('[');
                //if (Modifier != 1)
                {
                    host.OnObtainChar((char)Modifier);
                }
                host.OnObtainChar('A');
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                host.OnObtainChar('\x1B');
                host.OnObtainChar('[');
                //if (Modifier != 1)
                {
                    host.OnObtainChar((char)Modifier);
                }
                host.OnObtainChar('B');
            }
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                host.OnObtainChar('\t');
            }
            return;
        }
    }
    public static class Constants
    {
        public const int Shift = 2;
        public const int Alt = 3;
        public const int ShiftAlt = 4;
        public const int Ctrl = 5;
        public const int ShiftCtrl = 6;
        public const int AltCtrl = 7;
        public const int ShiftAltCtrl = 8;
        public const int Meta = 9;
    }
    [Serializable]
    public class KVDict<K, V>
    {
        public List<KVPair<K , V>> Data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<K , V> ToDictionary()
        {
            Dictionary<K , V> __RESULT = new Dictionary<K , V>();
            foreach (var item in Data)
            {
                __RESULT.Add(item.Key , item.Value);
            }
            return __RESULT;
        }
    }
}
