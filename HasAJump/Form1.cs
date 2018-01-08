using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace HasAJump
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public static string ADBCmd(string arguments)
        {
            string msg = string.Empty;
            using (Process p = new Process())
            {
                p.StartInfo.FileName = AppDomain.CurrentDomain.BaseDirectory + @"adb.exe";// @"C:\Android\sdk\platform-tools\adb.exe";
                p.StartInfo.Arguments = arguments;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardInput = true;   //重定向标准输入   
                p.StartInfo.RedirectStandardOutput = true;  //重定向标准输出   
                p.StartInfo.RedirectStandardError = true;   //重定向错误输出   
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                msg = p.StandardOutput.ReadToEnd();
                p.Close();
            }
            return msg;
        }

        public static string TakeScreen()
        {
            return ADBCmd("shell /system/bin/screencap -p /sdcard/screenshot.png");
        }

        public static string CopyPNG(int times)
        {
            if (System.IO.File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\" + "img" + times.ToString() + ".png"))
            {
                System.IO.File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\" + "img" + times.ToString() + ".png");
            }
            string root = System.IO.Directory.GetDirectoryRoot(AppDomain.CurrentDomain.BaseDirectory);
            string localPath = "\"" + AppDomain.CurrentDomain.BaseDirectory + "\\" + "img" + times.ToString() + ".png" + "\"";
            return ADBCmd("pull /sdcard/screenshot.png " + localPath);
        }
        public static string SendCal(int time)
        {
            string arg = string.Format("shell input touchscreen swipe 500 900 501 901 {0}", time);//通过短距离滑动模拟长按屏幕
            return ADBCmd(arg);
        }

        public static void Restart(int heigth, int width)
        {
            string arg = string.Format("shell input touchscreen swipe {0} {1} {2} {3} 100", width, heigth, width + 1, heigth + 1);//通过短距离滑动模拟长按屏幕
            ADBCmd(arg);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            var pcolor = txt_play.Text.Split(',').Select(p => Convert.ToInt32(p)).ToArray();
            playcolor = Color.FromArgb(pcolor[0], pcolor[1], pcolor[2]);
            //System.Diagnostics.Debug.WriteLine(Form1.TakeScreen());
            //System.Diagnostics.Debug.WriteLine(Form1.CopyPNG(1));
            //System.Diagnostics.Debug.WriteLine(Form1.SendCal(1000));
            if (!Stop)
            {
                RunWithThread();
            }
            else
            {
                Stop = true;
            }
        }
        private bool Stop = false;
        public void RunWithThread()
        {
            System.Threading.Thread t = new System.Threading.Thread(new System.Threading.ThreadStart(new Action(() =>
            {
                SetButton(false);
                for (var i = 1; i < 40000; i++)
                {
                    if (!BeginJump(i))
                    {
                         SetButton(true);
                        break;
                    }
                    if (Stop)
                    {
                        Stop = false;
                        SetButton(true);
                        SendMsg("停止了");
                        break;
                    }
                    //System.Diagnostics.Debug.WriteLine(i.ToString());
                }
            }
           )));
            t.IsBackground = true;
            t.Start();
        }

        bool CheckCyc()
        {
            if (checkBox1.InvokeRequired)
            {
                Func<bool> func = new Func<bool>(CheckCyc);
                var result = checkBox1.Invoke(func);
                return Convert.ToBoolean(result);
            }
            else
            {
                return checkBox1.Checked;
            }
        }

        int GetValue()
        {
            if (numericUpDown1.InvokeRequired)
            {
                Func<int> func = new Func<int>(GetValue);
                var result = numericUpDown1.Invoke(func);
                return Convert.ToInt32(result);
            }
            else
            {
                return (int)numericUpDown1.Value;
            }
        }

        decimal GetPressTime()
        {
            if (numericUpDown2.InvokeRequired)
            {
                Func<decimal> func = new Func<decimal>(GetPressTime);
                var result = numericUpDown2.Invoke(func);
                return Convert.ToInt32(result);
            }
            else
            {
                return (decimal)numericUpDown2.Value;
            }
        }

        void SendMsg(string msg)
        {
            if (richTextBox1.InvokeRequired)
            {
                richTextBox1.Invoke(new Action(() => { richTextBox1.AppendText(msg); }));
            }
            else
            {
                richTextBox1.AppendText(msg);
            }
        }

        void SetButton(bool isable)
        {
            if (button1.InvokeRequired)
            {
                button1.Invoke(new Action(() =>
                {
                    button1.Enabled = isable;
                }));
            }
            else
            {
                button1.Enabled = isable;
            }
        }

        void AddColumns(int times, int row, int length, int pressTime)
        {
            if (dataGridView1.InvokeRequired)
            {
                Action<int, int, int, int> action = new Action<int, int, int, int>(AddColumns);
                dataGridView1.Invoke(action, new object[] { times, row, length, pressTime });
            }
            else
            {
                if (!dataGridView1.Columns.Contains("col" + times.ToString()))
                {
                    dataGridView1.Columns.Add("col" + times.ToString(), times.ToString() + "跳长度");
                }
                if (dataGridView1.Rows.Count < row)
                {
                    dataGridView1.Rows.Insert(0, 1);
                }

                if (times == 1)
                {
                    dataGridView1[0, 0].Value = "Round:" + row.ToString();
                    dataGridView1[times, 0].Value = length.ToString() + "/" + pressTime.ToString() + "ms";
                }
                else
                {
                    dataGridView1[times, 0].Value = length.ToString() + "/" + pressTime.ToString() + "ms";
                }
            }
        }




        int leng_wait = 0;
        Color playcolor;
        int round = 1;//开始的次数
        public bool BeginJump(int times)
        {
            if (leng_wait < 1000)
                leng_wait = 1000;
            System.Threading.Thread.Sleep(leng_wait);
            Form1.TakeScreen();
            Form1.CopyPNG(1);
            //  System.Threading.Thread.Sleep(500);
            string localPath = AppDomain.CurrentDomain.BaseDirectory + "\\img1.png";
            int leng = 10;
            bool isCyc = false;
            using (Bitmap bitmap = (Bitmap)Bitmap.FromFile(localPath))
            {
                int cyc_Width = 0;
                int width = bitmap.Width;
                int height = bitmap.Height;
                int playPositionx = 0, playPositiony = 0;
                int topX = 0, topY = 0;
                Color firstColor = bitmap.GetPixel(0, height / 3);
                for (var j = height / 5; j < (height * 4 / 5); j++)
                {
                    Color? tempColorX = null;

                    for (var i = 0; i < width; i++)
                    {
                        var tempColor = bitmap.GetPixel(i, j);
                        if ((Math.Abs(firstColor.A - tempColor.A) > 20 || Math.Abs(firstColor.R - tempColor.R) > 20 || Math.Abs(firstColor.G - tempColor.G) > 20 || Math.Abs(firstColor.B - tempColor.B) > 20) && topX == 0)//黑线的顶点,某个什么肯定最黑
                        {
                            tempColorX = tempColor;
                            topX = i;
                            topY = j;
                        }

                        if (tempColorX.HasValue)
                        {
                            if (tempColor.R == tempColorX.Value.R && tempColor.G == tempColorX.Value.G && tempColor.B == tempColorX.Value.B)
                            {
                                cyc_Width++;
                            }
                            else
                            {
                                // SendMsg("调整" + cyc_Width.ToString());
                                if (cyc_Width == 1)
                                {
                                    tempColorX = tempColor;
                                }
                                else
                                {
                                    tempColorX = null;
                                }
                                topX += (int)(cyc_Width * 0.7);//取最后一个点，然后取中心一点
                                if (cyc_Width > 5)
                                {
                                    isCyc = true;
                                    SendMsg("是圆形？");
                                }
                            }
                        }
                        else if (tempColor.Equals(playcolor))
                        {
                            playPositionx = i;
                            playPositiony = j;
                        }
                    }
                }
                if (playPositionx == 0)
                {
                    int re_height = (int)(bitmap.PhysicalDimension.Height * 0.8);
                    int re_width = (int)bitmap.PhysicalDimension.Width / 2;
                    SendMsg("坐标是0？重新开始" + re_height.ToString() + "*" + re_width.ToString());
                    bitmap.Dispose();
                    round++;
                    Restart(re_height, re_width);
                    RunWithThread();
                    return false;// BeginJump(times);
                }
                //playPositionx = playPositionx + 10;

                Color topcolor = bitmap.GetPixel(topX, topY);
                if ((topcolor.R == 0 && topcolor.G == 0 && topcolor.B == 0) || Math.Abs(playPositionx - topX) < 20)
                {
                    SendMsg("画出四个点！超越了谁？");
                    //把角色的长方形给列出来，
                    int px1, py1, px2, py2, px3, py3, px4, py4;
                    int myWidth = 50;
                    int myHight = 110;
                    px1 = px4 = playPositionx - myWidth / 2;
                    px2 = px3 = playPositionx + myWidth / 2;
                    py1 = py2 = playPositiony - myHight;
                    py3 = py4 = playPositiony + 10;
                    topX = 0;//下面需要重新拿来做判断
                    for (var j = height / 5; j < (height * 4 / 5); j++)
                    {
                        for (var i = 0; i < width; i++)
                        {
                            if (i > px1 && i < px2 && j > py1 && j < py4)
                            {
                                //bitmap.SetPixel(i, j, System.Drawing.Color.Black);
                                continue;
                            }
                            var tempColor = bitmap.GetPixel(i, j);
                            if ((Math.Abs(firstColor.A - tempColor.A) > 20 || Math.Abs(firstColor.R - tempColor.R) > 20 || Math.Abs(firstColor.G - tempColor.G) > 20 || Math.Abs(firstColor.B - tempColor.B) > 20) && topX == 0)//黑线的顶点,某个什么肯定最黑
                            {
                                topX = i;
                                topY = j;
                                topcolor = tempColor;//如果不给判断，那么下面根本就过不去
                            }
                        }
                    }
                    //bitmap.SetPixel(px1, py1, System.Drawing.Color.Yellow);
                    //bitmap.SetPixel(px2, py2, System.Drawing.Color.Yellow);
                    //bitmap.SetPixel(px3, py3, System.Drawing.Color.Yellow);
                    //bitmap.SetPixel(px4, py4, System.Drawing.Color.Yellow);
                    if (Math.Abs(playPositionx - topX) < 50)
                    {
                        SendMsg("被干扰了！重新获取一次！如果重复出现，请手动跳！！");
                        bitmap.Dispose();
                        System.Threading.Thread.Sleep(2000);
                        return BeginJump(times);
                    }
                }


                leng = PointLeng(playPositionx, playPositiony, topX, topY);
                //bitmap.SetPixel(playPositionx, playPositiony, System.Drawing.Color.Red);
                //bitmap.SetPixel(topX, topY, System.Drawing.Color.Red);
                //bitmap.Save(AppDomain.CurrentDomain.BaseDirectory + times.ToString() + "-" + leng.ToString() + ".png", ImageFormat.Png);

            }


            //if (leng == 368)
            //{
            //    SendMsg("扯蛋了？？？？");
            //    return false;
            //}
            ////if (leng > 320)
            //{
            //    leng = (int)(leng * 3);// (1 + trackBar1.Value / 50);
            //}
            //else
            //{
            int leng_s = leng;
            leng = (int)(leng * GetPressTime());// - trackBar1.Value;// (1 + trackBar1.Value / 50);
                                                //}

            if (isCyc && CheckCyc())
            {
                leng += Math.Abs(GetValue());
            }
            else
            {
                leng += GetValue();
            }
            string msg = Form1.SendCal(leng);
            AddColumns(times, round, leng_s, leng);
            leng_wait = leng;
            // System.Diagnostics.Debug.WriteLine(msg);
            //System.Threading.Thread.Sleep(leng);
            //Form1.TakeScreen();
            //Form1.CopyPNG(10000 + times);
            //System.Threading.Thread.Sleep(500);
            //System.Diagnostics.Debug.WriteLine(leng);
            return true;
        }

        public int PointLeng(int x1, int y1, int x2, int y2)
        {
            return (int)Math.Abs(Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2)));
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Stop = true;
        }


        //private static List<ArgbPixel> GetPixelListFromBitmap(Bitmap sourceImage)
        //{
        //    BitmapData sourceData = sourceImage.LockBits(new Rectangle(0, 0,
        //                sourceImage.Width, sourceImage.Height),
        //                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);


        //    byte[] sourceBuffer = new byte[sourceData.Stride * sourceData.Height];
        //    Marshal.Copy(sourceData.Scan0, sourceBuffer, 0, sourceBuffer.Length);
        //    sourceImage.UnlockBits(sourceData);


        //    List<ArgbPixel> pixelList = new List<ArgbPixel>(sourceBuffer.Length / 4);


        //    using (MemoryStream memoryStream = new MemoryStream(sourceBuffer))
        //    {
        //        memoryStream.Position = 0;
        //        BinaryReader binaryReader = new BinaryReader(memoryStream);

        //        while (memoryStream.Position + 4 <= memoryStream.Length)
        //        {
        //            ArgbPixel pixel = new ArgbPixel(binaryReader.ReadBytes(4));
        //            pixelList.Add(pixel);
        //        }


        //        binaryReader.Close();
        //    }


        //    return pixelList;
        //}
        //public static Bitmap SwapColors(this Bitmap sourceImage,
        //                        ColourSwapType swapType,
        //                        byte fixedValue = 0)
        //{
        //    List<ArgbPixel> pixelListSource = GetPixelListFromBitmap(sourceImage);


        //    List<ArgbPixel> pixelListResult = null;


        //    switch (swapType)
        //    {
        //        case ColourSwapType.ShiftRight:
        //            {
        //                pixelListResult = (from t in pixelListSource
        //                                   select new ArgbPixel
        //                                   {
        //                                       blue = t.red,
        //                                       red = t.green,
        //                                       green = t.blue,
        //                                       alpha = t.alpha
        //                                   }).ToList();
        //                break;
        //            }
        //        case ColourSwapType.ShiftLeft:
        //            {
        //                pixelListResult = (from t in pixelListSource
        //                                   select new ArgbPixel
        //                                   {
        //                                       blue = t.green,
        //                                       red = t.blue,
        //                                       green = t.red,
        //                                       alpha = t.alpha
        //                                   }).ToList();
        //                break;
        //            }
        //        case ColourSwapType.SwapBlueAndRed:
        //            {
        //                pixelListResult = (from t in pixelListSource
        //                                   select new ArgbPixel
        //                                   {
        //                                       blue = t.red,
        //                                       red = t.blue,
        //                                       green = t.green,
        //                                       alpha = t.alpha
        //                                   }).ToList();
        //                break;
        //            }

        //        case ColourSwapType.SwapBlueAndRedFixGreen:
        //            {
        //                pixelListResult = (from t in pixelListSource
        //                                   select new ArgbPixel
        //                                   {
        //                                       blue = t.red,
        //                                       red = t.blue,
        //                                       green = fixedValue,
        //                                       alpha = t.alpha
        //                                   }).ToList();
        //                break;
        //            }
        //        case ColourSwapType.SwapBlueAndGreen:
        //            {
        //                pixelListResult = (from t in pixelListSource
        //                                   select new ArgbPixel
        //                                   {
        //                                       blue = t.green,
        //                                       red = t.red,
        //                                       green = t.blue,
        //                                       alpha = t.alpha
        //                                   }).ToList();
        //                break;
        //            }
        //        case ColourSwapType.SwapBlueAndGreenFixRed:
        //            {
        //                pixelListResult = (from t in pixelListSource
        //                                   select new ArgbPixel
        //                                   {
        //                                       blue = t.green,
        //                                       red = fixedValue,
        //                                       green = t.blue,
        //                                       alpha = t.alpha
        //                                   }).ToList();
        //                break;
        //            }
        //        case ColourSwapType.SwapRedAndGreen:
        //            {
        //                pixelListResult = (from t in pixelListSource
        //                                   select new ArgbPixel
        //                                   {
        //                                       blue = t.blue,
        //                                       red = t.green,
        //                                       green = t.red,
        //                                       alpha = t.alpha
        //                                   }).ToList();
        //                break;
        //            }
        //        case ColourSwapType.SwapRedAndGreenFixBlue:
        //            {
        //                pixelListResult = (from t in pixelListSource
        //                                   select new ArgbPixel
        //                                   {
        //                                       blue = fixedValue,
        //                                       red = t.green,
        //                                       green = t.red,
        //                                       alpha = t.alpha
        //                                   }).ToList();
        //                break;
        //            }
        //    }


        //    Bitmap resultBitmap = GetBitmapFromPixelList(pixelListResult,
        //                            sourceImage.Width, sourceImage.Height);


        //    return resultBitmap;
        //}

        //private static Bitmap GetBitmapFromPixelList(List<ArgbPixel> pixelList, int width, int height)
        //{
        //    Bitmap resultBitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);


        //    BitmapData resultData = resultBitmap.LockBits(new Rectangle(0, 0,
        //                resultBitmap.Width, resultBitmap.Height),
        //                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);


        //    byte[] resultBuffer = new byte[resultData.Stride * resultData.Height];


        //    using (MemoryStream memoryStream = new MemoryStream(resultBuffer))
        //    {
        //        memoryStream.Position = 0;
        //        BinaryWriter binaryWriter = new BinaryWriter(memoryStream);


        //        foreach (ArgbPixel pixel in pixelList)
        //        {
        //            binaryWriter.Write(pixel.GetColorBytes());
        //        }


        //        binaryWriter.Close();
        //    }


        //    Marshal.Copy(resultBuffer, 0, resultData.Scan0, resultBuffer.Length);
        //    resultBitmap.UnlockBits(resultData);


        //    return resultBitmap;
        //}

        //public static Bitmap FlipPixels(this Bitmap sourceImage)
        //{
        //    List<ArgbPixel> pixelList = GetPixelListFromBitmap(sourceImage);


        //    pixelList.Reverse();


        //    Bitmap resultBitmap = GetBitmapFromPixelList(pixelList,
        //                        sourceImage.Width, sourceImage.Height);


        //    return resultBitmap;
        //}

    }


    public class ArgbPixel
    {
        public byte blue = 0;
        public byte green = 0;
        public byte red = 0;
        public byte alpha = 0;


        public ArgbPixel()
        {

        }


        public ArgbPixel(byte[] colorComponents)
        {
            blue = colorComponents[0];
            green = colorComponents[1];
            red = colorComponents[2];
            alpha = colorComponents[3];
        }


        public byte[] GetColorBytes()
        {
            return new byte[] { blue, green, red, alpha };
        }
    }

    public enum ColourSwapType
    {
        ShiftRight,
        ShiftLeft,
        SwapBlueAndRed,
        SwapBlueAndRedFixGreen,
        SwapBlueAndGreen,
        SwapBlueAndGreenFixRed,
        SwapRedAndGreen,
        SwapRedAndGreenFixBlue
    }

}
