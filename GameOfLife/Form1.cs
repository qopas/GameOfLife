﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Automata;
using System.Media;

namespace GameOfLife
{
    public partial class Form1 : Form
    {

        static int speed = 50; // time in ms between cycles

        public static int WidthX = 335; // X dimension of cell grid
        public static int WidthY = 185; // Y dimension of cell grid
        //const int WidthX = 325; // X dimension of cell grid
        //const int WidthY = 165; // Y dimension of cell grid

        public static int oldWidthX = WidthX; // X dimension of cell grid
        public static int oldWidthY = WidthY; // Y dimension of cell grid

        Automata.GOLalgorithm algorithm = new Automata.GOLalgorithm();
        Automata.Rule30 rule30 = new Automata.Rule30();

        Bitmap crosshairCursorBitmap = (Bitmap)(new Bitmap(Properties.Resources.crosshair));
        Cursor redCrosshairCursor;

        /*       
        const int healthCondition1 = 2; // two adjacent dots required to survive
        const int healthCondition2 = 3; // three adjacent dots required to grow
        */




        const int Xoffset = 160; //X offset from upper left corner of window
        const int Yoffset = 40;

        public static int gridSize = 6; // distance between grids
        public static int cellSize = 4; //radius of dots

        public static bool gridChanged = false;
        public static bool T1wasRunning = false;

        //fullscreen stuff
        bool showFullScreen = false;
        Point oldLocation;
        bool hideControls = false;

        //brush 0 selected
        uint drawMode = 0;

        Bitmap bmp = new Bitmap((WidthX * gridSize) + (gridSize), (WidthY * gridSize) + (gridSize), System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
        Bitmap thumbnail = new Bitmap(50, 50, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

        static string patternCustomFileName = "pictures\\GOL1.bmp";

        static Tuple<int, int> mousePos = new Tuple<int, int>(0, 0);
        static Tuple<int, int> oldMousePos = new Tuple<int, int>(0, 0);
        static bool isMouseOverPic = false;


        Stopwatch frameStopwatch = new Stopwatch();
        double frameCounter = 0;
        double drawAvg = 0;
        double calcAvg = 0;


        SolidBrush dotcolor = new SolidBrush(Color.LavenderBlush);
        SolidBrush backcolor = new SolidBrush(Color.SeaGreen);
        //SolidBrush shadowcolor = new SolidBrush(Color.LightSalmon);


        static bool[,] board = new bool[WidthX, WidthY];
        static bool[,] oldboard = new bool[WidthX, WidthY];
        static bool[][,] rgbboardhistory = new bool[7][,]; //[ new bool[WidthX,WidthY]; // = new bool[,]>();


        static Color[] ShadowColors = new Color[] { Color.FromArgb(237,213,186),
                                                    Color.FromArgb(228,200,156),
                                                    Color.FromArgb(219,186,127),
                                                    Color.FromArgb(210,173,98),
                                                    Color.FromArgb(201,160,68),
                                                    Color.FromArgb(192,146,39),
                                                    Color.SeaGreen

        };



        static Random rand = new Random();


        public static Timer timer1 = new Timer();

        public static Timer timer2 = new Timer();

        static Timer timer3 = new Timer();

        Bitmap image1;



        public struct IconInfo
        {
            public bool fIcon;
            public int xHotspot;
            public int yHotspot;
            public IntPtr hbmMask;
            public IntPtr hbmColor;
        }


        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetIconInfo(IntPtr hIcon, ref IconInfo pIconInfo);
        [DllImport("user32.dll")]
        public static extern IntPtr CreateIconIndirect(ref IconInfo icon);

        /// <summary>
        /// Create a cursor from a bitmap without resizing and with the specified
        /// hot spot
        /// </summary>
        public static Cursor CreateCursorNoResize(Bitmap bmp, int xHotSpot, int yHotSpot)
        {
            IntPtr ptr = bmp.GetHicon();
            IconInfo tmp = new IconInfo();
            GetIconInfo(ptr, ref tmp);
            tmp.xHotspot = xHotSpot;
            tmp.yHotspot = yHotSpot;
            tmp.fIcon = false;
            ptr = CreateIconIndirect(ref tmp);
            return new Cursor(ptr);
        }


        private SoundPlayer soundPlayer;


        public Form1()
        {
            InitializeComponent();

            resizePicBox();

            this.KeyPreview = true;


            this.Icon = Properties.Resources.CGOL;

            crosshairCursorBitmap.MakeTransparent(Color.DarkGoldenrod);
            redCrosshairCursor = CreateCursorNoResize(crosshairCursorBitmap, 15, 15);

            using (var g = Graphics.FromImage(bmp))
            {

                Rectangle rect = new Rectangle(0, 0, (WidthX * gridSize) + 2 * gridSize + (gridSize / 2), (WidthY * gridSize) + 2 * gridSize + (gridSize / 2));

                // Define the coordinates for dividing the rectangle into quadrants
                int midX = rect.Width / 2;

                // Fill the top-left quadrant with a color
                Rectangle topLeft = new Rectangle(rect.Left, rect.Top, midX, rect.Height);
                using (SolidBrush brush1 = new SolidBrush(Color.Red)) // Example color
                {
                    g.FillRectangle(brush1, topLeft);
                }

                // Fill the top-right quadrant with a color
                Rectangle topRight = new Rectangle(midX, rect.Top, midX, rect.Height);
                using (SolidBrush brush2 = new SolidBrush(Color.Blue)) // Example color
                {
                    g.FillRectangle(brush2, topRight);
                }
                this.pictureBox1.Image = bmp;
            }



            for (int n = 0; n < 7; n++)
                rgbboardhistory[n] = new bool[WidthX, WidthY];



            for (int i = 0; i < WidthX; i++)
                for (int j = 0; j < WidthY; j++)
                {

                    board[i, j] = false;
                }

            for (int n = 0; n < rgbboardhistory.Length; n++)
                for (int i = 0; i < WidthX; i++)
                {
                    for (int j = 0; j < WidthY; j++)
                    {
                        rgbboardhistory[n][i, j] = false;
                    }
                }

            drawBoard();

            timer1.Interval = speed;
            timer1.Tick += Timer1_Tick;

            timer2.Interval = 333;
            timer2.Tick += Timer2_Tick;

            timer3.Interval = 30;
            timer3.Tick += Timer3_Tick;

            soundPlayer = new SoundPlayer("C:\\Users\\user\\source\\repos\\qopas\\GameOfLife\\BackSong.wav");

        }

        private void resizePicBox()
        {
            //this.Size = new Size(WidthX * gridSize - 220, WidthY * gridSize - 120);

            pictureBox1.Width = ((WidthX * gridSize) + (gridSize / 2 + (gridSize % 2 > 0 ? 1 : 0))); // x/y + (x % y > 0 ? 1 : 0)
            pictureBox1.Height = ((WidthY * gridSize) + (gridSize / 2 + (gridSize % 2 > 0 ? 1 : 0)));
            pictureBox1.Refresh();


        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (drawMode >= 1)
            {
                for (int n = 6; n > 0; n--)
                {
                    rgbboardhistory[n] = rgbboardhistory[n - 1];
                }
                rgbboardhistory[0] = board;
            }

            oldboard = board;

            frameStopwatch.Start();
            //board = calculateNextBoard();
            //board = algorithm.calculateNextBoard(board);
            if (drawMode == 2)
                board = rule30.calculateNextBoard(board);
            else if (drawMode == 3)
                board = rule30.calculateNextBoard(board);
            else if (drawMode == 1)
                board = algorithm.calculateNextBoard(board);
            else
                board = algorithm.calculateNextBoard(board);
            frameStopwatch.Stop();
            calcAvg += (double)frameStopwatch.Elapsed.TotalMilliseconds;
            frameStopwatch.Reset();

            frameStopwatch.Start();
            if (drawMode == 0)
                drawChangedCells(oldboard, board);
            //drawBoard();
            else if (drawMode >= 1)
                drawChangedCellsShadowed(oldboard, board);
            frameStopwatch.Stop();
            drawAvg += (double)frameStopwatch.Elapsed.TotalMilliseconds;
            frameStopwatch.Reset();

            frameCounter++;

        }

        private void Timer2_Tick(object sender, EventArgs e)
        {
            double drawavg = drawAvg / frameCounter;
            double calcavg = calcAvg / frameCounter;
            label5.Text = calcavg.ToString("0.000") + " ms";
            label6.Text = drawavg.ToString("0.000") + " ms";
            label4.Text = ((drawAvg + calcAvg) / frameCounter).ToString("0.000") + " ms";
            frameCounter = 0;
            drawAvg = 0;
            calcAvg = 0;

        }

        private void Timer3_Tick(object sender, EventArgs e)
        {
            drawBoard();
            using (var g = Graphics.FromImage(bmp))
            {
                delPreviewImage(g, oldMousePos.Item1, oldMousePos.Item2);


                oldMousePos = mousePos;

                if (isMouseOverPic == true)
                {
                    previewImage(g, oldMousePos.Item1, oldMousePos.Item2);

                }
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (timer1.Enabled)
            {
                button1.Text = "Start / Stop";
                timer1.Stop();
                timer2.Stop();
            }
            else
            {
                button1.Text = "Stop";
                timer1.Start();
                timer2.Start();
            }

        }

        void createRandomBoard()
        {
            Random rand = new Random();
            for (int i = 0; i < WidthX - 1; i++)
                for (int j = 0; j < WidthY - 1; j++)
                {
                    bool rnd = rand.NextDouble() > 0.6;
                    board[i, j] = rnd;
                }

        }

        void drawBoard()
        {
            //Graphics myGraphics = base.CreateGraphics();
            // Pen myPen = new Pen(Color.Red);
            // Pen blankPen = new Pen(BackColor);
            //SolidBrush mySolidBrush = dotcolor;
            //SolidBrush myBlankBrush = backcolor;
            //myGraphics.DrawEllipse(myPen, 50, 50, 150, 150);


            using (var g = Graphics.FromImage(bmp))
            {
                for (int i = 0; i < WidthX; i++)
                    for (int j = 0; j < WidthY; j++)
                    {
                        if (board[i, j] == true)
                            GraphicsExtensions.FillRectangle(g, dotcolor, i * gridSize + cellSize, j * gridSize + cellSize, cellSize);
                        else
                            GraphicsExtensions.FillRectangle(g, backcolor, i * gridSize + cellSize, j * gridSize + cellSize, cellSize);
                  
                    }
                this.pictureBox1.Image = bmp;
            }

        }



        void drawChangedCells(bool[,] oldboard, bool[,] Tempboard)
        {

            using (var g = Graphics.FromImage(bmp))
            {
                delPreviewImage(g, oldMousePos.Item1, oldMousePos.Item2);


                for (int i = 0; i < WidthX; i++)
                {
                    for (int j = 0; j < WidthY; j++)
                    {
                        if ((oldboard[i, j] == false) && (Tempboard[i, j] == true))
                            GraphicsExtensions.FillRectangle(g, dotcolor, i * gridSize + cellSize, j * gridSize + cellSize, cellSize);
                        else if ((oldboard[i, j] == true) && (Tempboard[i, j] == false))
                            GraphicsExtensions.FillRectangle(g, backcolor, i * gridSize + cellSize, j * gridSize + cellSize, cellSize);
                    }
                }

                oldMousePos = mousePos;

                if (isMouseOverPic == true)
                {
                    previewImage(g, oldMousePos.Item1, oldMousePos.Item2);

                }

                this.pictureBox1.Image = bmp;
            }


        }


        void drawChangedCellsShadowed(bool[,] oldboard, bool[,] Tempboard)
        {

            using (var g = Graphics.FromImage(bmp))
            {

                delPreviewImage(g, oldMousePos.Item1, oldMousePos.Item2);

                for (int i = 0; i < WidthX; i++)
                {
                    for (int j = 0; j < WidthY; j++)
                    {


                        if ((oldboard[i, j] == false) && (Tempboard[i, j] == true))
                        {
                            GraphicsExtensions.FillRectangle(g, dotcolor, i * gridSize + cellSize, j * gridSize + cellSize, cellSize);

                            if (drawMode == 2)
                            {
                                for (int n = 4; n > 0; n--)
                                {
                                    if ((rgbboardhistory[n][i, j] == true) && (rgbboardhistory[n + 1][i, j] == false))
                                    {
                                        /* if (drawMode == 1)
                                             GraphicsExtensions.FillRectangle(g, new SolidBrush(ShadowColors[n + 2]), i * gridSize + cellSize, j * gridSize + cellSize, cellSize);
                                         else if (drawMode == 2)
                                             GraphicsExtensions.FillRectangle(g, new SolidBrush(ShadowColors[n + 1]), i * gridSize + cellSize, j * gridSize + cellSize, cellSize);
                                         else if (drawMode == 3)*/
                                        GraphicsExtensions.FillRectangle(g, new SolidBrush(ShadowColors[n]), i * gridSize + cellSize, j * gridSize + cellSize, cellSize);
                                    }
                                }
                            }


                        }

                        else if ((oldboard[i, j] == true) && (Tempboard[i, j] == false))
                        {
                            GraphicsExtensions.FillRectangle(g, backcolor, i * gridSize + cellSize, j * gridSize + cellSize, cellSize);


                            if (drawMode == 3)
                            {
                                for (int n = 0; n < 5; n++)
                                {
                                    if ((rgbboardhistory[n][i, j] == true) && (rgbboardhistory[n + 1][i, j] == false))
                                    {
                                        /*if (drawMode == 1)*/
                                        GraphicsExtensions.FillRectangle(g, new SolidBrush(ShadowColors[n + 2]), i * gridSize + cellSize, j * gridSize + cellSize, cellSize);
                                        /*else if (drawMode == 2)
                                            GraphicsExtensions.FillRectangle(g, new SolidBrush(ShadowColors[n + 1]), i * gridSize + cellSize, j * gridSize + cellSize, cellSize);
                                        else if (drawMode == 3)
                                            GraphicsExtensions.FillRectangle(g, new SolidBrush(ShadowColors[n]), i * gridSize + cellSize, j * gridSize + cellSize, cellSize);*/
                                    }
                                }
                            }
                        }

                        if (drawMode == 1)
                        {
                            for (int n = 0; n < 5; n++)
                            {
                                if ((rgbboardhistory[n][i, j] == true) && (rgbboardhistory[n + 1][i, j] == false))
                                {
                                    //if (drawMode == 1)
                                    GraphicsExtensions.FillRectangle(g, new SolidBrush(ShadowColors[n + 2]), i * gridSize + cellSize, j * gridSize + cellSize, cellSize);
                                    /*else if (drawMode == 2)
                                        GraphicsExtensions.FillRectangle(g, new SolidBrush(ShadowColors[n + 1]), i * gridSize + cellSize, j * gridSize + cellSize, cellSize);
                                    else if (drawMode == 3)
                                        GraphicsExtensions.FillRectangle(g, new SolidBrush(ShadowColors[n]), i * gridSize + cellSize, j * gridSize + cellSize, cellSize);*/
                                }
                            }
                        }
                        else if (drawMode == 2)
                        {
                            for (int n = 0; n < 5; n++)
                            {
                                if ((rgbboardhistory[n][i, j] == true) && (rgbboardhistory[n + 1][i, j] == false))
                                {
                                    //if (drawMode == 1)
                                    //GraphicsExtensions.FillRectangle(g, new SolidBrush(ShadowColors[n + 2]), i * gridSize + cellSize, j * gridSize + cellSize, cellSize);
                                    ///else if (drawMode == 2)
                                    GraphicsExtensions.FillRectangle(g, new SolidBrush(ShadowColors[n + 1]), i * gridSize + cellSize, j * gridSize + cellSize, cellSize);
                                    //else if (drawMode == 3)
                                    //GraphicsExtensions.FillRectangle(g, new SolidBrush(ShadowColors[n]), i * gridSize + cellSize, j * gridSize + cellSize, cellSize);*/
                                }
                            }
                        }

                        //else if ((oldboard[i,j] == false))// && (oldboard[i,j] == false))
                        //GraphicsExtensions.FillRectangle(g, backcolor, i * gridSize + cellSize, j * gridSize + cellSize, cellSize);





                    }
                }

                oldMousePos = mousePos;

                if (isMouseOverPic == true)
                {
                    previewImage(g, oldMousePos.Item1, oldMousePos.Item2);

                }

                this.pictureBox1.Image = bmp;

            }


        }



        /*
        static bool[,] calculateNextBoard()
        {
            bool[,] Tempboard = new bool[WidthX, WidthY];

            //for (int i = 0; i < WidthX; i++)
             
            Parallel.For(0, WidthX, i =>
            {
                for (int j = 0; j < WidthY; j++)
                {
                    int willLive = 0;
                    if (j > 0) //upper row
                    {
                        if (i > 0) //left of
                            willLive += ToInt(board[i - 1, j - 1]);
                        willLive += ToInt(board[i, j - 1]);
                        if (i < WidthX - 1)
                            willLive += ToInt(board[i + 1, j - 1]);
                    }

                    if (i > 0) //same row
                        willLive += ToInt(board[i - 1, j]); //left of
                    if (i < WidthX - 1)
                        willLive += ToInt(board[i + 1, j]);

                    if (j < WidthY - 1)//lower row
                    {
                        if (i > 0) //left of
                            willLive += ToInt(board[i - 1, j + 1]);
                        willLive += ToInt(board[i, j + 1]);
                        if (i < WidthX - 1)
                            willLive += ToInt(board[i + 1, j + 1]);
                    }


                    if (board[i, j] == true)
                    {
                        if (willLive >= healthCondition1 && willLive <= healthCondition2)
                            Tempboard[i, j] = true;
                        else
                            Tempboard[i, j] = false;
                    }
                    else // if original cell was empty
                    {
                        if (willLive == healthCondition2)
                            Tempboard[i, j] = true;
                        else
                            Tempboard[i, j] = false;
                    }




                }
            });
            
            return Tempboard;
        }
        */



        private void loadImagefromBMP(string BMPname)
        {
            image1 = new Bitmap(BMPname, true);
            label1.Text = Path.GetFileName(BMPname) + Environment.NewLine + image1.PixelFormat.ToString().Substring(6) + Environment.NewLine;
            label1.Text += image1.Height + " x " + image1.Width;

            int x, y;

            // Loop through the images pixels to reset color.
            for (x = 0; x < image1.Width; x++)
            {
                for (y = 0; y < image1.Height; y++)
                {
                    int pixelColor = image1.GetPixel(x, y).ToArgb();
                    int empty = Color.Empty.ToArgb();
                    //Debug.WriteLine("Pixel: " + x + ":" + y + " - pixelcolor: " + pixelColor + " - empty: " + empty);
                    if (pixelColor < -65794)
                        board[x, y] = true;
                    else
                        board[x, y] = false;

                }
            }
        }

        private void loadImagetoPos(string BMPname, int Xoffset, int Yoffset)
        {

            if (radioButton5.Checked)
            {
                image1 = new Bitmap(Properties.Resources._1pxblack);
            }
            else
                image1 = new Bitmap(BMPname, true);

            label1.Text = Path.GetFileName(BMPname) + Environment.NewLine
                        + image1.PixelFormat.ToString().Substring(6) + Environment.NewLine
                        + image1.Height + " x " + image1.Width;

            Debug.WriteLine("(Xoffset / gridSize): {0}", (Xoffset / gridSize));
            Debug.WriteLine("(Yoffset / gridSize): {0}", (Yoffset / gridSize));
            Debug.WriteLine("image1 Width: {0}", image1.Width);
            Debug.WriteLine("image1 Height: {0}", image1.Height);
            Debug.WriteLine("WidthX: {0}", WidthX);
            Debug.WriteLine("WidthY: {0}", WidthY);

            if (
                //((Xoffset / gridSize) > (image1.Width)) && 
                //((Yoffset / gridSize) > (image1.Height)) && 
                (((Xoffset / gridSize) + (image1.Width - 1)) < (WidthX)) &&
                (((Yoffset / gridSize) + (image1.Height - 1)) < (WidthY))

                //((Xoffset / gridSize) < (WidthX - (image1.Width / 2) - 1)) && 
                //((Yoffset / gridSize) < (WidthY - (image1.Height / 2) - 1))
                )
            {

                Debug.WriteLine("BMP fits into pos");


                int x, y;

                // Loop through the images pixels
                for (y = 0; y < image1.Height; y++)
                {
                    for (x = 0; x < image1.Width; x++)
                    {
                        int pixelColor = image1.GetPixel(x, y).ToArgb();
                        //int empty = Color.Empty.ToArgb();
                        int posX = (Xoffset / gridSize) + x;
                        int posY = (Yoffset / gridSize) + y;
                        //Debug.WriteLine("PosX: {0}   PosY: {1}", posX, posY);
                        //Debug.WriteLine("Pixel: " + x + ":" + y + " - pixelcolor: " + pixelColor + " - empty: " + empty);
                        if (pixelColor < -65794)
                            board[posX, posY] = true;
                        else
                            board[posX, posY] = false;

                    }
                }

            }
            else
            {
                Debug.WriteLine("doesn't fit: ");


            }
        }


        private void previewImage(Graphics g, int Xoffset, int Yoffset)
        {

            string BMPname = "";

            if (radioButton12.Checked)
            {
                BMPname = @patternCustomFileName;
                image1 = new Bitmap(BMPname, true);
            }
            else if (radioButton5.Checked)
            {
                BMPname = "1pxblack.bmp";
                image1 = (Bitmap)(new Bitmap(Properties.Resources._1pxblack));
            }
            if (BMPname == "")
                return;





            if (
                (((Xoffset / gridSize) + (image1.Width - 1)) < (WidthX)) &&
                (((Yoffset / gridSize) + (image1.Height - 1)) < (WidthY))
                )
            {

                //Debug.WriteLine("BMP fits into pos");




                int x, y;

                int _xoffset = Xoffset - (Xoffset % gridSize);
                int _yoffset = Yoffset - (Yoffset % gridSize);

                // draw frame around pixels

                //SolidBrush FrameColor = new SolidBrush(Color.LightGoldenrodYellow);
                SolidBrush FrameColor = new SolidBrush(Color.Yellow);

                if (radioButton12.Checked)
                {
                    for (x = image1.Width - 1; x > 0; x--)
                    {
                        GraphicsExtensions.FillRectangle(g, FrameColor, _xoffset + x * gridSize + cellSize, _yoffset + cellSize, cellSize);
                        GraphicsExtensions.FillRectangle(g, FrameColor, _xoffset + x * gridSize + cellSize, _yoffset + (image1.Height - 1) * gridSize + cellSize, cellSize);
                    }
                    for (y = image1.Height - 1; y >= 0; y--)
                    {
                        GraphicsExtensions.FillRectangle(g, FrameColor, _xoffset + cellSize, _yoffset + y * gridSize + cellSize, cellSize);
                        GraphicsExtensions.FillRectangle(g, FrameColor, _xoffset + (image1.Width - 1) * gridSize + cellSize, _yoffset + y * gridSize + cellSize, cellSize);
                    }
                }

                // Loop through the images pixels

                for (y = 0; y < image1.Height; y++)
                {
                    for (x = 0; x < image1.Width; x++)
                    {
                        int pixelColor = image1.GetPixel(x, y).ToArgb();
                        //int empty = Color.Empty.ToArgb();

                        //int posX = (Xoffset / gridSize) + x;
                        //int posY = (Yoffset / gridSize) + y;

                        //Debug.WriteLine("PosX: {0}   PosY: {1}", posX, posY);
                        //Debug.WriteLine("Pixel: " + x + ":" + y + " - pixelcolor: " + pixelColor + " - empty: " + empty);
                        if (pixelColor < -65794)

                            GraphicsExtensions.FillRectangle(g, new SolidBrush(Color.LightSeaGreen), _xoffset + x * gridSize + cellSize, _yoffset + y * gridSize + cellSize, cellSize);

                        //else
                        //GraphicsExtensions.FillRectangle(g, backcolor, Xoffset + x * gridSize + cellSize, Yoffset + y * gridSize + cellSize, cellSize);

                    }
                }


                this.pictureBox1.Image = bmp;

            }
            else
            {
                //Debug.WriteLine("doesn't fit: ");


            }
        }



        private void delPreviewImage(Graphics g, int Xoffset, int Yoffset)
        {
            string BMPname = "";

            if (radioButton12.Checked)
            {
                BMPname = @patternCustomFileName;
                image1 = new Bitmap(BMPname, true);
            }
            else if (radioButton5.Checked)
            {
                BMPname = "1pxblack.bmp";
                image1 = (Bitmap)(new Bitmap(Properties.Resources._1pxblack));
            }

            if (BMPname == "")
                return;





            if (
                (((Xoffset / gridSize) + (image1.Width - 1)) >= 0) &&
                (((Yoffset / gridSize) + (image1.Height - 1)) >= 0) &&
                (((Xoffset / gridSize) + (image1.Width - 1)) < (WidthX)) &&
                (((Yoffset / gridSize) + (image1.Height - 1)) < (WidthY))
                )
            {


                int x, y;

                int _xoffset = Xoffset - (Xoffset % gridSize);
                int _yoffset = Yoffset - (Yoffset % gridSize);



                bool needPixelRestore = false;

                if (radioButton12.Checked)
                {




                    for (x = image1.Width - 1; x > 0; x--)
                    {
                        int posX = (_xoffset / gridSize) + x;
                        if (board[posX, (_yoffset / gridSize)] == true)
                        {
                            GraphicsExtensions.FillRectangle(g, dotcolor, _xoffset + x * gridSize + cellSize, _yoffset + cellSize, cellSize);

                            needPixelRestore = true;
                        }
                        else
                        {
                            GraphicsExtensions.FillRectangle(g, backcolor, _xoffset + x * gridSize + cellSize, _yoffset + cellSize, cellSize);
                        }

                        if (board[posX, ((_yoffset / gridSize) + image1.Height - 1)] == true)
                        {

                            GraphicsExtensions.FillRectangle(g, dotcolor, _xoffset + x * gridSize + cellSize, _yoffset + (image1.Height - 1) * gridSize + cellSize, cellSize);
                            needPixelRestore = true;
                        }
                        else
                        {
                            GraphicsExtensions.FillRectangle(g, backcolor, _xoffset + x * gridSize + cellSize, _yoffset + (image1.Height - 1) * gridSize + cellSize, cellSize);
                        }
                    }
                    for (y = image1.Height - 1; y >= 0; y--)
                    {
                        int posY = (_yoffset / gridSize) + y;
                        if (board[(_xoffset / gridSize), posY] == true)
                        {
                            GraphicsExtensions.FillRectangle(g, dotcolor, _xoffset + cellSize, _yoffset + y * gridSize + cellSize, cellSize);

                            needPixelRestore = true;
                        }
                        else
                        {
                            GraphicsExtensions.FillRectangle(g, backcolor, _xoffset + cellSize, _yoffset + y * gridSize + cellSize, cellSize);

                        }
                        if (board[((_xoffset / gridSize) + image1.Width - 1), posY] == true)
                        {
                            GraphicsExtensions.FillRectangle(g, dotcolor, _xoffset + (image1.Width - 1) * gridSize + cellSize, _yoffset + y * gridSize + cellSize, cellSize);
                        }
                        else
                        {
                            GraphicsExtensions.FillRectangle(g, backcolor, _xoffset + (image1.Width - 1) * gridSize + cellSize, _yoffset + y * gridSize + cellSize, cellSize);
                        }
                    }
                }

                // Loop through the images pixels

                for (y = 0; y < (image1.Height); y++)
                {
                    for (x = 0; x < (image1.Width); x++)
                    {
                        /*if (
                            (_xoffset >= 0) && 
                            (_yoffset >= 0) &&
                            (_xoffset < WidthX * gridSize) &&
                            (_yoffset < WidthY * gridSize)
                            )
                        {*/
                        int pixelColor = image1.GetPixel(x, y).ToArgb();
                        int empty = Color.Empty.ToArgb();
                        int posX = (_xoffset / gridSize) + x;
                        int posY = (_yoffset / gridSize) + y;
                        //Debug.WriteLine("_xoffset: {0}", _xoffset);
                        //Debug.WriteLine("_yoffset: {0}", _yoffset);
                        //Debug.WriteLine("PosX: {0}   PosY: {1}", posX, posY);
                        //Debug.WriteLine("Pixel: " + x + ":" + y + " - pixelcolor: " + pixelColor + " - empty: " + empty);
                        if (pixelColor < -65794)
                        {
                            if (board[posX, posY] == true)
                            {
                                GraphicsExtensions.FillRectangle(g, dotcolor, _xoffset + x * gridSize + cellSize, _yoffset + y * gridSize + cellSize, cellSize);
                                needPixelRestore = true;
                            }
                            else
                                GraphicsExtensions.FillRectangle(g, backcolor, _xoffset + x * gridSize + cellSize, _yoffset + y * gridSize + cellSize, cellSize);
                        }
                        //}
                    }
                }

                //if (needPixelRestore)
                //drawBoard();

                this.pictureBox1.Image = bmp;

            }
            else
            {
                //Debug.WriteLine("doesn't fit: ");


            }
        }



        /*
        public static int ToInt(bool value)
        {
            return value ? 1 : 0;
        }
        */

        private void button2_Click(object sender, EventArgs e)
        {
            loadImagefromBMP("pictures\\spaceship1.bmp");
            if (!(timer1.Enabled))
                drawBoard();
        }



        private void button3_Click(object sender, EventArgs e)
        {
            createRandomBoard();
            if (!(timer1.Enabled))
                drawBoard();
        }


        //protected override void OnPaint(PaintEventArgs e)
        //{
        //base.OnPaint(e);

        //var bmp = new Bitmap(this.pictureBox1.Width, this.pictureBox1.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);


        //}

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            timer1.Interval = trackBar1.Value + 1;
            label3.Text = trackBar1.Value + " ms";
        }



        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
                drawMode = 0;

        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
                drawMode = 1;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked)
                drawMode = 2;
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton4.Checked)
                drawMode = 3;
        }



        private void button6_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < WidthX; i++)
                for (int j = 0; j < WidthY; j++)
                {

                    board[i, j] = false;
                }

            for (int n = 0; n < rgbboardhistory.Length; n++)
                for (int i = 0; i < WidthX; i++)
                {
                    for (int j = 0; j < WidthY; j++)
                    {
                        rgbboardhistory[n][i, j] = false;
                    }
                }
            drawBoard();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            Point coordinates = me.Location;
            Debug.WriteLine(coordinates.ToString());
            if (me.Button == MouseButtons.Left)
                if (radioButton5.Checked)
                    loadImagetoPos("1pxblack.BMP", coordinates.X, coordinates.Y);
                else if (radioButton12.Checked)
                    loadImagetoPos(@patternCustomFileName, coordinates.X, coordinates.Y);

            if (timer1.Enabled == false)
                drawBoard();



        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            Point coordinates = e.Location;
            mousePos = new Tuple<int, int>(coordinates.X, coordinates.Y);
            if (e.Button == MouseButtons.Left)
                if (radioButton5.Checked)
                    if (mousePos.Item1 >= 0 && mousePos.Item2 >= 0)
                        loadImagetoPos("1pxblack.BMP", coordinates.X, coordinates.Y);


        }

        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
            isMouseOverPic = true;
            pictureBox1.Cursor = redCrosshairCursor;
            if (timer1.Enabled == false)
                timer3.Start();
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            isMouseOverPic = false;
            pictureBox1.Cursor = Cursors.Default;
            if (timer1.Enabled == false)
                timer3.Stop();
            using (var g = Graphics.FromImage(bmp))
            {
                delPreviewImage(g, oldMousePos.Item1, oldMousePos.Item2);
                oldMousePos = mousePos;

            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {

                openFileDialog.InitialDirectory = Path.Combine(Application.StartupPath, @"Pictures");
                openFileDialog.Filter = "Bitmap (*.bmp)|*.bmp|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    string filePath = openFileDialog.FileName;
                    radioButton12.Enabled = true;
                    radioButton12.Checked = true;
                    patternCustomFileName = filePath;

                    using (image1 = new Bitmap(filePath, true))
                    {
                        label1.Text = Path.GetFileName(patternCustomFileName) + Environment.NewLine + image1.PixelFormat.ToString() + Environment.NewLine;
                        label1.Text += image1.Height + " x " + image1.Width;

                        thumbnail = GraphicsExtensions.ScaleImage(image1, 100, 100);
                        this.pictureBox2.Image = thumbnail;
                    }
                    Debug.WriteLine(patternCustomFileName);
                    //Read the contents of the file into a stream
                    //var fileStream = openFileDialog.OpenFile();

                    /*using (StreamReader reader = new StreamReader(fileStream))
                    {
                        fileContent = reader.ReadToEnd();
                    }*/
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            bool isTimer1Enabled = false;
            if (timer1.Enabled)
                isTimer1Enabled = true;

            if (isTimer1Enabled)
                timer1.Stop();
            drawBoard();
            if (isTimer1Enabled)
                timer1.Start();
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            label1.Text = Path.GetFileName(patternCustomFileName) + Environment.NewLine + image1.PixelFormat.ToString() + Environment.NewLine;
            label1.Text += image1.Height + " x " + image1.Width;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Form2 settingsForm = new Form2();




            // Show the settings form
            settingsForm.ShowDialog();

            if (gridChanged)
            {

                bmp = new Bitmap((WidthX * gridSize) + (2 * gridSize), (WidthY * gridSize) + (2 * gridSize), System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

                using (var g = Graphics.FromImage(bmp))
                {

                    Rectangle rect = new Rectangle(0, 0, (WidthX * gridSize) + 2 * gridSize + (gridSize / 2), (WidthY * gridSize) + 2 * gridSize + (gridSize / 2));

                    // Fill rectangle to screen.
                    g.FillRectangle(backcolor, rect);

                    this.pictureBox1.Image = bmp;
                }

                bool[,] newboard = new bool[WidthX, WidthY];
                bool[,] newoldboard = new bool[WidthX, WidthY];


                for (int n = 0; n < 7; n++)
                    rgbboardhistory[n] = new bool[WidthX, WidthY];



                /*for (int i = 0; i < WidthX; i++)
                    for (int j = 0; j < WidthY; j++)
                    {

                        board[i, j] = false;
                    }
                */
                for (int n = 0; n < rgbboardhistory.Length; n++)
                    for (int i = 0; i < WidthX; i++)
                    {
                        for (int j = 0; j < WidthY; j++)
                        {
                            rgbboardhistory[n][i, j] = false;
                        }
                    }



                if (WidthX <= oldWidthX)
                    for (int x = 0; x < WidthX; x++)
                    {
                        if (WidthY <= oldWidthY)
                            for (int y = 0; y < WidthY; y++)
                            {
                                newboard[x, y] = board[x, y];
                                newoldboard[x, y] = oldboard[x, y];
                            }
                        else
                            for (int y = 0; y < oldWidthY; y++)
                            {
                                newboard[x, y] = board[x, y];
                                newoldboard[x, y] = oldboard[x, y];
                            }

                    }
                else
                    for (int x = 0; x < oldWidthX; x++)
                    {
                        if (WidthY <= oldWidthY)
                            for (int y = 0; y < WidthY; y++)
                            {
                                newboard[x, y] = board[x, y];
                                newoldboard[x, y] = oldboard[x, y];
                            }
                        else
                            for (int y = 0; y < oldWidthY; y++)
                            {
                                newboard[x, y] = board[x, y];
                                newoldboard[x, y] = oldboard[x, y];
                            }

                    }

                board = newboard;
                oldboard = newoldboard;

                newboard = null;
                newoldboard = null;

                drawBoard();
                resizePicBox();

                if (T1wasRunning)
                {
                    timer1.Start();
                    timer2.Start();
                    T1wasRunning = false;
                    button1.Text = "Stop";
                }
                else
                    button1.Text = "Start / Stop";


                gridChanged = false;
            }
        }

        private void GoFullscreen(bool fullscreen)
        {
            if (fullscreen)
            {


                if (timer1.Enabled)
                {
                    timer1.Stop();
                    timer2.Stop();
                    T1wasRunning = true;
                }

                var newbounds = Screen.PrimaryScreen.Bounds;
                int newWidthX = newbounds.Width / gridSize;
                int newWidthY = newbounds.Height / gridSize;

                bool[,] newboard = new bool[newWidthX, newWidthY];
                bool[,] newoldboard = new bool[newWidthX, newWidthY];


                oldLocation = pictureBox1.Location;
                pictureBox1.Location = new Point(0, 0);
                pictureBox1.Width = newbounds.Width;
                pictureBox1.Height = newbounds.Height;
                pictureBox1.SendToBack();

                bmp = null;
                bmp = new Bitmap((newWidthX * gridSize) + (2 * gridSize), (newWidthY * gridSize) + (2 * gridSize), System.Drawing.Imaging.PixelFormat.Format32bppRgb);

                using (var g = Graphics.FromImage(bmp))
                {

                    Rectangle rect = new Rectangle(0, 0, (newWidthX * gridSize) + 2 * gridSize + (gridSize / 2), (newWidthY * gridSize) + 2 * gridSize + (gridSize / 2));

                    // Fill rectangle to screen.
                    g.FillRectangle(backcolor, rect);

                    this.pictureBox1.Image = bmp;
                }

                if (WidthX <= newWidthX)
                {
                    for (int x = 0; x < WidthX; x++)
                    {
                        if (WidthY <= newWidthY)
                        {
                            for (int y = 0; y < WidthY; y++)
                            {
                                newboard[x, y] = board[x, y];
                                newoldboard[x, y] = oldboard[x, y];
                            }
                        }
                        else
                        {
                            for (int y = 0; y < newWidthY; y++)
                            {
                                newboard[x, y] = board[x, y];
                                newoldboard[x, y] = oldboard[x, y];
                            }
                        }

                    }
                }
                else
                {
                    for (int x = 0; x < newWidthX; x++)
                    {
                        if (WidthY <= newWidthY)
                        {
                            for (int y = 0; y < WidthY; y++)
                            {
                                newboard[x, y] = board[x, y];
                                newoldboard[x, y] = oldboard[x, y];
                            }
                        }
                        else
                        {
                            for (int y = 0; y < newWidthY; y++)
                            {
                                newboard[x, y] = board[x, y];
                                newoldboard[x, y] = oldboard[x, y];
                            }
                        }

                    }
                }

                //board = new bool[newWidthX, newWidthY];
                board = newboard;
                //oldboard = new bool[newWidthX, newWidthY];
                oldboard = newoldboard;

                newboard = null;
                newoldboard = null;

                for (int n = 0; n < 7; n++)
                    rgbboardhistory[n] = new bool[newWidthX, newWidthY];


                oldWidthX = WidthX;
                oldWidthY = WidthY;
                WidthX = newWidthX;
                WidthY = newWidthY;

                drawBoard();

                if (T1wasRunning)
                {
                    timer1.Start();
                    timer2.Start();
                    T1wasRunning = false;
                }

                this.WindowState = FormWindowState.Normal;
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                this.Bounds = Screen.PrimaryScreen.Bounds;
            }
            else // exit fullscreen
            {
                if (timer1.Enabled)
                {
                    timer1.Stop();
                    timer2.Stop();
                    T1wasRunning = true;
                }



                pictureBox1.Location = oldLocation;
                pictureBox1.Width = oldWidthX * gridSize;
                pictureBox1.Height = oldWidthY * gridSize;

                //bmp = null;
                bmp = new Bitmap((oldWidthX * gridSize) + (2 * gridSize), (oldWidthY * gridSize) + (2 * gridSize), System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                using (var g = Graphics.FromImage(bmp))
                {

                    Rectangle rect = new Rectangle(0, 0, (oldWidthX * gridSize) + 2 * gridSize + (gridSize / 2), (oldWidthY * gridSize) + 2 * gridSize + (gridSize / 2));

                    // Fill rectangle to screen.
                    g.FillRectangle(backcolor, rect);

                    this.pictureBox1.Image = bmp;
                }

                bool[,] newboard = new bool[oldWidthX, oldWidthY];
                bool[,] newoldboard = new bool[oldWidthX, oldWidthY];

                if (WidthX <= oldWidthX)
                    for (int x = 0; x < WidthX; x++)
                    {
                        if (WidthY <= oldWidthY)
                            for (int y = 0; y < WidthY; y++)
                            {
                                newboard[x, y] = board[x, y];
                                newoldboard[x, y] = oldboard[x, y];
                            }
                        else
                            for (int y = 0; y < oldWidthY; y++)
                            {
                                newboard[x, y] = board[x, y];
                                newoldboard[x, y] = oldboard[x, y];
                            }

                    }
                else
                    for (int x = 0; x < oldWidthX; x++)
                    {
                        if (WidthY <= oldWidthY)
                            for (int y = 0; y < WidthY; y++)
                            {
                                newboard[x, y] = board[x, y];
                                newoldboard[x, y] = oldboard[x, y];
                            }
                        else
                            for (int y = 0; y < oldWidthY; y++)
                            {
                                newboard[x, y] = board[x, y];
                                newoldboard[x, y] = oldboard[x, y];
                            }

                    }

                board = newboard;
                oldboard = newoldboard;

                newboard = null;
                newoldboard = null;

                for (int n = 0; n < 7; n++)
                    rgbboardhistory[n] = new bool[oldWidthX, oldWidthY];


                WidthX = oldWidthX;
                WidthY = oldWidthY;

                drawBoard();

                if (T1wasRunning)
                {
                    timer1.Start();
                    timer2.Start();
                    T1wasRunning = false;
                }


                this.WindowState = FormWindowState.Maximized;
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            }
        }


        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.Alt && e.KeyCode == Keys.Enter)
            {
                // ...
                showFullScreen = !showFullScreen;

                // if (showFullScreen)
                //GoFullscreen(true);
                // else
                // GoFullscreen(false);
            }
            if (e.Alt && e.KeyCode == Keys.F)
            {
                if (showFullScreen)
                {
                    if (!(hideControls))
                        pictureBox1.BringToFront();
                    else
                        pictureBox1.SendToBack();
                    hideControls = !hideControls;
                }


            }
            if (e.Alt && e.KeyCode == Keys.R)
            {
                bool isTimer1Enabled = false;
                if (timer1.Enabled)
                    isTimer1Enabled = true;

                if (isTimer1Enabled)
                    timer1.Stop();
                drawBoard();
                if (isTimer1Enabled)
                    timer1.Start();


            }

        }


        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, this.panel1.ClientRectangle, Color.DarkBlue, ButtonBorderStyle.Solid);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            soundPlayer.Play();

            // Start the timer to check and replay the music
            timer4.Interval = 1000; // 1 second interval (adjust as needed)
            timer4.Start();
        }

        private void timer4_Tick(object sender, EventArgs e)
        {
          
        }
    }

    public static class GraphicsExtensions
    {
        public static void DrawCircle(this Graphics g, Pen pen,
                                      float centerX, float centerY, float radius)
        {
            g.DrawEllipse(pen, centerX - radius, centerY - radius,
                          radius + radius, radius + radius);
        }

        public static void FillCircle(this Graphics g, Brush brush,
                                      float centerX, float centerY, float radius)
        {
            g.FillEllipse(brush, centerX - radius, centerY - radius,
                          radius + radius, radius + radius);
        }

        public static void FillRectangle(this Graphics g, Brush brush, int x, int y, int size)
        {
            // Create rectangle.
            Rectangle rect = new Rectangle(x - (size / 2), y - (size / 2), size, size);

            // Fill rectangle to screen.
            g.FillRectangle(brush, rect);
        }

        public static Bitmap ResizeBitmap(Bitmap bmp, int width, int height)
        {
            Bitmap result = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.DrawImage(bmp, 0, 0, width, height);
            }

            return result;
        }

        public static Bitmap ScaleImage(Bitmap bmp, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / bmp.Width;
            var ratioY = (double)maxHeight / bmp.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(bmp.Width * ratio);
            var newHeight = (int)(bmp.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);

            using (var graphics = Graphics.FromImage(newImage))
            {
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                graphics.DrawImage(bmp, 0, 0, newWidth, newHeight);
            }
            return newImage;
        }

    }
}
