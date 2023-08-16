using Domain;

namespace Application.Interfaces
{
    public interface IGetTextRowFromImage
    {
        Task<IEnumerable<TextRow>> GetTextRowList(byte[] image);
    }
}