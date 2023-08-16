using GoogleLensWpf.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoogleLensWpf.Interfaces
{
    public interface IOCRService
    {
        Task<string> GetJsonString(byte[] image);
    }
}