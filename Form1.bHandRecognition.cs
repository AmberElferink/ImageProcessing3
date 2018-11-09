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
        //--------------------------------------------------TRACEBOUNDARY---------------------------------

        drawPoint[] TraceBoundary(Color[,] InputImage)
        {
            Color backGrC = Color.FromArgb(255, 0, 0, 0);
            //Color foreGrC = Color.FromArgb(255, 255, 255, 255);

            Color previousColor = backGrC;

            List<drawPoint> returnValue = new List<drawPoint>();

            for (int v = 0; v < InputImage.GetLength(1); v++)
                for (int u = 0; u < InputImage.GetLength(0); u++) //x moet 'snelst' doorlopen
                {
                    Color currentColor = InputImage[u, v];
                    if (previousColor == backGrC && currentColor != backGrC)
                        returnValue = TraceFigureOutline(backGrC, returnValue, u, v, InputImage);
                }

            return returnValue.ToArray();
        }



        /// <summary>
        /// Handles the case that there is a transition from background to foreground in the image, and traces the figure found.
        /// </summary>
        /// <param name="backgrC">background color</param>
        List<drawPoint> TraceFigureOutline(Color backgrC, List<drawPoint> ContourPixels, int u, int v, Color[,] InputImage)
        {

            if (isContourPix(backgrC, u, v, InputImage))
            {
                if (InputImage[u, v] != backgrC)
                {
                    ContourPixels.Add(new drawPoint(u, v));

                    //This is N8 chain code, for N4 only consider the 4 pixels straight up, below, left and right
                    for (int x = -1; x <= 1; x++) //kijk van rechtsonder naar linksboven, dan loop je minder snel terug.
                        for (int y = -1; y <= 1; y++)
                        {
                            if (u + x >= 0 && v + y >= 0 && u + x < InputImage.GetLength(0) && v + y < InputImage.GetLength(1))
                                if (!ContourPixels.Contains(new drawPoint(u + x, v + y)))
                                {
                                    TraceFigureOutline(backgrC, ContourPixels, u + x, v + y, InputImage);
                                }
                        }
                }
            }
            //If all edge pixels are traced, return the complete list of contourpixels 
            return ContourPixels;
        }



        Boolean isContourPix(Color backgrC, int u, int v, Color[,] InputImage)
        {
            for (int x = -1; x <= 1; x++) //check if neighbouringpixels are background
                for (int y = -1; y <= 1; y++)
                {
                    if (u + x >= 0 && v + y >= 0 && u + x < InputImage.GetLength(0) && v + y < InputImage.GetLength(1))
                        if (InputImage[u + x, v + y] == backgrC)
                            return true;
                }

            return false;
        }





        void BoundaryToOutput(drawPoint[] drawPoints, Color[,] InputImage)
        {
            OutputImage.Dispose();
            OutputImage = new Bitmap(InputImage.GetLength(0), InputImage.GetLength(1));
            for (int x = 0; x < InputImage.GetLength(0); x++)
                for (int y = 0; y < InputImage.GetLength(1); y++)
                {
                    if (drawPoints.Contains(new drawPoint(x, y)))
                        OutputImage.SetPixel(x, y, Color.FromArgb(255, 0, 0, 0));
                    else
                        OutputImage.SetPixel(x, y, Color.FromArgb(255, 255, 255, 255));
                }

            outputBox1.Image = (Image)OutputImage;                         // Display output image
            progressBar.Visible = false;
        }


        //-----------------------------------------Corner Detection --------------------------------------------
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

                return Math.Sqrt(distx * distx + disty * disty);
            }

            public int U { get { return this.u; } }
            public int V { get { return this.v; } }

            public float Q { get { return this.q; } }
        }




        List<Corner> HarrisCornerDetection(float[,] Kx, float[,] Ky, Color[,] InputImage)
        {

            float[,] Avalues = new float[InputImage.GetLength(0), InputImage.GetLength(1)];
            float[,] Bvalues = new float[InputImage.GetLength(0), InputImage.GetLength(1)];
            float[,] Cvalues = new float[InputImage.GetLength(0), InputImage.GetLength(1)];

            //float[,] Hx = new float[,] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
            //float[,] Hy = new float[,] { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };

            int halfboxsize = Kx.GetLength(0) / 2;

            for (int v = halfboxsize; v < InputImage.GetLength(1) - halfboxsize; v++)
            {
                for (int u = halfboxsize; u < InputImage.GetLength(0) - halfboxsize; u++)
                {
                    float Ix = CalculateNewColor(u, v, Kx, halfboxsize, InputImage, false) / 8; //apply Kx to the image pixel
                    float Iy = CalculateNewColor(u, v, Ky, halfboxsize, InputImage, false) / 8; //apply Ky to the image pixel


                    //int edgeStrength = (int)Math.Sqrt(Ix * Ix + Iy * Iy); //calculate edgestrength by calculating the length of vector [Hx, Hy]
                    Avalues[u, v] = Ix * Ix;
                    Bvalues[u, v] = Iy * Iy;
                    Cvalues[u, v] = Ix * Iy;

                }
                progressBar.PerformStep();
            }
            Avalues = ApplyGaussianFilter(Avalues, 1.1f, 5, InputImage);
            Bvalues = ApplyGaussianFilter(Bvalues, 1.1f, 5, InputImage);
            Cvalues = ApplyGaussianFilter(Cvalues, 1.1f, 5, InputImage);
            float[,] Qvalues = CalculateQvalues(Avalues, Bvalues, Cvalues, InputImage);
            //float[,] highestQvalues = PickStrongestCorners(Qvalues, 10);
            List<Corner> cornerList = QToCorners(Qvalues, 2000000);
            List<Corner> goodCorners = cleanUpCorners(cornerList, 2.25); //dmin waarde opzoeken, Alg. 4.1 regel 8-16
            List<drawPoint> convexHull = ConvexHull(goodCorners);
            drawPointsToImage(convexHull);
            //CornersToImage(goodCorners);
            toOutputBitmap();
            return goodCorners;
        }

        float CalculateNewColor(int x, int y, float[,] matrix, int halfBoxSize, Color[,] InputImage, bool divideByTotal = true)
        {
            float linearColor = 0;
            float matrixTotal = 0;                // totale waarde van alle weights van de matrix bij elkaar opgeteld
            for (int a = -halfBoxSize; a <= halfBoxSize; a++)
            {
                for (int b = -halfBoxSize; b <= halfBoxSize; b++)
                {
                    //if(x + a < InputImage.GetLength(0) && y + b < InputImage.GetLength(1))
                    linearColor = linearColor + (InputImage[x + a, y + b].R * matrix[a + halfBoxSize, b + halfBoxSize]);
                    // weight van filter wordt per kernel pixel toegepast op image pixel
                    matrixTotal = matrixTotal + matrix[a + halfBoxSize, b + halfBoxSize];
                    // weight wordt opgeteld bij totaalsom van weights
                }
            }
            if (divideByTotal == true) // Voor Edgestrength moet niet door het totaal gedeeld, dus kan hij uitgezet worden.
                linearColor = linearColor / matrixTotal;

            return linearColor;
        }

        //classe corner maken, waardoor je die 3 waardes in een lijst kan zetten en cleanupcorners bouwen.

        List<Corner> cleanUpCorners(List<Corner> cornerList, double dmin)
        {
            //corners have to be sorted by descending Q.
            double dmin2 = dmin * dmin;
            Corner[] cornerArray = cornerList.ToArray();
            List<Corner> goodCorners = new List<Corner>();

            for (int i = 0; i < cornerArray.Length; i++)
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

            for (int u = 0; u < newImage.GetLength(0); u++)
                for (int v = 0; v < newImage.GetLength(1); v++)
                {
                    //fill the entire image with backgroundcolor
                    newImage[u, v] = Color.FromArgb(backgroundColor, backgroundColor, backgroundColor);
                }
            foreach (Corner c in corners)
            {
                int Q = (int)c.Q;
                Console.WriteLine(Q);
                newImage[c.U, c.V] = Color.FromArgb(clamp(Q), 0, 0);
            }
        }

        float[,] CalculateQvalues(float[,] Avalues, float[,] Bvalues, float[,] Cvalues, Color[,] InputImage)
        {
            float[,] Qvalues = new float[InputImage.GetLength(0), InputImage.GetLength(1)];

            for (int x = 0; x < Avalues.GetLength(0); x++)
            {
                for (int y = 0; y < Avalues.GetLength(1); y++)
                {
                    float A = Avalues[x, y];
                    float B = Bvalues[x, y];
                    float C = Cvalues[x, y];

                    float traceM = A + B;
                    float squareRoot = (float)Math.Sqrt(A * A - 2 * A * B + B * B + 4 * C * C);
                    float lambda1 = (float)(0.5 * (traceM + squareRoot));
                    float lambda2 = (float)(0.5 * (traceM - squareRoot));

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

        private float[,] ApplyGaussianFilter(float[,] valueArray, float sigma, int boxsize, Color[,] InputImage)
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
            for (int x = filterBorder; x < InputImage.GetLength(0) - filterBorder; x++)
            {
                for (int y = filterBorder; y < InputImage.GetLength(1) - filterBorder; y++)
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
            for (int x = 0; x < Qvalues.GetLength(0); x++)
                for (int y = 0; y < Qvalues.GetLength(1); y++)
                {

                    if (Qvalues[x, y] > threshold && IsLocalMax(Qvalues, x, y))
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
                    if (checkX > 0 && checkY > 0 && checkX < Qvalues.GetLength(0) && checkY < Qvalues.GetLength(1))
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
                        Kx[i, j] = Ky[j, i] = j - k;
                    else if (j > halfSide)
                        Kx[i, j] = Ky[j, i] = k - (side - j - 1);
                    else
                        Kx[i, j] = Ky[j, i] = 0;
                }
            }
        }


        //-------------------------------------Convex hull-------------------------------------------

        // This function produces a convex hull from a list corners of corners using the gift wrapping algorithm.
        // Original code copied from: https://stackoverflow.com/questions/10020949/gift-wrapping-algorithm
        public static List<drawPoint> ConvexHull(List<Corner> input)
        {
            if (input.Count < 3)
            {
                throw new ArgumentException("Convex hull could not be created, at least 3 corners are required", "input");
            }

            // De lijst corners die als input werd gegeven wordt omgezet naar een lijst drawpoints.
            List<drawPoint> points = new List<drawPoint>();
            foreach (var ele in input)
            {
                points.Add(new drawPoint(ele.U, ele.V));
            }

            List<drawPoint> hull = new List<drawPoint>();

            // Het eerste punt bevindt zich in de linkerbovenhoek van de afbeelding, dit is gegarandeerd in de convex hull te zitten.
            drawPoint vPointOnHull = points.Where(p => p.X == points.Min(min => min.X)).First();

            // Daarna wordt er per punt een lijn naar elk ander punt getrokken en wordt er gekeken of er nog andere punten aan die kant van de lijn liggen met behulp van de orientation functie.
            drawPoint vEndpoint;
            do
            {
                hull.Add(vPointOnHull);
                vEndpoint = points[0];

                for (int i = 1; i < points.Count; i++)
                {
                    if ((vPointOnHull == vEndpoint)
                        || (Orientation(vPointOnHull, vEndpoint, points[i]) == -1))
                    {
                        vEndpoint = points[i];
                    }
                }

                vPointOnHull = vEndpoint;

            }
            while (vEndpoint != hull[0]);

            return hull;
        }

        void drawPointsToImage(List<drawPoint> hull)
        {
            for (int i = 0; i < InputImage.Size.Width; i++)
            {
                for (int j = 0; j < InputImage.Size.Height; j++)
                {
                    newImage[i, j] = Color.FromArgb(255, 255, 255);
                }
            }
            foreach (var ele in hull)
            {
                newImage[ele.X, ele.Y] = Color.FromArgb(255, 0, 0);
                Console.WriteLine("(" + ele.X + ", " + ele.Y + ")");
            }
        }


        private static int Orientation(drawPoint p1, drawPoint p2, drawPoint p)
        {
            // Determinant
            int Orin = (p2.X - p1.X) * (p.Y - p1.Y) - (p.X - p1.X) * (p2.Y - p1.Y);

            if (Orin > 0)
                return -1; //          (* Orientation is to the left-hand side  *)
            if (Orin < 0)
                return 1; // (* Orientation is to the right-hand side *)

            return 0; //  (* Orientation is neutral aka collinear  *)
        }


        //-------------------------------------Convex hull defects-----------------------------------
        //finds the inner 'hull' corners of the hand.
        drawPoint[] convexHullStarCorners = {
            new drawPoint(8, 61), //NW
            new drawPoint(117, 11), //N
            new drawPoint(225, 61), //NO
            new drawPoint(225, 161), //ZO
            new drawPoint(117, 212), //Z
            new drawPoint(8, 161) //ZW
        };

        drawPoint[] ConvexDefects(drawPoint[] allCorners, drawPoint[] convexCorners, Color[,] InputImage)
        {
            //zowel tracedBoundary als Convex hull, gaan tegen de klok in.
            List<drawPoint> defects = new List<drawPoint>();
            drawPoint[] tracedBoundary = TraceBoundary(InputImage);

            for (int i = 0; i < convexCorners.Length - 1; i++)
            {
                int startIndex = SearchPointInArray(convexCorners[i], tracedBoundary, 6);
                int endIndex = SearchPointInArray(convexCorners[i + 1], tracedBoundary, 6);
                if (startIndex == -1)
                    throw new Exception("startingPoint is not found within the traced boundary");
                if (endIndex == -1)
                    throw new Exception("endPoint is not found within the traced boundary");

                if(endIndex < startIndex)
                {
                    int x = startIndex;
                    startIndex = endIndex;
                    endIndex = x;
                }

                drawPoint centroid = FindCentroid(allCorners);
                drawPoint defect = FindDefect(tracedBoundary, centroid, startIndex, endIndex);
                defects.Add(defect);
            }
            drawPointsToImage(defects);
            return defects.ToArray();

        }


        int SearchPointInArray(drawPoint p, drawPoint[] points, int maxDiff)
        {
            for(int i = 0; i < points.Length; i++)
            {
                if (Math.Abs(points[i].X - p.X) < maxDiff && (Math.Abs(points[i].Y - p.Y) < maxDiff))
                    return i;

            }
            return -1; //not found
        }



        drawPoint FindDefect(drawPoint[] tracedBoundary, drawPoint centroid, int startIndex, int endIndex)
        {
            int minimumDistCentroid = int.MaxValue;
            drawPoint defectPoint = new drawPoint(0, 0); //initial value, will be overwritten when a new minimum is found.

            int i = startIndex;
            //walk from the first convex hull corner to the second, or second to the third, etc
            //for each point, calculate the distance to the centroid. Update the minimum distance.
            //the final minimum distance gives the point that is the defect point (closest to the center of the hand on that line)
            while (i <= endIndex)
            {
                int distanceCentroid = SqDistancePoints(tracedBoundary[i], centroid);
                if (distanceCentroid < minimumDistCentroid)
                {
                    minimumDistCentroid = distanceCentroid;
                    defectPoint = tracedBoundary[i];
                }


                if (i + 1 > tracedBoundary.Length - 1)
                    i = 0;
                else
                    i++;
            }
            return defectPoint;
        }

        int SqDistancePoints(drawPoint p1, drawPoint p2)
        {
            int diffX = p1.X - p2.X;
            int diffY = p1.Y - p2.Y;
            return (diffX * diffX + diffY * diffY);
        }

        drawPoint FindCentroid(drawPoint[] input)
        {
            int totalX = 0;
            int totalY = 0;
            foreach (var ele in input)
            {
                totalX += ele.X;
                totalY += ele.Y;
            }
            totalX = totalX / input.Length;
            totalY = totalY / input.Length;
            return new drawPoint(totalX, totalY);
        }





       


    }
}