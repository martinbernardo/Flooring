using System;
using System.Text;

namespace Flooring
{
    public class Grid
    {
        public int GridSize = 3;

        public Tile[,] FloorGrid { get; private set; }

        public int Offset
        {
            get
            {
                return ((int)Math.Floor(GridSize / 2.0));
            }
        }

        //Idea here is to have center of the grid indexed by location {0, 0}
        //then have the Array dynamically grow if more space needed
        public Grid()
        {
            FloorGrid = new Tile[GridSize, GridSize];
        }

        public Tile GetTileAt(Coordinate coordinate)
        {
            return GetTileAt(coordinate.x, coordinate.y);
        }

        public Tile GetTileAt(int x, int y)
        {
            if (Offset + x < 0 ||
                Offset + x >= GridSize ||
                Offset + y < 0 ||
                Offset + y >= GridSize)
            {
                return null;
            }
            else
            {
                return this.FloorGrid[Offset + y, Offset + x];
            }
        }

        public void SetTileAt(Coordinate coordinates, Tile tile)
        {
            SetTileAt(coordinates.x, coordinates.y, tile);
        }

        public void SetTileAt(int x, int y, Tile tile)
        {
            if (Offset + x < 0 || Offset + x >= GridSize || Offset + y < 0 || Offset + y >= GridSize)
            {
                //increate Grid size
                IncreateGridSize();
            }

            //after growing the grid we still are out of bounds that we have problems (on top of the mental ones :P)
            if (Offset + x < 0 || Offset + x >= GridSize || Offset + y < 0 || Offset + y >= GridSize)
            {
                throw new Exception("I can only grow so fast man");
            }

            this.FloorGrid[Offset + y, Offset + x] = tile;
        }

        private void IncreateGridSize()
        {
            //increase by 2 on each side
            //this is potential performance impact (dont grow often please)
            int newGridSize = GridSize + 2;
            Tile[,] newFloorGrid = new Tile[newGridSize, newGridSize];
            int staringPoint = newFloorGrid.GetLength(0) + FloorGrid.GetLength(0);

            //copy over the old grid into the new one
            for (int y = 0; y < GridSize; y++)
            {
                for (int x = 0; x < GridSize; x++)
                {
                    newFloorGrid[y + 1, x + 1] = FloorGrid[y, x];
                }
            }

            FloorGrid = newFloorGrid;
            GridSize = newGridSize;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int y = 0; y < this.FloorGrid.GetLength(0); y++)
            {
                for (int x = 0; x < this.FloorGrid.GetLength(1); x++)
                {
                    //TODO: fix line breaks
                    sb.Append(this.FloorGrid[y, x].ToString());
                }
            }

            return sb.ToString();
        }
    }
}