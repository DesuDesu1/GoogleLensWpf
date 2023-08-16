using GoogleLensWpf.Interfaces;
using GoogleLensWpf.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GoogleLensWpf.ViewModels
{
    public class DisplayViewModel : ObservableObject
    {
        List<Hitbox> selectedHitboxes;
        private string _selectedText;
        public string SelectedText
        {
            get { return _selectedText; }
            set { SetProperty(ref _selectedText, value); }
        }
        private readonly IOCRProcessingService _ocrProcessingService;
        private ImageSource _image;
        public ImageSource Image
        {
            get { return _image; }
            set
            {
                _image = value;
                OnPropertyChanged("Image");
            }
        }

        private ObservableCollection<Hitbox> _hitboxes;
        public ObservableCollection<Hitbox> Hitboxes
        {
            get { return _hitboxes; }
            set
            {
                _hitboxes = value;
                OnPropertyChanged("Hitboxes");
            }
        }

        public DisplayViewModel(IOCRProcessingService ocrProcessingService)
        {
            _ocrProcessingService = ocrProcessingService;
            Hitboxes = new ObservableCollection<Hitbox>();
            _ocrProcessingService.NewOCRResult += OnNewOCRResult;
            selectedHitboxes = new List<Hitbox>();
        }
        private void OnNewOCRResult(object sender, OCRResult ocrResult)
        {
            using (var stream = new MemoryStream(ocrResult.Image.Data))
            {
                BitmapImage imageSource = new BitmapImage();
                imageSource.BeginInit();
                imageSource.CacheOption = BitmapCacheOption.OnLoad;
                imageSource.StreamSource = stream;
                imageSource.EndInit();
                Image = imageSource;
                Hitboxes.Clear();
                selectedHitboxes.Clear();
                SelectedText = "";
                foreach(var boundingBox in ocrResult.Rows.SelectMany(b => b.Symbols))
                {
                    double rotationAngle = (boundingBox.coordinates.Length >= 4) ? boundingBox.coordinates[4] : 0;
                    var hitbox = new Hitbox
                    {
                        OG = boundingBox,
                        IsSelected = false,
                        RotationAngle = rotationAngle
                    };
                    Hitboxes.Add(hitbox);
                }
                
                selectedHitboxes.Capacity = _hitboxes.Count;
            }
        }
        public void UnselectAllText()
        {
            selectedHitboxes.Clear();
            SelectedText = "";
        }
        public void HitboxSelected(Hitbox hitbox)
        {
            if (hitbox.IsSelected)
            {
                selectedHitboxes.Add(hitbox);
            }
            else
            {
                selectedHitboxes.Remove(hitbox);
            }
            SelectedText = String.Join("", selectedHitboxes.Select(x => x.OG.characters));
        }
    }
}
