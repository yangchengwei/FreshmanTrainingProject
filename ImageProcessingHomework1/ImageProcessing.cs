using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FreshmanTrainingProject
{
    public partial class ImageProcessing : Form
    {
        //##### global variable #####
        private Bitmap srcBitmap = null;//原始的Bitmap
        private Bitmap showBitmap1 = null;//顯示於pictureBox2的Bitmap
        private Bitmap showBitmap2 = null;//顯示於pictureBox2的Bitmap
        private Bitmap showBitmap3 = null;//顯示於pictureBox3的Bitmap
        private Bitmap showBitmap4 = null;//顯示於pictureBox4的Bitmap
        private int srcHeight = 0;
        private int srcWidth = 0;
        private int depth = 256;
        private string state = "start";
        private int[,] sobelFilterX = { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
        private int[,] sobelFilterY = { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };
        //private double[,] EroDilaKernel = { { 0.5455, 0.9717, 0.5455 }, { 0.9717, 1, 0.9717 }, { 0.5455, 0.9717, 0.5455 } }; private double EroDilaSum = 7.0688;
        private double[,] EroDilaKernel = { { 0.125, 0.5, 0.125 }, { 0.5, 1, 0.5 }, { 0.125, 0.5, 0.125 } }; private double EroDilaSum = 3.5;
        //private double[,] EroDilaKernel = { { 0, 1, 0 }, { 1, 1, 1 }, { 0, 1, 0 } }; private double EroDilaSum = 5;
        //private double[,] EroDilaKernel = { { 1, 1, 1 }, { 1, 1, 1 }, { 1, 1, 1 } }; private double EroDilaSum = 9;
        private double EroDilaThreshold = 1;
        private double[,] Morphological = null;
        private int[,] Gray = null;
        private int[,] R = null;
        private int[,] G = null;
        private int[,] B = null;

        public ImageProcessing()
        {
            InitializeComponent();
        }

        //##### function #####
        private void init()
        {
            copyBitmap(ref showBitmap2, ref showBitmap1);
            copyBitmap(ref showBitmap3, ref showBitmap1);
            copyBitmap(ref showBitmap4, ref showBitmap1);
        }
        private void intArraySort(int[] array, int size)//Insertion Sort
        {
            int i, j;
            int num;

            for (i = 1; i < size; i++)
            {
                num = array[i];
                for (j = i - 1; j >= 0; j--)
                {
                    if (num > array[j])
                    {
                        array[j + 1] = array[j];
                    }
                    else
                    {
                        break;
                    }
                }
                array[j + 1] = num;
            }
        }
        private Bitmap RGBToGray(Bitmap RGBmap)//maybe dont need
        {
            Bitmap graymap = new Bitmap(srcBitmap);
            Color color;
            int x, y;//指定位置用的變數
            int R, G, B;//修改顏色用的變數
            int average;//RGB的平均

            for (y = 0; y < RGBmap.Height; y++)
            {
                for (x = 0; x < RGBmap.Width; x++)
                {
                    color = RGBmap.GetPixel(x, y);

                    R = Convert.ToInt32(color.R);
                    G = Convert.ToInt32(color.G);
                    B = Convert.ToInt32(color.B);
                    average = (R + G + B) / 3;

                    graymap.SetPixel(x, y, Color.FromArgb(average, average, average));
                }
            }
            return graymap;
        }
        private Bitmap drawHistogram(Bitmap image)
        {
            int x, y;//指定位置用的變數
            int gray;
            int maxSrcHis = 0;
            int[] srcHis = Enumerable.Repeat(0, depth).ToArray();//顏色分布函數
            Color color;
            Bitmap histogram = new Bitmap(300, 300);
            int hisHeight = histogram.Height;
            int hisWidth = histogram.Width;

            for (y = 0; y < srcHeight; y++)
            {
                for (x = 0; x < srcWidth; x++)
                {
                    color = image.GetPixel(x, y);
                    gray = (Convert.ToInt32(color.R) + Convert.ToInt32(color.G) + Convert.ToInt32(color.B)) / 3;
                    srcHis[gray]++;
                }
            }

            for (x = 0; x < depth; x++)
            {
                if (maxSrcHis < srcHis[x])
                {
                    maxSrcHis = srcHis[x];
                }
            }

            for (x = 0; x < depth; x++)
            {
                srcHis[x] = srcHis[x] * hisHeight / maxSrcHis;
            }

            for (x = 0; x < hisWidth; x++)
            {
                for (y = 0; y < hisHeight; y++)
                {
                    if (y % (500 * hisHeight / maxSrcHis) == 0 && x % 5 != 0)
                    {
                        histogram.SetPixel(x, hisHeight - 1 - y, Color.Red);
                    }
                    else if (x >= depth)
                    {
                        histogram.SetPixel(x, hisHeight - 1 - y, Color.White);
                    }
                    else if (y < srcHis[x])
                    {
                        histogram.SetPixel(x, hisHeight - 1 - y, Color.Black);
                    }
                    else
                    {
                        histogram.SetPixel(x, hisHeight - 1 - y, Color.White);
                    }
                }
            }

            return histogram;
        }
        private void copyBitmap(ref Bitmap imageA, ref Bitmap imageB)
        {
            srcHeight = imageB.Height;
            srcWidth = imageB.Width;

            imageA = new Bitmap(imageB, srcWidth, srcHeight);

            //for (int y = 0; y < srcHeight; y++)
            //{
            //    for (int x = 0; x < srcWidth; x++)
            //    {
            //        imageA.SetPixel( x, y, imageB.GetPixel( x, y ) );
            //    }
            //}

            return;
        }
        private void loadBitmapToRGBGray(Bitmap inputBitmap)
        {
            Color color;

            srcWidth = inputBitmap.Width;
            srcHeight = inputBitmap.Height;

            Gray = new int[srcWidth, srcHeight];
            R = new int[srcWidth, srcHeight];
            G = new int[srcWidth, srcHeight];
            B = new int[srcWidth, srcHeight];
            Morphological = new double[srcWidth, srcHeight];

            for (int y = 0; y < srcHeight; y++)
            {
                for (int x = 0; x < srcWidth; x++)
                {
                    color = inputBitmap.GetPixel(x, y);
                    R[x, y] = Convert.ToInt32(color.R);
                    G[x, y] = Convert.ToInt32(color.G);
                    B[x, y] = Convert.ToInt32(color.B);
                    Gray[x, y] = Convert.ToInt32(Math.Round((R[x, y] + G[x, y] + B[x, y]) / 3.0));
                }
            }

        }
        private void setColor(Bitmap image, int i, int j, int colorCase)
        {
            switch (colorCase)
            {
                case 0:
                    // black black
                    // white white
                    image.SetPixel(i, j, Color.Black);
                    image.SetPixel(i, j + 1, Color.Black);
                    image.SetPixel(i + 1, j, Color.Transparent);
                    image.SetPixel(i + 1, j + 1, Color.Transparent);
                    break;
                case 1:
                    // black white
                    // black white
                    image.SetPixel(i, j, Color.Black);
                    image.SetPixel(i, j + 1, Color.Transparent);
                    image.SetPixel(i + 1, j, Color.Black);
                    image.SetPixel(i + 1, j + 1, Color.Transparent);
                    break;
                case 2:
                    // black white
                    // white black
                    image.SetPixel(i, j, Color.Black);
                    image.SetPixel(i, j + 1, Color.Transparent);
                    image.SetPixel(i + 1, j, Color.Transparent);
                    image.SetPixel(i + 1, j + 1, Color.Black);
                    break;
                case 3:
                    // white black
                    // black white
                    image.SetPixel(i, j, Color.Transparent);
                    image.SetPixel(i, j + 1, Color.Black);
                    image.SetPixel(i + 1, j, Color.Black);
                    image.SetPixel(i + 1, j + 1, Color.Transparent);
                    break;
                case 4:
                    // white black
                    // white black
                    image.SetPixel(i, j, Color.Transparent);
                    image.SetPixel(i, j + 1, Color.Black);
                    image.SetPixel(i + 1, j, Color.Transparent);
                    image.SetPixel(i + 1, j + 1, Color.Black);
                    break;
                case 5:
                    // white white
                    // black black
                    image.SetPixel(i, j, Color.Transparent);
                    image.SetPixel(i, j + 1, Color.Transparent);
                    image.SetPixel(i + 1, j, Color.Black);
                    image.SetPixel(i + 1, j + 1, Color.Black);
                    break;
                default:
                    // black black
                    // black black
                    image.SetPixel(i, j, Color.Black);
                    image.SetPixel(i, j + 1, Color.Black);
                    image.SetPixel(i + 1, j, Color.Black);
                    image.SetPixel(i + 1, j + 1, Color.Black);
                    break;
            }
        }

        //##### event #####
        private void loadImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = @"Bitmap文件(*.bmp)|*.bmp|Jpeg文件(*.jpg)|*.jpg|所有合適文件(*.bmp,*.jpg)|*.bmp;*.jpg ";

            if (DialogResult.OK == ofd.ShowDialog())
            {
                srcBitmap = new Bitmap(ofd.FileName);
                showBitmap1 = new Bitmap(srcBitmap);
                showBitmap2 = new Bitmap(srcBitmap);
                showBitmap3 = new Bitmap(srcBitmap);
                showBitmap4 = new Bitmap(srcBitmap);

                srcHeight = srcBitmap.Height;
                srcWidth = srcBitmap.Width;

                pictureBox1.Image = showBitmap1;
                pictureBox2.Image = null;
                pictureBox3.Image = null;
                pictureBox4.Image = null;

                label1.Text = "Source Image";
                label2.Text = "";
                label3.Text = "";
                label4.Text = "";

                colorExtraction.Enabled = true;
                colorTransformation.Enabled = true;
                meanFilter.Enabled = true;
                medianFilter.Enabled = true;
                histogramEqualization.Enabled = true;
                thresholding.Enabled = true;
                scrollBar.Enabled = true;
                scrollBarValue.Enabled = true;
                sobelEdgeDetection.Enabled = true;
                edgeOverlapping.Enabled = true;
                labelRed.Enabled = true;
                labelGreen.Enabled = true;
                labelBlue.Enabled = true;
                numericRed.Enabled = true;
                numericGreen.Enabled = true;
                numericBlue.Enabled = true;
                erosion.Enabled = true;
                dilation.Enabled = true;
                rotation.Enabled = true;
                labelDegree.Enabled = true;
                numericDegree.Enabled = true;
                stretching.Enabled = true;
                labelHorizontal.Enabled = true;
                labelVertical.Enabled = true;
                numericHorizontal.Enabled = true;
                numericVertical.Enabled = true;

                imageEncryption.Enabled = true;
                resetSourceImage.Enabled = true;
                saveImage.Enabled = true;

                loadBitmapToRGBGray(srcBitmap);

                state = "loadPicture_Click";
            }
        }

        private void colorExtraction_Click(object sender, EventArgs e)
        {
            init();
            int x, y;//指定位置用的變數

            for (y = 0; y < srcHeight; y++)
            {
                for (x = 0; x < srcWidth; x++)
                {
                    showBitmap2.SetPixel(x, y, Color.FromArgb(R[x, y], R[x, y], R[x, y]));
                    showBitmap3.SetPixel(x, y, Color.FromArgb(G[x, y], G[x, y], G[x, y]));
                    showBitmap4.SetPixel(x, y, Color.FromArgb(B[x, y], B[x, y], B[x, y]));
                }
            }

            pictureBox1.Image = showBitmap1;
            pictureBox2.Image = showBitmap2;
            pictureBox3.Image = showBitmap3;
            pictureBox4.Image = showBitmap4;

            label1.Text = "Original";
            label2.Text = "R Channel";
            label3.Text = "G Channel";
            label4.Text = "B Channel";

            state = "colorExtraction_Click";
        }
        private void colorTransformation_Click(object sender, EventArgs e)
        {
            init();
            int x, y;

            for (y = 0; y < srcHeight; y++)
            {
                for (x = 0; x < srcWidth; x++)
                {
                    showBitmap2.SetPixel(x, y, Color.FromArgb(Gray[x, y], Gray[x, y], Gray[x, y]));
                }
            }

            pictureBox1.Image = showBitmap1;
            pictureBox2.Image = showBitmap2;
            pictureBox3.Image = null;
            pictureBox4.Image = null;

            label1.Text = "Original";
            label2.Text = "Grayscale";
            label3.Text = "";
            label4.Text = "";

            state = "colorTransformation_Click";
        }
        private void meanFilter_Click(object sender, EventArgs e)
        {
            init();
            int x, y;//指定位置用的變數
            int i, j;//指定filter內位置用的變數
            int istart = 0, iend = 0;//指定filter內位置用的變數
            int jstart = 0, jend = 0;//指定filter內位置用的變數
            int meanR, meanG, meanB;//filter內的平均
            int numOfPixel;//filter的pixel數量

            for (y = 0; y < srcHeight; y++)
            {
                for (x = 0; x < srcWidth; x++)
                {
                    meanR = 0;
                    meanG = 0;
                    meanB = 0;

                    if (x == 0)//判斷x邊界
                    {
                        istart = 0; iend = 1;
                    }
                    else if (x > 0 && x < srcWidth - 1)
                    {
                        istart = -1; iend = 1;
                    }
                    else if (x == srcWidth - 1)
                    {
                        istart = -1; iend = 0;
                    }

                    if (y == 0)//判斷y邊界
                    {
                        jstart = 0; jend = 1;
                    }
                    else if (y > 0 && y < srcHeight - 1)
                    {
                        jstart = -1; jend = 1;
                    }
                    else if (y == srcHeight - 1)
                    {
                        jstart = -1; jend = 0;
                    }

                    numOfPixel = (iend - istart + 1) * (jend - jstart + 1);

                    for (i = istart; i <= iend; i++)
                    {
                        for (j = jstart; j <= jend; j++)
                        {
                            meanR += R[x + i, y + j];
                            meanG += G[x + i, y + j];
                            meanB += B[x + i, y + j];
                        }
                    }

                    meanR = Convert.ToInt32(Math.Round(Convert.ToDouble(meanR / numOfPixel)));
                    meanG = Convert.ToInt32(Math.Round(Convert.ToDouble(meanG / numOfPixel)));
                    meanB = Convert.ToInt32(Math.Round(Convert.ToDouble(meanB / numOfPixel)));

                    showBitmap2.SetPixel(x, y, Color.FromArgb(meanR, meanG, meanB));
                }
            }
            pictureBox1.Image = showBitmap1;
            pictureBox2.Image = showBitmap2;
            pictureBox3.Image = null;
            pictureBox4.Image = null;

            label1.Text = "Original";
            label2.Text = "Mean Filter";
            label3.Text = "";
            label4.Text = "";

            state = "meanFilter_Click";
        }
        private void medianFilter_Click(object sender, EventArgs e)
        {
            init();
            int x, y;//指定位置用的變數
            int i, j;//指定filter內位置用的變數
            int k;//讀取要排序的陣列用的變數
            int istart = 0, iend = 0;//指定filter內位置用的變數
            int jstart = 0, jend = 0;//指定filter內位置用的變數
            int medianR, medianG, medianB;//filter內的平均
            int numOfPixel;//filter的pixel數量
            int[] sortArrayR = new int[9];//排序用的array
            int[] sortArrayG = new int[9];//排序用的array
            int[] sortArrayB = new int[9];//排序用的array

            for (y = 0; y < srcHeight; y++)
            {
                for (x = 0; x < srcWidth; x++)
                {
                    if (x == 0)//判斷x邊界
                    {
                        istart = 0; iend = 1;
                    }
                    else if (x == srcWidth - 1)
                    {
                        istart = -1; iend = 0;
                    }
                    else //if (x > 0 && x < srcWidth - 1)
                    {
                        istart = -1; iend = 1;
                    }

                    if (y == 0)//判斷y邊界
                    {
                        jstart = 0; jend = 1;
                    }
                    else if (y == srcHeight - 1)
                    {
                        jstart = -1; jend = 0;
                    }
                    else //if (y > 0 && y < srcHeight - 1)
                    {
                        jstart = -1; jend = 1;
                    }

                    numOfPixel = (iend - istart + 1) * (jend - jstart + 1);

                    k = 0;
                    for (i = istart; i <= iend; i++)
                    {
                        for (j = jstart; j <= jend; j++)
                        {
                            sortArrayR[k] = R[x + i, y + j];
                            sortArrayG[k] = G[x + i, y + j];
                            sortArrayB[k] = B[x + i, y + j];
                            k++;
                        }
                    }

                    intArraySort(sortArrayR, numOfPixel);
                    intArraySort(sortArrayG, numOfPixel);
                    intArraySort(sortArrayB, numOfPixel);

                    medianR = (sortArrayR[Convert.ToInt32(Math.Floor((numOfPixel - 1) / 2.0))] + sortArrayR[Convert.ToInt32(Math.Ceiling((numOfPixel - 1) / 2.0))]) / 2;
                    medianG = (sortArrayG[Convert.ToInt32(Math.Floor((numOfPixel - 1) / 2.0))] + sortArrayG[Convert.ToInt32(Math.Ceiling((numOfPixel - 1) / 2.0))]) / 2;
                    medianB = (sortArrayB[Convert.ToInt32(Math.Floor((numOfPixel - 1) / 2.0))] + sortArrayB[Convert.ToInt32(Math.Ceiling((numOfPixel - 1) / 2.0))]) / 2;

                    showBitmap2.SetPixel(x, y, Color.FromArgb(medianR, medianG, medianB));
                }
            }
            pictureBox1.Image = showBitmap1;
            pictureBox2.Image = showBitmap2;
            pictureBox3.Image = null;
            pictureBox4.Image = null;

            label1.Text = "Original";
            label2.Text = "Median Filter";
            label3.Text = "";
            label4.Text = "";

            state = "medianFilter_Click";
        }
        private void histogramEqualization_Click(object sender, EventArgs e)
        {
            init();
            int[] srcHisR = Enumerable.Repeat(0, depth).ToArray();//顏色分布函數
            int[] srcHisG = Enumerable.Repeat(0, depth).ToArray();//顏色分布函數
            int[] srcHisB = Enumerable.Repeat(0, depth).ToArray();//顏色分布函數
            int[] equHisMapR = Enumerable.Repeat(0, depth).ToArray();//顏色分布函數
            int[] equHisMapG = Enumerable.Repeat(0, depth).ToArray();//顏色分布函數
            int[] equHisMapB = Enumerable.Repeat(0, depth).ToArray();//顏色分布函數
            int[] cdfR = Enumerable.Repeat(0, depth).ToArray();//顏色累積分布函數
            int[] cdfG = Enumerable.Repeat(0, depth).ToArray();//顏色累積分布函數
            int[] cdfB = Enumerable.Repeat(0, depth).ToArray();//顏色累積分布函數
            int x, y;//指定位置用的變數
            int i;
            int minHisR, minHisG, minHisB;

            minHisR = depth;
            minHisG = depth;
            minHisB = depth;

            for (y = 0; y < srcHeight; y++)
            {
                for (x = 0; x < srcWidth; x++)
                {
                    srcHisR[R[x, y]]++;
                    srcHisG[G[x, y]]++;
                    srcHisB[B[x, y]]++;

                    minHisR = R[x, y] < minHisR ? R[x, y] : minHisR;
                    minHisG = G[x, y] < minHisG ? G[x, y] : minHisG;
                    minHisB = B[x, y] < minHisB ? B[x, y] : minHisB;
                }
            }

            cdfR[0] = srcHisR[0];
            cdfG[0] = srcHisG[0];
            cdfB[0] = srcHisB[0];

            for (i = 1; i < depth; i++)
            {
                cdfR[i] = cdfR[i - 1] + srcHisR[i];
                cdfG[i] = cdfG[i - 1] + srcHisG[i];
                cdfB[i] = cdfB[i - 1] + srcHisB[i];
            }

            for (i = 0; i < depth; i++)
            {
                equHisMapR[i] = Convert.ToInt32(Math.Round((cdfR[i] - cdfR[minHisR]) / Convert.ToDouble(srcHeight * srcWidth - cdfR[minHisR]) * (depth - 1)));
                equHisMapG[i] = Convert.ToInt32(Math.Round((cdfG[i] - cdfG[minHisG]) / Convert.ToDouble(srcHeight * srcWidth - cdfG[minHisG]) * (depth - 1)));
                equHisMapB[i] = Convert.ToInt32(Math.Round((cdfB[i] - cdfB[minHisB]) / Convert.ToDouble(srcHeight * srcWidth - cdfB[minHisB]) * (depth - 1)));
            }

            for (y = 0; y < srcHeight; y++)
            {
                for (x = 0; x < srcWidth; x++)
                {
                    showBitmap2.SetPixel(x, y, Color.FromArgb(equHisMapR[R[x, y]], equHisMapG[G[x, y]], equHisMapB[B[x, y]]));
                }
            }

            pictureBox1.Image = showBitmap1;
            pictureBox2.Image = showBitmap2;
            pictureBox3.Image = drawHistogram(showBitmap1);
            pictureBox4.Image = drawHistogram(showBitmap2);

            label1.Text = "Original";
            label2.Text = "Histogram Equalization";
            label3.Text = "Histogram Of Original Image";
            label4.Text = "Histogram Equalization";

            state = "histogramEqualization_Click";
        }
        private void thresholding_Click(object sender, EventArgs e)
        {
            init();
            int x, y;

            for (y = 0; y < srcHeight; y++)
            {
                for (x = 0; x < srcWidth; x++)
                {
                    if (Gray[x, y] >= scrollBar.Value)
                    {
                        showBitmap2.SetPixel(x, y, Color.White);
                    }
                    else
                    {
                        showBitmap2.SetPixel(x, y, Color.Black);
                    }
                }
            }

            pictureBox1.Image = showBitmap1;
            pictureBox2.Image = showBitmap2;
            pictureBox3.Image = null;
            pictureBox4.Image = null;

            label1.Text = "Original";
            label2.Text = "Thresholding";
            label3.Text = "";
            label4.Text = "";

            state = "thresholding_Click";
        }
        private void scrollBar_ValueChanged(object sender, EventArgs e)
        {
            init();
            scrollBarValue.Text = scrollBar.Value.ToString();

            if (state == "thresholding_Click")
            {
                thresholding_Click(this, EventArgs.Empty);
            }
        }
        private void scrollBarValue_TextChanged(object sender, EventArgs e)
        {
            init();
            if (scrollBarValue.Text == "")
            {
                return;
            }

            int value = Convert.ToInt32(scrollBarValue.Text);

            if (value > 256)
            {
                scrollBar.Value = 256;
                scrollBarValue.Text = "256";
            }
            else if (value < 0)
            {
                scrollBar.Value = 0;
                scrollBarValue.Text = "0";
            }
            else
            {
                scrollBar.Value = value;
            }

            if (state == "thresholding_Click")
            {
                thresholding_Click(this, EventArgs.Empty);
            }
        }
        private void sobelEdgeDetection_Click(object sender, EventArgs e)
        {
            init();
            int x, y;//指定位置用的變數
            int i, j;//指定filter內位置用的變數
            int[,] Gx = new int[srcWidth, srcHeight];
            int[,] Gy = new int[srcWidth, srcHeight];
            int[,] Gxy = new int[srcWidth, srcHeight];

            for (y = 0; y < srcHeight; y++)
            {
                for (x = 0; x < srcWidth; x++)
                {
                    Gx[x, y] = 0;
                    Gy[x, y] = 0;
                }
            }

            for (y = 0; y < srcHeight; y++)
            {
                for (x = 0; x < srcWidth; x++)
                {
                    if (x != 0 && x != srcWidth - 1 && y != 0 && y != srcHeight - 1)
                    {
                        for (i = 0; i <= 2; i++)
                        {
                            for (j = 0; j <= 2; j++)
                            {
                                Gx[x, y] += Gray[x + i - 1, y + j - 1] * sobelFilterX[i, j];
                                Gy[x, y] += Gray[x + i - 1, y + j - 1] * sobelFilterY[i, j];
                            }
                        }
                    }
                    else
                    {
                        Gx[x, y] = 0;
                        Gy[x, y] = 0;
                    }
                    Gx[x, y] = Math.Abs(Gx[x, y]) > 255 ? 255 : Math.Abs(Gx[x, y]);
                    Gy[x, y] = Math.Abs(Gy[x, y]) > 255 ? 255 : Math.Abs(Gy[x, y]);
                    Gxy[x, y] = Convert.ToInt32(Math.Sqrt(Gx[x, y] * Gx[x, y] + Gy[x, y] * Gy[x, y])) > 255 ? 255 : Convert.ToInt32(Math.Sqrt(Gx[x, y] * Gx[x, y] + Gy[x, y] * Gy[x, y]));

                    showBitmap2.SetPixel(x, y, Color.FromArgb(Gx[x, y], Gx[x, y], Gx[x, y]));
                    showBitmap3.SetPixel(x, y, Color.FromArgb(Gy[x, y], Gy[x, y], Gy[x, y]));
                    showBitmap4.SetPixel(x, y, Color.FromArgb(Gxy[x, y], Gxy[x, y], Gxy[x, y]));

                }
            }
            pictureBox1.Image = showBitmap1;
            pictureBox2.Image = showBitmap2;
            pictureBox3.Image = showBitmap3;
            pictureBox4.Image = showBitmap4;

            label1.Text = "Original";
            label2.Text = "Sobel Edge Detection (Horizontal)";
            label3.Text = "Sobel Edge Detection (Vertical)";
            label4.Text = "Sobel Edge Detection (Combined)";

            state = "sobelEdgeDetection_Click";
        }
        private void edgeOverlapping_Click(object sender, EventArgs e)
        {
            init();
            if (showBitmap1.Size != srcBitmap.Size) return;

            int x, y;
            int r, g, b;
            Color srcColor;

            for (y = 0; y < srcHeight; y++)
            {
                for (x = 0; x < srcWidth; x++)
                {
                    if (Gray[x, y] == 255)
                    {
                        r = Convert.ToInt32(numericRed.Value);
                        g = Convert.ToInt32(numericGreen.Value);
                        b = Convert.ToInt32(numericBlue.Value);
                    }
                    else
                    {
                        srcColor = srcBitmap.GetPixel(x, y);
                        r = Convert.ToInt32(srcColor.R);
                        g = Convert.ToInt32(srcColor.G);
                        b = Convert.ToInt32(srcColor.B);

                    }
                    showBitmap2.SetPixel(x, y, Color.FromArgb(r, g, b));
                }
            }
            copyBitmap(ref showBitmap3, ref srcBitmap);

            pictureBox1.Image = showBitmap1;
            pictureBox2.Image = showBitmap2;
            pictureBox3.Image = showBitmap3;
            pictureBox4.Image = null;

            label1.Text = "Sobel Edge";
            label2.Text = "Edge Overlapping";
            label3.Text = "Source Image";
            label4.Text = "";

            state = "edgeOverlapping_Click";
        }
        private void erosion_Click(object sender, EventArgs e)
        {
            init();
            int x, y;
            int i, j;
            double[,] Erosion = new double[srcWidth, srcHeight];

            if (state != "erosion_Click" && state != "dilation_Click")
            {
                for (y = 0; y < srcHeight; y++)
                {
                    for (x = 0; x < srcWidth; x++)
                    {
                        Morphological[x, y] = Convert.ToDouble(Gray[x, y]);
                    }
                }
            }

            for (y = 0; y < srcHeight; y++)
            {
                for (x = 0; x < srcWidth; x++)
                {
                    Erosion[x, y] = 0;
                }
            }

            for (y = 0; y < srcHeight; y++)
            {
                for (x = 0; x < srcWidth; x++)
                {
                    for (i = -1; i <= 1; i++)
                    {
                        for (j = -1; j <= 1; j++)
                        {
                            if (x + i < 0 || x + i >= srcWidth || y + j < 0 || y + j >= srcHeight)
                            {
                                Erosion[x, y] += 255 * EroDilaKernel[1 + i, 1 + j];
                            }
                            else
                            {
                                Erosion[x, y] += Morphological[x + i, y + j] * EroDilaKernel[1 + i, 1 + j];
                            }
                        }
                    }

                    if (Erosion[x, y] > (EroDilaSum - EroDilaThreshold) * 255)
                    {
                        showBitmap2.SetPixel(x, y, Color.White);
                    }
                    else
                    {
                        showBitmap2.SetPixel(x, y, Color.Black);
                    }
                }
            }

            for (y = 0; y < srcHeight; y++)
            {
                for (x = 0; x < srcWidth; x++)
                {
                    if (Erosion[x, y] > (EroDilaSum - EroDilaThreshold) * 255)
                    {
                        Morphological[x, y] = Erosion[x, y] - (EroDilaSum - EroDilaThreshold) * 255;
                    }
                    else
                    {
                        Morphological[x, y] = 0;
                    }
                }
            }

            pictureBox1.Image = showBitmap1;
            pictureBox2.Image = showBitmap2;
            pictureBox3.Image = null;
            pictureBox4.Image = null;

            label1.Text = "Original";
            label2.Text = "Erosion";
            label3.Text = "";
            label4.Text = "";

            state = "erosion_Click";
        }
        private void dilation_Click(object sender, EventArgs e)
        {
            init();
            int x, y;
            int i, j;
            double[,] Dilation = new double[srcWidth, srcHeight];

            if (state != "erosion_Click" && state != "dilation_Click")
            {
                for (y = 0; y < srcHeight; y++)
                {
                    for (x = 0; x < srcWidth; x++)
                    {
                        Morphological[x, y] = Convert.ToDouble(Gray[x, y]);
                    }
                }
            }

            for (y = 0; y < srcHeight; y++)
            {
                for (x = 0; x < srcWidth; x++)
                {
                    Dilation[x, y] = 0;
                }
            }

            for (y = 0; y < srcHeight; y++)
            {
                for (x = 0; x < srcWidth; x++)
                {
                    for (i = -1; i <= 1; i++)
                    {
                        for (j = -1; j <= 1; j++)
                        {
                            if (x + i < 0 || x + i >= srcWidth || y + j < 0 || y + j >= srcHeight)
                            {
                                Dilation[x, y] += 0;
                            }
                            else
                            {
                                Dilation[x, y] += Morphological[x + i, y + j] * EroDilaKernel[1 + i, 1 + j];
                            }
                        }
                    }
                    if (Dilation[x, y] >= EroDilaThreshold * 255)
                    {
                        showBitmap2.SetPixel(x, y, Color.White);
                    }
                    else
                    {
                        showBitmap2.SetPixel(x, y, Color.Black);
                    }
                }
            }

            for (y = 0; y < srcHeight; y++)
            {
                for (x = 0; x < srcWidth; x++)
                {
                    if (Dilation[x, y] >= EroDilaThreshold * 255)
                    {
                        Morphological[x, y] = 255.0;
                    }
                    else
                    {
                        Morphological[x, y] = Dilation[x, y];
                    }
                }
            }

            pictureBox1.Image = showBitmap1;
            pictureBox2.Image = showBitmap2;
            pictureBox3.Image = null;
            pictureBox4.Image = null;

            label1.Text = "Original";
            label2.Text = "Dilation";
            label3.Text = "";
            label4.Text = "";

            state = "dilation_Click";
        }
        private void rotation_Click(object sender, EventArgs e)
        {
            init();
            double pi = Math.PI;
            double degree = Convert.ToDouble(numericDegree.Value) / 180.0 * pi; // radian
            
            int newHeight = Convert.ToInt32(Math.Abs(srcWidth * Math.Sin(degree)) + Math.Abs(srcHeight * Math.Cos(degree)));
            int newWidth = Convert.ToInt32(Math.Abs(srcWidth * Math.Cos(degree)) + Math.Abs(srcHeight * Math.Sin(degree)));

            showBitmap2 = new Bitmap(newWidth, newHeight);

            for (int y = 0; y < newHeight; y++) 
            {
                for (int x = 0; x < newWidth; x++)
                {
                    // get rotation position
                    double newX, newY;

                    if (degree < (pi / 2))
                    {
                        newX = x;
                        newY = y - srcWidth * Math.Sin(degree);
                    }
                    else if (degree >= (pi / 2) && degree < pi)
                    {
                        newX = x + srcWidth * Math.Cos(degree);
                        newY = y - srcWidth * Math.Sin(degree) + srcHeight * Math.Cos(degree);
                    }
                    else if (degree >= pi && degree < (pi * 3 / 2))
                    {
                        newX = x + srcWidth * Math.Cos(degree) + srcHeight * Math.Sin(degree);
                        newY = y + srcHeight * Math.Cos(degree);
                    }
                    else
                    {
                        newX = x + srcHeight * Math.Sin(degree);
                        newY = y;
                    }

                    double orgX, orgY;

                    orgX = newX * Math.Cos(degree) - newY * Math.Sin(degree);
                    orgY = newX * Math.Sin(degree) + newY * Math.Cos(degree);

                    double orgXtenths = orgX - Convert.ToInt32(Math.Floor(orgX));
                    double orgYtenths = orgY - Convert.ToInt32(Math.Floor(orgY));

                    Color srcColor1, srcColor2, srcColor3, srcColor4;

                    if (Math.Floor(orgX) >= 0 && Math.Ceiling(orgX) <= srcWidth - 1 && Math.Floor(orgY) >= 0 && Math.Ceiling(orgY) <= srcHeight - 1)
                    {
                        srcColor1 = showBitmap1.GetPixel(Convert.ToInt32(Math.Floor(orgX)), Convert.ToInt32(Math.Floor(orgY)));
                        srcColor2 = showBitmap1.GetPixel(Convert.ToInt32(Math.Ceiling(orgX)), Convert.ToInt32(Math.Floor(orgY)));
                        srcColor3 = showBitmap1.GetPixel(Convert.ToInt32(Math.Floor(orgX)), Convert.ToInt32(Math.Ceiling(orgY)));
                        srcColor4 = showBitmap1.GetPixel(Convert.ToInt32(Math.Ceiling(orgX)), Convert.ToInt32(Math.Ceiling(orgY)));
                    }
                    else
                    {
                        showBitmap2.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0));
                        continue;
                    }

                    int newR = Convert.ToInt32(srcColor1.R * (1.0 - orgXtenths) * (1.0 - orgYtenths) + srcColor2.R * (orgXtenths) * (1.0 - orgYtenths)
                                + srcColor3.R * (1.0 - orgXtenths) * (orgYtenths) + srcColor4.R * (orgXtenths) * (orgYtenths));
                    int newG = Convert.ToInt32(srcColor1.G * (1.0 - orgXtenths) * (1.0 - orgYtenths) + srcColor2.G * (orgXtenths) * (1.0 - orgYtenths)
                                + srcColor3.G * (1.0 - orgXtenths) * (orgYtenths) + srcColor4.G * (orgXtenths) * (orgYtenths));
                    int newB = Convert.ToInt32(srcColor1.B * (1.0 - orgXtenths) * (1.0 - orgYtenths) + srcColor2.B * (orgXtenths) * (1.0 - orgYtenths)
                                + srcColor3.B * (1.0 - orgXtenths) * (orgYtenths) + srcColor4.B * (orgXtenths) * (orgYtenths));

                    showBitmap2.SetPixel(x, y, Color.FromArgb(newR, newG, newB));

                }
            }

            pictureBox1.Image = showBitmap1;
            pictureBox2.Image = showBitmap2;
            pictureBox3.Image = null;
            pictureBox4.Image = null;

            label1.Text = "Original";
            label2.Text = "Rotation";
            label3.Text = "";
            label4.Text = "";

            state = "rotation_Click";

            Form form = new Form();
            PictureBox orgSize = new PictureBox();

            orgSize.Image = showBitmap2;
            orgSize.SizeMode = PictureBoxSizeMode.AutoSize;
            form.Width = orgSize.Width + 15;
            form.Height = orgSize.Height + 40;

            form.Controls.Add(orgSize);
            form.Show();
        }
        private void stretching_Click(object sender, EventArgs e)
        {
            init();
            float horValue = (float)numericHorizontal.Value;
            float verValue = (float)numericVertical.Value;
            int newHeight = Convert.ToInt32(srcHeight * verValue);
            int newWidth = Convert.ToInt32(srcWidth * horValue);
            double horRatio = Convert.ToDouble(srcWidth - 1) / Convert.ToDouble(newWidth - 1);
            double verRatio = Convert.ToDouble(srcHeight - 1) / Convert.ToDouble(newHeight - 1);

            if (newWidth <= 0 || newHeight <= 0) return;

            showBitmap2 = new Bitmap(newWidth, newHeight);

            for (int newY = 0; newY < newHeight; newY++)
            {
                for (int newX = 0; newX < newWidth; newX++)
                {
                    double orgX, orgY;
                    double orgXtenths = newX * horRatio - Convert.ToInt32(Math.Floor(newX * horRatio));
                    double orgYtenths = newY * verRatio - Convert.ToInt32(Math.Floor(newY * verRatio));

                    if (newX == 0) orgX = 0;
                    else if (newX == newWidth - 1) orgX = srcWidth - 1;
                    else orgX = newX * horRatio;

                    if (newY == 0) orgY = 0;
                    else if (newY == newHeight - 1) orgY = srcHeight - 1;
                    else orgY = newY * verRatio;

                    Color srcColor1 = showBitmap1.GetPixel(Convert.ToInt32(Math.Floor(orgX)), Convert.ToInt32(Math.Floor(orgY)));
                    Color srcColor2 = showBitmap1.GetPixel(Convert.ToInt32(Math.Ceiling(orgX)), Convert.ToInt32(Math.Floor(orgY)));
                    Color srcColor3 = showBitmap1.GetPixel(Convert.ToInt32(Math.Floor(orgX)), Convert.ToInt32(Math.Ceiling(orgY)));
                    Color srcColor4 = showBitmap1.GetPixel(Convert.ToInt32(Math.Ceiling(orgX)), Convert.ToInt32(Math.Ceiling(orgY)));

                    int newR = Convert.ToInt32(srcColor1.R * (1.0 - orgXtenths) * (1.0 - orgYtenths) + srcColor2.R * (orgXtenths) * (1.0 - orgYtenths)
                                + srcColor3.R * (1.0 - orgXtenths) * (orgYtenths) + srcColor4.R * (orgXtenths) * (orgYtenths));
                    int newG = Convert.ToInt32(srcColor1.G * (1.0 - orgXtenths) * (1.0 - orgYtenths) + srcColor2.G * (orgXtenths) * (1.0 - orgYtenths)
                                + srcColor3.G * (1.0 - orgXtenths) * (orgYtenths) + srcColor4.G * (orgXtenths) * (orgYtenths));
                    int newB = Convert.ToInt32(srcColor1.B * (1.0 - orgXtenths) * (1.0 - orgYtenths) + srcColor2.B * (orgXtenths) * (1.0 - orgYtenths)
                                + srcColor3.B * (1.0 - orgXtenths) * (orgYtenths) + srcColor4.B * (orgXtenths) * (orgYtenths));

                    showBitmap2.SetPixel(newX, newY, Color.FromArgb(newR, newG, newB));
                }
            }
            
            pictureBox1.Image = showBitmap1;
            pictureBox2.Image = showBitmap2;
            pictureBox3.Image = null;
            pictureBox4.Image = null;

            label1.Text = "Original";
            label2.Text = "Stretching";
            label3.Text = "";
            label4.Text = "";

            state = "stretching_Click";

            Form form = new Form();
            PictureBox orgSize = new PictureBox();

            orgSize.Image = showBitmap2;
            orgSize.SizeMode = PictureBoxSizeMode.AutoSize;
            form.Width = orgSize.Width + 15;
            form.Height = orgSize.Height + 40;

            form.Controls.Add(orgSize);
            form.Show();
        }

        private void imageEncryption_Click(object sender, EventArgs e)
        {
            init();
            int x, y;
            int average;
            int colorCase;
            Random rand = new Random();

            for (y = 0; y + 1 < srcHeight; y += 2)
            {
                for (x = 0; x + 1 < srcWidth; x += 2)
                {
                    average = (Gray[x, y] + Gray[x, y + 1] + Gray[x + 1, y] + Gray[x + 1, y + 1]) / 4;
                    colorCase = rand.Next(6);

                    if (rand.Next(256) < average)
                    { // white
                        setColor(showBitmap2, x, y, colorCase);
                        setColor(showBitmap3, x, y, colorCase);
                    }
                    else
                    {	// black
                        setColor(showBitmap2, x, y, colorCase);
                        setColor(showBitmap3, x, y, 5 - colorCase);
                    }
                }
            }

            pictureBox1.Image = showBitmap1;
            pictureBox2.Image = showBitmap2;
            pictureBox3.Image = showBitmap3;
            pictureBox4.Image = null; ;

            label1.Text = "Source Image";
            label2.Text = "Image Encryption 1";
            label3.Text = "Image Encryption 2";
            label4.Text = "";

            state = "imageEncryption_Click";
        }
        private void resetSourceImage_Click(object sender, EventArgs e)
        {
            loadBitmapToRGBGray(srcBitmap);

            copyBitmap(ref showBitmap1, ref srcBitmap);
            copyBitmap(ref showBitmap2, ref srcBitmap);
            copyBitmap(ref showBitmap3, ref srcBitmap);
            copyBitmap(ref showBitmap4, ref srcBitmap);

            pictureBox1.Image = showBitmap1;
            pictureBox2.Image = null;
            pictureBox3.Image = null;
            pictureBox4.Image = null; ;

            label1.Text = "Source Image";
            label2.Text = "";
            label3.Text = "";
            label4.Text = "";

            state = "resetSourceImage_Click";
        }
        private void saveImage_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Bitmap Files (.bmp)|*.bmp|Jpeg File(.jpg)|*.jpg|All Files|*.*";

            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                showBitmap1.Save(sfd.FileName);
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (pictureBox2.Image == null)
            {
                return;
            }
            copyBitmap(ref showBitmap2, ref showBitmap1);
            copyBitmap(ref showBitmap3, ref showBitmap1);
            copyBitmap(ref showBitmap4, ref showBitmap1);

            loadBitmapToRGBGray(showBitmap1);

            pictureBox1.Image = showBitmap1;
            pictureBox2.Image = null;
            pictureBox3.Image = null;
            pictureBox4.Image = null;

            label1.Text = "Original";
            label2.Text = "";
            label3.Text = "";
            label4.Text = "";

            if (state == "erosion_Click" || state == "dilation_Click")
            {
                state = "pictureBox1_Click";
            }
        }
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            if (pictureBox2.Image == null)
            {
                return;
            }
            copyBitmap(ref showBitmap1, ref showBitmap2);
            copyBitmap(ref showBitmap3, ref showBitmap2);
            copyBitmap(ref showBitmap4, ref showBitmap2);

            loadBitmapToRGBGray(showBitmap2);

            pictureBox1.Image = showBitmap1;
            pictureBox2.Image = null;
            pictureBox3.Image = null;
            pictureBox4.Image = null;

            label1.Text = label2.Text;
            label2.Text = "";
            label3.Text = "";
            label4.Text = "";
            
            if (state == "erosion_Click" || state == "dilation_Click")
            {
                state = "pictureBox2_Click";
            }
        }
        private void pictureBox3_Click(object sender, EventArgs e)
        {
            if (pictureBox3.Image == null || state == "histogramEqualization_Click")
            {
                return;
            }
            copyBitmap(ref showBitmap1, ref showBitmap3);
            copyBitmap(ref showBitmap2, ref showBitmap3);
            copyBitmap(ref showBitmap4, ref showBitmap3);

            loadBitmapToRGBGray(showBitmap3);

            pictureBox1.Image = showBitmap1;
            pictureBox2.Image = null;
            pictureBox3.Image = null;
            pictureBox4.Image = null;

            label1.Text = label3.Text;
            label2.Text = "";
            label3.Text = "";
            label4.Text = "";

            //state = "pictureBox3_Click";
        }
        private void pictureBox4_Click(object sender, EventArgs e)
        {
            if (pictureBox4.Image == null || state == "histogramEqualization_Click")
            {
                return;
            }
            copyBitmap(ref showBitmap1, ref showBitmap4);
            copyBitmap(ref showBitmap2, ref showBitmap4);
            copyBitmap(ref showBitmap3, ref showBitmap4);

            loadBitmapToRGBGray(showBitmap4);

            pictureBox1.Image = showBitmap1;
            pictureBox2.Image = null;
            pictureBox3.Image = null;
            pictureBox4.Image = null;

            label1.Text = label4.Text;
            label2.Text = "";
            label3.Text = "";
            label4.Text = "";

            //state = "pictureBox4_Click";
        }
    }
}