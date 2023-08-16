using GoogleLensWpf.Commands;
using GoogleLensWpf.Interfaces;
using GoogleLensWpf.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WK.Libraries.SharpClipboardNS;
using static WK.Libraries.SharpClipboardNS.SharpClipboard;

namespace GoogleLensWpf.ViewModels
{
    public class HomeViewModel : ObservableObject
    {
        private Stopwatch stopwatch = new Stopwatch();
        SharpClipboard clipboard;
        private readonly IImageProvider imageprovider;
        private readonly IOCRProcessingService ocr;
        private bool setResultInClipboard;
        private bool monitorClipboard;
        public ICommand LoadImageFromFileCommand { get; }
        public bool SetResultInClipboard
        {
            get { return setResultInClipboard; }
            set
            {
                setResultInClipboard = value;
                OnPropertyChanged(nameof(SetResultInClipboard));
                if (setResultInClipboard)
                {
                    ocr.NewOCRResult += SetResultInClipboardBuffer;
                }
                else
                {
                    ocr.NewOCRResult -= SetResultInClipboardBuffer;
                }
            }
        }
        public bool MonitorClipboard
        {
            get { return monitorClipboard; }
            set
            {
                monitorClipboard = value;
                OnPropertyChanged(nameof(monitorClipboard));
                if (monitorClipboard)
                {
                    clipboard.ClipboardChanged += ClipboardContentChanged;
                }
                else
                {
                    clipboard.ClipboardChanged -= ClipboardContentChanged;
                }
            }
        }

        private void SetResultInClipboardBuffer(object? sender, OCRResult e)
        {
            Clipboard.SetText(e.Result);
        }
        private string statusMessage;
        public string StatusMessage
        {
            get { return statusMessage; }
            set { SetProperty(ref statusMessage, value); }
        }
        private string errorMessage;
        public string ErrorMessage
        {
            get { return errorMessage; }
            set { SetProperty(ref errorMessage, value); }
        }
        public HomeViewModel(IImageProvider imageprovider, IOCRProcessingService ocr)
        {
            this.imageprovider = imageprovider;
            this.ocr = ocr;
            this.clipboard = new SharpClipboard();
            LoadImageFromFileCommand = new RelayCommand(LoadImageFromFile);
        }
        private void LoadImageFromFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files (*.jpg; *.png; *.bmp)|*.jpg; *.png; *.bmp|All Files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                // Get the selected image file path
                string imagePath = openFileDialog.FileName;

                // Create the ImageSource from the image file path
                ImageSource imageSource = new BitmapImage(new Uri(imagePath));

                // Get the image data
                byte[] imageData;
                using (FileStream stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                {
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        imageData = reader.ReadBytes((int)stream.Length);
                    }
                }

                // Create an instance of the Image class
                Image image = new Image
                {
                    Width = imageSource.Width,
                    Height = imageSource.Height,
                    Data = imageData
                };

                // Pass the image to another class or perform further operations
                SendImageToOcr(image);
            }
        }

        private async void ClipboardContentChanged(Object sender, ClipboardChangedEventArgs e)
        {
            if (e.ContentType is ContentTypes.Image)
            {
                var clipboardImage = clipboard.ClipboardImage;

                using (var stream = new MemoryStream())
                {
                    clipboardImage.Save(stream, System.Drawing.Imaging.ImageFormat.Png);

                    Image image = new Image
                    {
                        Width = clipboardImage.Width,
                        Height = clipboardImage.Height,
                        Data = stream.ToArray()
                    };
                    // Pass the image to another class or perform further operations
                    SendImageToOcr(image);
                }
            }
        }
        private async Task SendImageToOcr(Image img)
        {
            ErrorMessage = "";
            StatusMessage = "Sending Request...";
            stopwatch.Start();
            try
            {
                await ocr.PerformOcr(img);
            }
            catch (HttpRequestException ex) when
            (ex.InnerException is SocketException socketException
            && socketException.SocketErrorCode is SocketError.HostNotFound)
            {
                ErrorMessage = "Unknown host. " + ex.Message;
            }
            catch (HttpRequestException ex)
            {
                ErrorMessage = "No internet connection.  " + ex.Message;
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error occurred: " + ex.Message;
            }
            finally
            {
                stopwatch.Stop();
                double elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
                StatusMessage = $"Operation performed in {elapsedSeconds:F2} seconds";
                stopwatch.Reset();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
