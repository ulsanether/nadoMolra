using System;
using Prism.Mvvm;

namespace Mvvm.ViewModels
{
    public class CommunicationStatistics : BindableBase
    {
        private double _successRate;
        private int _errorCount;
        private DateTime _lastSuccessfulRead;
        private DateTime _lastError;
        private double _reconnectSuccessRate;
        private int _totalReads;
        private int _successfulReads;
        private int _reconnectAttempts;
        private int _successfulReconnects;

        public double SuccessRate
        {
            get => _successRate;
            set => SetProperty(ref _successRate, value);
        }

        public int ErrorCount
        {
            get => _errorCount;
            set => SetProperty(ref _errorCount, value);
        }

        public DateTime LastSuccessfulRead
        {
            get => _lastSuccessfulRead;
            set => SetProperty(ref _lastSuccessfulRead, value);
        }

        public DateTime LastError
        {
            get => _lastError;
            set => SetProperty(ref _lastError, value);
        }

        public double ReconnectSuccessRate
        {
            get => _reconnectSuccessRate;
            set => SetProperty(ref _reconnectSuccessRate, value);
        }

        public int TotalReads
        {
            get => _totalReads;
            set => SetProperty(ref _totalReads, value);
        }

        public int SuccessfulReads
        {
            get => _successfulReads;
            set => SetProperty(ref _successfulReads, value);
        }

        public int ReconnectAttempts
        {
            get => _reconnectAttempts;
            set => SetProperty(ref _reconnectAttempts, value);
        }

        public int SuccessfulReconnects
        {
            get => _successfulReconnects;
            set => SetProperty(ref _successfulReconnects, value);
        }
    }
}
