using System;

namespace Automata
{
    public class GOLalgorithm
    {
        //public readonly bool[,] newboard;

        /// <summary>
        /// horizontal size of grid to calculate next generation for
        /// </summary>
        private int WidthX;
        /// <summary>
        /// vertical size of grid to calculate next generation for
        /// </summary>
        private int WidthY;

        private const int healthCondition1 = 2; // two adjacent dots required to survive
        private const int healthCondition2 = 3; // three adjacent dots required to grow

        public GOLalgorithm()
        {

        }

        private int ToInt(bool value)
        {
            return value ? 1 : 0;
        }

        /// <summary>
        /// calculate next generation of game of life cells based on existing board of cells
        /// </summary>
        /// <param name="board">existing board to base calculations on</param>
        /// <returns>bool[,] array with next generation board of cells</returns>
        public bool[,] calculateNextBoard(bool[,] board)
        {
            WidthX = board.GetLength(0);
            WidthY = board.GetLength(1);

            bool[,] Tempboard = new bool[WidthX, WidthY];

            //for (int i = 0; i < WidthX; i++)

            for (int i = 0; i< WidthX; i++)
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
            }

            return Tempboard;

        }
    }



    public class Rule30
    {
        private int WidthX;
        private int WidthY;

        // Define the coordinates of the designated zone
        private int zoneStartX;
        private int zoneEndX;
        private int zoneStartY;
        private int zoneEndY;

        public Rule30()
        {
            // Initialize the coordinates of the designated zone
            zoneStartX = 0;  // Define the starting X coordinate of the zone
            zoneEndX = 1012;    // Define the ending X coordinate of the zone
            zoneStartY = 0;   // Define the starting Y coordinate of the zone
            zoneEndY = 562;    // Define the ending Y coordinate of the zone
        }

        private int ToInt(bool value)
        {
            return value ? 1 : 0;
        }

        public bool[,] calculateNextBoard(bool[,] board)
        {
            WidthX = board.GetLength(0);
            WidthY = board.GetLength(1);

            bool[,] Tempboard = new bool[WidthX, WidthY];

            for (int j = 0; j < WidthY - 1; j++)
            {
                for (int i = 0; i < WidthX - 3; i++)
                {
                    if (IsInZone(i, j)) // Check if the cell is within the designated zone
                    {
                        // Apply the new rule within the designated zone
                        // Example of a new rule: if a cell is alive, make its neighbor alive
                        if (board[i, j] == true)
                            Tempboard[i + 1, j + 1] = true;
                        else
                            Tempboard[i + 1, j + 1] = false;
                    }
                    else // Apply standard rules outside the designated zone
                    {
                        if (board[i, j] == true)
                            Tempboard[i, j] = true;

                        if (board[i, j] == true)
                        {
                            if (board[i + 1, j] == true)
                                Tempboard[i + 1, j + 1] = false;
                            else
                            {
                                if (board[i + 2, j] == true)
                                    Tempboard[i + 1, j + 1] = false;
                                else
                                    Tempboard[i + 1, j + 1] = true;
                            }
                        }
                        else if (board[i, j] == false)
                        {
                            if (board[i + 1, j] == true)
                                Tempboard[i + 1, j + 1] = true;
                            else if (board[i + 2, j] == true)
                                Tempboard[i + 1, j + 1] = true;
                            else
                                Tempboard[i + 1, j + 1] = false;
                        }
                    }
                }
            }

            return Tempboard;
        }

        // Helper method to check if a cell is within the designated zone
        private bool IsInZone(int x, int y)
        {
            return x >= zoneStartX && x <= zoneEndX && y >= zoneStartY && y <= zoneEndY;
        }
    }

}
