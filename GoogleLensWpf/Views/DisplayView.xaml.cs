using GoogleLensWpf.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GoogleLensWpf.Views
{
    /// <summary>
    /// Логика взаимодействия для DisplayView.xaml
    /// </summary>
    public partial class DisplayView : UserControl
    {
        public DisplayView()
        {
            InitializeComponent();
        }
        private void View_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (DataContext is DisplayViewModel viewModel)
            {
                if (e.OriginalSource.ToString() == "System.Windows.Controls.TextBoxView")
                {
                    return;
                }
                else
                {
                    foreach (var boundingBox in viewModel.Hitboxes)
                    {
                        boundingBox.IsSelected = false;
                    }
                    viewModel.UnselectAllText();
                }
            }
        }
        private void TextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            textBox.SelectAll();
        }
        private void Image_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var image = (Image)sender;
            var p = image.TranslatePoint(new Point(0, 0), this);
            if (DataContext is DisplayViewModel viewModel)
            {
                foreach (var boundingBox in viewModel.Hitboxes)
                {
                    boundingBox.Y = boundingBox.OG.coordinates[0] * e.NewSize.Height + p.Y;
                    boundingBox.X = boundingBox.OG.coordinates[1] * e.NewSize.Width + p.X;
                    boundingBox.Width = boundingBox.OG.coordinates[2] * e.NewSize.Width;
                    boundingBox.Height = boundingBox.OG.coordinates[3] * e.NewSize.Height;
                }
            }
        }
        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is not DisplayViewModel viewModel)
            {
                return;
            }
            var selectedRectangle = (Rectangle)sender;
            var p = selectedRectangle.TranslatePoint(new Point(0, 0), this);
            var selectedHitbox = viewModel.Hitboxes.OrderBy(item => Math.Abs(p.X - item.X)).FirstOrDefault();
            if (selectedHitbox != null)
            {
                selectedHitbox.IsSelected = !selectedHitbox.IsSelected;
                viewModel.HitboxSelected(selectedHitbox);
            }
        }
    }
}
