using System;
using System.Drawing;
using System.Windows.Forms;
using NAudio.Wave;

namespace MusicVisualizer
{
    public partial class Form1 : Form
    {
        private WasapiLoopbackCapture? capture;
        private WaveBuffer? buffer;
        private System.Windows.Forms.Timer timer;
        private Bitmap? trailBuffer;
        private Graphics? trailGraphics;

        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.BackColor = Color.Black;
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;

            trailBuffer = new Bitmap(Screen.PrimaryScreen!.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            trailGraphics = Graphics.FromImage(trailBuffer);
            trailGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            try
            {
                capture = new WasapiLoopbackCapture();
                capture.DataAvailable += OnDataAvailable;
                capture.StartRecording();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Audio initialization failed:\n" + ex.Message);
                Environment.Exit(1);
            }

            timer = new System.Windows.Forms.Timer();
            timer.Interval = 16;
            timer.Tick += (s, e) => Invalidate();
            timer.Start();
        }

        private void OnDataAvailable(object? sender, WaveInEventArgs e)
        {
            buffer = new WaveBuffer(e.Buffer);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (trailBuffer == null || trailGraphics == null)
                return;

            int width = this.Width;
            int height = this.Height;
            float centerY = height / 2f;

            // Fade out the old trail (persistence simulation)
            using (SolidBrush fade = new SolidBrush(Color.FromArgb(25, 0, 0, 0)))
            {
                trailGraphics.FillRectangle(fade, 0, 0, width, height);
            }

            // Draw new waveform onto the buffer
            if (buffer?.FloatBuffer != null && buffer.FloatBuffer.Length > 0)
            {
                int samplesPerPixel = Math.Max(1, buffer.FloatBuffer.Length / width);
                float scale = height * 0.4f;

                using var pen = new Pen(Color.Lime, 1);

                for (int x = 1; x < width; x++)
                {
                    int i1 = (x - 1) * samplesPerPixel;
                    int i2 = x * samplesPerPixel;
                    if (i2 >= buffer.FloatBuffer.Length) break;

                    float sample1 = buffer.FloatBuffer[i1];
                    float sample2 = buffer.FloatBuffer[i2];
                    sample1 = float.IsFinite(sample1) ? Math.Clamp(sample1, -1f, 1f) : 0f;
                    sample2 = float.IsFinite(sample2) ? Math.Clamp(sample2, -1f, 1f) : 0f;

                    float y1 = centerY + sample1 * scale;
                    float y2 = centerY + sample2 * scale;
                    y1 = Math.Clamp(y1, 0f, height - 1);
                    y2 = Math.Clamp(y2, 0f, height - 1);

                    trailGraphics.DrawLine(pen, x - 1, y1, x, y2);
                }
            }

            // Draw the trail buffer to the screen
            e.Graphics.DrawImageUnscaled(trailBuffer, 0, 0);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            timer?.Stop();
            capture?.StopRecording();
            capture?.Dispose();
            trailGraphics?.Dispose();
            trailBuffer?.Dispose();
            base.OnFormClosed(e);
        }
    }
}
