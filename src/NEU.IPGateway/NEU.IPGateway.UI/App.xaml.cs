using NEU.IPGateway.UI.Services;
using NEU.IPGateway.UI.Views;
using NEU.IPGateWay.Core.Services;
using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace NEU.IPGateway.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Locator.CurrentMutable.RegisterViewsForViewModels(Assembly.GetCallingAssembly());
            InitializeService();

           
        }

        private void InitializeService()
        {
            Locator.CurrentMutable.Register<IUserStorageService>(() => new UserStorageService());
            Locator.CurrentMutable.Register<IInternetGateWayService>(() => new InternetGateWayService());
            
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {


        }
    }
}
