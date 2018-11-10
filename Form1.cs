using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using drawPoint = System.Drawing.Point;
//using System.Windows.Forms.

namespace INFOIBV
{
    public partial class INFOIBV : Form
    {
        private Bitmap InputImage;
        private Bitmap OutputImage;
        Color[,] Image;
        Color[,] newImage;
        float[,] Kx;
        float[,] Ky;

        // Variabelen die voor de preprocessing pipeline nodig zijn
        int minx;
        int miny;
        int maxx;
        int maxy;
        int currentRegions;
        int regionCount;
        int optimalThreshold;
        int[,] optimalLabel;


        public INFOIBV()
        {
            InitializeComponent();
        }

        private void LoadImageButton1_Click(object sender, EventArgs e) { LoadAndCropImage(); }








        private void applyButton_Click(object sender, EventArgs e)
        {
            resetForApply();

            if (InputImage == null)
            {
                MessageBox2.Text = "Please Load an image first.";
                return;
            }
            if (BoundaryRadio.Checked)
            {
                drawPoint[] boundary = TraceBoundary();
                kernelInput.Text = WritedrawPointArr(boundary);
                BoundaryToOutput(boundary);
            }
            else if (cornerDetRadio.Checked)
            {
                int n = 0;
                CreateSobelKernel(n, ref Kx, ref Ky);
                HarrisCornerDetection(Kx, Ky);

            }

            else
            {
                if (ErosionRadio.Checked)
                    ApplyErosionDilationFilter(true);
                else if (DilationRadio.Checked)
                    ApplyErosionDilationFilter(false);
                else if (OpeningRadio.Checked)
                    ApplyOpeningClosingFilter(true);
                else if (ClosingRadio.Checked)
                    ApplyOpeningClosingFilter(false);
                else if (ValueRadio.Checked)
                    kernelInput.Text = "Unique values: " + valueCount(generateHistogram(Image));
                else if (FourierRadio.Checked)
                    kernelInput.Text = WritedrawVectArr(FourierComponents());
                else if (thresholdRadio.Checked)
                    ApplyThresholdFilter(thresholdTrackbar.Value);
                else if (edgeDetection.Checked)
                    ApplyEdgeDetection();
                else if (greyscaleRadio.Checked)
                    ApplyGreyscale();
                else if (preprocessingRadio.Checked)
                    PreprocessingPipeline();
                else if (regionLabelRadio.Checked)
                {
                    List<drawPoint> testlist = new List<drawPoint>();
                    drawPoint min = new drawPoint(272, 77);
                    drawPoint test1 = new drawPoint(9, 184);
                    testlist.Add(test1);
                    drawPoint test2 = new drawPoint(82, 184);
                    testlist.Add(test2);
                    drawPoint test3 = new drawPoint(31, 28);
                    testlist.Add(test3);
                    drawPoint test4 = new drawPoint(19, 10);
                    testlist.Add(test4);
                    drawPoint test5 = new drawPoint(12, 32);
                    testlist.Add(test5);
                    drawPointsInImage(testlist, min, 1);
                }

               toOutputBitmap();
                //greyscale, region labelling, opening closing (dus erosion dilation), 
                //weg: Fourier, complement WritedrawVectArr

            }
        }


        String WritedrawVectArr(Vector[] Cn)
        {

            String output = "{";
            for (int n = 0; n < Cn.Length; n++)
            {
                output = output + "(" + Cn[n].X + "," + Cn[n].Y + "), ";
            }
            output += "}";
            return output;
        }

        float Pi = (float)Math.PI;

        Vector[] FourierComponents()
        {
            int amountSamples = 1;

            try
            {
                amountSamples = int.Parse(textBox1.Text);
            }
            catch
            {
                MessageBox2.Text = "please enter an integer";
            }

            drawPoint[] points = TraceBoundary();
            drawPoint x0 = new drawPoint(-1, -1);
            Vector[] polarCoords = new Vector[points.Length / amountSamples + 1];

            int polarCoordIndex = 0;
            for (int i = 0; i < points.Length; i = i + amountSamples)
            {
                int xlength = points[i].X - x0.X;
                int ylength = points[i].Y - x0.Y;
                float totLength = (float)Math.Sqrt(xlength * xlength + ylength * ylength);
                float angle = (float)Math.Asin(ylength / totLength);
                polarCoords[polarCoordIndex] = new Vector(angle, totLength);
                polarCoordIndex++;
            }

            int nmax = 7;
            float M = polarCoordIndex;
            Vector[] Cn = new Vector[nmax];

            float sumX = 0;
            float sumY = 0;

            for (int m = 0; m < M; m++) //the mth point in the list
            {
                sumX += (float)polarCoords[m].X;
                sumY += (float)polarCoords[m].Y;
            }

            Cn[0] = new Vector(1 / M * sumX, 1 / M * sumY);

            sumX = 0;
            sumY = 0;

            for (int n = 1; n < nmax; n++) //n of cn
            {
                for (int m = 0; m < M; m++) //the mth point in the list
                {
                    double inCosSin = (1 / M) * (2 * Pi * n * m);
                    sumX += (float)(polarCoords[m].X * Math.Cos(inCosSin) + polarCoords[m].Y * Math.Sin(inCosSin));
                    sumY += (float)(polarCoords[m].Y * Math.Cos(inCosSin) - polarCoords[m].X * Math.Sin(inCosSin));

                }
                Cn[n] = new Vector(1 / M * sumX, 1 / M * sumY);
            }

            float[] CnReal = new float[nmax];
            float[] CnImag = new float[nmax];
            for (int n = 0; n < nmax; n++)
            {
                CnReal[n] = (float)Cn[n].X;
                CnImag[n] = (float)Cn[n].Y;
            }
            //Vector ReC0 = 1 / polarCoordIndex *
            chart1.Series[0].Points.DataBindY(CnReal);
            chart1.Series[1].Points.DataBindY(CnImag);
            return Cn;
        }




     




        private void saveButton_Click(object sender, EventArgs e)
        {
            if (OutputImage == null) return;                                // Get out if no output image
            if (saveImageDialog.ShowDialog() == DialogResult.OK)
                OutputImage.Save(saveImageDialog.FileName);                 // Save the output image
        }




        void resetForApply()
        {
            MessageBox2.Text = "";
            if (InputImage == null) return;                                 // Get out if no input image

            if (OutputImage != null)
            {
                if (RightAsInput.Checked)
                {
                    InputImage.Dispose();
                    InputImage = new Bitmap(OutputImage.Size.Width, OutputImage.Size.Height);

                    for (int x = 0; x < InputImage.Size.Width; x++)
                    {
                        for (int y = 0; y < InputImage.Size.Height; y++)
                        {
                            InputImage.SetPixel(x, y, newImage[x, y]);               // Set the pixel color at coordinate (x,y)
                                                                                  //OutputImage.SetPixel(x, y, newImage[x, y]);
                        }
                    }
                    pictureBox1.Image = InputImage;

                }


                OutputImage.Dispose();                 // Reset output image

            }             // Reset output image
            OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height); // Create new output image
            Image = new Color[InputImage.Size.Width, InputImage.Size.Height]; // Create array to speed-up operations (Bitmap functions are very slow)
            newImage = new Color[InputImage.Size.Width, InputImage.Size.Height];

            // Setup progress bar
            progressBar.Visible = true;
            progressBar.Minimum = 1;
            progressBar.Maximum = InputImage.Size.Width * InputImage.Size.Height;
            progressBar.Value = 1;
            progressBar.Step = 1;

            // Copy input Bitmap to array            
            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    Image[x, y] = InputImage.GetPixel(x, y);                // Set pixel color in array at (x,y)
                }
            }
        }





        /// <summary>
        /// Generates an image in picturBox2 based on the color matrix newImage.
        /// </summary>
        void toOutputBitmap()
        {
            //Image = newImage;
            // Copy array to output Bitmap
            OutputImage.Dispose();                 // Reset output image
            OutputImage = new Bitmap(newImage.GetLength(0), newImage.GetLength(1));

            for (int x = 0; x < OutputImage.Size.Width; x++)
            {
                for (int y = 0; y < OutputImage.Size.Height; y++)
                {
                    //OutputImage1.SetPixel(x, y, Image[x, y]);               // Set the pixel color at coordinate (x,y)
                    OutputImage.SetPixel(x, y, newImage[x, y]);
                }
            }

            outputBox1.Image = (Image)OutputImage;                         // Display output image
            progressBar.Visible = false;                                    // Hide progress bar

        }




        /// <summary>
        /// Reads the input of textbox1 and returns a matrix generated from the input.
        /// </summary>
        /// <returns></returns>
        int[,] ParseMatrix()
        {
            try
            {
                // split the rows
                string input = kernelInput.Text;
                string[] rows = input.Split(new string[] { "\r\n" }, StringSplitOptions.None);

                // split the columns and add to a 2D array

                int[,] matrix = new int[rows.Length, rows.Length]; //creer M x M matrix afhankelijk van ingevoerde values

                for (int i = 0; i < rows.Length; i++)
                {
                    // alle 3 de rijen parsen
                    int[] column = Array.ConvertAll(rows[i].Split(' '), int.Parse);

                    if (column.Length != rows.Length)
                    {
                        throw new Exception("Provide a square matrix, with equal number of rows and columns");
                    }
                    if (column.Length % 2 == 0)
                        throw new Exception("Provide a square matrix, with an odd number of columns and rows");


                    // deze kolom op de goede plek in de matrix zetten
                    for (int j = 0; j < rows.Length; j++)
                        matrix[i, j] = column[j];
                }

                return matrix;
                //de matrix is geparsed en de waardes zijn nu op te halen


            }
            catch (Exception e)
            {
                MessageBox2.Text = e.Message;
                return null;
            }
        }












        /// <summary>
        /// Checks if a number is binary (used to check the valuecount of a histogram for binary images).
        /// </summary>
        bool isBinary(int input)
        {
            if (checkBinary.Checked)
            {
                ApplyThresholdFilter();
                return true;
            }

            if (input == 2) return true;
            else return false;
        }




        /// <summary>
        /// Takes an integer value and clamps it to either the minimum or maximum RGB-value (0-255).
        /// </summary>
        int clamp(int i)
        {
            if (i < 0) return 0;
            else if (i > 255) return 255;
            else return i;
        }










        // -------------------------------------------- TRACE BOUNDARY CODE --------------------------------------------

       

        private void FourierRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (FourierRadio.Checked)
                textBox1.ReadOnly = false;
            else
                textBox1.ReadOnly = true;

        }

        void ApplyEdgeDetection()
        {
            // 
            float[,] Hx = new float[,] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
            float[,] Hy = new float[,] { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };


            int halfboxsize = Hx.GetLength(0) / 2;

            for (int x = halfboxsize; x < InputImage.Size.Width - halfboxsize; x++)
            {
                for (int y = halfboxsize; y < InputImage.Size.Height - halfboxsize; y++)
                {
                    float u = CalculateNewColor(x, y, Hx, halfboxsize, false) / 8; //apply Hx to the image pixel
                    float v = CalculateNewColor(x, y, Hy, halfboxsize, false) / 8; //apply Hy to the image pixel


                    int edgeStrength = (int)Math.Sqrt(u * u + v * v); //calculate edgestrength by calculating the length of vector [Hx, Hy]

                    //clamp to max nad min values
                    if (edgeStrength > 255)
                        edgeStrength = 255;
                    if (edgeStrength < 0)
                        edgeStrength = 0;

                    edgeStrength = 255 - edgeStrength;
                    Color updatedColor = Color.FromArgb(edgeStrength, edgeStrength, edgeStrength);
                    newImage[x, y] = updatedColor;
                }
                progressBar.PerformStep();
            }
            toOutputBitmap();
        }



       




        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void BoundaryRadio_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void FourierSamples_Click(object sender, EventArgs e)
        {

        }

        private void ValueRadio_CheckedChanged(object sender, EventArgs e)
        {

        }
        private void thresholdRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (thresholdRadio.Checked)
                thresholdTrackbar.Enabled = true;                
            else
                thresholdTrackbar.Enabled = false;
        }

        private void thresholdTrackbar_Scroll(object sender, EventArgs e)
        {
            thresholdValue.Text = thresholdTrackbar.Value.ToString();
        }
    }

   
}
