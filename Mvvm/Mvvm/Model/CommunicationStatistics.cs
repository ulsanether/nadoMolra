using System;

namespace Mvvm.Model
{
    public class CommunicationStatistics
    {
        public int TotalReads { get; private set; }
        public int SuccessfulReads { get; private set; }
        public int ErrorCount { get; private set; }
        public int ReconnectAttempts { get; private set; }
        public int SuccessfulReconnects { get; private set; }
        public DateTime LastSuccessfulRead { get; private set; }
        public DateTime LastError { get; private set; }

        public double SuccessRate => TotalReads == 0 ? 0 : (double)SuccessfulReads / TotalReads * 100;
        public double ReconnectSuccessRate => ReconnectAttempts == 0 ? 0 : (double)SuccessfulReconnects / ReconnectAttempts * 100;

        public void RecordSuccessfulRead()
        {
            TotalReads++;
            SuccessfulReads++;
            LastSuccessfulRead = DateTime.Now;
        }

        public void RecordError()
        {
            TotalReads++;
            ErrorCount++;
            LastError = DateTime.Now;
        }

        public void RecordReconnectAttempt()
        {
            ReconnectAttempts++;
        }

        public void RecordReconnectSuccess()
        {
            SuccessfulReconnects++;
        }

        public void RecordReconnectFailure()
        {
            // 재연결 실패 통계 기록
        }

        public void Reset()
        {
            TotalReads = 0;
            SuccessfulReads = 0;
            ErrorCount = 0;
            ReconnectAttempts = 0;
            SuccessfulReconnects = 0;
        }
    }
}
