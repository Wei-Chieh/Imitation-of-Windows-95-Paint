using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace painter
{
    public partial class 小畫家 : Form
    {
        private string Mode;
        private int YO = 1, brushstyle = 1, eraserstyle = 1, airbrushstyle = 1;
        private Pen p = new Pen(Color.Black);
        private Pen p1 = new Pen(Color.Black);
        private Pen p2 = new Pen(Color.White);
        private Pen dash = new Pen(Color.Black);
        private const int N = 20;
        private Bitmap[] save = new Bitmap[N];
        private int step = 0, first = 0, last = 0;
        private Random random = new Random();
        private Bitmap GG, GG2, copy = new Bitmap(1,1);
        private Graphics g;
        private Color c, c2;
        private bool down = false;
        private int x0, y0, x1, y1, X, Y, xmax, ymax, xmin, ymin, xx, yy;
        private int amp = 1, W, H;
        int[] x, y;
        private int r, theta;
        private bool[,] lalala = new bool[800,600];
        private byte[] pixels;
        private BitmapData bitmapdata = null;
        private IntPtr iptr = IntPtr.Zero;
        private GraphicsPath P;
        private Point[] bezier = new Point[3];
        
        public 小畫家()
        {
            InitializeComponent();
        }

        private void 新增NToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Text = "未命名 - 小畫家";
            openFileDialog1.FileName = "未命名";
            saveFileDialog1.FileName = openFileDialog1.FileName;
            pictureBox1.Image = new Bitmap(800,600);
            pictureBox1.Width = 800;
            pictureBox1.Height = 600;
            pictureBox1.Left = 3;
            pictureBox1.Top = 3;
            g = Graphics.FromImage(pictureBox1.Image);
            g.Clear(Color.White);
            reset(sender, e);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            新增NToolStripMenuItem_Click(sender,e);
        }

        private void 說明HToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void 開啟OToolStripMenuItem_Click(object sender, EventArgs e)
        {
            select_delete();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                using (FileStream fs = File.Open(openFileDialog1.FileName, FileMode.Open, FileAccess.Read))
                {
                    Image I = Image.FromStream(fs);
                    pictureBox1.Width=I.Width;
                    pictureBox1.Height = I.Height;
                    pictureBox1.Image = I;
                    fs.Close();
                }
                string[] p = openFileDialog1.FileName.Split('\\');
                int l = p.Length;
                string s = p[l - 1];
                for (int i = s.Length - 1; i >= 0; i--)
                {
                    if (s[i] == '.')
                    {
                        s = s.Remove(i);
                        break;
                    }
                }
                this.Text = s + " - 小畫家";
            }
            saveFileDialog1.FileName = openFileDialog1.FileName;
            saveFileDialog1.FilterIndex = openFileDialog1.FilterIndex;
            reset(sender, e);
        }

        private void 儲存SToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GG = new Bitmap(pictureBox1.Image);
            if (saveFileDialog1.FileName == "未命名")
            {
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    GG.Save(saveFileDialog1.FileName);
                    string[] p = saveFileDialog1.FileName.Split('\\');
                    int l = p.Length;
                    string s = p[l - 1];
                    for (int i = s.Length - 1; i >= 0; i--)
                    {
                        if (s[i] == '.')
                        {
                            s = s.Remove(i);
                            break;
                        }
                    }
                    this.Text = s + " - 小畫家";
                }
            }
            else
            {
                GG.Save(saveFileDialog1.FileName);
            }
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            
        }

        private void 另存新檔AToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                GG = new Bitmap(pictureBox1.Image);
                GG.Save(saveFileDialog1.FileName);
                string[] p = saveFileDialog1.FileName.Split('\\');
                int l=p.Length;
                string s = p[l - 1];
                for (int i = s.Length - 1; i >= 0; i--)
                {
                    if (s[i] == '.')
                    {
                        s = s.Remove(i);
                        break;
                    }
                }
                this.Text = s + " - 小畫家";
            }
        }

        private void 復原UToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (step != first)
            {
                if (select.Visible == true) select_clear();
                if (step == last) save[last] = new Bitmap(pictureBox1.Image);
                step += (N-1);
                step = step % N;
                pictureBox1.Image = new Bitmap(save[step]);
                W = save[step].Width;
                H = save[step].Height;
                pictureBox1.Width = W * amp;
                pictureBox1.Height = H * amp;
                ooo();
            }
        }

        private void 取消復原RToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (step != last)
            {
                if (select.Visible == true) select_clear();
                step ++;
                step = step % N;
                if (save[step]!=null) pictureBox1.Image = new Bitmap(save[step]);
                W = save[step].Width;
                H = save[step].Height;
                pictureBox1.Width = W * amp;
                pictureBox1.Height = H * amp;
                ooo();
            }
        }

        private void 複製CToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (select.Visible == true) copy = new Bitmap(GG);
        }

        private void 剪下TToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (select.Visible == true)
            {
                copy = new Bitmap(GG);
                select_delete();
            }
        }

        private void 貼上PToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (select.Visible == true) select_clear();
            select.Left = 0;
            select.Top = 0;
            select.Width = copy.Width * amp;
            select.Height = copy.Height * amp;
            select.Image = new Bitmap(copy);
            GG = new Bitmap(copy);
            g = Graphics.FromImage(select.Image);
            select_dash();
            select.Refresh();
            select_ooo();
        }

        private void 清除選取項目ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (select.Visible == true) select_delete();
        }

        private void 清除影像CToolStripMenuItem_Click(object sender, EventArgs e)
        {
            清除選取項目ToolStripMenuItem_Click(sender, e);
        }

        private void 編輯色彩EToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
                if (YO == 1)
                {
                    pictureBox25.BackColor = colorDialog1.Color;
                    p1.Color = colorDialog1.Color;
                }
                else
                {
                    pictureBox24.BackColor = colorDialog1.Color;
                    p2.Color = colorDialog1.Color;
                }
        }

        private void 編輯字型FToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fontDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Font = fontDialog1.Font;
                textBox1.ForeColor = fontDialog1.Color;
            }
        }

        private void 色彩對換IToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GG2 = new Bitmap(GG);
            bitmapdata = GG2.LockBits(new Rectangle(0, 0, GG.Width, GG.Height), ImageLockMode.ReadWrite, GG2.PixelFormat);
            iptr = bitmapdata.Scan0;
            pixels = new byte[GG.Width * GG.Height * 4];
            Marshal.Copy(iptr, pixels, 0, pixels.Length);
            for (int i = 0; i < pixels.Length; i++) 
                if (i % 4 != 3) pixels[i] = (byte)(255 - pixels[i]);
            Marshal.Copy(pixels, 0, iptr, pixels.Length);
            GG2.UnlockBits(bitmapdata);
            GG = new Bitmap(GG2);
            select.Image = new Bitmap(GG);
            g = Graphics.FromImage(select.Image);
            select_dash();
            select.Refresh();
        }

        private void 水平翻轉EToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Point[] YA = new Point[] { new Point(GG.Width, 0), new Point(0, 0), new Point(GG.Width, GG.Height) };
            rotate(YA, 1);
        }

        private void 垂直翻轉VToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Point[] YA = new Point[] { new Point(0, GG.Height), new Point(GG.Width, GG.Height), new Point(0, 0) };
            rotate(YA, 1);
        }

        private void 向右旋轉RToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Point[] YA = new Point[] { new Point(GG.Height, 0), new Point(GG.Height, GG.Width), new Point(0, 0) };
            rotate(YA, 2);
            select_ooo();
        }

        private void 向左旋轉LToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Point[] YA = new Point[] { new Point(0, GG.Width), new Point(0, 0), new Point(GG.Height, GG.Width) };
            rotate(YA, 2);
            select_ooo();
        }

        private void 結束XToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void reset(object sender, EventArgs e)
        {
            ooo();
            select_delete();
            Mode = "鉛筆";
            button6_Click(sender, e);
            dash.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            select_UU.Parent = pictureBox1;
            select_LU.Parent = pictureBox1;
            select_RU.Parent = pictureBox1;
            select_LL.Parent = pictureBox1;
            select_RR.Parent = pictureBox1;
            select_LD.Parent = pictureBox1;
            select_RD.Parent = pictureBox1;
            select_DD.Parent = pictureBox1;
            select.Parent = pictureBox1;
            textBox1.Parent = pictureBox1;
            label1.Parent = pictureBox82;
            label2.Parent = pictureBox82;
            label3.Parent = pictureBox82;
            pictureBox90.Parent = pictureBox82;
            pictureBox83.Parent = pictureBox82;
            pictureBox87.Parent = pictureBox82;
            pictureBox88.Parent = pictureBox82;
            pictureBox89.Parent = pictureBox82;
            pictureBox84.Parent = pictureBox82;
            pictureBox85.Parent = pictureBox82;
            pictureBox86.Parent = pictureBox82;
            label4.Parent = pictureBox83;
            label1.Top = 7;
            label2.Top = 7;
            label3.Top = 7;
            pictureBox84.Top = 0;
            pictureBox85.Top = 0;
            pictureBox86.Top = 0;
            GG = new Bitmap(220, 40);
            g = Graphics.FromImage(GG);
            g.Clear(Color.Transparent);
            g.DrawLine(new Pen(Color.Gray), 185, 1, 185, 23);
            pictureBox84.Image = new Bitmap(GG);
            pictureBox85.Image = new Bitmap(GG);
            pictureBox86.Image = new Bitmap(GG);
            pictureBox90.Image = new Bitmap(GG);
            pictureBox87.Top = 0;
            pictureBox88.Top = 0;
            pictureBox89.Top = 0;
            label4.Left = 20;
            label4.Top = 7;
            step = 0;
            first = 0;
            last = 0;
            amp = 1;
            W = pictureBox1.Width;
            H = pictureBox1.Height;
            label3.Text = W.ToString() + " x " + H.ToString() + "像素";
            if (p1.Width == 1) pictureBox44.BackColor = Color.DodgerBlue;
            p1.EndCap = System.Drawing.Drawing2D.LineCap.Round;
            p1.StartCap = System.Drawing.Drawing2D.LineCap.Round;
            p1.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
            p2.EndCap = System.Drawing.Drawing2D.LineCap.Round;
            p2.StartCap = System.Drawing.Drawing2D.LineCap.Round;
            p2.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
        }

        private void rotate(Point[] YA, int JJ)
        {
            if (JJ == 1) GG2 = new Bitmap(GG.Width, GG.Height);
            else GG2 = new Bitmap(GG.Height, GG.Width);
            g = Graphics.FromImage(GG2);
            g.DrawImage(GG, YA);
            GG = new Bitmap(GG2);
            select.Width = GG2.Width;
            select.Height = GG2.Height;
            select.Image = new Bitmap(GG2);
            g = Graphics.FromImage(select.Image);
            select_dash();
            select.Refresh();
        }

        private void ooo()
        {
            pictureBox2.Left = pictureBox1.Width + pictureBox1.Left;
            pictureBox2.Top = pictureBox1.Height + pictureBox1.Top;
            pictureBox3.Left = pictureBox1.Width + pictureBox1.Left;
            pictureBox3.Top = pictureBox1.Height / 2 + pictureBox1.Top - 9;
            pictureBox4.Left = pictureBox1.Width / 2 + pictureBox1.Left - 9;
            pictureBox4.Top = pictureBox1.Height + pictureBox1.Top;
        }

        private void select_ooo()
        {
            if (!select.Visible)
            {
                select.Visible = true;
                label2.Visible = true;
                label2.Text = GG.Width.ToString() + " x " + GG.Height.ToString() + "像素";
                label2.Refresh();
                select_UU.Visible = true;
                select_LU.Visible = true;
                select_RU.Visible = true;
                select_LL.Visible = true;
                select_RR.Visible = true;
                select_LD.Visible = true;
                select_RD.Visible = true;
                select_DD.Visible = true;
            }
            select_UU.Left = select.Left + (select.Width - select_UU.Width) / 2;
            select_UU.Top = select.Top - select_UU.Height + 2;
            select_RU.Left = select.Left + select.Width - 2;
            select_RU.Top = select.Top - select_RU.Height + 2;
            select_LU.Left = select.Left - select_RU.Width + 2;
            select_LU.Top = select.Top - select_RU.Height + 2;
            select_LL.Left = select.Left - select_RU.Width + 2;
            select_LL.Top = select.Top + (select.Height - select_LL.Height) / 2;
            select_RR.Left = select.Left + select.Width - 2;
            select_RR.Top = select.Top + (select.Height - select_RR.Height) / 2;
            select_RD.Left = select.Left + select.Width - 2;
            select_RD.Top = select.Top + select.Height - 2;
            select_LD.Left = select.Left - select_RU.Width + 2;
            select_LD.Top = select.Top + select.Height - 2;
            select_DD.Left = select.Left + (select.Width - select_DD.Width) / 2;
            select_DD.Top = select.Top + select.Height - 2;
        }

        private void Save()
        {
            save[step] = new Bitmap(pictureBox1.Image);
            step++;
            step = step % N;
            last = step;
            if (step == first)
            {
                first++;
                first = first % N;
            }
        }

        private void setcolor(Color CC, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                pictureBox25.BackColor = CC;
                p1.Color = CC;
            }
            else if (e.Button == MouseButtons.Right)
            {
                pictureBox24.BackColor = CC;
                p2.Color = CC;
            }
        }

        private void select_clear()
        {
            g = Graphics.FromImage(pictureBox1.Image);
            g.DrawImage(GG, select.Left / amp, select.Top / amp, GG.Width, GG.Height);
            pictureBox1.Refresh();
            select_delete();
        }

        private void select_delete()
        {
            select.Visible = false;
            label2.Visible = false;
            select_UU.Visible = false;
            select_LU.Visible = false;
            select_RU.Visible = false;
            select_LL.Visible = false;
            select_RR.Visible = false;
            select_LD.Visible = false;
            select_RD.Visible = false;
            select_DD.Visible = false;
        }

        private void text_clear()
        {
            g = Graphics.FromImage(pictureBox1.Image);
            g.DrawString(textBox1.Text, textBox1.Font, new SolidBrush(textBox1.ForeColor),
                new Rectangle(textBox1.Left, textBox1.Top + 3, textBox1.Width, textBox1.Height - 3));
            pictureBox1.Refresh();
            textBox1.Text = "";
            textBox1.Visible = false;
        }

        private void set(int x,int y)
        {
            x0 = x;
            y0 = y;
        }

        private void setXY()
        {
            x1 = x0;
            y1 = y0;
            X = xx;
            Y = yy;
            if (x1 > X)
            {
                X = x1;
                x1 = xx;
            }
            if (y1 > Y)
            {
                Y = y1;
                y1 = yy;
            }
        }

        private void setselect()
        {
            GG2 = new Bitmap(GG);
            select.Image = new Bitmap(select.Width / amp, select.Height / amp);
            g = Graphics.FromImage(select.Image);
            g.DrawImage(GG2, new Rectangle(0, 0, select.Width / amp, select.Height / amp));
            GG = new Bitmap(select.Image);
            select_dash();
            select_ooo();
            select.Refresh();
            label2.Text = GG.Width.ToString() + " x " + GG.Height.ToString() + "像素";
        }

        private void polygon()
        {
            pictureBox1.Image = new Bitmap(GG);
            Graphics g = Graphics.FromImage(pictureBox1.Image);
            g.DrawLine(p, x1, y1, x0, y0);
            pictureBox1.Refresh();
        }

        private void WidthOn()
        {
            pictureBox27.Visible = true;
            pictureBox41.Visible = true;
            pictureBox42.Visible = true;
            pictureBox43.Visible = true;
            pictureBox44.Visible = true;
            pictureBox45.Visible = true;
            pictureBox48.Visible = true;
            pictureBox47.Visible = true;
            p1.Width = 1;
            p2.Width = 1;
            clearblock1();
            pictureBox44.BackColor = Color.DodgerBlue;
        }

        private void WidthOff()
        {
            pictureBox27.Visible = false;
            pictureBox41.Visible = false;
            pictureBox42.Visible = false;
            pictureBox43.Visible = false;
            pictureBox44.Visible = false;
            pictureBox45.Visible = false;
            pictureBox48.Visible = false;
            pictureBox47.Visible = false;
            clearblock1();
        }

        private void clearblock1()
        {
            pictureBox44.BackColor = Color.Gainsboro;
            pictureBox45.BackColor = Color.Gainsboro;
            pictureBox48.BackColor = Color.Gainsboro;
            pictureBox47.BackColor = Color.Gainsboro;
        }

        private void BrushOn()
        {
            pictureBox40.Visible = true;
            pictureBox46.Visible = true;
            pictureBox49.Visible = true;
            pictureBox50.Visible = true;
            pictureBox51.Visible = true;
            pictureBox52.Visible = true;
            pictureBox53.Visible = true;
            pictureBox54.Visible = true;
            pictureBox55.Visible = true;
            pictureBox56.Visible = true;
            pictureBox57.Visible = true;
            pictureBox58.Visible = true;
            brushstyle = 1;
            clearblock2();
            pictureBox40.BackColor = Color.DodgerBlue;
        }

        private void BrushOff()
        {
            pictureBox40.Visible = false;
            pictureBox46.Visible = false;
            pictureBox49.Visible = false;
            pictureBox50.Visible = false;
            pictureBox51.Visible = false;
            pictureBox52.Visible = false;
            pictureBox53.Visible = false;
            pictureBox54.Visible = false;
            pictureBox55.Visible = false;
            pictureBox56.Visible = false;
            pictureBox57.Visible = false;
            pictureBox58.Visible = false;
            clearblock2();
        }

        private void clearblock2()
        {
            pictureBox40.BackColor = Color.Gainsboro;
            pictureBox46.BackColor = Color.Gainsboro;
            pictureBox49.BackColor = Color.Gainsboro;
            pictureBox50.BackColor = Color.Gainsboro;
            pictureBox51.BackColor = Color.Gainsboro;
            pictureBox52.BackColor = Color.Gainsboro;
            pictureBox53.BackColor = Color.Gainsboro;
            pictureBox54.BackColor = Color.Gainsboro;
            pictureBox55.BackColor = Color.Gainsboro;
            pictureBox56.BackColor = Color.Gainsboro;
            pictureBox57.BackColor = Color.Gainsboro;
            pictureBox58.BackColor = Color.Gainsboro;
        }

        private void AirbrushOn()
        {
            pictureBox79.Visible = true;
            pictureBox80.Visible = true;
            pictureBox81.Visible = true;
            airbrushstyle = 1;
            clearblock4();
            pictureBox79.BackColor = Color.DodgerBlue;
        }

        private void AirbrushOff()
        {
            pictureBox79.Visible = false;
            pictureBox80.Visible = false;
            pictureBox81.Visible = false;
            clearblock4();
        }

        private void clearblock4()
        {
            pictureBox79.BackColor = Color.Gainsboro;
            pictureBox80.BackColor = Color.Gainsboro;
            pictureBox81.BackColor = Color.Gainsboro;
        }

        private void EraserOn()
        {
            pictureBox75.Visible = true;
            pictureBox76.Visible = true;
            pictureBox77.Visible = true;
            pictureBox78.Visible = true;
            eraserstyle = 1;
            clearblock3();
            pictureBox75.BackColor = Color.DodgerBlue;
        }

        private void EraserOff()
        {
            pictureBox75.Visible = false;
            pictureBox76.Visible = false;
            pictureBox77.Visible = false;
            pictureBox78.Visible = false;
            clearblock3();
        }

        private void clearblock3()
        {
            pictureBox75.BackColor = Color.Gainsboro;
            pictureBox76.BackColor = Color.Gainsboro;
            pictureBox77.BackColor = Color.Gainsboro;
            pictureBox78.BackColor = Color.Gainsboro;
        }

        private void clearbutton()
        {
            pictureBox59.BackColor = Color.Silver;
            pictureBox60.BackColor = Color.Silver;
            pictureBox61.BackColor = Color.Silver;
            pictureBox62.BackColor = Color.Silver;
            pictureBox63.BackColor = Color.Silver;
            pictureBox64.BackColor = Color.Silver;
            pictureBox65.BackColor = Color.Silver;
            pictureBox66.BackColor = Color.Silver;
            pictureBox67.BackColor = Color.Silver;
            pictureBox68.BackColor = Color.Silver;
            pictureBox69.BackColor = Color.Silver;
            pictureBox70.BackColor = Color.Silver;
            pictureBox71.BackColor = Color.Silver;
            pictureBox72.BackColor = Color.Silver;
            pictureBox73.BackColor = Color.Silver;
            pictureBox74.BackColor = Color.Silver;
        }

        private void select_drag_start()
        {
            select.Image = new Bitmap(GG);
            select_UU.Visible = false;
            select_LU.Visible = false;
            select_RU.Visible = false;
            select_LL.Visible = false;
            select_RR.Visible = false;
            select_LD.Visible = false;
            select_RD.Visible = false;
            select_DD.Visible = false;
        }

        private void select_drag_finish()
        {
            g = Graphics.FromImage(select.Image);
            select_dash();
            select.Refresh();
            select_UU.Visible = true;
            select_LU.Visible = true;
            select_RU.Visible = true;
            select_LL.Visible = true;
            select_RR.Visible = true;
            select_LD.Visible = true;
            select_RD.Visible = true;
            select_DD.Visible = true;
        }

        private void select_dash()
        {
            g.DrawRectangle(new Pen(Color.White), 0, 0, select.Width / amp - 1, select.Height / amp - 1);
            dash.Color = Color.DodgerBlue;
            g.DrawRectangle(dash, 0, 0, select.Width / amp - 1, select.Height / amp - 1);
            dash.Color = Color.Black;
        }

        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
            label1.Visible = true;
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            label1.Visible = false;
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            xx = e.X / amp;
            yy = e.Y / amp;
            if (Mode == "填入色彩") {
                if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
                {
                    GG = new Bitmap(pictureBox1.Image);
                    c = GG.GetPixel(xx, yy);
                    GG2 = new Bitmap(1, 1);
                    GG2.SetPixel(0, 0, p.Color);
                    c2 = GG2.GetPixel(0, 0);
                    bitmapdata = GG.LockBits(new Rectangle(0, 0, W, H), ImageLockMode.ReadWrite, GG.PixelFormat);
                    iptr = bitmapdata.Scan0;
                    pixels = new byte[W * H * 4];
                    Marshal.Copy(iptr, pixels, 0, pixels.Length);
                    if (c != c2)
                    {
                        Queue<int> q = new Queue<int> {};
                        q.Enqueue((yy * W + xx) * 4);
                        while (q.Count != 0)
                        {
                            int i = q.Dequeue();
                            if ((int)(i / (4 * W)) == (int)((i + 4) / (4 * W)) && c == Color.FromArgb(pixels[i + 7], pixels[i + 6], pixels[i + 5], pixels[i + 4]))
                            {
                                pixels[i + 4] = c2.B;
                                pixels[i + 5] = c2.G;
                                pixels[i + 6] = c2.R;
                                pixels[i + 7] = c2.A;
                                q.Enqueue(i + 4);
                            }
                            if ((int)(i / (4 * W)) == (int)((i - 4) / (4 * W)) && i - 4 >= 0 && c == Color.FromArgb(pixels[i - 1], pixels[i - 2], pixels[i - 3], pixels[i - 4]))
                            {
                                pixels[i - 4] = c2.B;
                                pixels[i - 3] = c2.G;
                                pixels[i - 2] = c2.R;
                                pixels[i - 1] = c2.A;
                                q.Enqueue(i - 4);
                            }
                            if (i + W * 4 < pixels.Length && c == Color.FromArgb(pixels[i + 3 + W * 4], pixels[i + 2 + W * 4], pixels[i + 1 + W * 4], pixels[i + W * 4]))
                            {
                                pixels[i + W * 4] = c2.B;
                                pixels[i + 1 + W * 4] = c2.G;
                                pixels[i + 2 + W * 4] = c2.R;
                                pixels[i + 3 + W * 4] = c2.A;
                                q.Enqueue(i + W * 4);
                            }
                            if (i - W * 4 >= 0 && c == Color.FromArgb(pixels[i + 3 - W * 4], pixels[i + 2 - W * 4], pixels[i + 1 - W * 4], pixels[i - W * 4]))
                            {
                                pixels[i - W * 4] = c2.B;
                                pixels[i + 1 - W * 4] = c2.G;
                                pixels[i + 2 - W * 4] = c2.R;
                                pixels[i + 3 - W * 4] = c2.A;
                                q.Enqueue(i - W * 4);
                            }
                        }
                        Marshal.Copy(pixels, 0, iptr, pixels.Length);
                        GG.UnlockBits(bitmapdata);
                        pictureBox1.Image = new Bitmap(GG);
                    }
                }
            }
            else if (Mode == "挑選顏色")
            {
                GG = new Bitmap(pictureBox1.Image);
                if (e.Button == MouseButtons.Left)
                {
                    pictureBox25.BackColor = GG.GetPixel(xx, yy);
                    p1.Color = pictureBox25.BackColor;
                }
                else if (e.Button == MouseButtons.Right)
                {
                    pictureBox24.BackColor = GG.GetPixel(xx, yy);
                    p2.Color = pictureBox24.BackColor;
                }
            }
            else if (Mode == "多邊形2")
            {
                Graphics g = Graphics.FromImage(pictureBox1.Image);
                g.DrawLine(p, x0, y0, xx, yy);
                pictureBox1.Refresh();
            }
            else if (Mode == "放大鏡")
            {
                if (e.Button == MouseButtons.Left && amp <= 7) amp++;
                else if (e.Button == MouseButtons.Right && amp >= 2) amp--;
                pictureBox1.Width = W * amp;
                pictureBox1.Height = H * amp;
                ooo();
                label4.Text = (amp * 100).ToString() + "%";
            }
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (select.Visible == true) select_clear();
            if (e.Button == MouseButtons.Left) p = p1;
            else if (e.Button == MouseButtons.Right) p = p2;
            if (Mode != "曲線2" && Mode != "曲線3") GG = new Bitmap(pictureBox1.Image);
            g = Graphics.FromImage(pictureBox1.Image);
            if (Mode != "多邊形2")
            {
                if (Mode != "選擇" && Mode != "選擇任意範圍" && Mode != "放大鏡") Save();
                x0 = e.X / amp;
                y0 = e.Y / amp;
            }
            if (Mode == "多邊形")
            {
                x1 = x0;
                y1 = y0;
            }
            else if (Mode == "曲線2")
            {
                g.DrawBezier(p, bezier[0], new Point(xx, yy), new Point(xx, yy), bezier[2]);
                pictureBox1.Refresh();
            }
            else if (Mode == "曲線3")
            {
                g.DrawBezier(p, bezier[0], bezier[1], new Point(xx, yy), bezier[2]);
                pictureBox1.Refresh();
            }
            else if (Mode == "噴槍")
            {
                X = e.X / amp;
                Y = e.Y / amp;
                x0 = Cursor.Position.X;
                y0 = Cursor.Position.Y;
                down = true;
                pictureBox1.Refresh();
            }
            else if (Mode == "選擇")
            {
                label2.Visible = true;
            }
            else if (Mode == "選擇任意範圍")
            {
                P = new GraphicsPath();
                x1 = x0;
                y1 = y0;
                xmin = x0;
                ymin = y0;
                xmax = x0;
                ymax = y0;
            }
            else if (Mode == "粉刷")
            {
                p = new Pen(p.Color);
                if (brushstyle == 1)
                {
                    x = new int[61] { 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 
                        -2, -2, -2, -2, -2, -2, -2, 2, 2, 2, 2, 2, 2, 2, -3, -3, -3, -3, -3, -3, -3, 3, 3, 3, 3, 3, 3, 3, -4, -4, -4, 4, 4, 4};
                    y = new int[61] { 0, 1, 2, 3, 4, -1, -2, -3, -4, 0, 1, 2, 3, 4, -1, -2, -3, -4, 0, 1, 2, 3, 4, -1, -2, -3, -4, 
                        0, 1, 2, 3, -1, -2, -3, 0, 1, 2, 3, -1, -2, -3, 0, 1, 2, 3, -1, -2, -3, 0, 1, 2, 3, -1, -2, -3, 0, 1, -1, 0, 1, -1};
                }
                else if (brushstyle == 2)
                {
                    x = new int[21] { 0, 0, 0, 0, 0, -1, -1, -1, -1, -1, 1, 1, 1, 1, 1, -2, -2, -2, 2, 2, 2 };
                    y = new int[21] { 0, 1, 2, -1, -2, 0, 1, 2, -1, -2, 0, 1, 2, -1, -2, 0, 1, -1, 0, 1, -1 };
                }
                else if (brushstyle == 3)
                {
                    x = new int[5] { 0, 0, 0, -1, 1 };
                    y = new int[5] { 0, -1, 1, 0, 0 };
                }
                else if (brushstyle == 4)
                {
                    x = new int[64] { 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 3, -1, -1, 
                        -1, -1, -1, -1, -1, -1, -2, -2, -2, -2, -2, -2, -2, -2, -3, -3, -3, -3, -3, -3, -3, -3, -4, -4, -4, -4, -4, -4, -4, -4 };
                    y = new int[64] { 0, 1, 2, 3, -1, -2, -3, -4, 0, 1, 2, 3, -1, -2, -3, -4, 0, 1, 2, 3, -1, -2, -3, -4, 0, 1, 2, 3,-1, -2, 
                        -3, -4, 0, 1, 2, 3, -1, -2, -3, -4, 0, 1, 2, 3, -1, -2, -3, -4, 0, 1, 2, 3, -1, -2, -3, -4, 0, 1, 2, 3, -1, -2, -3, -4 };
                }
                else if (brushstyle == 5)
                {
                    x = new int[25] { 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, -1, -1, -1, -1, -1, -2, -2, -2, -2, -2 };
                    y = new int[25] { 0, 1, 2, -1, -2, 0, 1, 2, -1, -2, 0, 1, 2, -1, -2, 0, 1, 2, -1, -2, 0, 1, 2, -1, -2 };
                }
                else if (brushstyle == 6)
                {
                    x = new int[9] { 0, 0, 0, 1, 1, 1, -1, -1, -1 };
                    y = new int[9] { 0, 1, -1, 0, 1, -1, 0, 1, -1 };
                }
                else if (brushstyle == 7)
                {
                    x = new int[15] { 0, 0, 1, 1, 2, 2, 3, -1, -1, -2, -2, -3, -3, -4, -4 };
                    y = new int[15] { 0, -1, -1, -2, -2, -3, -3, 1, 0, 2, 1, 3, 2, 4, 3 };
                }
                else if (brushstyle == 8)
                {
                    x = new int[9] { 0, 0, 1, 1, 2, -1, -1, -2, -2 };
                    y = new int[9] { 0, -1, -1, -2, -2, 1, 0, 2, 1 };
                }
                else if (brushstyle == 9)
                {
                    x = new int[5] { 0, -1, 1, 0, 1 };
                    y = new int[5] { 0, 1, -1, 1, 0 };
                }
                else if (brushstyle == 10)
                {
                    x = new int[15] { 0, 0, 1, 1, 2, 2, 3, -1, -1, -2, -2, -3, -3, -4, -4 };
                    y = new int[15] { 0, 1, 1, 2, 2, 3, 3, -1, 0, -2, -1, -3, -2, -4, -3 };
                }
                else if (brushstyle == 11)
                {
                    x = new int[9] { 0, 0, 1, 1, 2, -1, -1, -2, -2 };
                    y = new int[9] { 0, 1, 1, 2, 2, -1, 0, -2, -1 };
                }
                else if (brushstyle == 12)
                {
                    x = new int[5] { 0, -1, 1, 0, 1 };
                    y = new int[5] { 0, -1, 1, -1, 0 };
                }
            }
            else if (Mode == "橡皮擦")
            {
                p = new Pen(p2.Color);
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            xx = e.X / amp;
            yy = e.Y / amp;
            label1.Text = xx.ToString() + ", " + yy.ToString() + "像素" ;
            label1.Refresh();
            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
            {
                if (Mode == "鉛筆")
                {
                    g.DrawLine(p, xx, yy, x0, y0);
                    x0 = xx;
                    y0 = yy;
                    pictureBox1.Refresh();
                }
                else if (Mode == "粉刷")
                {
                    for (int i = 0; i < x.Length; i++) g.DrawLine(p, xx + x[i], yy + y[i], x0 + x[i], y0 + y[i]);
                    x0 = xx;
                    y0 = yy;
                    pictureBox1.Refresh();
                }
                else if (Mode == "直線" || Mode == "曲線")
                {
                    pictureBox1.Image = new Bitmap(GG);
                    g = Graphics.FromImage(pictureBox1.Image);
                    g.DrawLine(p, x0, y0, xx, yy);
                    pictureBox1.Refresh();
                }
                else if (Mode == "矩形")
                {
                    setXY();
                    pictureBox1.Image = new Bitmap(GG);
                    g = Graphics.FromImage(pictureBox1.Image);
                    if (x1 == X || y1 == Y) g.DrawLine(p, x1, y1, X, Y);
                    else g.DrawRectangle(p, x1, y1, X - x1, Y - y1);
                    pictureBox1.Refresh();
                }
                else if (Mode == "橢圓形")
                {
                    pictureBox1.Image = new Bitmap(GG);
                    g = Graphics.FromImage(pictureBox1.Image);
                    g.DrawEllipse(p, x0, y0, xx - x0, yy - y0);
                    pictureBox1.Refresh();
                }
                else if (Mode == "曲線2")
                {
                    pictureBox1.Image = new Bitmap(GG);
                    g = Graphics.FromImage(pictureBox1.Image);
                    g.DrawBezier(p, bezier[0], new Point(xx, yy), new Point(xx, yy), bezier[2]);
                    pictureBox1.Refresh();
                }
                else if (Mode == "曲線3")
                {
                    pictureBox1.Image = new Bitmap(GG);
                    g = Graphics.FromImage(pictureBox1.Image);
                    g.DrawBezier(p, bezier[0], bezier[1], new Point(xx, yy), bezier[2]);
                    pictureBox1.Refresh();
                }
                else if (Mode == "圓角矩形")
                {
                    pictureBox1.Image = new Bitmap(GG);
                    g = Graphics.FromImage(pictureBox1.Image);
                    int w = xx - x0;
                    int h = yy - y0;
                    if (System.Math.Abs(w) < 5 || System.Math.Abs(h) < 5)
                    {
                        setXY();
                        if (x1 == X || y1 == Y) g.DrawLine(p, x1, y1, X, Y);
                        else g.DrawRectangle(p, x1, y1, X - x1, Y - y1);
                    }
                    else
                    {
                        g.DrawLine(p, x0 + (int)(0.1 * w), y0, x0 + +(int)(0.9 * w), y0);
                        g.DrawLine(p, xx - (int)(0.1 * w), yy, xx + -(int)(0.9 * (w)), yy);
                        g.DrawLine(p, x0, y0 + (int)(0.1 * h), x0, y0 + (int)(0.9 * h));
                        g.DrawLine(p, xx, yy - (int)(0.1 * h), xx, yy - (int)(0.9 * h));
                        if ((int)(0.2 * w) != 0 && (int)(0.2 * h) != 0)
                        {
                            if (w > 0)
                            {
                                if (h > 0)
                                {
                                    g.DrawArc(p, new Rectangle(x0, y0, (int)(0.2 * w), (int)(0.2 * h)), 180, 90);
                                    g.DrawArc(p, new Rectangle(xx - (int)(0.2 * w), y0, (int)(0.2 * w), (int)(0.2 * h)), 270, 90);
                                    g.DrawArc(p, new Rectangle(x0, yy - (int)(0.2 * h), (int)(0.2 * w), (int)(0.2 * h)), 90, 90);
                                    g.DrawArc(p, new Rectangle(xx - (int)(0.2 * w), yy - (int)(0.2 * h), (int)(0.2 * w), (int)(0.2 * h)), 0, 90);
                                }
                                else
                                {
                                    g.DrawArc(p, new Rectangle(x0, y0 + (int)(0.2 * h), (int)(0.2 * w), -(int)(0.2 * h)), 90, 90);
                                    g.DrawArc(p, new Rectangle(xx - (int)(0.2 * w), y0 + (int)(0.2 * h), (int)(0.2 * w), -(int)(0.2 * h)), 0, 90);
                                    g.DrawArc(p, new Rectangle(x0, yy, (int)(0.2 * w), -(int)(0.2 * h)), 180, 90);
                                    g.DrawArc(p, new Rectangle(xx - (int)(0.2 * w), yy, (int)(0.2 * w), -(int)(0.2 * h)), 270, 90);
                                }
                            }
                            else
                            {
                                if (h > 0)
                                {
                                    g.DrawArc(p, new Rectangle(x0 + (int)(0.2 * w), y0, -(int)(0.2 * w), (int)(0.2 * h)), 270, 90);
                                    g.DrawArc(p, new Rectangle(xx, y0, -(int)(0.2 * w), (int)(0.2 * h)), 180, 90);
                                    g.DrawArc(p, new Rectangle(x0 + (int)(0.2 * w), yy - (int)(0.2 * h), -(int)(0.2 * w), (int)(0.2 * h)), 0, 90);
                                    g.DrawArc(p, new Rectangle(xx, yy - (int)(0.2 * h), -(int)(0.2 * w), (int)(0.2 * h)), 90, 90);
                                }
                                else
                                {
                                    g.DrawArc(p, new Rectangle(x0 + (int)(0.2 * w), y0 + (int)(0.2 * h), -(int)(0.2 * w), -(int)(0.2 * h)), 0, 90);
                                    g.DrawArc(p, new Rectangle(xx, y0 + (int)(0.2 * h), -(int)(0.2 * w), -(int)(0.2 * h)), 90, 90);
                                    g.DrawArc(p, new Rectangle(x0 + (int)(0.2 * w), yy, -(int)(0.2 * w), -(int)(0.2 * h)), 270, 90);
                                    g.DrawArc(p, new Rectangle(xx, yy, -(int)(0.2 * w), -(int)(0.2 * h)), 180, 90);
                                }
                            }
                        }
                    }
                    pictureBox1.Refresh();
                }
                else if (Mode == "多邊形")
                {
                    pictureBox1.Image = new Bitmap(GG);
                    g = Graphics.FromImage(pictureBox1.Image);
                    g.DrawLine(p, x0, y0, xx, yy);
                    pictureBox1.Refresh();
                }
                else if (Mode == "多邊形2")
                {
                    pictureBox1.Image = new Bitmap(GG);
                    g = Graphics.FromImage(pictureBox1.Image);
                    g.DrawLine(p, x0, y0, xx, yy);
                    pictureBox1.Refresh();
                }
                else if (Mode == "曲線2")
                {
                    pictureBox1.Image = new Bitmap(GG);
                    g = Graphics.FromImage(pictureBox1.Image);
                    g.DrawBezier(p, bezier[0], new Point(xx, yy), new Point(xx, yy), bezier[2]);
                    pictureBox1.Refresh();
                }
                else if (Mode == "曲線3")
                {
                    pictureBox1.Image = new Bitmap(GG);
                    g = Graphics.FromImage(pictureBox1.Image);
                    g.DrawBezier(p, bezier[0], bezier[1], new Point(xx, yy), bezier[2]);
                    pictureBox1.Refresh();
                }
                else if (Mode == "橡皮擦")
                {
                    for (int i = -eraserstyle - 1; i <= eraserstyle; i++)
                        for (int j = -eraserstyle - 1; j <= eraserstyle; j++)
                            g.DrawLine(new Pen(p2.Color), x0 + i, y0 + j, xx + i, yy + j);
                    x0 = xx;
                    y0 = yy;
                    pictureBox1.Refresh();
                }
                else if (Mode == "選擇")
                {
                    setXY();
                    pictureBox1.Image = new Bitmap(GG);
                    g = Graphics.FromImage(pictureBox1.Image);
                    g.DrawRectangle(dash, x1, y1, X - x1, Y - y1);
                    pictureBox1.Refresh();
                    label2.Text = (X - x1).ToString() + " x " + (Y - y1).ToString() + "像素";
                    label2.Refresh();
                }
                else if (Mode == "選擇任意範圍")
                {
                    g = Graphics.FromImage(pictureBox1.Image);
                    g.DrawLine(new Pen(Color.Black), x1, y1, xx, yy);
                    pictureBox1.Refresh();
                    P.AddLine(x1, y1, xx, yy);
                    x1 = xx;
                    y1 = yy;
                    if (xx < xmin) xmin = xx;
                    if (yy < ymin) ymin = yy;
                    if (xx > xmax) xmax = xx;
                    if (yy > ymax) ymax = yy;
                }
                else if (Mode == "文字" || Mode == "文字2")
                {
                    if (Mode == "文字2")
                    {
                        text_clear();
                        GG = new Bitmap(pictureBox1.Image);
                        Mode = "文字";
                    }
                    setXY();
                    pictureBox1.Image = new Bitmap(GG);
                    g = Graphics.FromImage(pictureBox1.Image);
                    if (x1 == X || y1 == Y) g.DrawLine(dash, x1, y1, X, Y);
                    else g.DrawRectangle(dash, x1, y1, X - x1, Y - y1);
                    pictureBox1.Refresh();
                }
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
            {
                xx = e.X / amp;
                yy = e.Y / amp;
                if (Mode == "多邊形")
                {
                    Mode = "多邊形2";
                    x0 = xx;
                    y0 = yy;
                    GG = new Bitmap(pictureBox1.Image);
                }
                else if (Mode == "多邊形2")
                {
                    if (System.Math.Sqrt((xx - x1) * (xx - x1) + (yy - y1) * (yy - y1)) <= 9)
                    {
                        polygon();
                        Mode = "多邊形";
                    }
                    x0 = xx;
                    y0 = yy;
                    GG = new Bitmap(pictureBox1.Image);
                }
                else if (Mode == "噴槍")
                {
                    down = false;
                }
                else if (Mode == "曲線")
                {
                    bezier[0] = new Point(x0, y0);
                    bezier[2] = new Point(xx, yy);
                    Mode = "曲線2";
                }
                else if (Mode == "曲線2")
                {
                    bezier[1] = new Point(xx, yy);
                    Mode = "曲線3";
                }
                else if (Mode == "曲線3")
                {
                    Mode = "曲線";
                }
                else if (Mode == "選擇")
                {
                    if (x0 != xx && y0 != yy)
                    {
                        setXY();
                        pictureBox1.Image = new Bitmap(GG);
                        Save();
                        select.Left = x1 * amp;
                        select.Top = y1 * amp;
                        select.Width = (X - x1) * amp;
                        select.Height = (Y - y1) * amp;
                        select.Image = new Bitmap(X - x1, Y - y1);
                        g = Graphics.FromImage(select.Image);
                        g.DrawImage(pictureBox1.Image, 0, 0, new Rectangle(x1, y1,X - x1, Y - y1), GraphicsUnit.Pixel);
                        select.Refresh();
                        GG = new Bitmap(select.Image);
                        select_dash();
                        select.Refresh();
                        g = Graphics.FromImage(pictureBox1.Image);
                        g.FillRectangle(new SolidBrush(Color.White), x1, y1, X - x1, Y - y1);
                        pictureBox1.Refresh();
                        select_ooo();
                    }
                    else label2.Visible = false;
                }
                else if (Mode == "選擇任意範圍" && xmin != xmax && ymin != ymax)
                {
                    pictureBox1.Image = new Bitmap(GG);
                    Save();
                    P.AddLine(xx, yy, x0, y0);
                    select.Left = (xmin - 2) * amp;
                    select.Top = (ymin - 2) * amp;
                    select.Width = (xmax - xmin + 5) * amp;
                    select.Height = (ymax - ymin + 5) * amp;
                    select.Image = new Bitmap(select.Width / amp, select.Height / amp);
                    GG = new Bitmap(W, H);
                    g = Graphics.FromImage(GG);
                    g.Clear(Color.Transparent);
                    g.SetClip(P);
                    g.DrawImage(pictureBox1.Image, 0, 0, new Rectangle(0, 0, W, H), GraphicsUnit.Pixel);
                    g = Graphics.FromImage(select.Image);
                    g.DrawImage(GG, 0, 0, new Rectangle(select.Left / amp, select.Top / amp, select.Width / amp, select.Height / amp), GraphicsUnit.Pixel);
                    GG = new Bitmap(select.Image);
                    select_dash();
                    select.Refresh();
                    g = Graphics.FromImage(pictureBox1.Image);
                    g.FillPath(new SolidBrush(Color.White), P);
                    pictureBox1.Refresh();
                    select_ooo();
                }
                else if (Mode == "文字")
                {
                    pictureBox1.Image = new Bitmap(GG);
                    if (x0 == xx)
                    {
                        x1 = x0;
                        X = x0;
                    }
                    if (y0 == yy)
                    {
                        y1 = y0;
                        Y = y0;
                    }
                    if (X - x1 <= 100) textBox1.Width = 100;
                    else textBox1.Width = X - x1;
                    if (Y - y1 <= 24) textBox1.Height = 24;
                    else textBox1.Height = Y - y1;
                    textBox1.Left = x1;
                    textBox1.Top = y1;
                    textBox1.Visible = true;
                    Mode = "文字2";
                }
                else if (Mode == "文字2")
                {
                    text_clear();
                    Mode = "文字";
                }
            }
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (Mode == "噴槍" && down)
            {
                for (int i = 0; i < 15; i++)
                {
                    if (airbrushstyle == 1) r = (int)System.Math.Pow(random.Next(0, 36), 0.5);
                    if (airbrushstyle == 2) r = (int)System.Math.Pow(random.Next(0, 100), 0.5);
                    if (airbrushstyle == 3) r = (int)System.Math.Pow(random.Next(0, 196), 0.5);
                    theta = random.Next(0, 360);
                    x1 = (int)(X + (Cursor.Position.X - x0) / amp + r * System.Math.Cos(theta * System.Math.PI / 180));
                    y1 = (int)(Y + (Cursor.Position.Y - y0) / amp + r * System.Math.Sin(theta * System.Math.PI / 180));
                    if (x1 >= 0 && x1 < W && y1 >= 0 && y1 < H) GG.SetPixel(x1, y1, p.Color);
                }
                pictureBox1.Image = new Bitmap(GG);
            }
        }

        private void pictureBox2_MouseDown(object sender, MouseEventArgs e)
        {
            Save();
            x0 = e.X;
            y0 = e.Y;
            select.Left = 0;
            select.Top = 0;
            select.Width = pictureBox1.Width;
            select.Height = pictureBox1.Height;
            select.Image = new Bitmap(pictureBox1.Image);
            select.Visible = true;
            g = Graphics.FromImage(pictureBox1.Image);
            g.Clear(Color.White);
        }

        private void pictureBox2_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                pictureBox1.Width += e.X - x0;
                pictureBox1.Height += e.Y - y0;
                W += (e.X - x0) / amp;
                H += (e.Y - y0) / amp;
                label3.Text = W.ToString() + " x " + H.ToString() + "像素";
                label3.Refresh();
                pictureBox2.Left += e.X - x0;
                pictureBox2.Top += e.Y - y0;
                pictureBox3.Left += e.X - x0;
                pictureBox3.Top = (pictureBox1.Top + pictureBox2.Top) / 2 - 9;
                pictureBox4.Left = (pictureBox1.Left + pictureBox2.Left) / 2 - 9;
                pictureBox4.Top += e.Y - y0;
            }
        }

        private void pictureBox2_MouseUp(object sender, MouseEventArgs e)
        {
            select.Visible = false;
            GG = new Bitmap((int)(pictureBox1.Width / amp), (int)(pictureBox1.Height / amp));
            g = Graphics.FromImage(GG);
            g.Clear(Color.White);
            g.DrawImage(select.Image, 0, 0, new Rectangle(0, 0, GG.Width, GG.Height), GraphicsUnit.Pixel);
            pictureBox1.Image = new Bitmap(GG);
        }

        private void pictureBox3_MouseDown(object sender, MouseEventArgs e)
        {
            Save();
            x0 = e.X;
            select.Left = 0;
            select.Top = 0;
            select.Width = pictureBox1.Width;
            select.Height = pictureBox1.Height;
            select.Image = new Bitmap(pictureBox1.Image);
            select.Visible = true;
            g = Graphics.FromImage(pictureBox1.Image);
            g.Clear(Color.White);
        }

        private void pictureBox3_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                pictureBox1.Width += e.X - x0;
                W += (e.X - x0) / amp;
                label3.Text = W.ToString() + " x " + H.ToString() + "像素";
                label3.Refresh();
                pictureBox2.Left += e.X - x0;
                pictureBox3.Left += e.X - x0;
                pictureBox4.Left = (pictureBox1.Left + pictureBox2.Left) / 2 - 9;
            }
        }

        private void pictureBox3_MouseUp(object sender, MouseEventArgs e)
        {
            select.Visible = false;
            GG = new Bitmap((int)(pictureBox1.Width / amp), (int)(pictureBox1.Height / amp));
            g = Graphics.FromImage(GG);
            g.Clear(Color.White);
            g.DrawImage(select.Image, 0, 0, new Rectangle(0, 0, GG.Width, GG.Height), GraphicsUnit.Pixel);
            pictureBox1.Image = new Bitmap(GG);
            W = GG.Width;
        }

        private void pictureBox4_MouseDown(object sender, MouseEventArgs e)
        {
            Save();
            y0 = e.Y;
            select.Left = 0;
            select.Top = 0;
            select.Width = pictureBox1.Width;
            select.Height = pictureBox1.Height;
            select.Image = new Bitmap(pictureBox1.Image);
            select.Visible = true;
            g = Graphics.FromImage(pictureBox1.Image);
            g.Clear(Color.White);
        }

        private void pictureBox4_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                pictureBox1.Height += e.Y - y0;
                H += (e.Y - y0) / amp;
                label3.Text = W.ToString() + " x " + H.ToString() + "像素";
                label3.Refresh();
                pictureBox2.Top += e.Y - y0;
                pictureBox3.Top = (pictureBox1.Top + pictureBox2.Top) / 2 - 9;
                pictureBox4.Top += e.Y - y0;
            }
        }

        private void pictureBox4_MouseUp(object sender, MouseEventArgs e)
        {
            select.Visible = false;
            GG = new Bitmap((int)(pictureBox1.Width / amp), (int)(pictureBox1.Height / amp));
            g = Graphics.FromImage(GG);
            g.Clear(Color.White);
            g.DrawImage(select.Image, 0, 0, new Rectangle(0, 0, GG.Width, GG.Height), GraphicsUnit.Pixel);
            pictureBox1.Image = new Bitmap(GG);
            H = GG.Height;
        }

        private void select_MouseDown(object sender, MouseEventArgs e)
        {
            x0 = e.X;
            y0 = e.Y;
            select.Image = new Bitmap(GG);
            select_drag_start();
        }

        private void select_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                select.Left += e.X - x0;
                select.Top += e.Y - y0;
            }
        }

        private void select_MouseUp(object sender, MouseEventArgs e)
        {
            select_drag_finish();
            select_ooo();
        }

        private void 預覽列印VToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {
            clearbutton();
            pictureBox65.BackColor = Color.Gainsboro;
            if (Mode == "多邊形2") polygon();
            else if (select.Visible == true) select_clear();
            else if (Mode == "文字2") text_clear();
            Mode = "鉛筆";
            if (!pictureBox27.Visible) WidthOn();
            if (pictureBox40.Visible) BrushOff();
            if (pictureBox75.Visible) EraserOff();
            if (pictureBox79.Visible) AirbrushOff();
        }

        private void button14_Click(object sender, EventArgs e)
        {
            clearbutton();
            pictureBox67.BackColor = Color.Gainsboro;
            if (Mode == "多邊形2") polygon();
            else if (select.Visible == true) select_clear();
            Mode = "直線";
            if (!pictureBox27.Visible) WidthOn();
            if (pictureBox40.Visible) BrushOff();
            if (pictureBox75.Visible) EraserOff();
            if (pictureBox79.Visible) AirbrushOff();
        }

        private void button13_Click(object sender, EventArgs e)
        {
            clearbutton();
            pictureBox72.BackColor = Color.Gainsboro;
            if (Mode == "多邊形2") polygon();
            else if (select.Visible == true) select_clear();
            else if (Mode == "文字2") text_clear();
            Mode = "曲線";
            if (!pictureBox27.Visible) WidthOn();
            if (pictureBox40.Visible) BrushOff();
            if (pictureBox75.Visible) EraserOff();
            if (pictureBox79.Visible) AirbrushOff();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            clearbutton();
            pictureBox68.BackColor = Color.Gainsboro;
            if (Mode == "多邊形2") polygon();
            else if (select.Visible == true) select_clear();
            else if (Mode == "文字2") text_clear();
            Mode = "矩形";
            if (!pictureBox27.Visible) WidthOn();
            if (pictureBox40.Visible) BrushOff();
            if (pictureBox75.Visible) EraserOff();
            if (pictureBox79.Visible) AirbrushOff();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            clearbutton();
            pictureBox74.BackColor = Color.Gainsboro;
            if (Mode == "多邊形2") polygon();
            else if (select.Visible == true) select_clear();
            else if (Mode == "文字2") text_clear();
            Mode = "圓角矩形";
            if (!pictureBox27.Visible) WidthOn();
            if (pictureBox40.Visible) BrushOff();
            if (pictureBox75.Visible) EraserOff();
            if (pictureBox79.Visible) AirbrushOff();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            clearbutton();
            pictureBox69.BackColor = Color.Gainsboro;
            if (Mode == "多邊形2") polygon();
            else if (select.Visible == true) select_clear();
            else if (Mode == "文字2") text_clear();
            Mode = "橢圓形";
            if (!pictureBox27.Visible) WidthOn();
            if (pictureBox40.Visible) BrushOff();
            if (pictureBox75.Visible) EraserOff();
            if (pictureBox79.Visible) AirbrushOff();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            clearbutton();
            pictureBox73.BackColor = Color.Gainsboro;
            if (Mode == "多邊形2") polygon();
            else if (select.Visible == true) select_clear();
            else if (Mode == "文字2") text_clear();
            Mode = "多邊形";
            if (!pictureBox27.Visible) WidthOn();
            if (pictureBox40.Visible) BrushOff();
            if (pictureBox75.Visible) EraserOff();
            if (pictureBox79.Visible) AirbrushOff();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            clearbutton();
            pictureBox70.BackColor = Color.Gainsboro;
            if (Mode == "多邊形2") polygon();
            else if (select.Visible == true) select_clear();
            else if (Mode == "文字2") text_clear();
            Mode = "粉刷";
            if (pictureBox27.Visible) WidthOff();
            if (pictureBox75.Visible) EraserOff();
            if (pictureBox79.Visible) AirbrushOff();
            BrushOn();
        }

        private void button16_Click(object sender, EventArgs e)
        {
            clearbutton();
            pictureBox66.BackColor = Color.Gainsboro;
            if (Mode == "多邊形2") polygon();
            else if (select.Visible == true) select_clear();
            else if (Mode == "文字2") text_clear();
            Mode = "噴槍";
            if (pictureBox27.Visible) WidthOff();
            if (pictureBox40.Visible) BrushOff();
            if (pictureBox75.Visible) EraserOff();
            AirbrushOn();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            clearbutton();
            pictureBox62.BackColor = Color.Gainsboro;
            if (Mode == "多邊形2") polygon();
            else if (select.Visible == true) select_clear();
            else if (Mode == "文字2") text_clear();
            Mode = "填入色彩";
            if (pictureBox27.Visible) WidthOff();
            if (pictureBox40.Visible) BrushOff();
            if (pictureBox75.Visible) EraserOff();
            if (pictureBox79.Visible) AirbrushOff();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            clearbutton();
            pictureBox61.BackColor = Color.Gainsboro;
            if (Mode == "多邊形2") polygon();
            else if (select.Visible == true) select_clear();
            else if (Mode == "文字2") text_clear();
            Mode = "橡皮擦";
            if (pictureBox27.Visible) WidthOff();
            if (pictureBox40.Visible) BrushOff();
            if (pictureBox79.Visible) AirbrushOff();
            EraserOn();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            clearbutton();
            pictureBox64.BackColor = Color.Gainsboro;
            if (Mode == "多邊形2") polygon();
            else if (select.Visible == true) select_clear();
            else if (Mode == "文字2") text_clear();
            Mode = "放大鏡";
            if (pictureBox27.Visible) WidthOff();
            if (pictureBox40.Visible) BrushOff();
            if (pictureBox75.Visible) EraserOff();
            if (pictureBox79.Visible) AirbrushOff();
        }

        private void button15_Click(object sender, EventArgs e)
        {
            clearbutton();
            pictureBox71.BackColor = Color.Gainsboro;
            if (Mode == "多邊形2") polygon();
            else if (select.Visible == true) select_clear();
            else if (Mode == "文字2") text_clear();
            Mode = "文字";
            if (pictureBox27.Visible) WidthOff();
            if (pictureBox40.Visible) BrushOff();
            if (pictureBox75.Visible) EraserOff();
            if (pictureBox79.Visible) AirbrushOff();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            clearbutton();
            pictureBox63.BackColor = Color.Gainsboro;
            if (Mode == "多邊形2") polygon();
            else if (select.Visible == true) select_clear();
            else if (Mode == "文字2") text_clear();
            Mode = "挑選顏色";
            if (pictureBox27.Visible) WidthOff();
            if (pictureBox40.Visible) BrushOff();
            if (pictureBox75.Visible) EraserOff();
            if (pictureBox79.Visible) AirbrushOff();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            clearbutton();
            pictureBox60.BackColor = Color.Gainsboro;
            if (Mode == "多邊形2") polygon();
            else if (select.Visible == true) select_clear();
            Mode = "選擇";
            if (pictureBox27.Visible) WidthOff();
            if (pictureBox40.Visible) BrushOff();
            if (pictureBox75.Visible) EraserOff();
            if (pictureBox79.Visible) AirbrushOff();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            clearbutton();
            pictureBox59.BackColor = Color.Gainsboro;
            if (Mode == "多邊形2") polygon();
            else if (select.Visible == true) select_clear();
            else if (Mode == "文字2") text_clear();
            Mode = "選擇任意範圍";
            if (pictureBox27.Visible) WidthOff();
            if (pictureBox40.Visible) BrushOff();
            if (pictureBox75.Visible) EraserOff();
            if (pictureBox79.Visible) AirbrushOff();
        }

        private void pictureBox1_MouseHover(object sender, MouseEventArgs e)
        {
        }

        private void pictureBox25_Click(object sender, EventArgs e)
        {
            YO = 1;
        }

        private void pictureBox24_Click(object sender, EventArgs e)
        {
            YO = 2;
        }

        private void pictureBox8_MouseClick(object sender, MouseEventArgs e)
        {
            setcolor(Color.Black, e);
        }

        private void pictureBox9_MouseClick(object sender, MouseEventArgs e)
        {
            setcolor(Color.White, e);
        }

        private void pictureBox11_MouseClick(object sender, MouseEventArgs e)
        {
            setcolor(Color.Gray, e);
        }

        private void pictureBox10_MouseClick(object sender, MouseEventArgs e)
        {
            setcolor(Color.Silver, e);
        }

        private void pictureBox15_MouseClick(object sender, MouseEventArgs e)
        {
            setcolor(Color.Maroon, e);
        }

        private void pictureBox14_MouseClick(object sender, MouseEventArgs e)
        {
            setcolor(Color.Red, e);
        }

        private void pictureBox13_MouseClick(object sender, MouseEventArgs e)
        {
            setcolor(Color.Olive, e);
        }

        private void pictureBox12_MouseClick(object sender, MouseEventArgs e)
        {
            setcolor(Color.Yellow, e);
        }

        private void pictureBox23_MouseClick(object sender, MouseEventArgs e)
        {
            setcolor(Color.Green, e);
        }

        private void pictureBox22_MouseClick(object sender, MouseEventArgs e)
        {
            setcolor(Color.Lime, e);
        }

        private void pictureBox21_MouseClick(object sender, MouseEventArgs e)
        {
            setcolor(Color.Teal, e);
        }

        private void pictureBox20_MouseClick(object sender, MouseEventArgs e)
        {
            setcolor(Color.Cyan, e);
        }

        private void pictureBox19_MouseClick(object sender, MouseEventArgs e)
        {
            setcolor(Color.Navy, e);
        }

        private void pictureBox18_MouseClick(object sender, MouseEventArgs e)
        {
            setcolor(Color.Blue, e);
        }

        private void pictureBox17_MouseClick(object sender, MouseEventArgs e)
        {
            setcolor(Color.Purple, e);
        }

        private void pictureBox16_MouseClick(object sender, MouseEventArgs e)
        {
            setcolor(Color.Fuchsia, e);
        }

        private void pictureBox39_MouseClick(object sender, MouseEventArgs e)
        {
            setcolor(Color.DarkKhaki, e);
        }

        private void pictureBox38_MouseClick(object sender, MouseEventArgs e)
        {
            setcolor(Color.LemonChiffon, e);
        }

        private void pictureBox37_MouseClick(object sender, MouseEventArgs e)
        {
            setcolor(Color.DarkSlateGray, e);
        }

        private void pictureBox36_MouseClick(object sender, MouseEventArgs e)
        {
            setcolor(Color.MediumSpringGreen, e);
        }

        private void pictureBox35_MouseClick(object sender, MouseEventArgs e)
        {
            setcolor(Color.DeepSkyBlue, e);
        }

        private void pictureBox34_MouseClick(object sender, MouseEventArgs e)
        {
            setcolor(Color.PowderBlue, e);
        }

        private void pictureBox33_MouseClick(object sender, MouseEventArgs e)
        {
            setcolor(Color.DarkBlue, e);
        }

        private void pictureBox32_MouseClick(object sender, MouseEventArgs e)
        {
            setcolor(Color.MediumSlateBlue, e);
        }

        private void pictureBox31_MouseClick(object sender, MouseEventArgs e)
        {
            setcolor(Color.MediumBlue, e);
        }

        private void pictureBox30_MouseClick(object sender, MouseEventArgs e)
        {
            setcolor(Color.DarkMagenta, e);
        }

        private void pictureBox29_MouseClick(object sender, MouseEventArgs e)
        {
            setcolor(Color.SaddleBrown, e);
        }

        private void pictureBox28_MouseClick(object sender, MouseEventArgs e)
        {
            setcolor(Color.Orange, e);
        }

        private void pictureBox42_Click(object sender, EventArgs e)
        {
            p1.Width = 1;
            p2.Width = 1;
            clearblock1();
            pictureBox44.BackColor = Color.DodgerBlue;
        }

        private void pictureBox41_Click(object sender, EventArgs e)
        {
            p1.Width = 2;
            p2.Width = 2;
            clearblock1();
            pictureBox45.BackColor = Color.DodgerBlue;
        }

        private void pictureBox27_Click(object sender, EventArgs e)
        {
            p1.Width = 3;
            p2.Width = 3;
            clearblock1();
            pictureBox48.BackColor = Color.DodgerBlue;
        }

        private void pictureBox43_Click(object sender, EventArgs e)
        {
            p1.Width = 4;
            p2.Width = 4;
            clearblock1();
            pictureBox47.BackColor = Color.DodgerBlue;
        }

        private void pictureBox40_Click(object sender, EventArgs e)
        {
            brushstyle = 1;
            clearblock2();
            pictureBox40.BackColor = Color.DodgerBlue;
        }

        private void pictureBox46_Click(object sender, EventArgs e)
        {
            brushstyle = 2;
            clearblock2();
            pictureBox46.BackColor = Color.DodgerBlue;
        }

        private void pictureBox49_Click(object sender, EventArgs e)
        {
            brushstyle = 3;
            clearblock2();
            pictureBox49.BackColor = Color.DodgerBlue;
        }

        private void pictureBox50_Click(object sender, EventArgs e)
        {
            brushstyle = 4;
            clearblock2();
            pictureBox50.BackColor = Color.DodgerBlue;
        }

        private void pictureBox51_Click(object sender, EventArgs e)
        {
            brushstyle = 5;
            clearblock2();
            pictureBox51.BackColor = Color.DodgerBlue;
        }

        private void pictureBox52_Click(object sender, EventArgs e)
        {
            brushstyle = 6;
            clearblock2();
            pictureBox52.BackColor = Color.DodgerBlue;
        }

        private void pictureBox53_Click(object sender, EventArgs e)
        {
            brushstyle = 7;
            clearblock2();
            pictureBox53.BackColor = Color.DodgerBlue;
        }

        private void pictureBox54_Click(object sender, EventArgs e)
        {
            brushstyle = 8;
            clearblock2();
            pictureBox54.BackColor = Color.DodgerBlue;
        }

        private void pictureBox55_Click(object sender, EventArgs e)
        {
            brushstyle = 9;
            clearblock2();
            pictureBox55.BackColor = Color.DodgerBlue;
        }

        private void pictureBox56_Click(object sender, EventArgs e)
        {
            brushstyle = 10;
            clearblock2();
            pictureBox56.BackColor = Color.DodgerBlue;
        }

        private void pictureBox57_Click(object sender, EventArgs e)
        {
            brushstyle = 11;
            clearblock2();
            pictureBox57.BackColor = Color.DodgerBlue;
        }

        private void pictureBox58_Click(object sender, EventArgs e)
        {
            brushstyle = 12;
            clearblock2();
            pictureBox58.BackColor = Color.DodgerBlue;
        }

        private void pictureBox75_Click(object sender, EventArgs e)
        {
            eraserstyle = 1;
            clearblock3();
            pictureBox75.BackColor = Color.DodgerBlue;
        }

        private void pictureBox76_Click(object sender, EventArgs e)
        {
            eraserstyle = 2;
            clearblock3();
            pictureBox76.BackColor = Color.DodgerBlue;
        }

        private void pictureBox77_Click(object sender, EventArgs e)
        {
            eraserstyle = 3;
            clearblock3();
            pictureBox77.BackColor = Color.DodgerBlue;
        }

        private void pictureBox78_Click(object sender, EventArgs e)
        {
            eraserstyle = 4;
            clearblock3();
            pictureBox78.BackColor = Color.DodgerBlue;
        }

        private void pictureBox79_Click(object sender, EventArgs e)
        {
            airbrushstyle = 1;
            clearblock4();
            pictureBox79.BackColor = Color.DodgerBlue;
        }

        private void pictureBox80_Click(object sender, EventArgs e)
        {
            airbrushstyle = 2;
            clearblock4();
            pictureBox80.BackColor = Color.DodgerBlue;
        }

        private void pictureBox81_Click(object sender, EventArgs e)
        {
            airbrushstyle = 3;
            clearblock4();
            pictureBox81.BackColor = Color.DodgerBlue;
        }

        private void select_LU_MouseUp(object sender, MouseEventArgs e)
        {
            select_drag_finish();
            select.Left += e.X - x0;
            select.Top += e.Y - y0;
            select.Width += x0 - e.X;
            select.Height += y0 - e.Y;
            setselect();
        }

        private void select_UU_MouseUp(object sender, MouseEventArgs e)
        {
            select_drag_finish();
            select.Top += e.Y - y0;
            select.Height += y0 - e.Y;
            setselect();
        }

        private void select_RU_MouseUp(object sender, MouseEventArgs e)
        {
            select_drag_finish();
            select.Top += e.Y - y0;
            select.Width += e.X - x0;
            select.Height += y0 - e.Y;
            setselect();
        }

        private void select_LL_MouseUp(object sender, MouseEventArgs e)
        {
            select_drag_finish();
            select.Left += e.X - x0;
            select.Width += x0 - e.X;
            setselect();
        }

        private void select_RR_MouseUp(object sender, MouseEventArgs e)
        {
            select_drag_finish();
            select.Width += e.X - x0;
            setselect();
        }

        private void select_LD_MouseUp(object sender, MouseEventArgs e)
        {
            select_drag_finish();
            select.Left += e.X - x0;
            select.Width += x0 - e.X;
            select.Height += e.Y - y0;
            setselect();
        }

        private void select_DD_MouseUp(object sender, MouseEventArgs e)
        {
            select_drag_finish();
            select.Height += e.Y - y0;
            setselect();
        }

        private void select_RD_MouseUp(object sender, MouseEventArgs e)
        {
            select_drag_finish();
            select.Width += e.X - x0;
            select.Height += e.Y - y0;
            setselect();
        }
    }
}
