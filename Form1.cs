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



        void LoadAndCropImage()
        {
            /*cornerDetection test
            int startx = 0;
            int starty = 0;
            int startWidth = 100;
            int startHeight = 100;
            */
            int startx = 20;
            int starty = 0;
            int startWidth = 458;
            int startHeight = 264;

            if (openImageDialog.ShowDialog() == DialogResult.OK)             // Open File Dialog
            {
                String file = openImageDialog.FileName;                     // Get the file name

                imageFileName1.Text = file;

                if (InputImage != null) InputImage.Dispose();               // Reset image
                InputImage = new Bitmap(file);                              // Create new Bitmap from file

                resetForApply();
                Color[,] croppedImage = CutSubImageBox(Image, startx, starty, startWidth, startHeight);
                InputImage = new Bitmap(croppedImage.GetLength(0), croppedImage.GetLength(1));
                for (int x = 0; x < croppedImage.GetLength(0); x++)
                    for (int y = 0; y < croppedImage.GetLength(1); y++)
                    {
                        InputImage.SetPixel(x, y, croppedImage[x, y]);
                    }
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
            // Variabelen die nodig zijn om de bounding box en region count worden geinitializeerd
            minx = InputImage.Size.Width;
            miny = InputImage.Size.Height;
            maxx = 0;
            maxy = 0;
            regionCount = int.MaxValue;
            currentRegions = int.MaxValue;
            optimalLabel = new int[InputImage.Size.Width, InputImage.Size.Height];

            // Allereerst wordt de inputimage grijs gemaakt en gekopieerd naar een aparte color[,] array.
            ApplyGreyscale();
            Color[,] pipelineImage = new Color[InputImage.Size.Width, InputImage.Size.Height];
            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    pipelineImage[x, y] = newImage[x, y];
                }
            }
            // Kernelinput en threshold start worden ook geinitialiseerd.
            kernelInput.Text = "0 0 0\r\n0 0 0\r\n0 0 0";
            int greyscale = 70;

            // Daarna begint het checken.
            // De grijsafbeelding die was gekopieerd wordt weer opnieuw ingeladen. Hier wordt eerst een threshold en daarna een opening filter op gebruikt.
            // Daarna wordt de region labeling functie uitgevoerd, waaruit het aantal regions van die afbeelding rolt.
            // Als er een gelijk aantal of minder regions dan het minimale aantal uitkomt, wordt de bounding box geupdated.
            // Dit gaat door totdat de bounding box niet meer geupdated wordt en er 3 extra regions zijn ontdekt (gebeurt vanzelf door ruis).
            while (regionCount - 3 <= currentRegions)
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
                ApplyThresholdFilter(greyscale);
                RightAsInput.Checked = true;
                resetForApply();
                ApplyOpeningClosingFilter(true);
                greyscale++;
                RegionLabeling(greyscale);
                Console.WriteLine("regionCount: " + regionCount + ", currentRegions: " + currentRegions);
            }

            // Hierna wordt de bounding box uit de grijsafbeelding gesneden en als output verder verwerkt.
            // De coordinaten van de bounding box blijven staan, dus die kunnen later gebruikt worden om in de originele afbeelding de positie van de hand te weergeven.
            
            // Dit stukje is eigenlijk niet meer nodig, maar kan nog gebruikt worden om een threshold te krijgen van de optimale threshold die eerder berekend is
            /*
            resetForApply();
            RightAsInput.Checked = false;
            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    Image[x, y] = pipelineImage[x, y];
                }
            }
            ApplyThresholdFilter(optimalThreshold);
            RightAsInput.Checked = true;
            resetForApply();
            ApplyOpeningClosingFilter(true);
            */
            

            newImage = new Color[maxx - minx, maxy - miny];
            for (int x = 0; x < maxx - minx; x++)
            {
                for (int y = 0; y < maxy - miny; y++)
                {
                    //newImage[x, y] = pipelineImage[x + minx, y + miny];
                    // EN DIT OOK, EN DAN DE ANDERE NEWIMAGE STATEMENT UITCOMMENTEN JUIST
                    int labelColor = 255 - optimalLabel[x + minx, y + miny];
                    Color updatedColor = Color.FromArgb(labelColor, labelColor, labelColor);
                    newImage[x, y] = updatedColor;
                }
            }
            toOutputBitmap();
            RightAsInput.Checked = false;
            Console.WriteLine("(Minx, miny): (" + minx + ", " + miny + ") - (Maxx, maxy): (" + maxx + ", " + maxy + ")");
            Console.WriteLine("Optimal greyscale threshold value: " + optimalThreshold);
        }

        void RegionLabeling(int greyscale)
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
                    // Mocht er trouwens een pixel gedetecteerd worden met alpha value 0 (doorzichtig), dan wordt die tot de achtergrond gerekend.
                    if (Image[x - 1, y - 1].R == 0)
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
                                conflict.Add(new drawPoint(x, y));
                            label[x, y] = label[x, y - 1];
                            isLabeled = true;
                        }
                        if (label[x + 1, y - 1] != 0)
                        {
                            if (isLabeled && label[x, y] != label[x + 1, y - 1] && !conflict.Contains(new drawPoint(x, y)))
                                conflict.Add(new drawPoint(x, y));
                            label[x, y] = label[x + 1, y - 1];
                            isLabeled = true;
                        }
                        if (label[x - 1, y] != 0)
                        {
                            if (isLabeled && label[x, y] != label[x - 1, y] && !conflict.Contains(new drawPoint(x, y)))
                                conflict.Add(new drawPoint(x, y));
                            label[x, y] = label[x - 1, y];
                            isLabeled = true;
                        }
                        if (Image[x - 1, y - 1].A == 0)
                        {
                            label[x, y] = 0;
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
                        int label1 = label[ele.X, ele.Y];
                        int label2 = label[ele.X + i, ele.Y + j];
                        if (label2 != 0 && (!connection[label1, label2] || !connection[label2, label1]))
                        {
                            connection[label1, label2] = true;
                            connection[label2, label1] = true;
                            //Console.WriteLine("Label " + label1 + " and label " + label2 + " describe the same region.");
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
            }
            for (int i = 1; i < newLabel.Length; i++)
            {
                List<int> visited = new List<int>();
                newLabel[i] = smallestRegion(i, newLabel, visited, connection);
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

                    int newColor = 0;
                    if (Image[x - 1, y - 1].A == 0)
                    {
                        label[x, y] = 0;
                    }
                    if (label[x, y] == 0)
                    {
                        newColor = 255;
                    }
                    Color updatedColor = Color.FromArgb(newColor, newColor, newColor);
                    newImage[x - 1, y - 1] = updatedColor;
                }
            }

            // De waardes van labels worden geprint en er wordt gekeken wat de grootste region is (afgezien van de background).
            // Het aantal regions wordt ook geteld.
            int largestLabel = 1;
            regionCount = 0;
            for (int i = 0; i < newLabelCount.Length; i++)
            {
                if (newLabelCount[i] > 0)
                {
                    Console.WriteLine("Label " + i + ": " + newLabelCount[i]);
                    regionCount++;
                }
                if (newLabelCount[i] > newLabelCount[largestLabel] && i != 0)
                {
                    largestLabel = i;
                    Console.WriteLine(i + "is now the largest region.");
                }
            }

            // Als er minder of evenveel regions zijn als de vorige afbeelding, wordt de bounding box geupdated om de mogelijke hand van de huidige afbeelding te bevatten.
            // Hier wordt een marge van 10 pixels extra bijgerekend, om corner detection nauwkeuriger te maken en eventuele missende vingers er nog aan te plakken.
            if (regionCount <= currentRegions)
            {
                optimalThreshold = greyscale;
                for (int x = 1; x < InputImage.Size.Width + 1; x++)
                {
                    for (int y = 1; y < InputImage.Size.Height + 1; y++)
                    {
                        if (label[x, y] == largestLabel)
                        {
                            optimalLabel[x - 1, y - 1] = 255;
                            if (x - 1 < minx)
                            {
                                minx = x - 1 - 10;
                                if (minx < 0)
                                    minx = 0;
                            }
                            if (y - 1 < miny)
                            {
                                miny = y - 1 - 10;
                                if (miny < 0)
                                    miny = 0;
                            }
                            if (x - 1 > maxx)
                            {
                                maxx = x - 1 + 10;
                                if (maxx > InputImage.Size.Width)
                                    maxx = InputImage.Size.Width;
                            }
                            if (y - 1 > maxy)
                            {
                                maxy = y - 1 + 10;
                                if (maxy > InputImage.Size.Height)
                                    maxy = InputImage.Size.Height;
                            }
                        }
                    }
                }
                currentRegions = regionCount;
            }
        }

        // Per label input wordt nagelopen of de andere labels er aan grenzen en of die nog niet zijn bezocht
        // Zo ja, dan wordt er recursief gekeken of andere labels een lagere newLabel waarde hebben dan de huidige
        // Als dat het geval is, dan wordt deze geupdatet
        int smallestRegion(int input, int[] newLabel, List<int> visited, bool[,] connection)
        {
            for (int i = 1; i < newLabel.Length; i++)
            {
                if (!visited.Contains(i) && (connection[i, input] || connection[input, i]))
                {
                    visited.Add(i);
                    int compare = smallestRegion(i, newLabel, visited, connection);
                    if (compare < newLabel[input])
                    {
                        return compare;
                    }
                }
            }
            return newLabel[input];
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
                        returnValue = TraceFigureOutline(backGrC, returnValue, u, v);
                }

            return returnValue.ToArray();
        }



        /// <summary>
        /// Handles the case that there is a transition from background to foreground in the image, and traces the figure found.
        /// </summary>
        /// <param name="backgrC">background color</param>
        List<drawPoint> TraceFigureOutline(Color backgrC, List<drawPoint> ContourPixels, int u, int v)
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
                                    TraceFigureOutline(backgrC, ContourPixels, u + x, v + y);
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

        float CalculateNewColor(int x, int y, float[,] matrix, int halfBoxSize, bool divideByTotal = true)
        {
            float linearColor = 0;
            float matrixTotal = 0;                // totale waarde van alle weights van de matrix bij elkaar opgeteld
            for (int a = -halfBoxSize; a <= halfBoxSize; a++)
            {
                for (int b = -halfBoxSize; b <= halfBoxSize; b++)
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

        
        void HarrisCornerDetection(float[,] Kx, float[,] Ky)
        {
            
            float[,] Avalues = new float[InputImage.Size.Width, InputImage.Size.Height];
            float[,] Bvalues = new float[InputImage.Size.Width, InputImage.Size.Height];
            float[,] Cvalues = new float[InputImage.Size.Width, InputImage.Size.Height];

            //float[,] Hx = new float[,] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
            //float[,] Hy = new float[,] { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };

            int halfboxsize = Kx.GetLength(0) / 2;

            for (int v = halfboxsize; v < InputImage.Size.Height - halfboxsize; v++) 
            {
                for (int u = halfboxsize; u < InputImage.Size.Width - halfboxsize; u++)
                {
                    float Ix = CalculateNewColor(u, v, Kx, halfboxsize, false) / 8; //apply Kx to the image pixel
                    float Iy = CalculateNewColor(u, v, Ky, halfboxsize, false) / 8; //apply Ky to the image pixel


                    //int edgeStrength = (int)Math.Sqrt(Ix * Ix + Iy * Iy); //calculate edgestrength by calculating the length of vector [Hx, Hy]
                    Avalues[u,v] = Ix * Ix;
                    Bvalues[u,v] = Iy * Iy;
                    Cvalues[u,v] = Ix * Iy;

                }
                progressBar.PerformStep();
            }
            Avalues = ApplyGaussianFilter(Avalues, 1.1f, 5);
            Bvalues = ApplyGaussianFilter(Bvalues, 1.1f, 5);
            Cvalues = ApplyGaussianFilter(Cvalues, 1.1f, 5);
            float[,] Qvalues = CalculateQvalues(Avalues, Bvalues, Cvalues);
            //float[,] highestQvalues = PickStrongestCorners(Qvalues, 10);
            List<Corner> cornerList = QToCorners(Qvalues, 2000000);
            List<Corner> goodCorners = cleanUpCorners(cornerList, 2.25); //dmin waarde opzoeken, Alg. 4.1 regel 8-16
            CornersToImage(goodCorners);
            toOutputBitmap();
        }


        //classe corner maken, waardoor je die 3 waardes in een lijst kan zetten en cleanupcorners bouwen.
        
        List<Corner> cleanUpCorners(List<Corner> cornerList, double dmin)
        {
            //corners have to be sorted by descending Q.
            double dmin2 = dmin * dmin;
            Corner[] cornerArray = cornerList.ToArray();
            List<Corner> goodCorners = new List<Corner>();

            for(int i = 0; i< cornerArray.Length; i++)
                if (cornerArray[i] != null)
                {
                    Corner c1 = cornerArray[i];
                    goodCorners.Add(c1);

                    for (int j = i + 1; j < cornerArray.Length; j++)
                        if (cornerArray[j] != null)
                        {
                            Corner c2 = cornerArray[j];
                            if (c1.CornerDistanceUV(c2) < dmin2) //compare squared distances
                            {
                                //if there are too many corners in one distance
                                cornerArray[j] = null; //remove corner c2
                            }
                        }
                }

            return goodCorners;
        }

        void CornersToImage(List<Corner> corners)
        {
            int backgroundColor = 255;
            int foregroundColor = 0;
            
            for(int u = 0; u < newImage.GetLength(0); u++)
                for (int v = 0; v < newImage.GetLength(1); v++)
                {
                    //fill the entire image with backgroundcolor
                    newImage[u, v] = Color.FromArgb(backgroundColor, backgroundColor, backgroundColor);
                }
            foreach( Corner c in corners)
            {
                int Q = (int) c.Q;
                Console.WriteLine(Q);
                newImage[c.U, c.V] = Color.FromArgb(clamp(Q), 0, 0);
            }
        }

        float[,] CalculateQvalues(float[,] Avalues, float[,] Bvalues, float[,] Cvalues)
        {
            float[,] Qvalues = new float[InputImage.Size.Width, InputImage.Size.Height];

            for (int x = 0; x < Avalues.GetLength(0); x++)
            {
                for(int y = 0; y < Avalues.GetLength(1); y++)
                {
                    float A = Avalues[x, y];
                    float B = Bvalues[x, y];
                    float C = Cvalues[x, y];

                    float traceM = A + B;
                    float squareRoot = (float) Math.Sqrt(A * A - 2 * A * B + B * B + 4 * C * C);
                    float lambda1 = (float) (0.5 * (traceM + squareRoot));
                    float lambda2 = (float) (0.5 * (traceM - squareRoot));

                    float alfa = 0.05f;

                    float Quv = (A * B - C * C) - (alfa * traceM * traceM);

                    Qvalues[x, y] = (int)Quv;
                }
            }
            return Qvalues;
            
        }

        float[] Create1DKernel(int boxsize, float sigma, int filterBorder)
        {
            float[] kernel = new float[boxsize];

            // berekenen 1D gaussian filter, uit boek pagina 115
            float sigma2 = sigma * sigma;
            float countingKernel = 0;
            for (int j = 0; j < kernel.Length; j++)
            {
                float r = filterBorder - j;
                kernel[j] = (float)Math.Exp(-0.5 * (r * r) / sigma2);
                countingKernel += kernel[j];
            }
            for (int j = 0; j < kernel.Length; j++)
            {
                kernel[j] = kernel[j] / countingKernel;
            }
            return kernel;
        }

        private float[,] ApplyGaussianFilter(float[,] valueArray, float sigma, int boxsize)
        {
            // input lezen: eerst een float voor de sigma, dan een int voor de kernel size
            
            int filterBorder = (boxsize - 1) / 2;                                       //hulpvariabele voor verdere berekeningen

            float[] kernel = Create1DKernel(boxsize, sigma, filterBorder);

            // maak arrays om tijdelijke variabelen in op te slaan
            float[,] gaussian1DColor = new float[boxsize, boxsize];
            float[,] gaussian2DColor = new float[boxsize, boxsize];
            float[,] weight = new float[boxsize, boxsize];

            // verzamel- en telvariabelen
            float gaussianTotal = 0;
            float weightTotal = 0;
            int i = 0;

            float[,] newValueArray = new float[valueArray.GetLength(0), valueArray.GetLength(1)];


            //Doorloop de image per pixel
            for (int x = filterBorder; x < InputImage.Size.Width - filterBorder; x++)
            {
                for (int y = filterBorder; y < InputImage.Size.Height - filterBorder; y++)
                {
                    gaussianTotal = 0;
                    weightTotal = 0;
                    i = 0;

                    // 1D-gaussian wordt eerst horizontaal toegepast. Nieuwe grijswaarden en weights worden in bijbehorende arrays opgeslagen.
                    for (int a = -filterBorder; a <= filterBorder; a++)
                    {
                        i = 0;
                        for (int b = -filterBorder; b <= filterBorder; b++)
                        {
                            gaussian1DColor[a + filterBorder, b + filterBorder] = ((float)valueArray[x + a, y + b] * kernel[i]);
                            weight[a + filterBorder, b + filterBorder] = kernel[i];
                            i++;
                        }
                    }

                    // 1D-gaussian wordt opnieuw toegepast, nu verticaal.
                    // Grijswaarden uit de eerste keer worden gebruikt om de nieuwe grijswaarden te maken, en deze worden bij elkaar opgeteld.
                    // Weights worden ook herberekend en bij elkaar opgeteld.
                    i = 0;
                    for (int a = -filterBorder; a <= filterBorder; a++)
                    {
                        for (int b = -filterBorder; b <= filterBorder; b++)
                        {
                            gaussian2DColor[a + filterBorder, b + filterBorder] = (gaussian1DColor[a + filterBorder, b + filterBorder] * kernel[i]);
                            gaussianTotal = gaussianTotal + gaussian2DColor[a + filterBorder, b + filterBorder];
                            weight[a + filterBorder, b + filterBorder] = weight[a + filterBorder, b + filterBorder] * kernel[i];
                            weightTotal = weightTotal + weight[a + filterBorder, b + filterBorder];
                        }
                        i++;
                    }


                    // Totale som grijswaarden delen door totale weights om uiteindelijke grijswaarde te krijgen.
                    int gaussianColor = (int)(gaussianTotal / weightTotal);

                    newValueArray[x, y] = gaussianColor;
                }
            }
            return newValueArray;
        }



        List<Corner> QToCorners(float[,] Qvalues, int threshold)
        {
            List<Corner> cornerList = new List<Corner>();
            for(int x = 0; x < Qvalues.GetLength(0); x++)
                for(int y = 0; y < Qvalues.GetLength(1); y++)
                {
                    
                    if (Qvalues[x,y] > threshold && IsLocalMax(Qvalues, x, y))
                    {
                        //10500000
                        //3900000
                        Corner corner = new Corner(x, y, Qvalues[x, y]);
                        cornerList.Add(corner);
                    }
                    
                }
            cornerList.Sort();
            return cornerList;
        }

        bool IsLocalMax(float[,] Qvalues, int x, int y)
        {
            int halfBoxSize = 2;
            float centerQ = Qvalues[x, y];

            for (int a = -halfBoxSize; a <= halfBoxSize; a++) //loop through the pixels around the pixel.
            {
                for (int b = -halfBoxSize; b <= halfBoxSize; b++)
                {
                    int checkX = x + a;
                    int checkY = y + b;
                    if(checkX > 0 && checkY > 0 && checkX < Qvalues.GetLength(0) && checkY < Qvalues.GetLength(1))
                    if (Qvalues[checkX, checkY] > centerQ)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        //Creating Sobel kernel of odd size > 1 of arbitrary size
        //Copied from: https://stackoverflow.com/questions/9567882/sobel-filter-kernel-of-large-size/41065243#41065243
        public static void CreateSobelKernel(int n, ref float[,] Kx, ref float[,] Ky)
        {
            int side = n * 2 + 3;
            Kx = new float[side, side];
            Ky = new float[side, side];
            int halfSide = side / 2;
            for (int i = 0; i < side; i++)
            {
                int k = (i <= halfSide) ? (halfSide + i) : (side + halfSide - i - 1);
                for (int j = 0; j < side; j++)
                {
                    if (j < halfSide)
                        Kx[i,j] = Ky[j,i] = j - k;
                    else if (j > halfSide)
                        Kx[i,j] = Ky[j,i] = k - (side - j - 1);
                    else
                        Kx[i,j] = Ky[j,i] = 0;
                }
            }
        }


        /// <summary>
        /// crops the image to a certain size. The pixel selected lands in the left upper corner
        /// </summary>
        /// <param name="fullImage">input image</param>
        /// <param name="u">x pixel for the left upper corner of new image</param>
        /// <param name="v">y pixel for the left upper corner of new image</param>
        /// <param name="width">new width</param>
        /// <param name="height">new height</param>
        /// <returns></returns>
        Color[,] CutSubImageBox(Color[,] fullImage, int u, int v, int width, int height)
        {
            if (width >= fullImage.GetLength(0))
                return fullImage;
            if (height >= fullImage.GetLength(1))
                return fullImage;

            Color[,] subImage = new Color[width, height];
            for( int x = 0; x < width; x++)
                for (int y = 0; y < height; y++ )
                {
                    subImage[x, y] = fullImage[u + x, v + y];
                }
            return subImage;
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

    public class Corner : IComparable<Corner>
    {
        int u;
        int v;
        float q;

        public Corner(int u, int v, float q)
        {
            this.u = u;
            this.v = v;
            this.q = q;
        }

        public int CompareTo(Corner that)
        {
            if (this.q > that.q) return -1;
            if (this.q == that.q) return 0;
            return 1;
        }

        public double CornerDistanceUV(Corner c2)
        {
            double distx = this.u - c2.u;
            double disty = this.v - c2.v;

            return  Math.Sqrt(distx * distx + disty * disty);
        }

        public int U { get { return this.u; } }
        public int V { get { return this.v; } }

        public float Q { get { return this.q; } }
    }
}
