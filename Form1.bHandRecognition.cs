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

        drawPoint[] TraceBoundary(Color[,] InputImage, Color backgroundColor)
        {
            List<drawPoint> contourPixels = new List<drawPoint>();

            Stack<drawPoint> firstTracePoint = FindStartingPoint(InputImage, backgroundColor);
            List<drawPoint> contourPoints = TraceContourPixels(backgroundColor, firstTracePoint, contourPixels, InputImage);

            return contourPixels.ToArray();
        }

        Stack<drawPoint> FindStartingPoint(Color[,] InputImage, Color backGrC)
        {
            Stack<drawPoint> firstTracePoint = new Stack<drawPoint>();
            Color previousColor = backGrC;

            for (int v = 0; v < InputImage.GetLength(1); v++)
                for (int u = 0; u < InputImage.GetLength(0); u++) //x moet 'snelst' doorlopen
                {
                    Color currentColor = InputImage[u, v];
                    if (previousColor == backGrC && currentColor != backGrC)
                    {
                        firstTracePoint.Push(new drawPoint(u, v));
                        return firstTracePoint;
                    }
                }
            throw new Exception("no startingPoint was found to trace the boundary");
            return new Stack<drawPoint>();
        }



        /// <summary>
        /// Handles the case that there is a transition from background to foreground in the image, and traces the figure found.
        /// </summary>
        /// <param name="backgrC">background color</param>
        List<drawPoint> TraceContourPixels(Color backgrC, Stack<drawPoint> nextTracePoints, List<drawPoint> contourPixels, Color[,] InputImage)
        {
            //if the first item on the stack is a contourPix,
            //add the pixel to the contourPixelList
            //for all pixels around it, check for each pixel:
            //is it background, and Is it already in the nextTracePoints?
            //If that is not the case, add it to the nextTracePoints.
            drawPoint currentPoint = nextTracePoints.Pop();

            if (isContourPix(backgrC, currentPoint, InputImage))
            {
                contourPixels.Add(currentPoint);
                if ((contourPixels.Count > 10) && SameApprPoint(contourPixels[0], currentPoint))
                return contourPixels; //the startingpoint has been reached.

                //This is N8 chain code, for N4 only consider the 4 pixels straight up, below, left and right
                for (int x = -1; x <= 1; x++) //kijk van rechtsonder naar linksboven, dan loop je minder snel terug.
                    for (int y = -1; y <= 1; y++)
                    {
                        int xvalue = currentPoint.X + x;
                        int yvalue = currentPoint.Y + y;
                        if (!(x == 0 && y == 0)) //it is currentPoint, which was already processed
                        {
                            if (xvalue >= 0 && yvalue >= 0 && xvalue < InputImage.GetLength(0) && yvalue < InputImage.GetLength(1)) //check if pixel is not outside of image
                            {
                                if (InputImage[xvalue, yvalue] != backgrC) //backGr pixels can never be a border
                                {
                                    drawPoint nextTracePoint = new drawPoint(xvalue, yvalue);
                                    if (!nextTracePoints.Contains(nextTracePoint)) //if the nextTracePoint is not already in the Queue
                                    {
                                        if (!contourPixels.Contains(nextTracePoint)) //if the nextTracePoint has not already been processed
                                            nextTracePoints.Push(nextTracePoint);
                                    }
                                }

                            }
                        }

                    }
            }
            
            if (nextTracePoints.Count > 0)
                TraceContourPixels(backgrC, nextTracePoints, contourPixels, InputImage);
            //If all edge pixels are traced, return the complete list of contourpixels 
            return contourPixels;
        }


        Boolean SameApprPoint(drawPoint p1, drawPoint p2, int maximumDistance = 6)
        {
                if (Math.Abs(p1.X - p2.X) < maximumDistance && Math.Abs(p1.Y - p2.Y) <= maximumDistance)
                    return true;
            return false;
        }



        Boolean isContourPix(Color backgrC, drawPoint pixel, Color[,] InputImage)
        {
            for (int x = -1; x <= 1; x++) //check if neighbouringpixels are background
                for (int y = -1; y <= 1; y++)
                {
                    int xvalue = pixel.X + x;
                    int yvalue = pixel.Y + y;
                    if (xvalue >= 0 && yvalue >= 0 && xvalue < InputImage.GetLength(0) && yvalue < InputImage.GetLength(1))
                        if (InputImage[xvalue, yvalue] == backgrC)
                            return true;
                }

            return false;
        }





        void BoundaryToOutput(drawPoint[] drawPoints, Color[,] InputImage)
        {
            Color backgroundColor = getBackgroundColor();
            Color foregroundColor = getForegroundColor();
            OutputImage.Dispose();
            OutputImage = new Bitmap(InputImage.GetLength(0), InputImage.GetLength(1));
            for (int x = 0; x < InputImage.GetLength(0); x++)
                for (int y = 0; y < InputImage.GetLength(1); y++)
                {
                    if (drawPoints.Contains(new drawPoint(x, y)))
                        OutputImage.SetPixel(x, y, foregroundColor);
                    else
                        OutputImage.SetPixel(x, y, backgroundColor);
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
            CornersToImage(goodCorners);
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
            Color backgroundColor = getBackgroundColor();

            for (int u = 0; u < newImage.GetLength(0); u++)
                for (int v = 0; v < newImage.GetLength(1); v++)
                {
                    //fill the entire image with backgroundcolor
                    newImage[u, v] = getBackgroundColor();
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
        drawPoint[] ConvexHull(List<Corner> input)
        {
            try
            {
                if (input.Count < 3)
                    throw new ArgumentException("Convex hull could not be created, at least 3 corners are required", "input");

                // De lijst corners die als input werd gegeven wordt omgezet naar een lijst drawpoints.
                List<drawPoint> points = new List<drawPoint>();
                foreach (var ele in input)
                    points.Add(new drawPoint(ele.U, ele.V));

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
                        if ((vPointOnHull == vEndpoint || (Orientation(vPointOnHull, vEndpoint, points[i]) == -1)))
                            vEndpoint = points[i];
                    }
                    vPointOnHull = vEndpoint;
                }
                while (vEndpoint != hull[0]);

                drawPoint[] hullArray = hull.ToArray();
                return hull.ToArray();
            }
            catch(Exception e)
            {
                MessageBox2.Text = "There are less than 3 corners found. Convex hull cannot be created, are you sure you have at least 3 corners?";
                    return new drawPoint[0]; //it never reaches this.
            }
            //drawPointsToImage(hullArray);
            
           
        }

        void drawPointsToImage(drawPoint[] hull, Color[,] InputImage)
        {
            for (int i = 0; i < InputImage.GetLength(0); i++)
            {
                for (int j = 0; j < InputImage.GetLength(1); j++)
                {
                    newImage[i, j] = Color.FromArgb(255, 255, 255);
                }
            }
            foreach (var ele in hull)
            {
                newImage[ele.X, ele.Y] = Color.FromArgb(255, 0, 0);
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

        // This function draws points in the original image based on a provided list of points, the position of the upperleft corner of the bounding box and a state describing what the object is.
        void crossesInImage(drawPoint[] point, drawPoint min, int state, Color[,] InputImage)
        {
            int R = 0;
            int G = 0;
            int B = 0;
            // We use green pixels for state 1 (pointing hand), yellow pixels for state 2 (spread hand), and red pixels for state 3 (unidentified object).
            if (state == 1 || state == 2)
                G = 255;
            if (state == 2 || state == 3)
                R = 255;

            // We copy the original image and then add pixels for each point in the list.
            for (int i = 0; i < InputImage.GetLength(0); i++)
            {
                for (int j = 0; j < InputImage.GetLength(1); j++)
                {
                    newImage[i, j] = Image[i, j];
                }
            }
            Color stateColor = Color.FromArgb(R, G, B);
            for (int i = 0; i < point.Length; i++) {
                newImage[min.X + point[i].X, min.Y + point[i].Y] = stateColor;
                if (min.X + point[i].X + 1 <= InputImage.GetLength(0) && min.Y + point[i].Y + 1 <= InputImage.GetLength(1))
                    newImage[min.X + point[i].X + 1, min.Y + point[i].Y + 1] = stateColor;
                if (min.X + point[i].X - 1 >= 0 && min.Y + point[i].Y + 1 <= InputImage.GetLength(1))
                    newImage[min.X + point[i].X - 1, min.Y + point[i].Y + 1] = stateColor;
                if (min.X + point[i].X + 1 <= InputImage.GetLength(0) && min.Y + point[i].Y - 1 >= 0)
                    newImage[min.X + point[i].X + 1, min.Y + point[i].Y - 1] = stateColor;
                if (min.X + point[i].X - 1 >= 0 && min.Y + point[i].Y - 1 >= 0)
                    newImage[min.X + point[i].X - 1, min.Y + point[i].Y - 1] = stateColor;
            }
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

        drawPoint[] ToArray(ArraySegment<drawPoint> arraySegment)
        {
            drawPoint[] array = new drawPoint[arraySegment.Count];
            Array.Copy(arraySegment.Array, arraySegment.Offset, array, 0, arraySegment.Count);
            return array;
        }

        drawPoint[] AddConvexDefects(drawPoint[] allCorners, drawPoint[] convexCorners, Color[,] InputImage)
        {
            //zowel tracedBoundary als Convex hull, gaan tegen de klok in.
            List<drawPoint> convexAndDefects = new List<drawPoint>();
            drawPoint[] tracedBoundary = TraceBoundary(InputImage, getBackgroundColor());
            //drawPointsToImage(ToArray(new ArraySegment<drawPoint>(tracedBoundary, 400, 42)));
            for (int i = 0; i < tracedBoundary.Length - 1; i = i + 5)
            {
                Console.WriteLine("index: " + i + "X, Y: " + tracedBoundary[i].ToString());
            }


            drawPoint centroid = FindCentroid(allCorners, InputImage);

            for (int i = 0; i < convexCorners.Length - 1; i++)
            {
                drawPoint defect = WalkBetwConvexPoints(i, i + 1, convexCorners, tracedBoundary, 6, centroid);
                convexAndDefects.Add(convexCorners[i]);
                convexAndDefects.Add(defect);

            }

            convexAndDefects.Add(convexCorners[convexCorners.Length - 1]);
            convexAndDefects.Add(WalkBetwConvexPoints(convexCorners.Length - 1, 0, convexCorners, tracedBoundary, 6, centroid));
            drawPoint[] convexAndDefectsArray  = RemoveRedundantPoints(convexAndDefects);
            drawPointsToImage(convexAndDefectsArray, InputImage);

            return convexAndDefectsArray;

        }
        
        /// <summary>
        /// removes points too close to eachother
        /// </summary>
        /// <param name="points"></param>
        /// <param name="maxDistance"></param>
        /// <returns></returns>
        drawPoint[] RemoveRedundantPoints(List<drawPoint> points, int minDistance = 6)
        {
            List<drawPoint> points2 = new List<drawPoint>();
            for(int i = 0; i < points.Count -1; i++)
            {
                if (SqDistancePoints(points[i], points[i + 1]) > minDistance)
                    points2.Add(points[i]);

            }

            return points2.ToArray();
            
        }

        drawPoint WalkBetwConvexPoints(int convexCornerIndex1, int convexCornerIndex2, drawPoint[] convexCorners, drawPoint[] tracedBoundary, int maxDiff, drawPoint centroid)
        {
            int endIndex = SearchPointInArray(convexCorners[convexCornerIndex1], tracedBoundary, 9);
            int startIndex = SearchPointInArray(convexCorners[convexCornerIndex2], tracedBoundary, 9);
            if (startIndex == -1)
                throw new Exception("startingPoint is not found within the traced boundary");
            if (endIndex == -1)
                throw new Exception("endPoint is not found within the traced boundary");

            Console.WriteLine("start: " + startIndex + " end: " + endIndex);
            drawPoint defect = FindDefect(tracedBoundary, centroid, startIndex, endIndex);

            return defect;
        }

        int SearchPointInArray(drawPoint p, drawPoint[] points, int maxDiff)
        {
            for (int i = 0; i < points.Length; i++)
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
            while (i != endIndex)
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

        drawPoint FindCentroid(drawPoint[] input, Color[,] InputImage)
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
            drawPoint[] point = new drawPoint[1];
            return new drawPoint(totalX, totalY);
        }


        float[] cornerOfConvex(drawPoint[] point)
        {
            float[] angleList = new float[point.Length];
            Console.WriteLine(point.Length);
            for (int i = 0; i < point.Length; i++)
            {
                int previousPoint = i - 1;
                if (i - 1 < 0)
                    previousPoint = point.Length - 1;
                int nextPoint = i + 1;
                if (i + 1 > point.Length - 1)
                    nextPoint = 0;
                Vector vector1 = new Vector(point[i].X - point[previousPoint].X, point[i].Y - point[previousPoint].Y);
                double vector1length = Math.Sqrt((vector1.X * vector1.X) + (vector1.Y * vector1.Y));
                Vector vector2 = new Vector(point[i].X - point[nextPoint].X, point[i].Y - point[nextPoint].Y);
                double vector2length = Math.Sqrt((vector2.X * vector2.X) + (vector2.Y * vector2.Y));
                double dot = (vector1.X * vector2.X) + (vector1.Y * vector2.Y);
                double vec1vec2length = vector1length * vector2length;
                double cosineRule = dot / vec1vec2length;
                float angle = (float)Math.Acos(cosineRule);
                angleList[i] = angle;
            }
            return angleList;
        }

        int determineState(float[] angleList)
        {
            int fingers = 0;
            for (int i = 0; i < angleList.Length; i++)
            {
                if (angleList[i] < 0.2)
                    fingers++;
            }
            if (fingers == 1)
                return 1;
            else if (fingers > 1)
                return 2;
            else
                return 3;
        }





    }
}