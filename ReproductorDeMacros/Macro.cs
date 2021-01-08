using ReproductorDeMacros;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

//using Windows.UI.Notifications;

namespace MiLibreria
{
    public class Macro
    {
        private Thread      recordThread;
        private Thread      playThread;
        private bool        recording;
        private bool        playing;
        public  bool        macro_exists;
        public  string      macro_path;
        public bool leftclicking;
        public bool rightclicking;
        public event EventHandler<OnPlayStartEventArgs> OnPlayStarted;
        public event EventHandler<OnRecordStartEventArgs> OnRecordStarted;
        public event EventHandler OnPlayIterate;
        public event EventHandler<OnPlayFinishedEventArgs> OnPlayFinished;
        public event EventHandler<OnRecordFinishedEventArgs> OnRecordFinished;
        public event EventHandler<OnKeyPressedEventArgs> OnKeyPressed;

        public class OnKeyPressedEventArgs : EventArgs
        {
            private readonly int _KeyCode;
            private readonly string _Key;
            public OnKeyPressedEventArgs(int KeyCode, string Key)
            {
                this._KeyCode = KeyCode;
                this._Key = Key;
            }

            public string Key 
            {
                get { return _Key; }
            }
            public int KeyCode
            {
                get { return _KeyCode; }
            }
        }


        public class OnRecordFinishedEventArgs : EventArgs
        {
            private readonly string _MacroName;
            private readonly string _MacroPath;


            public OnRecordFinishedEventArgs(string name, string path)
            {
                this._MacroName = name;
                this._MacroPath = path;
            }

            public string MacroName
            {
                get { return _MacroName; }
            }
            public string MacroPath
            {
                get { return _MacroPath; }
            }
        }
        public class OnRecordStartEventArgs : EventArgs
        {
            private readonly string _MacroName;
            private readonly string _MacroPath;
            private readonly int _TickRate;

            public OnRecordStartEventArgs(string name, string path, int tickrate)
            {
                this._MacroName = name;
                this._MacroPath = path;
                this._TickRate = tickrate;
            }

            public string MacroName
            {
                get { return _MacroName; }
            }
            public string MacroPath
            {
                get { return _MacroPath; }
            }
            public int TickRate
            {
                get { return _TickRate; }
            }
        }
        public class OnPlayStartEventArgs : EventArgs
        {
            private readonly string _MacroName;
            private readonly string _MacroPath;
            private readonly int    _TickRate;

            public OnPlayStartEventArgs(string name, string path, int tickrate)
            {
                this._MacroName = name;
                this._MacroPath = path;
                this._TickRate = tickrate;
            }

            public string MacroName
            {
                get { return _MacroName; }
            }
            public string MacroPath
            {
                get { return _MacroPath; }
            }
            public int TickRate
            {
                get { return _TickRate; }
            }
        }
        public class OnPlayFinishedEventArgs : EventArgs
        {
            private readonly string _MacroName;
            private readonly string _MacroPath;


            public OnPlayFinishedEventArgs(string name, string path )
            {
                this._MacroName = name;
                this._MacroPath = path;

            }

            public string MacroName
            {
                get { return _MacroName; }
            }
            public string MacroPath
            {
                get { return _MacroPath; }
            }
        }
        public string filename;
        public bool IsRecording()
        {
            return recording;
        }

        public bool IsPlaying()
        {
            return playing;
        }

        public Macro(string filename, string folder = null)
        {
            var systemPath = System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var complete = Path.Combine(systemPath, Application.ProductName);
            complete = Path.Combine(complete, "Macros");
            if (!Directory.Exists(complete))
            {
                Directory.CreateDirectory(complete);
            }
            if(folder != null)
            {
                complete = Path.Combine(complete, folder);
                if (!Directory.Exists(complete))
                {
                    Directory.CreateDirectory(complete);
                }
            }
            complete = Path.Combine(complete, filename);
            macro_path = complete;
            if(!File.Exists(complete))
            {
                macro_exists = false;
            } else
            {
                macro_exists = true;
            }
            this.filename = filename;
        }
        private void OnRightMouseButtonClicked(object sender, EventArgs e)
        {
            rightclicking = true;
        }
        private void OnLeftMouseButtonClicked(object sender, EventArgs e)
        {
            leftclicking = true;
        }
        public void Play()
        {
            playing = true;
            playThread = new Thread(new ThreadStart(PlayThread));
            Hooks.Start();
            playThread.Start();

        }

        private int recordTickRate;

        public void StartRecord(int TickRate)
        {
            recordTickRate = TickRate;
            recordThread = new Thread(new ThreadStart(RecordThread));
            recording = true;
            Hooks.Start();
            Hooks.OnLeftMouseClick += new EventHandler(OnLeftMouseButtonClicked);//Set our hook      //Start a standard application method loop
            Hooks.OnRightMouseClick += new EventHandler(OnRightMouseButtonClicked);
            recordThread.Start();
        }

        public void FinishRecord()
        {
            recording = false;
            OnRecordFinished(this, new OnRecordFinishedEventArgs(filename, macro_path));
            Hooks.stop();
        }

        public void FinishPlay()
        {
            playing = false;
            OnPlayFinished(this, new OnPlayFinishedEventArgs(filename, macro_path));
        }

        private void PlayThread()
        {
                using (StreamReader fs = new StreamReader(macro_path))
                {
                    bool input_params = true;
                    int original_width, original_height;
                    int diferencia_width = 0, diferencia_height = 0;
                    string line;
                    int PlayTickRate = 0;
                    while ((line = fs.ReadLine()) != null)
                    {
                        if (playing)
                        {
                            string[] parametros = line.Split('>');
                            if (input_params)
                            {
                                original_width = Int32.Parse(parametros[1].Trim());
                                original_height = Int32.Parse(parametros[2].Trim());
                                diferencia_width = Screen.PrimaryScreen.Bounds.Width - original_width;
                                diferencia_height = Screen.PrimaryScreen.Bounds.Height - original_height;
                                if (parametros[3] == null)
                                {
                                    PlayTickRate = 8;
                                }
                                else
                                {
                                    PlayTickRate = Int32.Parse(parametros[3]);
                                }
                                OnPlayStarted(this, new OnPlayStartEventArgs(filename, macro_path, PlayTickRate));
                                input_params = false;
                            }
                            else
                            {
                     
                                if (Hooks.CurrentKey != null)
                                {
                                    OnKeyPressed(this, new OnKeyPressedEventArgs(Hooks.CurrentKeyCode, Hooks.CurrentKey));
                                }
                                //X: parametros[1]
                                //Y: parametros[2]
                                //Key: parametros[3]
                                //clicked?: parametros[4]
                                if (parametros[3] != "none" && parametros[3].Length > 0)
                                {
                                    keybd_event(byte.Parse(parametros[3]), 0x45, 0, 0);
                                }
                                if (bool.Parse(parametros[4].Trim()))
                                {
                                    mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, (uint)Int32.Parse(parametros[1].Trim()), (uint)Int32.Parse(parametros[2].Trim()), 0, 0);
                                }
                                if (bool.Parse(parametros[5].Trim()))
                                {
                                    mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, (uint)Int32.Parse(parametros[1].Trim()), (uint)Int32.Parse(parametros[2].Trim()), 0, 0);
                                }
                                Cursor.Position = new Point(Int32.Parse(parametros[1].Trim()) + diferencia_width, Int32.Parse(parametros[2].Trim()) + diferencia_height);
                            }
                        }
                        else
                        {
                            break;
                        }
                        Thread.Sleep(PlayTickRate);
                    }
                    fs.Close();
                }

                Hooks.stop();
                if (OnPlayFinished != null)
                {
                    OnPlayFinished(this, new OnPlayFinishedEventArgs(filename, macro_path));
                }
            
        }
        private void RecordThread()
        {
            try
            {
                System.IO.File.WriteAllText(macro_path, string.Empty);
                bool input_params = true;
                using (StreamWriter macro_file = new StreamWriter(macro_path))
                {
                    OnRecordStarted(this, new OnRecordStartEventArgs(filename, macro_path, recordTickRate));
                    while (true)
                    {
                        if (input_params)
                        {

                            input_params = false;
                            string input;
                                input = ">" + Screen.PrimaryScreen.Bounds.Width + ">"
                            + Screen.PrimaryScreen.Bounds.Height + ">" + recordTickRate.ToString();
                            macro_file.WriteLine(input);
                        }
                        else
                        {
                            if (Hooks.CurrentKey == null)
                            {
                                Hooks.CurrentKey = "none";
                            }
                            if(Hooks.CurrentKey != null && Hooks.CurrentKey != "none")
                            {
                                OnKeyPressed(this, new OnKeyPressedEventArgs(Hooks.CurrentKeyCode, Hooks.CurrentKey));
                            }
                            Point cursorPos;
                            GetCursorPos(out cursorPos);
                            var frame_input = ">" + cursorPos.X.ToString() + ">" + cursorPos.Y.ToString() + ">" + Hooks.CurrentKey + ">" + leftclicking.ToString() + ">" + rightclicking.ToString();
                            macro_file.WriteLine(frame_input);
                            Hooks.CurrentKey = "none";
                            leftclicking = false;
                            rightclicking = false;
                        }
                        if (!recording)
                        {
                            break;
                        }
                        Thread.Sleep(recordTickRate);
                    }
                    macro_file.Close();
                }
            }
            catch(Exception ex)
            {

            }
        }
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out Point lpPoint);
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);
        [DllImport("User32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;
    }
    
}
