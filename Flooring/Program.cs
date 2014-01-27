using System;

namespace Flooring
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                Floor floor = new Floor(new Tile[] {
                    //new Tile(new int[,] { { 1, 3, 5, 7 }, { 1, 1, 0, 8 }, { 7, 0, 0, 8 }, { 7, 4, 5, 8 } }),
                    new Tile(new int[,] { { 1, 0, 0, 7 }, { 8, 2, 0, 4 }, { 2, 0, 0, 5 }, { 1, 4, 5, 8 } }),
                    new Tile(new int[,] { { 7, 3, 5, 7 }, { 7, 3, 0, 8 }, { 1, 0, 0, 8 }, { 1, 7, 1, 1 } }),
                    new Tile(new int[,] { { 1, 0, 4, 1 }, { 7, 4, 0, 8 }, { 3, 0, 0, 2 }, { 7, 8, 8, 8 } }),
                    new Tile(new int[,] { { 7, 0, 0, 1 }, { 3, 6, 0, 1 }, { 5, 0, 0, 2 }, { 7, 9, 3, 1 } }),
                    new Tile(new int[,] { { 1, 7, 7, 7 }, { 1, 7, 0, 3 }, { 7, 0, 0, 3 }, { 6, 5, 2, 0 } }),
                    new Tile(new int[,] { { 8, 2, 8, 1 }, { 5, 8, 0, 1 }, { 4, 0, 0, 1 }, { 1, 5, 3, 1 } }),
                    new Tile(new int[,] { { 1, 7, 1, 1 }, { 8, 9, 0, 1 }, { 6, 0, 0, 1 }, { 1, 9, 3, 2 } }),
                    new Tile(new int[,] { { 1, 2, 1, 1 }, { 9, 0, 0, 3 }, { 0, 0, 0, 1 }, { 3, 6, 3, 0 } })
                });

                floor.PrintAvailableTiles();

                floor.PlaceTilesOnFloor();
                floor.PrintGrid();
                floor.PrintAvailableTiles();

                Console.ReadKey();
            }
            catch (Exception ex)
            {
                //TODO: log this
                Console.Error.Write(ex.Message);
                Console.ReadKey();
            }
        }
    }
}