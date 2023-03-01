using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using LiveCharts;
using LiveCharts.Wpf;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace JebenaVestica_KrekovanAlatka
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            InitializeComponent();
        }

        #region WholeFormDragable   
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_NCHITTEST)
                m.Result = (IntPtr)(HT_CAPTION);
        }

        private const int WM_NCHITTEST = 0x84;
        private const int HT_CLIENT = 0x1;
        private const int HT_CAPTION = 0x2;
        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {
            client = new WebClient();

            string command = "arp";
            string args = "-a";

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/c " + command + " " + args;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;

            Process process = new Process();
            process.StartInfo = startInfo;
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(); 


            int index = output.IndexOf("Type");
            string result = "";
            if (index != -1)
            {
                result = output.Substring(index + "Type".Length).Trim();
            }

            int pos = 0;
            ListViewItem item2 = null;

            foreach (string item in result.Split())
            {
                if (!string.IsNullOrWhiteSpace(item))
                {
                    pos++;
                    switch (pos)
                    {
                        case 1:
                            item2 = new ListViewItem(item);

                            break;
                        case 2:
                            item2.SubItems.Add(item);

                            break;
                        case 3:
                            item2.SubItems.Add(item);
                            listView1.Items.Add(item2);
                            pos = 0;
                            break;
                    }
                }
            }

        }

        WebClient client;
        static List<double> lista = new List<double>();
        double MaxSpeed;

        async Task Coroutines()
        {
            await Task.Delay(1000);
            Stopwatch sw = new Stopwatch();

            sw.Start();
            client.DownloadFile(new Uri("http://speedtest.tele2.net/5MB.zip"), "testfile");
            sw.Stop();

            double speed = new FileInfo("testfile").Length / (sw.ElapsedMilliseconds / 1000.0) / 1000000.0;
            speed = Math.Round(speed, 2);

            if (speed > MaxSpeed)
            {
                solidGauge1.To = speed;
            }
            
            solidGauge1.Value = speed;
            solidGauge1.Text += "Mb";

            cartesianChart1.Series.Clear();

            if(lista.Count > 4)
            {
                lista.Clear();
            }

            lista.Add(speed);
            cartesianChart1.Series.Add(new LineSeries() { Title = "Sample on 5MB file download speed", Values = new ChartValues<double>(lista) });

            File.Delete("testfile");
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void StartTesting_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 5; i++)
            {
                Coroutines();

            }
        }
    }
}
