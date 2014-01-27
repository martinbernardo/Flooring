using System;
using System.Linq;
using System.Text;

namespace Flooring
{
    public class Tile
    {
        public int[,] matrix { get; private set; }

        //edges
        public int edgeLength { get; private set; }

        public int[][] edges { get; private set; }

        private RotationEnum rotation { get; set; }

        public Coordinate Coordinates { get; private set; }

        // Only accepting matrix in this format
        // new int[,] { { 1, 2, 3, 4 }, { 5, 0, 0, 6 }, { 7, 0, 0, 8 }, { 9, 0, 1, 2 } };
        public Tile(int[,] _matrix)
        {
            if (_matrix.Rank > 2 || _matrix.Length != 16)
                throw new ArgumentException("The Tile matrix is not valid");

            matrix = _matrix;
            rotation = RotationEnum.zero;
            edgeLength = matrix.GetLength(0);

            edges = new int[edgeLength][];
            edges[0] = new int[edgeLength]; //top
            edges[1] = new int[edgeLength]; //left
            edges[2] = new int[edgeLength]; //bottom
            edges[3] = new int[edgeLength]; //right

            //get all edges
            for (int i = 0; i < edgeLength; i++)
            {
                edges[(int)EdgeEnum.top][i] = matrix[0, i];
                edges[(int)EdgeEnum.left][i] = matrix[edgeLength - 1 - i, 0];
                edges[(int)EdgeEnum.botton][i] = matrix[edgeLength - 1, edgeLength - 1 - i];
                edges[(int)EdgeEnum.right][i] = matrix[i, edgeLength - 1];
            }
        }

        //all rotations are in clockwise direction
        public void Rotate(RotationEnum angle)
        {
            int[] tmp;
            int[,] ret = new int[edgeLength, edgeLength];

            rotation = (RotationEnum)(((int)rotation + (int)angle) % 360);

            switch (angle)
            {
                case RotationEnum.zero:
                    break;

                case RotationEnum.nighty:
                    tmp = edges[(int)EdgeEnum.top];
                    edges[(int)EdgeEnum.top] = edges[(int)EdgeEnum.left];
                    edges[(int)EdgeEnum.left] = edges[(int)EdgeEnum.botton];
                    edges[(int)EdgeEnum.botton] = edges[(int)EdgeEnum.right];
                    edges[(int)EdgeEnum.right] = tmp;

                    //rotate matrix
                    for (int i = 0; i < edgeLength; ++i)
                    {
                        for (int j = 0; j < edgeLength; ++j)
                        {
                            ret[i, j] = matrix[edgeLength - j - 1, i];
                        }
                    }
                    matrix = ret;
                    break;

                case RotationEnum.oneEighty:
                    tmp = edges[(int)EdgeEnum.top];
                    edges[(int)EdgeEnum.top] = edges[(int)EdgeEnum.botton];
                    edges[(int)EdgeEnum.botton] = tmp;
                    tmp = edges[(int)EdgeEnum.left];
                    edges[(int)EdgeEnum.left] = edges[(int)EdgeEnum.right];
                    edges[(int)EdgeEnum.right] = tmp;

                    //rotate matrix
                    for (int i = 0; i < edgeLength; ++i)
                    {
                        for (int j = 0; j < edgeLength; ++j)
                        {
                            ret[i, j] = matrix[edgeLength - i - 1, edgeLength - j - 1];
                        }
                    }
                    matrix = ret;
                    break;

                case RotationEnum.twoSeventy:
                    tmp = edges[(int)EdgeEnum.top];
                    edges[(int)EdgeEnum.top] = edges[(int)EdgeEnum.right];
                    edges[(int)EdgeEnum.right] = edges[(int)EdgeEnum.botton];
                    edges[(int)EdgeEnum.botton] = edges[(int)EdgeEnum.left];
                    edges[(int)EdgeEnum.left] = tmp;

                    //rotate matrix
                    for (int i = 0; i < edgeLength; ++i)
                    {
                        for (int j = 0; j < edgeLength; ++j)
                        {
                            ret[i, j] = matrix[j, edgeLength - i - 1];
                        }
                    }
                    matrix = ret;

                    break;

                default:
                    break;
            }
        }

        public void SetCoordinates(int x, int y)
        {
            if (Coordinates == null)
                Coordinates = new Coordinate(x, y);
            else
            {
                Coordinates.x = x;
                Coordinates.y = y;
            }
        }

        public void SetCoordinates(Coordinate newCoordinate)
        {
            Coordinates = newCoordinate;
        }

        public EdgeEnum GetRotationOfEdge(int[] edge)
        {
            for (int i = 0; i < this.edges.Length; i++)
            {
                if (this.edges[i].SequenceEqual(edge))
                    return (EdgeEnum)i;
            }

            //we didn't find that edge in this tile
            throw new ArgumentException("we didn't find that edge in this tile");
        }

        #region Print

        public string Print(bool isIncludeNextLine)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Rotation: " + this.rotation);

            for (int i = 0; i < edgeLength; ++i)
            {
                sb.Append(" [");
                for (int j = 0; j < edgeLength; ++j)
                {
                    sb.Append(matrix[i, j]);
                    if (j < edgeLength - 1)
                        sb.Append(", ");
                }
                sb.Append("] ");
                if (isIncludeNextLine)
                    sb.AppendLine();
            }

            return sb.ToString();
        }

        public static StringBuilder PrintRow(Tile tile, int rowNum)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            for (int j = 0; j < tile.edgeLength; ++j)
            {
                sb.Append(tile.matrix[rowNum, j]);
                if (j < tile.edgeLength - 1)
                    sb.Append(", ");
            }
            sb.Append("]");
            return sb;
        }

        #endregion Print
    }
}