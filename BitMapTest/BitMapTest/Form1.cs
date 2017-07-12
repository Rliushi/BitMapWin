using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace BitMapTest
{
    public partial class Form1 : Form
    {
        string[] filelist = { @"D:\workspace\bitfiletest\bz4w01x006y010_1_All.dat", @"D:\workspace\bitfiletest\bz4w01x006y010_16_All.dat", @"D:\workspace\bitfiletest\bz4w01x006y010_32_All.dat", @"D:\workspace\bitfiletest\bz4w01x006y010_64_All.dat", @"D:\workspace\bitfiletest\bz4w01x006y010_128_All.dat", @"D:\workspace\bitfiletest\bz4w01x006y010_256_All.dat" };
        int[] sizelist = { 16384, 1024, 512, 256, 128, 64 };
        List<BitArray>[] coll = { new List<BitArray>(), new List<BitArray>(), new List<BitArray>(), new List<BitArray>(), new List<BitArray>(), new List<BitArray>() };
        //Bitmap[] bmlist = { new Bitmap(1024, 1024), new Bitmap(1024, 1024), new Bitmap(1024, 1024), new Bitmap(1024, 1024), new Bitmap(1024, 1024) };
        Bitmap m_bmp;               //画布中的图像
        Bitmap m_bmp_tmp;
        Bitmap bt;
        Point m_ptCanvas;           //画布原点在设备上的坐标
        Point m_ptCanvasBuf;        //重置画布坐标计算时用的临时变量，用户右键拖拽时使用
        Point m_ptBmp;              //图像位于画布坐标系中的坐标
        float m_nScale = 1.0F;      //缩放比例
        int Flag = 128;             //表示 Scale 数字
        Point m_ptMouseDown;        //鼠标点下是在画布坐标上的坐标


        public Form1()
        {
            InitializeComponent();
            getBaseInfo();         //获取基础信息
            // m_bmp = Getbmp(Flag);
            m_ptCanvas = new Point(pictureBox1.Width / 2, pictureBox1.Height / 2);    //初始偏移量
            this.StartPosition = FormStartPosition.CenterScreen;
            this.pictureBox1.BorderStyle = BorderStyle.FixedSingle;
            this.pictureBox1.BackColor = Color.DarkGray;
            this.pictureBox2.BackColor = Color.DarkGray;
            m_bmp = Getbmp(Flag, 512, 128);        //PicBox1 默认显示Scale 128 的图片
            m_bmp_tmp = Getbmp(256, 256, 64);    // PicBox2 默认显示Scale 256 的图片
            m_ptBmp = new Point(-(m_bmp.Width / 2), -(m_bmp.Height / 2));
            //m_ptBmp = new Point(-(m_bmp.Width / 2),-(m_bmp.Height / 2));

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Flag = 128;
            m_bmp = Getbmp(Flag, 512, 128);
            m_ptCanvas = new Point(pictureBox1.Width / 2, pictureBox1.Height / 2);
            m_ptBmp = new Point(-(m_bmp.Width / 2), -(m_bmp.Height / 2));
            trackBar1.Value = 1;
            m_nScale = 1.0F;
            pictureBox1.Refresh();
        }


        public void fileload(string file, int chunk_size, List<BitArray> lb)
        {
            if (lb.Count != 0)
            {
                return;
            }
            using (FileStream fsRead = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader br = new BinaryReader(fsRead))
                {
                    byte[] chunk;
                    chunk = br.ReadBytes(chunk_size);    //一次读取该 Scale 的一行的 Byte 数
                    while (chunk.Length > 0)
                    {
                        BitArray ba = new BitArray(chunk);
                        lb.Add(ba);
                        chunk = br.ReadBytes(chunk_size);
                    }
                }

            }
        }

        public Bitmap drawmapsub(int Flag, int x, int y, string file, int chunk_size, List<BitArray> lb)
        {
            string[] binNumber = new string[8];
            if (Flag == 256)
            {
                bt = new Bitmap(512, 128);
            }
            else
            {
                bt = new Bitmap(1024, 1024);
            }
            int tmpnumber = 0;                          //表示8个 Bit 的倍数，画图使用，需要跳过的 Bit 数
            int tmpy = 0;                               // 记录行数
            int tmpnum1 = 0;                            // 行起始位置
            int tmpnum2 = 512;                          // 行终止位置
            int tmpnum3 = x - 512 < 0 ? 0 : x - 512;        // 列起始位置
            int tmpnum4 = x + 512;                       // 列终止位置
            int bits = 0;
            int binary = 7;
            if (lb.Count == 0)
            {
                fileload(file, chunk_size, lb);
            }
            if (Flag == 1)
            {
                tmpnum1 = y - 512 > 0 ? y - 512 : 0;    // 不同 Scale 需要不同偏移量计算方式
                tmpnum2 = y + 512 < 32768 ? y + 512 : 32768;
                tmpnum4 = x + 512 > 131072 ? 131072 : x + 512;
            }
            else if (Flag == 128)
            {
                tmpnum2 = 256;
            }
            else if (Flag == 256)
            {
                tmpnum2 = 128;
                tmpnum3 = 0;
                tmpnum4 = 512;
            }
            else if (Flag == 16)
            {
                tmpnum1 = y - 512 > 0 ? y - 512 : 0;
                tmpnum2 = y + 512 < 2048 ? y + 512 : 2048;
                tmpnum4 = x + 512 > 8192 ? 8192 : x + 512;
            }
            else if (Flag == 32)
            {
                tmpnum1 = y - 512;
                tmpnum2 = y + 512;
            }
            for (int i = tmpnum1; i < tmpnum2; i++)   // 表示行数，一列一列读取，总长度 1024
            {
                tmpnumber = 0;
                for (int j = tmpnum3; j < tmpnum4; j++)  // 表示列数，总长度 1024
                {
                    if (lb[i].Get(j) == true)
                        binNumber[binary] = "1";
                    else
                        binNumber[binary] = "0";
                    bits++;
                    binary--;
                    if ((bits % 8) == 0)    //每读取8个 Bit，反转一次，每次画 8 个 Bit
                    {
                        binary = 7;
                        bits = 0;
                        for (int ji = 0; ji <= 7; ji++)
                        {
                            if (binNumber[ji].ToString() == "0")
                            {
                                bt.SetPixel(tmpnumber * 8 + ji, tmpy, Color.FromArgb(255, 255, 255));
                            }
                            else
                            {
                                bt.SetPixel(tmpnumber * 8 + ji, tmpy, Color.FromArgb(0, 0, 0));
                            }
                        }
                        tmpnumber++;
                    }

                }
                tmpy++;
            }
            return bt;
        }

        public Bitmap Getbmp(int Flag, int x, int y)
        {
            if (Flag == 256)
            {
                return drawmapsub(Flag, x, y, filelist[5], sizelist[5], coll[5]);
            }
            if (Flag == 128)
            {
                return drawmapsub(Flag, x, y, filelist[4], sizelist[4], coll[4]);
            }
            else if (Flag == 64)
            {
                return drawmapsub(Flag, x, y, filelist[3], sizelist[3], coll[3]);
            }
            else if (Flag == 32)
            {
                return drawmapsub(Flag, x, y, filelist[2], sizelist[2], coll[2]);
            }
            else if (Flag == 16)
            {
                return drawmapsub(Flag, x, y, filelist[1], sizelist[1], coll[1]);
            }
            else if (Flag == 1)
            {
                return drawmapsub(Flag, x, y, filelist[0], sizelist[0], coll[0]);
            }
            else
            {
                return null;
            }

        }
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            //pictureBox1.Image = null;
            //pictureBox1.Invalidate();
            if (trackBar1.Value == 1)
            {
                Flag = 128;
                m_bmp = Getbmp(Flag, 512, 128);      //重置 PicBox1 的 bitmap 
                m_ptBmp = new Point(-(m_bmp.Width / 2), -(m_bmp.Height / 2));  // 重置 PicBox1 Bitmap 的偏移量
            }
            else if (trackBar1.Value == 2)
            {
                Flag = 64;
                m_bmp = Getbmp(Flag, 1024, 256);
                m_ptBmp = new Point(-(m_bmp.Width / 2), -(m_bmp.Height / 2));
            }
            else if (trackBar1.Value == 3)
            {
                Flag = 32;
                m_bmp = Getbmp(Flag, 2048, 512);
                m_ptBmp = new Point(-(m_bmp.Width / 2), -(m_bmp.Height / 2));
            }
            else if (trackBar1.Value == 4)
            {
                Flag = 16;
                m_bmp = Getbmp(Flag, 4096, 1024);
                m_ptBmp = new Point(-(m_bmp.Width / 2), -(m_bmp.Height / 2));
            }
            else if (trackBar1.Value == 5)
            {
                Flag = 1;
                m_bmp = Getbmp(Flag, 65536, 16384);
                m_ptBmp = new Point(-(m_bmp.Width / 2), -(m_bmp.Height / 2));
            }
            pictureBox1.Refresh();
        }

        /*private void pictureBox1_MouseEnter(object sender, EventArgs e)//当鼠标移到pictuBox内，获取焦点  
        {
            pictureBox1.Focus();
        }*/

        private void pictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (m_nScale <= 0.3 && e.Delta <= 0) return;        //缩小下线
            if (m_nScale >= 4.9 && e.Delta >= 0) return;        //放大上线
            //获取 当前点到画布坐标原点的距离
            SizeF szSub = (Size)m_ptCanvas - (Size)e.Location;
            //当前的距离差除以缩放比还原到未缩放长度
            float tempX = szSub.Width / m_nScale;           //这里
            float tempY = szSub.Height / m_nScale;          //将画布比例
            //还原上一次的偏移                               //按照当前缩放比还原到
            m_ptCanvas.X -= (int)(szSub.Width - tempX);     //没有缩放
            m_ptCanvas.Y -= (int)(szSub.Height - tempY);    //的状态
            //重置距离差为  未缩放状态                       
            szSub.Width = tempX;
            szSub.Height = tempY;
            m_nScale += e.Delta > 0 ? 0.2F : -0.2F;
            //重新计算 缩放并 重置画布原点坐标
            m_ptCanvas.X += (int)(szSub.Width * m_nScale - szSub.Width);
            m_ptCanvas.Y += (int)(szSub.Height * m_nScale - szSub.Height);
            pictureBox1.Refresh();
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {

            // Graphics g = new System.Windows.Forms.PaintEventArgs.Graphics;
            // pictureBox1.Invalidate();
            Graphics g = e.Graphics;

            g.TranslateTransform(m_ptCanvas.X, m_ptCanvas.Y);       //设置坐标偏移，画布中心点偏移
            g.ScaleTransform(m_nScale, m_nScale);                   //设置缩放比，X 轴和 Y轴
            g.DrawImage(m_bmp, m_ptBmp);                            //绘制图像，使用偏移后的坐标系统

            g.ResetTransform();                                     //重置坐标系，还原中心点坐标
        }

        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {

            // Graphics g = new System.Windows.Forms.PaintEventArgs.Graphics;
            // pictureBox1.Invalidate();
            Graphics g = e.Graphics;

            g.TranslateTransform(256, 64);       //设置 Scale 64 坐标偏移
            g.ScaleTransform(m_nScale, m_nScale);                   //设置缩放比
            g.DrawImage(m_bmp_tmp, new Point(-256, -64));                            //绘制Scale 64图像

            g.ResetTransform();                                     //重置坐标系
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {      //如果右键点下    初始化计算要用的临时数据
                m_ptMouseDown = e.Location;
                m_ptCanvasBuf = m_ptCanvas;
            }
            pictureBox1.Focus();
            Point p = pictureBox1.PointToClient(MousePosition);
            label3.Text = p.X.ToString();
            label4.Text = p.Y.ToString();

        }

        private void pictureBox2_MouseDown(object sender, MouseEventArgs e)
        {
            Point tmppoint = new Point();
            if (e.Button == MouseButtons.Left)
            {      //如果左键点下    初始化计算要用的临时数据
                tmppoint = e.Location;
            }
            //显示 SCale 16 的图片
            //int tmp_x = tmppoint.X*16;
            //int tmp_y = tmppoint.Y*16;
            //m_bmp = Getbmp(16, tmp_x, tmp_y);
            int tmp_x = tmppoint.X * 256;            //显示 Scale 1 的图片
            int tmp_y = tmppoint.Y * 256;
            m_bmp = Getbmp(1, tmp_x, tmp_y);
            pictureBox1.Invalidate();
        }
        //平移图像
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {      //移动过程中 中键点下 重置画布坐标系
                //m_ptCanvas = (Point)((Size)m_ptCanvasBuf + ((Size)e.Location - (Size)m_ptMouseDown));
                //m_ptCanvas = new Point(pictureBox1.Width / 2, pictureBox1.Height / 2);
                //Point tmp_point = (Point)(((Size)e.Location - (Size)m_ptMouseDown));
                if (Flag == 1)
                {
                    //m_ptCanvas = new Point(pictureBox1.Width / 2, pictureBox1.Height / 2);
                    Point tmp_point = (Point)(((Size)e.Location - (Size)m_ptMouseDown));
                    m_bmp = Getbmp(Flag, 65536 - (int)(tmp_point.X * 10), 16384 - (int)(tmp_point.Y * 10));
                }
                else if (Flag == 16)
                {
                    //m_ptCanvas = new Point(pictureBox1.Width / 2, pictureBox1.Height / 2);
                    Point tmp_point = (Point)(((Size)e.Location - (Size)m_ptMouseDown));
                    m_bmp = Getbmp(Flag, 4096 - (int)(tmp_point.X * 5), 1024 - (int)(tmp_point.Y * 5));
                }
                else
                {
                    m_ptCanvas = (Point)((Size)m_ptCanvasBuf + ((Size)e.Location - (Size)m_ptMouseDown));
                }

                m_ptBmp = new Point(-(m_bmp.Width / 2), -(m_bmp.Height / 2));
                pictureBox1.Invalidate();
            }
        }

        //获取Wafer基础信息
        public void getBaseInfo()
        {
            string lotname = null ;
            string wafernumber = null;
            string slotnumber = null;
            string deviceid = null;
            string testid = null;
            XmlDocument doc = new XmlDocument();
            doc.Load(@"D:\workspace\bitfiletest\bz4w01_wafer.xml"); //读取 XML 文件
            XmlNode xn = doc.SelectSingleNode("wfbmap-wafer");      //获取根节点
            XmlNodeList xnl = xn.ChildNodes;                       //获取所有子节点
            foreach(XmlNode xnd in xnl)                            //遍历子节点
            {
                //XmlElement xe = (XmlElement)xnd;
                if (xnd.Name.ToString().Equals("header"))          //找到 Header 获取基础信息后跳出循环，因为多个 Header 信息是相同的
                {
                    XmlNodeList xh = xnd.ChildNodes;
                    lotname = xh.Item(1).InnerText;
                    wafernumber = xh.Item(3).InnerText;
                    slotnumber = xh.Item(4).InnerText;
                    deviceid = xh.Item(8).InnerText;
                    testid = xh.Item(10).InnerText;

                    break;
                }
            }
            label15.Text = lotname;
            label12.Text = wafernumber;
            label11.Text = slotnumber;
            label14.Text = deviceid;
            label13.Text = testid;

        }

    }
} 
