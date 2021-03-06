﻿using System;
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
        Color[,] greyscaleImage;
        int leftUpperBbX;
        int leftUpperBbY;
        int maxx;
        int maxy;
        int currentRegions;
        int regionCount;
        int optimalThreshold;
        bool[,] optimalLabel;


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
                drawPoint[] boundary = TraceBoundary(Image, getBackgroundColor());
                kernelInput.Text = WritedrawPointArr(boundary);
                BoundaryToOutput(boundary, Image);
            }

            else if (cornerDetRadio.Checked)
            {
                int n = 0;
                CreateSobelKernel(n, ref Kx, ref Ky);
                List<Corner> corners = HarrisCornerDetection(Kx, Ky, Image);
                CornersToImage(corners);
                kernelInput.Text = WritedrawPointArr(CornerListToArray(corners));
            }


            if (ValueRadio.Checked)
                kernelInput.Text = "Unique values: " + valueCount(generateHistogram(Image));
            else if (thresholdRadio.Checked)
                toOutputBitmap(ApplyThresholdFilter(Image, thresholdTrackbar.Value));
            else if (edgeDetection.Checked)
                toOutputBitmap(ApplyEdgeDetection(Image));
            else if (findCentroid.Checked)
            {
                try
                {
                    drawPoint[] point = new drawPoint[1];
                    drawPoint centroid = FindCentroid(ColorArrayToDrawPoints(Image));
                    point[0] = centroid;
                    drawPointsToImage(point, Image);
                }
                catch (Exception error) { MessageBox2.Text = error.Message; }
            }

            else if (greyscaleRadio.Checked)
                toOutputBitmap(ApplyGreyscale(Image));
            else if (preprocessingRadio.Checked)
                toOutputBitmap(PreprocessingPipeline(Image));

            else if (pipelineRadio.Checked)
            {
                drawPoint[] conDefList = new drawPoint[0];
                drawPoint leftUpperBoundingBox = new drawPoint(0, 0);
                float[] angleList = new float[0];
                try
                {
                    Color[,] pipelineImage = PreprocessingPipeline(Image);
                    CreateSobelKernel(0, ref Kx, ref Ky);
                    List<Corner> cornerList = HarrisCornerDetection(Kx, Ky, pipelineImage);
                    drawPoint[] cornerArray = CornerListToArray(cornerList);

                    try
                    {
                        //calculate the convex hull, by drawing a 'border' around the outer pixels, and add the inward points close to the far center in the hand
                        conDefList = AddConvexDefects(cornerArray, ConvexHull(cornerList), pipelineImage);
                        //calculate the angles between each of the neigbouring points in the hand.
                        angleList = cornerOfConvex(conDefList);
                        leftUpperBoundingBox = new drawPoint(leftUpperBbX, leftUpperBbY);
                    }
                    catch (Exception)
                    {
                        //if only two points were found, no border can be drawn, and output will be given to determine why not.
                        toOutputBitmap(pipelineImage);
                        AddDrawPointsToImage(pipelineImage, cornerArray); //show the preprocessed structure with found corners.
                        throw new Exception("Largest area after preprocessing does not have enough corners to proceed. This is not a hand, or only the sleeve has been processed");
                    }



                    try
                    {
                        //the most corners from convexhull and defects are likely found in the hand
                        //scan over the image and find the box that contains the most points
                        //this can be used to calculate a more accurate centroid, and get more better defects.
                        Tuple<Color[,], List<Corner>, drawPoint> hand = isolateHand(pipelineImage, conDefList, cornerArray);

                        Console.WriteLine("Size hand: " + hand.Item1.GetLength(0) + "x" + hand.Item1.GetLength(1));
                        Console.WriteLine("Number of corners: " + hand.Item2.Count);
                        Console.WriteLine("Upperleft corner: (" + (hand.Item3.X + leftUpperBbX) + ", " + (hand.Item3.Y + leftUpperBbY) + ")");

                        //calculate the convex d
                        drawPoint[] handConDefList = AddConvexDefects(CornerListToArray(hand.Item2), ConvexHull(hand.Item2), hand.Item1);
                        angleList = cornerOfConvex(handConDefList);
                        drawPoint handLeftUpperBoundingBox = hand.Item3;

                        conDefList = handConDefList;
                        leftUpperBoundingBox = new drawPoint(handLeftUpperBoundingBox.X + leftUpperBbX, handLeftUpperBoundingBox.Y + leftUpperBbY);
                    }
                    catch { throw new Exception("Hand isolation failed - continuing with full region."); }


                }
                catch (Exception error) { MessageBox2.Text = error.Message; }

                kernelInput.Text = WriteAnglesDistances(angleList, conDefList, 8, 40);

                drawPoint[] oImageConDef = OriginalImagePoints(conDefList, leftUpperBoundingBox);
                Color drawColor = StateToColor(determineObject(angleList, 8, 40));
                toOutputBitmap(CrossesInImage(oImageConDef, DrawLinesBetwPoints(oImageConDef, greyscaleImage, Color.CadetBlue), drawColor));

                //kernelInput.Text = WritedrawPointArr(AddConvexDefects(CornerListToArray(cornerList), ConvexHull(cornerList), pipelineImage));

                // catch (Exception error) { MessageBox2.Text = "Error - please try another image."; }
            }
            else if (ErosionRadio.Checked)
                toOutputBitmap(ApplyErosionDilationFilter(Image, true));
            else if (DilationRadio.Checked)
                toOutputBitmap(ApplyErosionDilationFilter(Image, false));
            else if (OpeningRadio.Checked)
                toOutputBitmap(ApplyOpeningClosingFilter(Image, true));
            else if (ClosingRadio.Checked)
                toOutputBitmap(ApplyOpeningClosingFilter(Image, false));

            //toOutputBitmap(newImage);
            //greyscale, region labelling, opening closing (dus erosion dilation), 
            //weg: Fourier, complement WritedrawVectArr

        }




        Color[,] AddDrawPointsToImage(Color[,] InputImage, drawPoint[] points)
        {

            foreach (drawPoint p in points)
                InputImage[p.X, p.Y] = Color.Red;
            return InputImage;

        }

        Color[,] DrawLinesBetwPoints(drawPoint[] points, Color[,] InputImage, Color drawColor)
        {
            Stack<drawPoint> sPoints = new Stack<drawPoint>(points);
            drawPoint firstPoint = sPoints.Peek();
            while(sPoints.Count > 1)
            {
                drawPoint p1 = sPoints.Pop();
                drawPoint p2 = sPoints.Pop();
                InputImage = DrawLine(p1, p2, InputImage, drawColor);
                sPoints.Push(p2);
            }
            InputImage = DrawLine(firstPoint, sPoints.Pop(), InputImage, drawColor);
            return InputImage;
        }

        Color[,] DrawLine(drawPoint A, drawPoint B, Color[,] InputImage, Color drawColor)
        {
            Color foreGrColor = drawColor;
            float x0 = A.X;
            float y0 = A.Y;
            float deltaX = B.X - A.X;
            float deltaY = B.Y - A.Y;
            float maximum = Math.Max(Math.Abs(deltaX), Math.Abs(deltaY));
            deltaX /= maximum;
            deltaY /= maximum;
            for (float n = 0; n < maximum; ++n)
            {
                // draw pixel at ( A.X, A.Y )
                x0 += deltaX; y0 += deltaY;
                InputImage[(int)x0, (int)y0] = foreGrColor;
            }
            return InputImage;
        }

        drawPoint[] ColorArrayToDrawPoints(Color[,] image)
        {
            Color backgroundColor = getBackgroundColor();
            List<drawPoint> drawpoints = new List<drawPoint>();
            for (int u = 0; u < image.GetLength(0); u++)
                for (int v = 0; v < image.GetLength(1); v++)
                {
                    if (image[u, v] != backgroundColor)
                        drawpoints.Add(new drawPoint(u, v));

                }
            return drawpoints.ToArray();
        }

        drawPoint[] CornerListToArray(List<Corner> cornerList)
        {
            drawPoint[] cornerArray = new drawPoint[cornerList.Count];
            for (int i = 0; i < cornerList.Count; i++)
                cornerArray[i] = new drawPoint(cornerList[i].U, cornerList[i].V);
            return cornerArray;
        }

        Color getBackgroundColor()
        {
            if (checkBlackBackground.Checked)
                return Color.FromArgb(255, 0, 0, 0);

            return Color.FromArgb(255, 255, 255, 255);
        }

        Color getForegroundColor()
        {
            if (checkBlackBackground.Checked)
                return Color.FromArgb(255, 255, 255, 255);

            return Color.FromArgb(255, 0, 0, 0);
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

        String WritedrawPointArr(drawPoint[] drawPoints)
        {
            String output = "";
            for (int n = 0; n < drawPoints.Length; n++)
            {
                output = output + drawPoints[n].X + "\t" + drawPoints[n].Y + "\r\n";
            }
            return output;
        }

        String WriteAnglesDistances(float[] angles, drawPoint[] points, int lowerThr, int upperThr)
        {
            String output = "";
            if (angles[0] > lowerThr && angles[0] < upperThr)
                    output = output + angles[0] + "\t" + SqDistancePoints(points[points.Length - 1], points[0]) + "\t" + SqDistancePoints(points[0], points[1]) + "\r\n";
            for (int n = 1; n < points.Length - 1; n++)
            {
                if(angles[n] > lowerThr && angles[n] < upperThr)
                { 

                    output = output + angles[n] + "\t" + SqDistancePoints(points[n], points[n - 1]) + "\t" + SqDistancePoints(points[n], points[n + 1]) + "\r\n";
                }

            }
            if (angles[0] > lowerThr && angles[0] < upperThr)
                output = output + angles[points.Length - 1] + "\t" + SqDistancePoints(points[points.Length - 2], points[points.Length -1]) + "\t" + SqDistancePoints(points[points.Length - 1], points[0]) + "\r\n";
            return output;
        }

        String WritedrawFloatArr(float[] fs)
        {
            String output = "";
            for (int i = 0; i < fs.Length; i++)
            {
                output = output + fs[i] + "\r\n";
            }
            return output;
        }

        float Pi = (float)Math.PI;











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
                            InputImage.SetPixel(x, y, OutputImage.GetPixel(x, y));               // Set the pixel color at coordinate (x,y)
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
            greyscaleImage = new Color[InputImage.Size.Width, InputImage.Size.Height];

            // Setup progress bar
            progressBar.Visible = true;
            progressBar.Minimum = 1;
            progressBar.Maximum = InputImage.Size.Width * InputImage.Size.Height;
            progressBar.Value = 1;
            progressBar.Step = 1;

            Image = BitmapToArray(InputImage);
        }

        Color[,] BitmapToArray(Bitmap InputImage)
        {
            Color[,] OutputImage = new Color[InputImage.Size.Width, InputImage.Size.Height];
            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    OutputImage[x, y] = InputImage.GetPixel(x, y);                // Set pixel color in array at (x,y)
                }
            }
            return OutputImage;
        }



        /// <summary>
        /// Generates an image in picturBox2 based on the color matrix newImage.
        /// </summary>
        void toOutputBitmap(Color[,] newImage)
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
        Tuple<Color[,], Boolean> MaybeBinary(Color[,] InputImage, int input)
        {
            if (checkBinary.Checked)
            {
                InputImage = ApplyThresholdFilter(InputImage);
                return new Tuple<Color[,], Boolean>(InputImage, true);
            }

            if (input == 2) return new Tuple<Color[,], Boolean>(InputImage, true);
            return new Tuple<Color[,], bool>(InputImage, false);
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




        Color[,] ApplyEdgeDetection(Color[,] InputImage)
        {
            Color[,] OutputImage = new Color[InputImage.GetLength(0), InputImage.GetLength(1)];
            // 
            float[,] Hx = new float[,] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
            float[,] Hy = new float[,] { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };


            int halfboxsize = Hx.GetLength(0) / 2;

            for (int x = halfboxsize; x < InputImage.GetLength(0) - halfboxsize; x++)
            {
                for (int y = halfboxsize; y < InputImage.GetLength(1) - halfboxsize; y++)
                {
                    float u = CalculateNewColor(x, y, Hx, halfboxsize, InputImage, false) / 8; //apply Hx to the image pixel
                    float v = CalculateNewColor(x, y, Hy, halfboxsize, InputImage, false) / 8; //apply Hy to the image pixel


                    int edgeStrength = (int)Math.Sqrt(u * u + v * v); //calculate edgestrength by calculating the length of vector [Hx, Hy]

                    //clamp to max nad min values
                    if (edgeStrength > 255)
                        edgeStrength = 255;
                    if (edgeStrength < 0)
                        edgeStrength = 0;

                    edgeStrength = 255 - edgeStrength;
                    Color updatedColor = Color.FromArgb(edgeStrength, edgeStrength, edgeStrength);
                    OutputImage[x, y] = updatedColor;
                }
                progressBar.PerformStep();
            }
            return OutputImage;
        }








        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void BoundaryRadio_CheckedChanged(object sender, EventArgs e)
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

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void INFOIBV_Load(object sender, EventArgs e)
        {

        }

        private void radioButton1_CheckedChanged_1(object sender, EventArgs e)
        {

        }

        private void kernelInput_TextChanged(object sender, EventArgs e)
        {

        }

        private void progressBar_Click(object sender, EventArgs e)
        {

        }
    }


}
