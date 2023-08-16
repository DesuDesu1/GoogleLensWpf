using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GoogleLensWpf.Interfaces
{
    public interface INavigationService
    {
        void NavigateTo<T>() where T : FrameworkElement;
    }
}
