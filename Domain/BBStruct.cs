namespace Domain
{
    public sealed class BoundingBoxes
    {
        public BoundingBoxes(string characters, double[] coordinates)
        {
            this.characters = characters;
            this.coordinates = coordinates;
        }

        public string? characters { get; init; }
        public double[] coordinates { get; init; }
    }
}