using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using GoogleLensWpf.Interfaces;
using GoogleLensWpf.Services;
using GoogleLensWpf.ViewModels;
using GoogleLensWpf.Views;
using Microsoft.Extensions.DependencyInjection;

namespace GoogleLensWpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private ServiceProvider serviceProvider;
        public App()
        {
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            serviceProvider = services.BuildServiceProvider();
        }
        private void ConfigureServices(ServiceCollection services)
        {
            services.AddHttpClient<IOCRService, GoogleLensOCRService>();
            services.AddTransient<IImageProvider, ClipboardImageProvider>();
            services.AddSingleton<IOCRProcessingService, OCRProcessingService>();
            services.AddSingleton<HomeViewModel>();
            services.AddSingleton<DisplayViewModel>();
            services.AddSingleton<MainWindowViewModel>();
            services.AddScoped<MainWindow>();
        }
        private void OnStartup(object sender, StartupEventArgs e)
        {
            var mainWindow = serviceProvider.GetService<MainWindow>();
            mainWindow!.Show();
        }
    }
}
