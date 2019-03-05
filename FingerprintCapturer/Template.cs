using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HorioFingerprintCapturer
{
    public class Template
    {
        #region Fileds
        private int _id;
        private string _fingerTemplate;
        //private TemplateStatus _status;
        private string _templateID;
        #endregion

        #region Properties
        public int ID
        {
            get { return _id; }
            set { _id = value; }
        }

        public string TemplateID
        {
            get { return _templateID; }
            set
            {
                if (value == _templateID) return;
                _templateID = value;
            }
        }

        public string FingerTemplate
        {
            get { return _fingerTemplate; }
            set { _fingerTemplate = value; }
        }

        //public EmployeeModel Employee
        //{
        //    get { return _employee; }
        //    set
        //    {
        //        if (value != _employee)
        //        {
        //            _employee = value;
        //            TemplateID = _employee.EmpId;
        //            NotifyPropertyChanged("Employee");
        //        }
        //    }
        //}

        //public TemplateStatus Status
        //{
        //    get { return _status; }
        //    set
        //    {
        //        if (value != _status)
        //        {
        //            _status = value;
        //            NotifyPropertyChanged("Status");
        //        }
        //    }
        //}
        #endregion        
    }
}
