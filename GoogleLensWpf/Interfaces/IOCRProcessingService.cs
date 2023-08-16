using GoogleLensWpf.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoogleLensWpf.Interfaces
{
    public interface IOCRProcessingService
    {
        event EventHandler<OCRResult> NewOCRResult;
        Task PerformOcr(Image image);
    }
}