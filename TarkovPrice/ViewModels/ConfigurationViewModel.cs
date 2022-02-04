using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using WindowsDisplayAPI;
using Caliburn.Micro;
using TarkovPrice.XmlModels;

namespace TarkovPrice.ViewModels
{
    public class ConfigurationViewModel : Screen
    {
        public FileAccess FileAccess => new FileAccess();

        private BindableCollection<string> _monitorsComboBox = new BindableCollection<string>();
        private string _selectedMonitor = String.Empty;
        private string _apiKey;

        public BindableCollection<string> MonitorsComboBox
        {
            get => _monitorsComboBox;
            set
            {
                _monitorsComboBox = value;
                //NotifyOfPropertyChange(() => MonitorsComboBox);
            }
        }

        public string SelectedMonitor
        {
            get => _selectedMonitor;
            set
            {
                _selectedMonitor = value;
                NotifyOfPropertyChange(() => SelectedMonitor);
                Configuration.SetPickedMonitor(value);
            }
        }

        public string ApiKey
        {
            get
            {
                return _apiKey;
            }
            set
            {
                _apiKey = value;
                NotifyOfPropertyChange(() => ApiKey);
                Configuration.SetApiKey(value);
            }
        }

        public ConfigurationViewModel()
        {
            var allScreens = Display.GetDisplays();
            LoadMonitorComboBoxData(allScreens);

            //SelectedMonitor = Configuration.PickedMonitor;
            ApiKey = Configuration.GetStringValue(Configuration.ApiKey);
        }

        public void SaveConfiguration()
        {
        //    Configuration.PickedMonitor = SelectedMonitor;
        //    Configuration.ApiKey = ApiKey;

            Configurations configurations = new Configurations();
            var configurationDictionary = Configuration.ConfigurationDictionary;
            foreach (var configuration in configurationDictionary)
            {
                configurations.ConfigurationCollection.Add(new XmlModels.Configuration(configuration.Key, configuration.Value));
            }

            FileAccess.SaveConfigurationFile(configurations);

            CloseConfigurationWindow();
        }

        public void CancelConfiguration()
        {
            CloseConfigurationWindow();
        }

        private void LoadMonitorComboBoxData(IEnumerable<Display> displays)
        {
            foreach (var display in displays)
            {
                MonitorsComboBox.Add(display.DeviceName);
            }
        }

        private async void CloseConfigurationWindow()
        {
            await TryCloseAsync();
        }
    }
}