using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.WaveFormRenderer;
using System.Drawing.Imaging;
using NAudio.Wave;
using System.IO;
using NAudio.Utils;
using System.Threading;

namespace AudioSpliter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private static string inputfilepath;
        private static MemoryStream outputStream = new MemoryStream();
        private static double start = 0, end = 0;
        private static double starttemp = 0, endtemp = 0, startX = 0, endX = 0;
        private static WaveOutEvent player = new WaveOutEvent();
        private static WaveOutEvent playertrimed = new WaveOutEvent();
        private static StandardWaveFormRendererSettings myRendererSettings = new StandardWaveFormRendererSettings();
        private static WaveFormRenderer renderer = new WaveFormRenderer();
        private static MediaFoundationReader mediafundationreader;
        private static Image image;
        public static Brush brush = (Brush)Brushes.MediumPurple;
        public static string buttonpressed, buttonpressedtrimed;
        private static WaveFileReader reader, readertrimed;
        private static bool temp = false, closed = false, setprogresssong = false, setprogresssongtrimed = false;
        public static double totaltime, currenttime, totaltimetrimed, currenttimetrimed;
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            closed = true;
        }
        private void Form1_Shown(object sender, EventArgs e)
        {
            Task.Run(() => Start());
        }
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Filter = "All Files(*.*)|*.*";
            if (op.ShowDialog() == DialogResult.OK)
            {
                inputfilepath = op.FileName;
                mediafundationreader = new MediaFoundationReader(inputfilepath);
                this.Text = inputfilepath;
                myRendererSettings.Width = 1040;
                myRendererSettings.TopHeight = 60;
                myRendererSettings.BottomHeight = 60;
                MediaFoundationReader reader = new MediaFoundationReader(inputfilepath);
                image = renderer.Render(reader, myRendererSettings);
                pictureBox1.BackgroundImage = image;
                startX = 0;
                starttemp = 0;
                endX = 0;
                endtemp = 0;
                temp = false;
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            SaveFileDialog sf = new SaveFileDialog();
            sf.Filter = "All Files(*.*)|*.*";
            if (sf.ShowDialog() == DialogResult.OK)
            {
                using (var mp3Stream = new StreamMediaFoundationReader(outputStream))
                {
                    WaveFileWriter.CreateWaveFile(sf.FileName, mp3Stream);
                }
            }
            myRendererSettings.Width = 1040;
            myRendererSettings.TopHeight = 60;
            myRendererSettings.BottomHeight = 60;
            MediaFoundationReader reader = new MediaFoundationReader(sf.FileName);
            Image image = renderer.Render(reader, myRendererSettings);
            pictureBox2.BackgroundImage = image;
        }
        private void button3_Click(object sender, EventArgs e)
        {
            if (buttonpressed == "pause")
            {
                player.Play();
            }
            else
            {
                mediafundationreader.Position = 0;
                outputStream = new MemoryStream();
                using (var waveFileWriter = new WaveFileWriter(new IgnoreDisposeStream(outputStream), mediafundationreader.WaveFormat))
                {
                    byte[] buffer = new byte[mediafundationreader.WaveFormat.AverageBytesPerSecond];
                    int read;
                    while ((read = mediafundationreader.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        waveFileWriter.Write(buffer, 0, read);
                    }
                }
                outputStream.GetBuffer();
                outputStream.Position = 0;
                reader = new WaveFileReader(outputStream);
                player.Init(reader);
                player.Play();
                totaltime = reader.TotalTime.TotalSeconds;
                trackBar1.Value = 0;
            }
            buttonpressed = "play";
        }
        private void button4_Click(object sender, EventArgs e)
        {
            if (buttonpressedtrimed == "pause")
            {
                playertrimed.Play();
            }
            else
            {
                if (starttemp <= endtemp)
                {
                    start = starttemp;
                    end = endtemp;
                }
                else
                {
                    start = endtemp;
                    end = starttemp;
                }
                mediafundationreader.Position = 0;
                outputStream = new MemoryStream(); 
                using (var waveFileWriter = new WaveFileWriter(new IgnoreDisposeStream(outputStream), mediafundationreader.WaveFormat))
                {
                    byte[] buffer = new byte[mediafundationreader.WaveFormat.AverageBytesPerSecond];
                    int read;
                    while ((read = mediafundationreader.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        if (mediafundationreader.CurrentTime.TotalSeconds >= start & mediafundationreader.CurrentTime.TotalSeconds <= end)
                        {
                            waveFileWriter.Write(buffer, 0, read);
                        }
                    }
                }
                outputStream.GetBuffer();
                outputStream.Position = 0;
                MediaFoundationReader streamreader = new StreamMediaFoundationReader(outputStream);
                Image image = renderer.Render(streamreader, myRendererSettings);
                pictureBox2.BackgroundImage = image;
                outputStream.GetBuffer();
                outputStream.Position = 0;
                readertrimed = new WaveFileReader(outputStream);
                playertrimed.Init(readertrimed);
                playertrimed.Play();
                totaltimetrimed = readertrimed.TotalTime.TotalSeconds;
                trackBar2.Value = 0;
                myRendererSettings.Width = 1040;
                myRendererSettings.TopHeight = 60;
                myRendererSettings.BottomHeight = 60;
            }
            buttonpressedtrimed = "play";
        }
        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
            pictureBox1.Cursor = Cursors.Cross;
        }
        private void trackBar1_MouseDown(object sender, MouseEventArgs e)
        {
            setprogresssong = true;
        }
        private void trackBar1_MouseUp(object sender, MouseEventArgs e)
        {
            reader.CurrentTime = TimeSpan.FromSeconds(trackBar1.Value / 10000f * totaltime);
            setprogresssong = false;
        }
        private void SetProgressSong()
        {
            try
            {
                if (!setprogresssong)
                {
                    currenttime = reader.CurrentTime.TotalSeconds;
                    trackBar1.Value = (int)(currenttime * 10000f / totaltime);
                }
            }
            catch { }
        }
        private void trackBar2_MouseDown(object sender, MouseEventArgs e)
        {
            setprogresssongtrimed = true;
        }
        private void trackBar2_MouseUp(object sender, MouseEventArgs e)
        {
            readertrimed.CurrentTime = TimeSpan.FromSeconds(trackBar2.Value / 10000f * totaltimetrimed);
            setprogresssongtrimed = false;
        }
        private void SetProgressSongTrimed()
        {
            try
            {
                if (!setprogresssongtrimed)
                {
                    currenttimetrimed = readertrimed.CurrentTime.TotalSeconds;
                    trackBar2.Value = (int)(currenttimetrimed * 10000f / totaltimetrimed);
                }
            }
            catch { }
        }
        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (!temp)
            {
                startX = e.X;
                starttemp = (double)e.X / (double)pictureBox1.ClientSize.Width * mediafundationreader.TotalTime.TotalSeconds;
                temp = true;
            }
            else
            {
                endX = e.X;
                endtemp = (double)e.X / (double)pictureBox1.ClientSize.Width * mediafundationreader.TotalTime.TotalSeconds;
                temp = false;
            }
            Bitmap bmp = new Bitmap(image);
            Graphics graphics = Graphics.FromImage(bmp as Image);
            graphics.FillRectangle(brush, Convert.ToSingle(startX) - 1, 0, 1, this.pictureBox1.Height);
            graphics.FillRectangle(brush, Convert.ToSingle(endX) - 1, 0, 1, this.pictureBox1.Height);
            this.pictureBox1.BackgroundImage = bmp;
        }
        private void button5_Click(object sender, EventArgs e)
        {
            player.Pause();
            buttonpressed = "pause";
        }
        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                player.Stop();
            }
            catch { }
            buttonpressed = "stop";
        }
        private void button7_Click(object sender, EventArgs e)
        {
            playertrimed.Pause();
            buttonpressedtrimed = "pause";
        }
        private void button8_Click(object sender, EventArgs e)
        {
            try
            {
                playertrimed.Stop();
            }
            catch { }
            buttonpressedtrimed = "stop";
        }
        public void Start()
        {
            while (!closed)
            {
                SetProgressSong();
                SetProgressSongTrimed();
                System.Threading.Thread.Sleep(40);
            }
        }
    }
}