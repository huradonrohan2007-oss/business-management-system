using System;
using System.IO;
using System.Windows.Forms;

namespace BusinessManagement.App
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            string profileFile = "business_profile.config";
            string activeProfile = string.Empty;

            if (File.Exists(profileFile))
            {
                activeProfile = File.ReadAllText(profileFile);
            }

            // If no profile is saved yet, launch the setup wizard on first boot
            if (string.IsNullOrEmpty(activeProfile))
            {
                using (var wizard = new SetupWizardForm())
                {
                    if (wizard.ShowDialog() == DialogResult.OK)
                    {
                        activeProfile = wizard.SelectedProfile;
                    }
                    else
                    {
                        return; // Exit application if setup is cancelled
                    }
                }
            }

            // Launch your actual main dashboard
            Application.Run(new MainDashboardForm());
        }
    }
}