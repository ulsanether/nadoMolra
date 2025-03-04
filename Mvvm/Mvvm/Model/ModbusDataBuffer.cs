using System;
using System.Collections.Generic;
using System.Linq;
using Mvvm.ViewModels;

namespace Mvvm.Model
{
    public class ModbusDataBuffer
    {
        private readonly int bufferSize = 1000;
        private readonly Dictionary<int, Queue<DataPoint>> dataBuffer = new Dictionary<int, Queue<DataPoint>>();
        private readonly object lockObject = new object();

        public void StoreValues(List<ParameterModel> parameters)
        {
            lock (lockObject)
            {
                foreach (var param in parameters)
                {
                    if (!dataBuffer.ContainsKey(param.Address))
                    {
                        dataBuffer[param.Address] = new Queue<DataPoint>();
                    }

                    var queue = dataBuffer[param.Address];
                    queue.Enqueue(new DataPoint(DateTime.Now, param.DefaultActual));

                    while (queue.Count > bufferSize)
                    {
                        queue.Dequeue();
                    }
                }
            }
        }

        public List<ParameterModel> GetLastValues(int count)
        {
            lock (lockObject)
            {
                return dataBuffer.Select(kvp => new ParameterModel
                {
                    Address = kvp.Key,
                    Label = $"Register {kvp.Key}",
                    DefaultActual = kvp.Value.Any() ? kvp.Value.Last().Value : 0,
                    DefaultValue = (kvp.Value.Any() ? kvp.Value.Last().Value : 0).ToString(),
                    ModbusUnit = "Raw"
                }).ToList();
            }
        }


        public DataPoint[] GetHistoricalData(int address, TimeSpan timeSpan)
        {
            lock (lockObject)
            {
                if (!dataBuffer.ContainsKey(address))
                    return Array.Empty<DataPoint>();

                var cutoffTime = DateTime.Now - timeSpan;
                return dataBuffer[address]
                    .Where(dp => dp.Timestamp >= cutoffTime)
                    .ToArray();
            }
        }
    }

    public struct DataPoint
    {
        public DateTime Timestamp { get; }
        public double Value { get; }

        public DataPoint(DateTime timestamp, double value)
        {
            Timestamp = timestamp;
            Value = value;
        }
    }
}
