using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleLensWpf.Models
{
    public class OCRResult
    {
        public Image Image { get; }
        public IEnumerable<TextRow> Rows { get; }
        public string Result { get; }

        public OCRResult(Image image, IEnumerable<TextRow> rows, string result)
        {
            Image = image;
            Rows = rows;
            Result = result;
        }
    }
}
