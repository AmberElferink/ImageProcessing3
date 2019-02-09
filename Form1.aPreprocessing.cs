using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using drawPoint = System.Drawing.Point;

namespace INFOIBV
{
    public partial class INFOIBV : Form
    {

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
                Color[,] croppedImage = CropImage(Image, startx, starty, startWidth, startHeight);
                InputImage = new Bitmap(croppedImage.GetLength(0), croppedImage.GetLength(1));
                for (int x = 0; x < croppedImage.GetLength(0); x++)
                    for (int y = 0; y < croppedImage.GetLength(1); y++)
                    {
                        InputImage.SetPixel(x, y, croppedImage[x, y]);
                    }
                pictureBox1.Image = (Image)InputImage;
            }
        }


        Color[,] ApplyGreyscale(Color[,] InputImage)
        {
            Color[,] greyscaleImage = new Color[InputImage.GetLength(0), InputImage.GetLength(1)];
            for (int x = 0; x < InputImage.GetLength(0); x++)
            {
                for (int y = 0; y < InputImage.GetLength(1); y++)
                {
                    Color pixelColor = InputImage[x, y];                                 // Get the pixel color at coordinate (x,y)
                    int grey = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;    // aanmaken grijswaarde op basis van RGB-values
                    Color updatedColor = Color.FromArgb(grey, grey, grey);          // toepassen grijswaarde
                    greyscaleImage[x, y] = updatedColor;
                }
            }
            return greyscaleImage;
        }


         Color[,] ApplyThresholdFilter(Color[,] InputImage, int thresholdLimit = 125)
        {
            int inputImageW = InputImage.GetLength(0);
            int inputImageH = InputImage.GetLength(1);
            Color[,] OutputImage = new Color[inputImageW, inputImageH];



            Color updatedColor = Color.FromArgb(255, 255, 255, 255);
            Color backGroundColor = getBackgroundColor();
            Color foreGroundColor = getForegroundColor();
            for (int x = 0; x < inputImageW; x++)
            {
                for (int y = 0; y < inputImageH; y++)
                {
                    if (InputImage[x, y].R < thresholdLimit)
                        updatedColor = foreGroundColor;
                    else
                        updatedColor = backGroundColor;
                    OutputImage[x, y] = updatedColor;

                }
            }
            return OutputImage;

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

            for (int x = 0; x < image.GetLength(0); x++)
            {
                for (int y = 0; y < image.GetLength(1); y++)
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
        /// Dilates a binary image using the provided matrix as structuring element.
        /// </summary>
        Color[,] CalculateDilationBinary(Color[,] InputImage, Color[,] OutputImage, int x, int y, int[,] matrix, int halfBoxSize, Color backGrC, Color foreGrC)
        {
            for (int a = (halfBoxSize * -1); a <= halfBoxSize; a++)
            {
                for (int b = (halfBoxSize * -1); b <= halfBoxSize; b++)
                {
                    // every pixel that exists on the structuring element and is currently in the background gets transformed to the foreground
                    if (matrix[a + halfBoxSize, b + halfBoxSize] != -1 && InputImage[x + a, y + b] == backGrC)
                    {
                        OutputImage[x + a, y + b] = foreGrC;
                    }
                    // every pixel that doesn't meet these conditions retains its former color
                    else OutputImage[x, y] = foreGrC;
                }
            }
            return OutputImage;
        }




        /// <summary>
        /// Erodes a binary image using the provided matrix as a structuring element.
        /// </summary>
        Color[,] CalculateErosionBinary(Color[,] InputImage, Color[,] OutputImage, int x, int y, int[,] matrix, int halfBoxSize, Color backGrC)
        {
            for (int a = (halfBoxSize * -1); a <= halfBoxSize; a++)
            {
                for (int b = (halfBoxSize * -1); b <= halfBoxSize; b++)
                {
                    // if a pixel in the structuring element is detected that isn't in the foreground, 
                    // the hotspot gets transformed to the background and the function ends
                    if (matrix[a + halfBoxSize, b + halfBoxSize] != -1 && InputImage[x + a, y + b] == backGrC)
                    {
                        OutputImage[x, y] = backGrC;
                        return OutputImage;
                    }
                }
            }
            // if the surrounding pixels pass all checks of the structuring element, the hotspot can stay in the foreground
            OutputImage[x, y] = InputImage[x, y];
            return OutputImage;
        }




        /// <summary>
        /// Dilates a greyscale image using the provided matrix as a structuring element.
        /// </summary>
        int CalculateDilation(Color[,] InputImage, int x, int y, int[,] matrix, int halfBoxSize, bool isMinMax)
        {
            int newColor = 0;
            for (int a = (halfBoxSize * -1); a <= halfBoxSize; a++)
            {
                for (int b = (halfBoxSize * -1); b <= halfBoxSize; b++)
                {
                    // The maximum value of the structuring element added to the surrounding pixels is chosen and returned as the new greyscale value for the hotspot.
                    if (matrix[a + halfBoxSize, b + halfBoxSize] != -1 && (InputImage[x + a, y + b].R + matrix[a + halfBoxSize, b + halfBoxSize]) > newColor)
                    {
                        newColor = clamp(InputImage[x + a, y + b].R + matrix[a + halfBoxSize, b + halfBoxSize]);
                    }
                }
            }
            return newColor;
        }




        /// <summary>
        /// Erodes a greyscale image using the provided matrix as a structuring element.
        /// </summary>
        int CalculateErosion(Color[,] InputImage, int x, int y, int[,] matrix, int halfBoxSize, bool isMinMax)
        {
            int newColor = int.MaxValue;
            for (int a = (halfBoxSize * -1); a <= halfBoxSize; a++)
            {
                for (int b = (halfBoxSize * -1); b <= halfBoxSize; b++)
                {
                    // The minimum value of the structuring element subtracted from the surrounding pixels is chosen and returned as the new greyscale value for the hotspot.
                    if (matrix[a + halfBoxSize, b + halfBoxSize] != -1 && (InputImage[x + a, y + b].R - matrix[a + halfBoxSize, b + halfBoxSize]) < newColor)
                    {
                        newColor = clamp(InputImage[x + a, y + b].R - matrix[a + halfBoxSize, b + halfBoxSize]);
                    }

                }
            }
            return newColor;
        }



        /// <summary>
        /// Apply an erosion or dilation filter to an input image using the matrix provided in textbox1 as a structuring element.
        /// </summary>
        Color[,] ApplyErosionDilationFilter(Color[,] InputImage, bool isErosion)
        {
            int[,] matrix = ParseMatrix();
            int newColor1 = 0;
            Color[,] OutputImage = new Color[InputImage.GetLength(0), InputImage.GetLength(1)];

            Color backGrColor = getBackgroundColor();
            Color foreGrColor = getForegroundColor();

            //check if the image is binary
            bool binary = MaybeBinary(InputImage, valueCount(generateHistogram(InputImage))).Item2;

            if (matrix != null)
            {
                int boxsize = matrix.GetLength(0);                                           // length matrix
                int halfBoxSize = (boxsize - 1) / 2;                                        // help variable

                //loop through the image
                for (int x = halfBoxSize; x < InputImage.GetLength(0) - halfBoxSize; x++)
                {
                    progressBar.PerformStep();
                    for (int y = halfBoxSize; y < InputImage.GetLength(1) - halfBoxSize; y++)
                    {
                        // binary images: binary erosion/dilation
                        if (binary)
                        {
                            if (InputImage[x, y] == foreGrColor)
                            {
                                if (isErosion) OutputImage = CalculateErosionBinary(InputImage, OutputImage, x, y, matrix, halfBoxSize, backGrColor);
                                else OutputImage = CalculateDilationBinary(InputImage, OutputImage, x, y, matrix, halfBoxSize, backGrColor, foreGrColor);
                            }
                            else
                            {
                                OutputImage[x, y] = backGrColor;
                            }
                        }
                        // greyscale images: greyscale erosion/dilation and apply new color
                        else
                        {
                            if (isErosion) newColor1 = CalculateErosion(InputImage, x, y, matrix, halfBoxSize, false);
                            else newColor1 = CalculateDilation(InputImage, x, y, matrix, halfBoxSize, false);
                            Color updatedColor = Color.FromArgb(newColor1, newColor1, newColor1);
                            OutputImage[x, y] = updatedColor;
                        }
                    }
                }
            }
            return OutputImage;
        }





        /// <summary>
        /// Apply an opening or closing filter to an input image using the matrix provided in textbox1 as a structuring element.
        /// </summary>
        Color[,] ApplyOpeningClosingFilter(Color[,] InputImage, bool isOpening)
        {
            Color[,] OutputImage = new Color[InputImage.GetLength(0), InputImage.GetLength(1)];
            if (isOpening) OutputImage = ApplyErosionDilationFilter(InputImage, true);
            else OutputImage = ApplyErosionDilationFilter(InputImage, false);

            if (isOpening) OutputImage = ApplyErosionDilationFilter(OutputImage, false);
            else OutputImage = ApplyErosionDilationFilter(OutputImage, true);
            return OutputImage;
        }

        Color[,] PreprocessingPipeline(Color[,] InputImage)
        {
            if(valueCount(generateHistogram(InputImage)) == 2)
            {
                MessageBox2.Text = "Please load a non thresholded image to preprocess";
                return new Color[InputImage.GetLength(0), InputImage.GetLength(1)];
            }
            // Variabelen die nodig zijn om de bounding box en region count worden geinitializeerd
            
            leftUpperBbX = InputImage.GetLength(0);
            leftUpperBbY = InputImage.GetLength(1);
            maxx = 0;
            maxy = 0;
            regionCount = int.MaxValue;
            currentRegions = int.MaxValue;
            optimalLabel = new bool[InputImage.GetLength(0), InputImage.GetLength(1)];

            // Allereerst wordt de inputimage grijs gemaakt en gekopieerd naar een aparte color[,] array.
            greyscaleImage = ApplyGreyscale(InputImage);
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
                Color[,] thresholdImage = ApplyThresholdFilter(greyscaleImage, greyscale);
                Color[,] OpeningImage = ApplyOpeningClosingFilter(thresholdImage, true);
                RegionLabeling(greyscale, OpeningImage);
                Console.WriteLine("regionCount: " + regionCount + " at greyscale " + greyscale + ", currentRegions: " + currentRegions);
                greyscale++;
                if (greyscale > 100)
                {
                    MessageBox2.Text = "Threshold value is above 100, please check if the left image is a non preprocessed image";
                    break;
                }

            }

            // Hierna wordt de bounding box uit de grijsafbeelding gesneden en als output verder verwerkt.
            // De coordinaten van de bounding box blijven staan, dus die kunnen later gebruikt worden om in de originele afbeelding de positie van de hand te weergeven.

            Color [,] boundingBox = new Color[maxx - leftUpperBbX, maxy - leftUpperBbY];
            for (int x = 0; x < maxx - leftUpperBbX; x++)
            {
                for (int y = 0; y < maxy - leftUpperBbY; y++)
                {
                    if (optimalLabel[x + leftUpperBbX, y + leftUpperBbY])
                        boundingBox[x, y] = getForegroundColor();
                    else
                        boundingBox[x, y] = getBackgroundColor();
                }
            }
            RightAsInput.Checked = false;
            return boundingBox;

            //Console.WriteLine("(Minx, miny): (" + minx + ", " + miny + ") - (Maxx, maxy): (" + maxx + ", " + maxy + ")");
            //Console.WriteLine("Optimal greyscale threshold value: " + optimalThreshold);
        }

        void RegionLabeling(int greyscale, Color[,] InputImage)
        {
            // Region labeling maakt een nieuwe array aan die een border van 1 pixel meer heeft vergeleken met de originele afbeelding
            // Dit is om te zorgen dat de calculaties geen error geven bij foreground check van de afbeelding aan de randen
            Color background = getBackgroundColor();
            Color foreground = getForegroundColor();
            int[,] label = new int[InputImage.GetLength(0) + 2, InputImage.GetLength(1) + 2];
            int labelIndex = 1;
            bool isLabeled = false;
            List<drawPoint> conflict = new List<drawPoint>();
            for (int x = 1; x < InputImage.GetLength(0) + 1; x++)
            {
                for (int y = 1; y < InputImage.GetLength(1) + 1; y++)
                {
                    // Per foreground pixel wordt gekeken of een van de omringende pixels al is gelabeld.
                    // Zo ja, dan krijgt de pixel deze waarde. Mocht dit meerdere keren gebeuren, dan wordt er een conflict genoteerd.
                    // Zo niet, dan wordt een nieuwe waarde aangemaakt en krijgt de pixel deze waarde.
                    // Mocht er trouwens een pixel gedetecteerd worden met alpha value 0 (doorzichtig), dan wordt die tot de achtergrond gerekend.
                    if (InputImage[x - 1, y - 1] == foreground)
                    {
                        isLabeled = false;
                        drawPoint conflictCheck = new drawPoint(x, y);
                        if (label[x - 1, y - 1] != 0)
                        {
                            label[x, y] = label[x - 1, y - 1];
                            isLabeled = true;
                        }
                        if (label[x, y - 1] != 0)
                        {
                            if (isLabeled == true && label[x, y] != label[x, y - 1] && !conflict.Contains(conflictCheck))
                                conflict.Add(conflictCheck);
                            label[x, y] = label[x, y - 1];
                            isLabeled = true;
                        }
                        if (label[x + 1, y - 1] != 0)
                        {
                            if (isLabeled && label[x, y] != label[x + 1, y - 1] && !conflict.Contains(conflictCheck))
                                conflict.Add(conflictCheck);
                            label[x, y] = label[x + 1, y - 1];
                            isLabeled = true;
                        }
                        if (label[x - 1, y] != 0)
                        {
                            if (isLabeled && label[x, y] != label[x - 1, y] && !conflict.Contains(conflictCheck))
                                conflict.Add(conflictCheck);
                            label[x, y] = label[x - 1, y];
                            isLabeled = true;
                        }
                        if (InputImage[x - 1, y - 1].A == 0)
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
            for (int x = 1; x < InputImage.GetLength(0) + 1; x++)
            {
                for (int y = 1; y < InputImage.GetLength(1) + 1; y++)
                {
                    label[x, y] = newLabel[label[x, y]];
                    newLabelCount[label[x, y]]++;
                    if (InputImage[x - 1, y - 1].A == 0)
                        label[x, y] = 0;
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
                    //Console.WriteLine("Label " + i + ": " + newLabelCount[i]);
                    regionCount++;
                }
                if (newLabelCount[i] > newLabelCount[largestLabel] && i != 0)
                {
                    largestLabel = i;
                    //Console.WriteLine(i + "is now the largest region.");
                }
            }

            // Als er minder of evenveel regions zijn als de vorige afbeelding, wordt de bounding box geupdated om de mogelijke hand van de huidige afbeelding te bevatten.
            // Hier wordt een marge van 10 pixels extra bijgerekend, om corner detection nauwkeuriger te maken en eventuele missende vingers er nog aan te plakken.
            if (regionCount <= currentRegions)
            {
                optimalThreshold = greyscale;
                for (int x = 1; x < InputImage.GetLength(0) + 1; x++)
                {
                    for (int y = 1; y < InputImage.GetLength(1) + 1; y++)
                    {
                        if (label[x, y] == largestLabel)
                        {
                            optimalLabel[x - 1, y - 1] = true;
                            if (x - 1 < leftUpperBbX)
                            {
                                leftUpperBbX = x - 1 - 10;
                                if (leftUpperBbX < 0)
                                    leftUpperBbX = 0;
                            }
                            if (y - 1 < leftUpperBbY)
                            {
                                leftUpperBbY = y - 1 - 10;
                                if (leftUpperBbY < 0)
                                    leftUpperBbY = 0;
                            }
                            if (x - 1 > maxx)
                            {
                                maxx = x - 1 + 10;
                                if (maxx > InputImage.GetLength(0))
                                    maxx = InputImage.GetLength(0);
                            }
                            if (y - 1 > maxy)
                            {
                                maxy = y - 1 + 10;
                                if (maxy > InputImage.GetLength(1))
                                    maxy = InputImage.GetLength(1);
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







        /// <summary>
        /// crops the image to a certain size. The pixel selected lands in the left upper corner
        /// </summary>
        /// <param name="fullImage">input image</param>
        /// <param name="u">x pixel for the left upper corner of new image</param>
        /// <param name="v">y pixel for the left upper corner of new image</param>
        /// <param name="width">new width</param>
        /// <param name="height">new height</param>
        /// <returns></returns>
        Color[,] CropImage(Color[,] fullImage, int u, int v, int width, int height)
        {
            if (width >= fullImage.GetLength(0))
                return fullImage;
            if (height >= fullImage.GetLength(1))
                return fullImage;

            Color[,] subImage = new Color[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    subImage[x, y] = fullImage[u + x, v + y];
                }
            return subImage;
        }

    }
}