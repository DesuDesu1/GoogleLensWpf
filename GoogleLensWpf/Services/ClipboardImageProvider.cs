using GoogleLensWpf.Interfaces;
using GoogleLensWpf.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using WK.Libraries.SharpClipboardNS;

namespace GoogleLensWpf.Services
{
    public class ClipboardImageProvider : IImageProvider
    {
        public Image GetImage()
        {
            var clipboardImage = Clipboard.GetImage();
            using (var stream = new MemoryStream())
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(clipboardImage));
                encoder.Save(stream);
                return new Image
                {
                    Width = clipboardImage.Width,
                    Height = clipboardImage.Height,
                    Data = stream.ToArray()
                };
            }
        }
    }
}
