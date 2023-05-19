using System.Drawing;
using System.Xml;
using static System.Net.Mime.MediaTypeNames;

namespace CharacterMapGen
{
    public class Options
    {
        public string? Font;
        public int FontSize = 1;
        public int W = 16;
        public int H = 16;
        public int AA =0 ;
        public int TextureW = 320;
        public int TextureH = 320;
        public string output = "a.png";
        public int Character = 0;
        public bool ShowHelp;
        public static Options FromArguments(string [ ] args)
        {
            Options options = new Options();
            var l = args.Length;
            for (int i = 0 ; i < l ; i++)
            {
                var item = args [ i ];
                switch (item)
                {
                    case "-h":
                    case "--?":
                    case "--help":
                    case "--h":
                        {
                            options.ShowHelp = true;
                        }
                        break;
                    case "-H":
                    case "--height":
                        {
                            i++;
                            item = args [ i ];
                            if (int.TryParse(item , out var _i))
                            {
                                options.H = _i;
                            }
                            else
                            {
                                Console.WriteLine($"\"{item}\" cannot be converted to int32.");
                            }
                        }
                        break;
                    case "-W":
                    case "--width":
                        {
                            i++;
                            item = args [ i ];
                            if (int.TryParse(item , out var _i))
                            {
                                options.W = _i;
                            }
                            else
                            {
                                Console.WriteLine($"\"{item}\" cannot be converted to int32.");
                            }
                        }
                        break;
                    case "-TH":
                    case "--texture-height":
                        {
                            i++;
                            item = args [ i ];
                            if (int.TryParse(item , out var _i))
                            {
                                options.TextureH = _i;
                            }
                            else
                            {
                                Console.WriteLine($"\"{item}\" cannot be converted to int32.");
                            }
                        }
                        break;
                    case "-TW":
                    case "--texture-width":
                        {
                            i++;
                            item = args [ i ];
                            if (int.TryParse(item , out var _i))
                            {
                                options.TextureW = _i;
                            }
                            else
                            {
                                Console.WriteLine($"\"{item}\" cannot be converted to int32.");
                            }
                        }
                        break;
                    case "-AA":
                    case "--anti-alias":
                        {
                            i++;
                            item = args [ i ];
                            if (int.TryParse(item , out var _i))
                            {
                                options.AA = _i;
                            }
                            else
                            {
                                Console.WriteLine($"\"{item}\" cannot be converted to int32.");
                            }
                        }
                        break;
                    case "-F":
                    case "--font":
                        {
                            i++;
                            item = args [ i ];
                            options.Font = item;
                        }
                        break;
                    case "-O":
                    case "--output":
                        {
                            i++;
                            item = args [ i ];
                            options.output = item;
                        }
                        break;
                    case "-FS":
                    case "--font-size":
                        {
                            i++;
                            item = args [ i ];
                            if (int.TryParse(item , out var _i))
                            {
                                options.FontSize = _i;
                            }
                            else
                            {
                                Console.WriteLine($"\"{item}\" cannot be converted to int32.");
                            }
                        }
                        break;
                    case "-C":
                    case "--character":
                        {
                            i++;
                            item = args [ i ];
                            if (int.TryParse(item , out var _i))
                            {
                                options.Character = _i;
                            }
                            else
                            {
                                Console.WriteLine($"\"{item}\" cannot be converted to int32.");
                            }
                        }
                        break;
                    default:
                        {
                            Console.WriteLine($"\x1b[31mError\x1b[39m: Unidentified arguments {item}.");
                        }
                        break;
                }
            }
            return options;
        }
    }
    internal class Program
    {
        static void ShowHelp()
        {
            Console.WriteLine("This tool generates character map texture that uTerminalEmulator can use.");
            Console.WriteLine("Usage:");
            Console.WriteLine();
            Console.WriteLine("\tCharacterMapGen [options]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("--width <int>");
            Console.WriteLine("-W <int>\tCharacter Width in char count.");
            Console.WriteLine("--height <int>");
            Console.WriteLine("-H <int>\tCharacter Height in char count.");
            Console.WriteLine("--texture-height <int>");
            Console.WriteLine("-TH <int>\tOutput Texture Height.");
            Console.WriteLine("--texture-width <int>");
            Console.WriteLine("-TW <int>\tOutput Texture Width.");
            Console.WriteLine("--anti-alias <int>");
            Console.WriteLine("-AA <int>\tOutput Texture Width.");
            Console.WriteLine("\tNote:");
            Console.WriteLine("\t\r\n        SystemDefault = 0,\r\n        SingleBitPerPixelGridFit = 1,\r\n        SingleBitPerPixel = 2,\r\n        AntiAliasGridFit = 3,\r\n        AntiAlias = 4,\r\n        ClearTypeGridFit = 5,");
            Console.WriteLine("--font <string>");
            Console.WriteLine("-F <string>\tFont Name.");
            Console.WriteLine("--font-size <int>");
            Console.WriteLine("-FS <int>\tFont Size.");
            Console.WriteLine("--character <int>");
            Console.WriteLine("-C <int>\tCharacter that will present in the map.");
            Console.WriteLine("--output <string>");
            Console.WriteLine("-O <string>\tOutput file.");
        }
        static void Main(string [ ] args)
        {
            var opt = Options.FromArguments(args);
            if (opt.ShowHelp)
            {
                ShowHelp();
                return;
            }
            {

                Bitmap image = new Bitmap(opt.TextureH , opt.TextureW);
                var graphics = Graphics.FromImage(image);
                int Block = opt.W * opt.H;
                int StartPoint = (opt.Character / Block) * Block;
                graphics.TextRenderingHint = (System.Drawing.Text.TextRenderingHint)opt.AA;
                float cW = (float)opt.TextureW / (float)opt.W;
                float cH = (float)opt.TextureH / (float)opt.H;
                graphics.FillRectangle(new SolidBrush(Color.Black) , new Rectangle(-1 , -1 , opt.TextureW + 2 , opt.TextureH + 2));
                var font = new Font(opt.Font , opt.FontSize , FontStyle.Regular);
                var WhiteBrush = new SolidBrush(Color.White);
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center ,
                    LineAlignment = StringAlignment.Center
                };
                for (int y = 0 ; y < opt.H ; y++)
                {
                    for (int x = 0 ; x < opt.W ; x++)
                    {
                        graphics.DrawString($"{(char)StartPoint}" ,
                                            font ,
                                            WhiteBrush ,
                                            new RectangleF(new PointF(x * cW , y * cH) , new SizeF(cW , cH)) ,
                                            sf);
                        StartPoint++;

                    }
                }
                graphics.Save();
                image.Save(opt.output);
            }
        }
    }
}