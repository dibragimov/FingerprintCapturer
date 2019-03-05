using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows;

namespace HorioFingerprintCapturer
{
    public class MainWindowViewModel : ViewModelBase
    {
        private RelayCommand _searchCommand;
        private RelayCommand _captureCommand;
        private RelayCommand _saveCommand;
        //private string ConnectionString;
        private Person _selectedPerson;
        private List<Person> _employees;
        private SqlDBContext SqlClient;
        private string _searchText;
        
        public MainWindowViewModel()
        {
            //Properties.Resources.Culture = new System.Globalization.CultureInfo(Settings.CurrentCulture);
            //ConnectionString = Settings.ConnectionString;
            SqlClient = new SqlDBContext();
        }

        public List<Person> Employees
        {
            get
            {
                return _employees;
            }
            set
            {
                _employees = value;
                OnPropertyChanged("Employees");
            }
        }

        public Person SelectedPerson
        {
            get
            {
                return _selectedPerson;
            }
            set
            {
                _selectedPerson = value;
                OnPropertyChanged("SelectedPerson");
                OnPropertyChanged("IsCaptureEnabled");
            }
        }

        public string SearchText
        {
            get
            {
                return _searchText;
            }
            set
            {
                _searchText = value;
                OnPropertyChanged("SearchText");
                OnPropertyChanged("IsSearchEnabled");
            }
        }

        public bool IsSearchEnabled
        {
            get
            {
                return !string.IsNullOrEmpty(SearchText);
            }
        }

        public bool IsCaptureEnabled
        {
            get
            {
                return SelectedPerson != null;
            }
        }

        public bool IsSaveEnabled
        {
            get
            {
                return (SelectedPerson != null) && !string.IsNullOrEmpty(SelectedPerson.Template);
            }
        }

        public ICommand SearchCommand
        {
            get
            {
                if (_searchCommand == null)
                    _searchCommand = new RelayCommand(c => this.SearchEmployee());
                return _searchCommand;
            }
        }

        private void SearchEmployee()
        {
            Employees = SqlClient.GetEmployees(SearchText);
            SelectedPerson = null;
            OnPropertyChanged("IsCaptureEnabled");
            OnPropertyChanged("IsSaveEnabled");
            //MessageBox.Show("Search pressed");
        }

        public ICommand CaptureCommand
        {
            get
            {
                if (_captureCommand == null)
                    _captureCommand = new RelayCommand(c => this.CaptureTemplate());
                return _captureCommand;
            }
        }

        private void CaptureTemplate()
        {
            CaptureTemplateDialog dialog = new CaptureTemplateDialog();
            bool? dialogResult = dialog.ShowDialog();
            if (dialogResult.HasValue && dialogResult.Value)
            {
                
                if (SelectedPerson.Template == null)
                    SelectedPerson.Template = dialog.FingerPrint;
                else
                    SelectedPerson.Template = dialog.FingerPrint;
                base.OnPropertyChanged("SelectedPerson");
            }
            OnPropertyChanged("IsSaveEnabled");
            //MessageBox.Show("Capture pressed");
        }

        public ICommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                    _saveCommand = new RelayCommand(c => this.SaveTemplate());
                return _saveCommand;
            }
        }

        private void SaveTemplate()
        {
            bool result = SqlClient.SaveTemplate(SelectedPerson);
            if(result)
                MessageBox.Show(Resource.Resources.MessageSaveSuccessful);
            else
                MessageBox.Show(Resource.Resources.MessageSaveFailed);
        }
    }
}
