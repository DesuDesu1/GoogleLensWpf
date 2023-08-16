namespace GoogleLensWpf.Models
{
    public readonly struct BoundingBoxes
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