using System;
using System.Collections.Generic;
using System.Linq;

namespace Flooring
{
    public class Floor
    {
        public Dictionary<int[], List<Tile>> AvailableEdges { get; private set; }

        public List<KeyValuePair<int[], Tile>> GlueEdges { get; private set; }

        public Grid FloorGrid { get; private set; }

        public Floor(Tile[] _availableTiles)
        {
            //we need at least one tile
            if (_availableTiles.Count() == 0)
                throw new ArgumentException("We need to have at least one tile");

            GlueEdges = new List<KeyValuePair<int[], Tile>>();
            AvailableEdges = new Dictionary<int[], List<Tile>>(new IntArrayComparer());
            FloorGrid = new Grid();

            CreateAvailableEdges(_availableTiles.ToList());
        }

        //build a Dictionary of edges that will be used for searching
        //depends on AvailableEdges to be not null
        private void CreateAvailableEdges(List<Tile> availableTiles)
        {
            foreach (var tile in availableTiles)
            {
                foreach (var edge in tile.edges)
                {
                    var edgeReversed = edge.Reverse().ToArray();        //reverse the order of the array for easier match

                    if (!AvailableEdges.ContainsKey(edgeReversed))
                        AvailableEdges.Add(edgeReversed, new List<Tile>() { tile });
                    else
                        AvailableEdges[edgeReversed].Add(tile);
                }
            }
        }

        //once we place the tile on the floorGrid we want to remove it from the available edged dictionary
        private void RemoveTileFromAvailableEdges(Tile tile)
        {
            foreach (var edge in tile.edges)
            {
                var edgeReversed = edge.Reverse().ToArray();

                if (AvailableEdges.ContainsKey(edgeReversed))
                {
                    AvailableEdges[edgeReversed].Remove(tile);
                    if (AvailableEdges[edgeReversed].Count() == 0)
                        AvailableEdges.Remove(edgeReversed);
                }
            }
        }

        private List<Tile> FindTileInAvailableEdges(int[] searchEdge)
        {
            if (!AvailableEdges.ContainsKey(searchEdge))
                return null;

            return AvailableEdges[searchEdge];
        }

        private void AddTileToUsedEdges(int[][] newEdges, Tile tile)
        {
            foreach (int[] edge in newEdges)
            {
                GlueEdges.Add(new KeyValuePair<int[], Tile>(edge, tile));
            }
        }

        //*** This will be greedy (as with any greedy alg you can run into local minimum problem) ***
        //start placing tiles on the floor grid
        public void PlaceTilesOnFloor()
        {
            //init by picking a first tile in the Available Tiles and see if any tiles match
            var firstTile = AvailableEdges[AvailableEdges.Keys.First()].First();

            //this is potential issue if the first one is the one that doesn't belong to any other (FIX this later)
            AddTileToUsedEdges(firstTile.edges, firstTile);
            this.FloorGrid.SetTileAt(0, 0, firstTile);
            firstTile.SetCoordinates(0, 0);
            RemoveTileFromAvailableEdges(firstTile);

            for (int userEdgeIdx = 0; userEdgeIdx < GlueEdges.Count(); userEdgeIdx++)
            {
                var usedEdgeKVP = GlueEdges[userEdgeIdx];

                var potentialTiles = FindTileInAvailableEdges(usedEdgeKVP.Key);

                if (potentialTiles != null)
                {
                    foreach (var prospectTile in potentialTiles)
                    {
                        EdgeEnum usedEdgeRotation = usedEdgeKVP.Value.GetRotationOfEdge(usedEdgeKVP.Key);
                        EdgeEnum prospectEdgeRotation = prospectTile.GetRotationOfEdge(usedEdgeKVP.Key.Reverse().ToArray());
                        var angleToRotate = CalculateRotation(usedEdgeRotation, prospectEdgeRotation);
                        var prospectCoordinates = CalculateCoordinates(usedEdgeKVP.Value.Coordinates, usedEdgeRotation);
                        var neighbors = GetPotentialNeighbors(usedEdgeKVP.Value.Coordinates, prospectCoordinates);

                        bool isNeighborConflict = false;
                        //for each potential neighbor, is there any edge conflict
                        foreach (var neighbor in neighbors) //max 3 neighbors
                        {
                            //is there any edge conflict
                            isNeighborConflict = GetPotentialConflict(neighbor, angleToRotate, prospectCoordinates, prospectTile);

                            //if so then go to next potential tile
                            if (isNeighborConflict)
                                break;
                        }

                        if (!isNeighborConflict)
                        {
                            //so we don't have any neighbor conflict so we can place this tile
                            AddRemoveEdgesUsed(prospectTile, prospectEdgeRotation, angleToRotate, userEdgeIdx, neighbors);
                            this.FloorGrid.SetTileAt(prospectCoordinates, prospectTile);
                            prospectTile.Rotate(angleToRotate);
                            prospectTile.SetCoordinates(prospectCoordinates);
                            RemoveTileFromAvailableEdges(prospectTile);
                            //break the loop or prospectTiles
                            break;
                        }
                    }
                }
            }
        }

        private void AddRemoveEdgesUsed(Tile prospectTile, EdgeEnum prospectEdgeRotation, RotationEnum angleToRotate, int usedEdgeIdx, Dictionary<EdgeEnum, Tile> neighbors)
        {
            List<int[]> edgesToBeAdded = new List<int[]>();
            //for each side
            foreach (EdgeEnum side in Enum.GetValues(typeof(EdgeEnum)))
            {
                if (prospectEdgeRotation != side)
                    edgesToBeAdded.Add(prospectTile.edges[(int)side]);
            }

            //Remove edge of usedEdge
            //this.GlueEdges.RemoveAt(usedEdgeIdx);
            //Remove any neighbor edges that were not conflicting (You better have them)
            foreach (var neighbor in neighbors)
            {
                //get edge on prospectTile
                EdgeEnum prospectSide = GetProspectSide(neighbor.Key, angleToRotate);
                int[] prospectEdge = prospectTile.edges[(int)prospectSide];
                edgesToBeAdded.Remove(prospectEdge);

                //get edge on neighbor
                EdgeEnum neighborSide = GetOppositeSide(neighbor.Key);
                int[] neighborEdge = neighbor.Value.edges[(int)neighborSide];

                int neighborIdx = this.GlueEdges.FindIndex(a => neighborEdge.SequenceEqual(a.Key) && a.Value == neighbor.Value);

                if (neighborIdx < 0 || neighborIdx < usedEdgeIdx)
                    throw new Exception("we have problem locating the neighbor");
                else
                    this.GlueEdges.RemoveAt(neighborIdx);
            }

            //add new edges from prospectTile minus attached edges
            AddTileToUsedEdges(edgesToBeAdded.ToArray(), prospectTile);
        }

        private bool GetPotentialConflict(KeyValuePair<EdgeEnum, Tile> neighbor, RotationEnum angleToRotate, Coordinate prospectCoordinates, Tile prospectTile)
        {
            //get edge on prospectTile
            EdgeEnum prospectSide = GetProspectSide(neighbor.Key, angleToRotate);
            int[] prospectEdge = prospectTile.edges[(int)prospectSide];

            //get edge on neighbor
            EdgeEnum neighborSide = GetOppositeSide(neighbor.Key);
            int[] neighborEdge = neighbor.Value.edges[(int)neighborSide];

            //and compare them
            return !prospectEdge.SequenceEqual(neighborEdge.Reverse().ToArray());
        }

        private EdgeEnum GetProspectSide(EdgeEnum edgeEnum, RotationEnum angleToRotate)
        {
            switch (angleToRotate)
            {
                case RotationEnum.zero:
                    return edgeEnum;
                    break;

                case RotationEnum.nighty:
                    return (EdgeEnum)(((int)edgeEnum + 1) % 4);
                    break;

                case RotationEnum.oneEighty:
                    return (EdgeEnum)(((int)edgeEnum + 2) % 4);
                    break;

                case RotationEnum.twoSeventy:
                    return (EdgeEnum)(((int)edgeEnum + 3) % 4);
                    break;

                default:
                    throw new ArgumentException("Invalid angleToRotate");
                    break;
            }
        }

        private EdgeEnum GetOppositeSide(EdgeEnum edgeEnum)
        {
            switch (edgeEnum)
            {
                case EdgeEnum.top:
                    return EdgeEnum.botton;
                    break;

                case EdgeEnum.left:
                    return EdgeEnum.right;
                    break;

                case EdgeEnum.botton:
                    return EdgeEnum.top;
                    break;

                case EdgeEnum.right:
                    return EdgeEnum.left;
                    break;

                default:
                    throw new ArgumentException("Invalid edgeEnum");
                    break;
            }
        }

        //return Dictionary of neighbors and side from prospect tile to the neighbor, ignoring the existing tile Coordinate
        private Dictionary<EdgeEnum, Tile> GetPotentialNeighbors(Coordinate existingTileCoordinate, Coordinate potentialCoordinates)
        {
            //travel to each side from potentialCoordinates while ignoring Coordinate from tile being attached
            Dictionary<EdgeEnum, Tile> neighbors = new Dictionary<EdgeEnum, Tile>();

            //for each side
            foreach (EdgeEnum side in Enum.GetValues(typeof(EdgeEnum)))
            {
                var neighborCoordinate = CalculateCoordinates(potentialCoordinates, side);

                //if not the same as existing tile Coordinate and we have the neighbor then add to list
                if (!neighborCoordinate.IsSame(existingTileCoordinate))
                {
                    var neighbor = FloorGrid.GetTileAt(neighborCoordinate);
                    if (neighbor != null)
                        neighbors.Add(side, neighbor);
                }
            }

            return neighbors;
        }

        private Coordinate CalculateCoordinates(Coordinate coordinate, EdgeEnum edgeRotation)
        {
            switch (edgeRotation)
            {
                case EdgeEnum.top:
                    return new Coordinate(coordinate.x, coordinate.y - 1);
                    break;

                case EdgeEnum.left:
                    return new Coordinate(coordinate.x - 1, coordinate.y);
                    break;

                case EdgeEnum.botton:
                    return new Coordinate(coordinate.x, coordinate.y + 1);
                    break;

                case EdgeEnum.right:
                    return new Coordinate(coordinate.x + 1, coordinate.y);
                    break;

                default:
                    throw new ArgumentException("Invalid edgeRotation");
                    break;
            }
        }

        private RotationEnum CalculateRotation(EdgeEnum headEdge, EdgeEnum addedTileEdge)
        {
            int result = 0;
            switch (headEdge)
            {
                case EdgeEnum.top:
                    result = 180;
                    break;

                case EdgeEnum.left:
                    result = 90;
                    break;

                case EdgeEnum.botton:
                    result = 0;
                    break;

                case EdgeEnum.right:
                    result = -90;
                    break;

                default:
                    break;
            }

            switch (addedTileEdge)
            {
                case EdgeEnum.top:
                    result = (result + 0) % 360;
                    break;

                case EdgeEnum.left:
                    result = (result + 90) % 360;
                    break;

                case EdgeEnum.botton:
                    result = (result + 180) % 360;
                    break;

                case EdgeEnum.right:
                    result = (result + 270) % 360;
                    break;

                default:
                    break;
            }
            return (RotationEnum)result;
        }

        internal void PrintGrid()
        {
            Console.WriteLine("Result:");

            for (int y = 0; y < this.FloorGrid.GridSize; y++)
            {
                for (int row = 0; row < 4; row++)
                {
                    for (int x = 0; x < this.FloorGrid.GridSize; x++)
                    {
                        var tile = this.FloorGrid.GetTileAt(x - this.FloorGrid.Offset, y - this.FloorGrid.Offset);
                        if (tile != null)
                        {
                            Console.Write(Tile.PrintRow(tile, row));
                        }
                        else
                            Console.Write("            ");
                    }
                    Console.WriteLine();
                }
            }
        }

        internal void PrintAvailableTiles()
        {
            Console.WriteLine("Available Tiles:");

            List<Tile> availableTiles = new List<Tile>();

            foreach (var item in this.AvailableEdges)
            {
                foreach (var tile in item.Value)
                {
                    if (!availableTiles.Contains(tile))
                        availableTiles.Add(tile);
                }
            }
            foreach (Tile tile in availableTiles)
            {
                Console.WriteLine("Tile:");
                Console.Write(tile.Print(true));
                Console.WriteLine();
            }
        }
    }

    internal class IntArrayComparer : IEqualityComparer<int[]>
    {
        public bool Equals(int[] x, int[] y)
        {
            if (x.Length != y.Length)
            {
                return false;
            }
            return x.SequenceEqual(y);
        }

        public int GetHashCode(int[] obj)
        {
            int ret = 0;
            for (int i = 0; i < obj.Length; ++i)
            {
                ret ^= obj[i].GetHashCode();
            }
            return ret;
        }
    }
}