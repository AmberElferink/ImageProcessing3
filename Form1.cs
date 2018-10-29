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


        public INFOIBV()
        {
            InitializeComponent();
        }

        private void LoadImageButton1_Click(object sender, EventArgs e) { LoadImage(); }



        void LoadImage()
        {

            if (openImageDialog.ShowDialog() == DialogResult.OK)             // Open File Dialog
            {
                String file = openImageDialog.FileName;                     // Get the file name

                imageFileName1.Text = file;

                if (InputImage != null) InputImage.Dispose();               // Reset image
                InputImage = new Bitmap(file);                              // Create new Bitmap from file
                if (InputImage.Size.Height <= 0 || InputImage.Size.Width <= 0 ||
                    InputImage.Size.Height > 512 || InputImage.Size.Width > 512) // Dimension check
                    MessageBox.Show("Error in image dimensions (have to be > 0 and <= 512)");
                pictureBox1.Image = (Image)InputImage;
            }
        }





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
            else if (regionLabelRadio.Checked)
            {
                RegionLabeling();
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
                else if (complementRadio.Checked)
                    //kernelInput.Text = detectBackground(generateHistogram(ref alow, ref ahigh)).ToString();
                    GenerateComplement();
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


                toOutputBitmap();

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




        private void ApplyThresholdFilter(int thresholdLimit = 125)
        {
            int inputImageW;
            int inputImageH;
            Color[,] img;





            inputImageW = InputImage.Size.Width;
            inputImageH = InputImage.Size.Height;

            img = new Color[inputImageW, inputImageH];

            for (int x = 0; x < inputImageW; x++)
                for (int y = 0; y < inputImageH; y++)
                {
                    img[x, y] = Image[x, y];
                }




            int thresholdColor = 0;
            for (int x = 0; x < inputImageW; x++)
            {
                for (int y = 0; y < inputImageH; y++)
                {
                    if (Image[x, y].R < thresholdLimit)
                    {
                        thresholdColor = 0;
                    }
                    else
                    {
                        thresholdColor = 255;
                    }
                    Color updatedColor = Color.FromArgb(thresholdColor, thresholdColor, thresholdColor);
                    newImage[x, y] = updatedColor;

                }
            }
            for (int x = 0; x < inputImageW; x++)
                for (int y = 0; y < inputImageH; y++)
                {
                    Image[x, y] = newImage[x, y];
                }



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
            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
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
        /// Generates a histogram of the currently loaded image.
        /// </summary>
        /// <param name="alow"></param>
        /// <param name="ahigh"></param>
        /// <returns></returns>
        int[] generateHistogram(Color[,] image)
        {
            int[] histogram = new int[256];     //histogram aanmaken, alow en ahigh initialiseren

            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    Color pixelColor = image[x, y];                                 // Get the pixel color at coordinate (x,y)
                    int grey = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;    // aanmaken grijswaarde op basis van RGB-values
                    Color updatedColor = Color.FromArgb(grey, grey, grey);          // toepassen grijswaarde
                    histogram[grey]++;                                              // histogram updaten

                }
            }
            return histogram;
        }





        /// <summary>
        /// Returns the unique number of values in a histogram.
        /// </summary>
        int valueCount(int[] histogram)
        {
            int count = 0;
            for (int i = 0; i < histogram.Length; i++)
            {
                if (histogram[i] > 0) count++;
            }
            return count;
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
        /// Returns the foreground (smallest number of pixels) value of a binary image using its histogram as input.
        /// </summary>
        int detectBackground(int[] histogram)
        {
            // if (!isBinary(valueCount(histogram))) return 256;

            int a = 0;
            int b = 0;
            for (int i = 0; i < histogram.Length; i++)
            {
                if (a == 0 && histogram[i] != 0) a = i;
                if (b == 0 && histogram[i] != 0 && i != a) b = i;
            }
            if (a < b) return b;
            else return a;
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

        void GenerateComplement()
        {
            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    int newColor = Image[x, y].R;                         // Get the pixel color at coordinate (x,y)
                    newColor = 255 - newColor; // Negative image
                    newImage[x, y] = Color.FromArgb(newColor, newColor, newColor);                             // Set the new pixel color at coordinate (x,y)
                    progressBar.PerformStep();                              // Increment progress bar
                }
            }
        }






        /// <summary>
        /// Dilates a binary image using the provided matrix as structuring element.
        /// </summary>
        void CalculateDilationBinary(int x, int y, int[,] matrix, int halfBoxSize, int backGrC)
        {
            int newColor = 0;
            if (backGrC == 0)
                newColor = 255;
            Color updatedColor = Color.FromArgb(newColor, newColor, newColor);
            for (int a = (halfBoxSize * -1); a <= halfBoxSize; a++)
            {
                for (int b = (halfBoxSize * -1); b <= halfBoxSize; b++)
                {

                    // every pixel that exists on the structuring element and is currently in the background gets transformed to the foreground
                    if (matrix[a + halfBoxSize, b + halfBoxSize] != -1 && Image[x + a, y + b].R == backGrC)
                    {
                        newImage[x + a, y + b] = updatedColor;
                    }
                    // every pixel that doesn't meet these conditions retains its former color
                    else newImage[x, y] = updatedColor;

                }
            }
        }




        /// <summary>
        /// Erodes a binary image using the provided matrix as a structuring element.
        /// </summary>
        void CalculateErosionBinary(int x, int y, int[,] matrix, int halfBoxSize, int backGrC)
        {
            int newcolor = backGrC;
            Color updatedColor = Color.FromArgb(newcolor, newcolor, newcolor);
            for (int a = (halfBoxSize * -1); a <= halfBoxSize; a++)
            {
                for (int b = (halfBoxSize * -1); b <= halfBoxSize; b++)
                {
                    // if a pixel in the structuring element is detected that isn't in the foreground, 
                    // the hotspot gets transformed to the background and the function ends
                    if (matrix[a + halfBoxSize, b + halfBoxSize] != -1 && Image[x + a, y + b].R == backGrC)
                    {
                        newImage[x, y] = updatedColor;
                        return;
                    }
                }
            }
            // if the surrounding pixels pass all checks of the structuring element, the hotspot can stay in the foreground
            newImage[x, y] = Image[x, y];
        }




        /// <summary>
        /// Dilates a greyscale image using the provided matrix as a structuring element.
        /// </summary>
        int CalculateDilation(int x, int y, int[,] matrix, int halfBoxSize, bool isMinMax)
        {
            int newColor = 0;
            for (int a = (halfBoxSize * -1); a <= halfBoxSize; a++)
            {
                for (int b = (halfBoxSize * -1); b <= halfBoxSize; b++)
                {
                    // The maximum value of the structuring element added to the surrounding pixels is chosen and returned as the new greyscale value for the hotspot.
                    if (matrix[a + halfBoxSize, b + halfBoxSize] != -1 && (Image[x + a, y + b].R + matrix[a + halfBoxSize, b + halfBoxSize]) > newColor)
                    {
                        newColor = clamp(Image[x + a, y + b].R + matrix[a + halfBoxSize, b + halfBoxSize]);
                    }
                }
            }
            return newColor;
        }




        /// <summary>
        /// Erodes a greyscale image using the provided matrix as a structuring element.
        /// </summary>
        int CalculateErosion(int x, int y, int[,] matrix, int halfBoxSize, bool isMinMax)
        {
            int newColor = int.MaxValue;
            for (int a = (halfBoxSize * -1); a <= halfBoxSize; a++)
            {
                for (int b = (halfBoxSize * -1); b <= halfBoxSize; b++)
                {
                    // The minimum value of the structuring element subtracted from the surrounding pixels is chosen and returned as the new greyscale value for the hotspot.
                    if (matrix[a + halfBoxSize, b + halfBoxSize] != -1 && (Image[x + a, y + b].R - matrix[a + halfBoxSize, b + halfBoxSize]) < newColor)
                    {
                        newColor = clamp(Image[x + a, y + b].R - matrix[a + halfBoxSize, b + halfBoxSize]);
                    }

                }
            }
            return newColor;
        }



        /// <summary>
        /// Apply an erosion or dilation filter to an input image using the matrix provided in textbox1 as a structuring element.
        /// </summary>
        void ApplyErosionDilationFilter(bool isErosion)
        {
            int[,] matrix = ParseMatrix();
            int newColor1 = 0;
            int backGrColor = 0;
            int foreGrColor = 255;

            if (checkBlackBackground.Checked)
            {
                backGrColor = 0;
                foreGrColor = 255;
            }
            else
            {
                backGrColor = 255;
                foreGrColor = 0;
            }

            //check if the image is binary
            bool binary = isBinary(valueCount(generateHistogram(Image)));


            if (matrix != null)
            {
                int boxsize = matrix.GetLength(0);                                           // length matrix
                int halfBoxSize = (boxsize - 1) / 2;                                        // help variable

                //loop through the image
                for (int x = halfBoxSize; x < InputImage.Size.Width - halfBoxSize; x++)
                {
                    progressBar.PerformStep();
                    for (int y = halfBoxSize; y < InputImage.Size.Height - halfBoxSize; y++)
                    {
                        // binary images: binary erosion/dilation
                        if (binary)
                        {
                            if (Image[x, y].R == foreGrColor)
                            {
                                if (isErosion) CalculateErosionBinary(x, y, matrix, halfBoxSize, backGrColor);
                                else CalculateDilationBinary(x, y, matrix, halfBoxSize, backGrColor);
                            }
                            else
                            {
                                newColor1 = backGrColor;
                                Color UpdatedColor = Color.FromArgb(newColor1, newColor1, newColor1);
                                newImage[x, y] = UpdatedColor;
                            }
                        }
                        // greyscale images: greyscale erosion/dilation and apply new color
                        else
                        {
                            if (isErosion) newColor1 = CalculateErosion(x, y, matrix, halfBoxSize, false);
                            else newColor1 = CalculateDilation(x, y, matrix, halfBoxSize, false);
                            Color updatedColor = Color.FromArgb(newColor1, newColor1, newColor1);
                            newImage[x, y] = updatedColor;
                        }
                    }
                }
            }
        }





        /// <summary>
        /// Apply an opening or closing filter to an input image using the matrix provided in textbox1 as a structuring element.
        /// </summary>
        void ApplyOpeningClosingFilter(bool isOpening)
        {
            if (isOpening) ApplyErosionDilationFilter(true);
            else ApplyErosionDilationFilter(false);

            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    Image[x, y] = newImage[x, y];
                }
            }

            if (isOpening) ApplyErosionDilationFilter(false);
            else ApplyErosionDilationFilter(true);
        }

        void PreprocessingPipeline()
        {
            ApplyGreyscale();
            Color[,] pipelineImage = new Color[InputImage.Size.Width, InputImage.Size.Height];
            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    pipelineImage[x, y] = newImage[x, y];
                }
            }
            String[] inputName = imageFileName1.Text.Split(new string[] { "." }, StringSplitOptions.None);
            kernelInput.Text = "0 0 0\r\n0 0 0\r\n0 0 0";
            for (int g = 70; g <= 80; g++)
            {
                RightAsInput.Checked = false;
                resetForApply();
                for (int x = 0; x < InputImage.Size.Width; x++)
                {
                    for (int y = 0; y < InputImage.Size.Height; y++)
                    {
                        Image[x, y] = pipelineImage[x, y];
                    }
                }
                ApplyThresholdFilter(g);
                RightAsInput.Checked = true;
                resetForApply();
                ApplyOpeningClosingFilter(true);
                toOutputBitmap();
                OutputImage.Save(inputName[0] + " t" + g + ".bmp");
            }
        }

        void RegionLabeling()
        {
            // Region labeling maakt een nieuwe array aan die een border van 1 pixel meer heeft vergeleken met de originele afbeelding
            // Dit is om te zorgen dat de calculaties geen error geven bij foreground check van de afbeelding aan de randen
            int[,] label = new int[InputImage.Size.Width + 2, InputImage.Size.Height + 2];
            int labelIndex = 1;
            bool isLabeled = false;
            List<drawPoint> conflict = new List<drawPoint>();
            for (int x = 1; x < InputImage.Size.Width + 1; x++)
            {
                for (int y = 1; y < InputImage.Size.Height + 1; y++)
                {
                    // Per foreground pixel wordt gekeken of een van de omringende pixels al is gelabeld.
                    // Zo ja, dan krijgt de pixel deze waarde. Mocht dit meerdere keren gebeuren, dan wordt er een conflict genoteerd.
                    // Zo niet, dan wordt een nieuwe waarde aangemaakt en krijgt de pixel deze waarde.
                    if (Image[x - 1, y - 1].R == 255)
                    {
                        isLabeled = false;
                        if (label[x - 1, y - 1] != 0)
                        {
                            label[x, y] = label[x - 1, y - 1];
                            isLabeled = true;
                        }
                        if (label[x, y - 1] != 0)
                        {
                            if (isLabeled == true && label[x, y] != label[x, y - 1] && !conflict.Contains(new drawPoint(x, y)))
                            {
                                conflict.Add(new drawPoint(x, y));
                            }
                            label[x, y] = label[x, y - 1];
                            isLabeled = true;
                        }
                        if (label[x + 1, y - 1] != 0)
                        {
                            if (isLabeled && label[x, y] != label[x + 1, y - 1] && !conflict.Contains(new drawPoint(x, y)))
                            {
                                conflict.Add(new drawPoint(x, y));
                            }
                            label[x, y] = label[x + 1, y - 1];
                            isLabeled = true;
                        }
                        if (label[x - 1, y] != 0)
                        {
                            if (isLabeled && label[x, y] != label[x - 1, y] && !conflict.Contains(new drawPoint(x, y)))
                            {
                                conflict.Add(new drawPoint(x, y));
                            }
                            label[x, y] = label[x - 1, y];
                            isLabeled = true;
                        }
                        if (isLabeled == false)
                        {
                            label[x, y] = labelIndex;
                            labelIndex++;
                        }
                    }
                }
            }

            // Daarna wordt gekeken welke labels hetzelfde figuur omschrijven (en dus samengevoegd kunnen worden)
            // Alle conflictpunten worden nagelopen. Als er pixels worden ontdekt die aan elkaar grenzen, worden ze genoteerd hetzelfde te zijn.
            bool[,] connection = new bool[labelIndex, labelIndex];
            foreach (var ele in conflict)
            {
                for (int i = -1; i < 1; i++)
                {
                    for (int j = -1; j < 1; j++)
                    {
                        if (label[ele.X + i, ele.Y + j] != 0)
                        {
                            connection[label[ele.X, ele.Y], label[ele.X + i, ele.Y + j]] = true;
                            connection[label[ele.X + i, ele.Y + j], label[ele.X, ele.Y]] = true;
                        }
                    }
                }
            }

            // Deze array schrijft nieuwe labels om naar hun kleinst mogelijke waarde.
            int[] newLabel = new int[labelIndex];
            newLabel[0] = 0;
            for (int i = 1; i < newLabel.Length; i++)
            {
                newLabel[i] = i;
                for (int j = 1; j < newLabel.Length; j++)
                {
                    if (connection[i, j] && j < newLabel[i])
                    {
                        newLabel[i] = j;
                    }
                }
            }

            // Hier worden de labels in de afbeelding daadwerkelijk overgeschreven.
            // Ook wordt per new label bijgehouden hoeveel pixels die waarde hebben. 
            // Dit kan later gebruikt worden om te kijken of een region groot genoeg is om een hand te kunnen zijn.
            int[] newLabelCount = new int[labelIndex];
            for (int x = 1; x < InputImage.Size.Width + 1; x++)
            {
                for (int y = 1; y < InputImage.Size.Height + 1; y++)
                {
                    label[x, y] = newLabel[label[x, y]];
                    newLabelCount[label[x, y]]++;

                    int newColor = label[x, y] * 25;
                    Color updatedColor = Color.FromArgb(newColor, newColor, newColor);
                    newImage[x - 1, y - 1] = updatedColor;
                }
            }

            for (int i = 0; i < newLabelCount.Length; i++)
            {
                Console.WriteLine("Label " + i + ": " + newLabelCount[i]);
            }
        }






        // -------------------------------------------- TRACE BOUNDARY CODE --------------------------------------------

        drawPoint[] TraceBoundary()
        {
            Color backGrC = Color.FromArgb(255, 0, 0, 0);
            //Color foreGrC = Color.FromArgb(255, 255, 255, 255);

            Color previousColor = backGrC;

            List<drawPoint> returnValue = new List<drawPoint>();

            for (int v = 0; v < InputImage.Height; v++)
                for (int u = 0; u < InputImage.Width; u++) //x moet 'snelst' doorlopen
                {
                    Color currentColor = Image[u, v];
                    if (previousColor == backGrC && currentColor != backGrC)
                        returnValue = TransFgBg(backGrC, returnValue, u, v);
                }

            return returnValue.ToArray();
        }



        /// <summary>
        /// Handles the case that there is a transition from background to foreground in the image, and traces the figure found.
        /// </summary>
        /// <param name="backgrC">background color</param>
        List<drawPoint> TransFgBg(Color backgrC, List<drawPoint> ContourPixels, int u, int v)
        {

            if (isContourPix(backgrC, u, v))
            {
                if (Image[u, v] != backgrC)
                {
                    ContourPixels.Add(new drawPoint(u, v));

                    //This is N8 chain code, for N4 only consider the 4 pixels straight up, below, left and right
                    for (int x = -1; x <= 1; x++) //kijk van rechtsonder naar linksboven, dan loop je minder snel terug.
                        for (int y = -1; y <= 1; y++)
                        {
                            if (u + x >= 0 && v + y >= 0 && u + x < InputImage.Width && v + y < InputImage.Height)
                                if (!ContourPixels.Contains(new drawPoint(u + x, v + y)))
                                {
                                    TransFgBg(backgrC, ContourPixels, u + x, v + y);
                                }
                        }
                }
            }
            //If all edge pixels are traced, return the complete list of contourpixels 
            return ContourPixels;
        }



        Boolean isContourPix(Color backgrC, int u, int v)
        {
            for (int x = -1; x <= 1; x++) //check if neighbouringpixels are background
                for (int y = -1; y <= 1; y++)
                {
                    if (u + x >= 0 && v + y >= 0 && u + x < InputImage.Width && v + y < InputImage.Height)
                        if (Image[u + x, v + y] == backgrC)
                            return true;
                }

            return false;
        }



        String WritedrawPointArr(drawPoint[] drawPoints)
        {
            String output = "{";
            for (int i = 0; i < drawPoints.Length; i++)
            {
                output = output + "(" + drawPoints[i].X + "," + drawPoints[i].Y + "), ";
            }
            output += "}";
            return output;
        }



        void BoundaryToOutput(drawPoint[] drawPoints)
        {
            for (int x = 0; x < InputImage.Width; x++)
                for (int y = 0; y < InputImage.Height; y++)
                {
                    if (drawPoints.Contains(new drawPoint(x, y)))
                        OutputImage.SetPixel(x, y, Color.FromArgb(255, 0, 0, 0));
                    else
                        OutputImage.SetPixel(x, y, Color.FromArgb(255, 255, 255, 255));
                }

            outputBox1.Image = (Image)OutputImage;                         // Display output image
            progressBar.Visible = false;
        }

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
            int[,] Hx = new int[,] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
            int[,] Hy = new int[,] { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };


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

        void ApplyGreyscale()
        {
            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    Color pixelColor = Image[x, y];                                 // Get the pixel color at coordinate (x,y)
                    int grey = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;    // aanmaken grijswaarde op basis van RGB-values
                    Color updatedColor = Color.FromArgb(grey, grey, grey);          // toepassen grijswaarde
                    newImage[x, y] = updatedColor;
                    progressBar.PerformStep();
                }
            }
        }

        int CalculateNewColor(int x, int y, int[,] matrix, int halfBoxSize, bool divideByTotal = true)
        {
            int linearColor = 0;
            int matrixTotal = 0;                // totale waarde van alle weights van de matrix bij elkaar opgeteld
            for (int a = (halfBoxSize * -1); a <= halfBoxSize; a++)
            {
                for (int b = (halfBoxSize * -1); b <= halfBoxSize; b++)
                {
                    linearColor = linearColor + (Image[x + a, y + b].R * matrix[a + halfBoxSize, b + halfBoxSize]);
                    // weight van filter wordt per kernel pixel toegepast op image pixel
                    matrixTotal = matrixTotal + matrix[a + halfBoxSize, b + halfBoxSize];
                    // weight wordt opgeteld bij totaalsom van weights
                }
            }
            if (divideByTotal == true) // Voor Edgestrength moet niet door het totaal gedeeld, dus kan hij uitgezet worden.
                linearColor = linearColor / matrixTotal;

            return linearColor;
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
