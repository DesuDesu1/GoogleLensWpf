namespace Domain
{
    public sealed class TextRow
    {
        public List<BoundingBoxes> Symbols { get; init; }
        public double[] RowBoundingBox { get; init; }
        public TextRow(List<BoundingBoxes> s, List<double> d)
        {
            Symbols = new List<BoundingBoxes>();
            Symbols.AddRange(s);
            RowBoundingBox = d.ToArray();
        }
    }
}