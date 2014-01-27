namespace Flooring
{
    public class Coordinate
    {
        public int x;
        public int y;

        public Coordinate(int _x, int _y)
        {
            x = _x;
            y = _y;
        }

        public bool IsSame(Coordinate anotherCoordinate)
        {
            return this.x == anotherCoordinate.x && this.y == anotherCoordinate.y;
        }
    }
}