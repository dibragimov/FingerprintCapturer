using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Suprema;
using System.ComponentModel;
using System.Windows.Threading;
using System.Threading;

namespace HorioFingerprintCapturer
{
    /// <summary>
    /// Interaction logic for CaptureTemplateDialog.xaml
    /// </summary>
    public partial class CaptureTemplateDialog : Window
    {
        private readonly UFScannerManager _scannerManager;
        private readonly ISynchronizeInvoke _mInvoker;

        private string _template = string.Empty;
        private int _templateSize = 384;
        private const int TemplateBufferSize = 384;

        public CaptureTemplateDialog()
        {
            InitializeComponent();
            _mInvoker = new DispatcherWinFormsCompatAdapter(this.Dispatcher);

            _scannerManager = new UFScannerManager(_mInvoker);
            _scannerManager.ScannerEvent += ScannerEvent;
            InitializeScanners();
            //BindTemplate();
            Title = string.Format(Resource.Resources.Window_TitleFormat, Resource.Resources.Title_CaptureTemplate);

        }

        //void BindTemplate()
        //{
            
        //}

        public void ScannerEvent(object sender, UFScannerManagerScannerEventArgs e)
        {
            btnCapture.IsEnabled = UpdateScannersList() > 0;
        }

        private void InitializeScanners()
        {
            try
            {

                WriteLog("Initializing scanners");

                UFS_STATUS ufsRes = _scannerManager.Init();

                if (ufsRes == UFS_STATUS.OK)
                {

                }
                else
                {
                    string error = string.Empty;
                    UFScanner.GetErrorString(ufsRes, out error);
                    WriteLog(error, false);
                    //init failed probably problem with license
                    btnCapture.IsEnabled = false;
                    return;
                }

                btnCapture.IsEnabled = UpdateScannersList() > 0;
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message, true);
            }
        }

        int UpdateScannersList()
        {
            int nScannerNumber = _scannerManager.Scanners.Count;
            lstScanners.Items.Clear();
            if (nScannerNumber > 0)
            {
                WriteLog(false, "Scanners found {0}", nScannerNumber.ToString());


                for (int i = 0; i < nScannerNumber; i++)
                {
                    UFScanner scanner = _scannerManager.Scanners[i];
                    scanner.DetectCore = true;
                    scanner.UseSIF = false;
                    scanner.TemplateSize = 384;



                    lstScanners.Items.Add(string.Format("{0} : {1}", i + 1, GetScannerTypeString(scanner.ScannerType)));
                }
            }
            else
            {
                WriteLog(false, "No available scanners");
            }
            return nScannerNumber;
        }

        void WriteLog(bool isError, string format, params object[] args)
        {
            WriteLog(string.Format(format, args), isError);
        }

        void WriteLog(string message)
        {
            WriteLog(message, false);
        }

        void WriteLog(string message, bool isError)
        {
            message = string.Format("{0}\r\n", message);
            if (isError)
                message = "ERROR: " + message;
            txtLogText.AppendText(message);
        }

        private static string GetScannerTypeString(UFS_SCANNER_TYPE scannerType)
        {
            switch (scannerType)
            {
                case UFS_SCANNER_TYPE.SFR200:
                    return "SFR200";
                case UFS_SCANNER_TYPE.SFR300:
                    return "SFR300";
                case UFS_SCANNER_TYPE.SFR300v2:
                    return "SFR300v2";
                default:
                    return "Unknown scanner type";
            }
        }

        UFScanner GetSelectedScanner()
        {
            try
            {
                if (lstScanners.SelectedIndex >= 0)
                {
                    UFScanner scanner = _scannerManager.Scanners[lstScanners.SelectedIndex];
                    if (scanner == null)
                    {
                        WriteLog("Please check that selected scanner is connected", true);
                    }
                    else
                        return scanner;
                }
                else
                {
                    WriteLog("Select scanner and try again");
                }
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message, true);
            }
            return null;
        }

        private void ClearLog(object sender, RoutedEventArgs e)
        {
            txtLogText.Clear();
        }

        private void RefreshScanners(object sender, RoutedEventArgs e)
        {
            btnCapture.IsEnabled = UpdateScannersList() > 0;
        }

        private void CaptureTemplate(object sender, RoutedEventArgs e)
        {
            try
            {
                UFScanner scanner = GetSelectedScanner();
                if (scanner == null)
                    return;

                this.Cursor = Cursors.Wait;
                scanner.ClearCaptureImageBuffer();

                UFS_STATUS ufsRes = scanner.CaptureSingleImage();


                if (ufsRes == UFS_STATUS.OK)
                {
                    UFS_STATUS capturingStatus = ExtractTemplate(scanner);
                }
                else
                {
                    string error = string.Empty;
                    UFScanner.GetErrorString(ufsRes, out error);
                    WriteLog(error, false);
                }

            }
            catch (Exception exc)
            {
                WriteLog(exc.Message, true);
            }
            this.Cursor = Cursors.Arrow;
        }

        private UFS_STATUS ExtractTemplate(UFScanner scanner)
        {
            try
            {
                byte[] template = new byte[TemplateBufferSize];
                int enrollQuality = 0;


                UFS_STATUS ufsRes = scanner.Extract(template, out _templateSize, out enrollQuality);

                if (ufsRes == UFS_STATUS.OK)
                {
                    if (template.Length != TemplateBufferSize)
                    {
                        //WriteLog(true, "Invalid template size, try again");
                        //return UFS_STATUS.ERR_NOT_GOOD_IMAGE;
                        byte[] tmp = new byte[TemplateBufferSize];
                        for (int i = template.Length - 1; i < tmp.Length; i++)
                        {
                            tmp[i] = 0;
                        }
                        template = tmp;
                    }


                    WriteLog("Template capturing completed");
                    WriteLog(string.Format("Template quality is {0}", enrollQuality));

                    if (enrollQuality < Settings.MinimalTemplateQualityPercent)
                    {
                        MessageBoxResult result = MessageBox.Show("The quality of captured template is too low for using it, Would you like to retry template capturing?",
                            "Poor template quality", MessageBoxButton.YesNo, MessageBoxImage.Information);
                        if (result == MessageBoxResult.Yes)
                        {
                            CaptureTemplate(this, null);
                        }
                    }
                    else
                    {
                        _template = Encoding.ASCII.GetString(TemplateConverter.ConvertToSynelTemplateFormat(template));

                        System.Drawing.Bitmap bitm = null;
                        int resolution = 0;
                        scanner.GetCaptureImageBuffer(out bitm, out resolution);
                        fingerImage.Source = loadBitmap(bitm);                        
                    }
                }
                else
                {
                    string error = string.Empty;
                    UFScanner.GetErrorString(ufsRes, out error);
                    WriteLog(error, false);
                }

                return ufsRes;
            }
            catch (Exception exc)
            {
                WriteLog(exc.Message, true);
            }
            return UFS_STATUS.ERROR;
        }

        public static BitmapSource loadBitmap(System.Drawing.Bitmap source)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(source.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_template))
            {
                DialogResult = true;
                return;
            }
            DialogResult = false;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _scannerManager.ScannerEvent -= ScannerEvent;
            _scannerManager.Uninit();
        }

        public string FingerPrint
        {
            get { return _template; }
        }
    }

    internal class DispatcherWinFormsCompatAdapter : ISynchronizeInvoke
    {
        #region IAsyncResult implementation
        private class DispatcherAsyncResultAdapter : IAsyncResult
        {
            private DispatcherOperation m_op;
            private object m_state;

            public DispatcherAsyncResultAdapter(DispatcherOperation operation)
            {
                m_op = operation;
            }

            public DispatcherAsyncResultAdapter(DispatcherOperation operation, object state)
                : this(operation)
            {
                m_state = state;
            }

            public DispatcherOperation Operation
            {
                get { return m_op; }
            }

            #region IAsyncResult Members

            public object AsyncState
            {
                get { return m_state; }
            }

            public WaitHandle AsyncWaitHandle
            {
                get { return null; }
            }

            public bool CompletedSynchronously
            {
                get { return false; }
            }

            public bool IsCompleted
            {
                get { return m_op.Status == DispatcherOperationStatus.Completed; }
            }

            #endregion
        }
        #endregion
        private Dispatcher m_disp;
        public DispatcherWinFormsCompatAdapter(Dispatcher dispatcher)
        {
            m_disp = dispatcher;
        }
        #region ISynchronizeInvoke Members

        public IAsyncResult BeginInvoke(Delegate method, object[] args)
        {
            if (args != null && args.Length > 1)
            {
                object[] argsSansFirst = GetArgsAfterFirst(args);
                DispatcherOperation op = m_disp.BeginInvoke(DispatcherPriority.Normal, method, args[0], argsSansFirst);
                return new DispatcherAsyncResultAdapter(op);
            }
            else
            {
                if (args != null)
                {
                    return new DispatcherAsyncResultAdapter(m_disp.BeginInvoke(DispatcherPriority.Normal, method, args[0]));
                }
                else
                {
                    return new DispatcherAsyncResultAdapter(m_disp.BeginInvoke(DispatcherPriority.Normal, method));
                }
            }
        }

        private static object[] GetArgsAfterFirst(object[] args)
        {
            object[] result = new object[args.Length - 1];
            Array.Copy(args, 1, result, 0, args.Length - 1);
            return result;
        }

        public object EndInvoke(IAsyncResult result)
        {
            DispatcherAsyncResultAdapter res = result as DispatcherAsyncResultAdapter;
            if (res == null)
                throw new InvalidCastException();

            while (res.Operation.Status != DispatcherOperationStatus.Completed || res.Operation.Status == DispatcherOperationStatus.Aborted)
            {
                Thread.Sleep(50);
            }

            return res.Operation.Result;
        }

        public object Invoke(Delegate method, object[] args)
        {
            if (args != null && args.Length > 1)
            {
                object[] argsSansFirst = GetArgsAfterFirst(args);
                return m_disp.Invoke(DispatcherPriority.Normal, method, args[0], argsSansFirst);
            }
            else
            {
                if (args != null)
                {
                    return m_disp.Invoke(DispatcherPriority.Normal, method, args[0]);
                }
                else
                {
                    return m_disp.Invoke(DispatcherPriority.Normal, method);
                }
            }
        }

        public bool InvokeRequired
        {
            get { return m_disp.Thread != Thread.CurrentThread; }
        }

        #endregion
    }
}
