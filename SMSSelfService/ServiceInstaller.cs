using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;
using XMLConfig;

namespace SMSSelfService
{
    [RunInstaller(true)]
    public class WindowsServiceInstaller : Installer
    {
        ServiceProcessInstaller serviceProcessInstaller;
        ServiceInstaller serviceInstaller;

        public WindowsServiceInstaller()
        {
            serviceProcessInstaller = new ServiceProcessInstaller();
            serviceInstaller = new ServiceInstaller();

            Config config = new Config(Utils.ApplicationLocationPath() + "\\config.xml", true);

            if(config.GetValue("Domain/UseAdminAccount", false))
                serviceProcessInstaller.Account = ServiceAccount.LocalService;
            else
                serviceProcessInstaller.Account = ServiceAccount.User;

            serviceInstaller.DisplayName = "SMS Self Service Server";
            serviceInstaller.ServiceName = "smsselfservice";
            serviceInstaller.StartType = ServiceStartMode.Automatic;
            serviceInstaller.Description = "Receives SMS messages and automatically resets user passwords";
            this.Installers.Add(serviceProcessInstaller);
            this.Installers.Add(serviceInstaller);
        }
    }
}