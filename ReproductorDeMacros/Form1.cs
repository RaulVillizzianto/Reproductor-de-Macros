
using MiLibreria;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic;

namespace ReproductorDeMacros
{
    public partial class Form1 : Form
    {
        const int tickrate = 4;
        int iterationsleft;
        int selectedMacro;

        public Form1()
        {
            InitializeComponent();
        }
        List<Macro> macro_list = new List<Macro>();
        private void Form1_Load(object sender, EventArgs e)
        {
            CargarMacros();
        }
        private void CargarMacros()
        {

            var systemPath = System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var complete = Path.Combine(systemPath, this.ProductName);
            complete = Path.Combine(complete, "Macros");
            if(Directory.Exists(complete))
            {
                var Files = Directory.GetFiles(complete);
                comboBox1.Items.Clear();
                dataGridView1.Rows.Clear();
                foreach (string _file in Files)
                {
                    string file = _file.Substring(_file.LastIndexOf('\\')+1);
                    Macro macro = new Macro(file, null);
                    macro.OnRecordStarted += Macro_OnRecordStarted;
                    macro.OnRecordFinished += Macro_OnRecordFinished;
                    macro.OnPlayStarted += Macro_OnPlayStarted;
                    macro.OnPlayFinished += Macro_OnPlayFinished;
                    macro.OnKeyPressed += Macro_OnKeyPressed;
                    macro_list.Add(macro);
                    comboBox1.Items.Add(macro.filename);
                    dataGridView1.Rows.Add(macro.filename);
                }

                if(comboBox1.Items.Count > 0)
                {
                    comboBox1.SelectedIndex = 0;
                }
            }
            else
            {
                Directory.CreateDirectory(complete);
            }
        }

        private void Macro_OnKeyPressed(object sender, Macro.OnKeyPressedEventArgs e)
        {
            if(e.KeyCode == 0x1B)
            {
                foreach(Macro macro in macro_list)
                {
                    if(macro.IsRecording())
                    {
                        macro.FinishRecord();
                    } else if(macro.IsPlaying())
                    {
                        macro.FinishPlay();
                    }
                }
            }
        }

        private void Macro_OnPlayStarted(object sender, Macro.OnPlayStartEventArgs e)
        {
            ShowNotification("Reproduccion iniciada", "Reproduccion iniciada de " + e.MacroName, "play");

        }

        private void Macro_OnPlayFinished(object sender, Macro.OnPlayFinishedEventArgs e)
        {
            ShowNotification("Reproduccion finalizada", "Se ha detenido la reproduccion de " + e.MacroName, "stop");
            if(iterationsleft > 0)
            {
                iterationsleft--;
                macro_list.ElementAt(selectedMacro).Play();
            }
        }

        private void Macro_OnRecordFinished(object sender, Macro.OnRecordFinishedEventArgs e)
        {
            ShowNotification("Grabacion finalizada", "Se ha detenido la grabacion de " + e.MacroName, "stop");
        }

        private void Macro_OnRecordStarted(object sender, Macro.OnRecordStartEventArgs e)
        {
            ShowNotification("Grabacion iniciada", "Grabacion inicializada de " + e.MacroName, "play");
        }
        public void ShowNotification(string title, string text, string icon_name)
        {
            notifyIcon1.Icon = new System.Drawing.Icon(Path.GetFullPath(@"image\" + icon_name + ".ico"));
            notifyIcon1.Text = "Reproductor de Macros";
            notifyIcon1.Visible = true;
            notifyIcon1.BalloonTipTitle = title;
            notifyIcon1.BalloonTipText = text;
            notifyIcon1.ShowBalloonTip(100);
            Thread escondericono = new Thread(new ThreadStart(HideIcon));
            escondericono.Start();
        }
        public void HideIcon()
        {
            Thread.Sleep(7300);
            notifyIcon1.Visible = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            a:
            string nuevamacro_name = Microsoft.VisualBasic.Interaction.InputBox("Ingresa el nombre de la macro", "Atención", "");
            if (!String.IsNullOrWhiteSpace(nuevamacro_name))
            {
                Macro macro = new Macro(nuevamacro_name);
                macro_list.Add(macro);
                comboBox1.Items.Add(macro.filename);
                dataGridView1.Rows.Add(macro.filename);
                macro.OnRecordStarted += Macro_OnRecordStarted;
                macro.OnRecordFinished += Macro_OnRecordFinished;
                macro.OnPlayStarted += Macro_OnPlayStarted;
                macro.OnPlayFinished += Macro_OnPlayFinished;
                macro.OnKeyPressed += Macro_OnKeyPressed;
                macro.StartRecord(tickrate);
                ShowNotification("Atención", "Se ha iniciado una nueva grabación (" + nuevamacro_name + ") Presiona ESC para finalizar.", "play");
                this.WindowState = FormWindowState.Minimized;
            }
            else goto a;
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            switch(e.ColumnIndex)
            {
                case 1:
                    {
                        macro_list.ElementAt(e.RowIndex).Play();
                        break;
                    }
                case 2:
                    {

                        macro_list.ElementAt(e.RowIndex).OnRecordStarted += Macro_OnRecordStarted;
                        macro_list.ElementAt(e.RowIndex).OnRecordFinished += Macro_OnRecordFinished;
                        macro_list.ElementAt(e.RowIndex).OnPlayStarted += Macro_OnPlayStarted;
                        macro_list.ElementAt(e.RowIndex).OnPlayFinished += Macro_OnPlayFinished;
                        macro_list.ElementAt(e.RowIndex).OnKeyPressed += Macro_OnKeyPressed;
                        macro_list.ElementAt(e.RowIndex).StartRecord(tickrate);
                        this.WindowState = FormWindowState.Minimized;
                        break;
                    }
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if(textBox1.Text.Length > 0)
            {
                iterationsleft = Int32.Parse(textBox1.Text)-1;
                selectedMacro = comboBox1.SelectedIndex;
                macro_list.ElementAt(selectedMacro).Play();
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            int result;
            if(!Int32.TryParse(textBox1.Text, out result) && textBox1.Text.Length > 0)
            {
                textBox1.Text = "";
                MessageBox.Show("Carácter inválido!");
            }
        }
    }
}
