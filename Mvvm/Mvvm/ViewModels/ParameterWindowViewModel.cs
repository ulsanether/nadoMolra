using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Threading.Tasks;
using NModbus;
using System.Windows;
using Prism.Commands;
using Prism.Mvvm;
using NModbus.Serial;
using System.Collections.ObjectModel;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;


namespace Mvvm.ViewModels
{
    class ParameterWindowViewModel:BindableBase
    {

     

       //버튼 숫자들 만큼 생성. 
        public IList<string> setPoint { get; set; }
        public IList<double> setDefaultValue { get; set; }
        public IList<double> actualValue { get; set; }
        public IList<double> proPosed { get; set; }

        private IModbusMaster master = null;



        #region  동적 생성 구현 부분

        private int _parametercount;

        public int ParameterCount
        {
            get { return _parametercount; }
            set {
               SetProperty(ref _parametercount, value);
                GenerateParameters();
            }
        }


        public ObservableCollection<ParameterModel> Parameters { get; set; } = new();

        public DelegateCommand ApplyCommand { get; }

        public ParameterWindowViewModel()
        {
            ApplyCommand = new DelegateCommand(ApplyChanges);
        }

        private void GenerateParameters() {

           Parameters.Clear();
            for (int i = 1; i <= ParameterCount; i++)
            {
                Parameters.Add(new ParameterModel { Label = $"Parameter {i}", DefaultActual = 100 });
            }

        }


        private void ApplyChanges()
        {
        MessageBox.Show("Applied");
        }


 
        


        #endregion




        private SerialPort port = null;
        public DelegateCommand TestFuncDel { get; set; }

        int _baudRate;
        int _dataBits;

        Parity _parity;
        StopBits _stopBits;
        int readTimeout;
        int writeTimeout;


        int ReadTimeout { get; set; }
        int WriteTimeout { get; set; }  


        public int baudRate 
        {
            get { return _baudRate; }
            set { SetProperty(ref _baudRate, value); }
        }

        public int dataBits
        {
            get { return _dataBits; }
            set { SetProperty(ref _dataBits, value); }
        }


        
        public async Task ConnectToPort(string portName)
        {
            try
            {
                if (port != null && port.IsOpen)
                {
                    port.Close();
                }
                port = new SerialPort(portName)
                {
                    BaudRate = baudRate,
                    DataBits = dataBits,
                    Parity = _parity,
                    StopBits = _stopBits,
                    ReadTimeout = readTimeout,
                    WriteTimeout = writeTimeout
                };

            

                await Task.Run(() =>
                {
                    try
                    {
                        port.Open();
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        throw new Exception($"포트 {portName}에 접근할 수 없습니다. (다른 프로그램에서 사용 중일 가능성 있음)\n{ex.Message}");
                    }
                    catch (IOException ex)
                    {
                        throw new Exception($"포트 {portName}를 열 수 없습니다. (존재하지 않거나 응답 없음)\n{ex.Message}");
                    }
                });

                var factory = new ModbusFactory();
                master = factory.CreateRtuMaster(port);
                master.Transport.ReadTimeout = 2000;
                master.Transport.WriteTimeout = 2000;

                MessageBox.Show("연결에 성공했습니다.", "정보", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"포트 연결 실패: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }






        void TestFunc() { }


    }
}
