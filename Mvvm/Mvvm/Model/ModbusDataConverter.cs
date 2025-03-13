using System;

namespace Mvvm.Model
{
    public static class ModbusDataConverter
    {
        public static float ToFloat(ushort[] registers)
        {
            if (registers.Length < 2)
                throw new ArgumentException("Float 변환에는 2개의 레지스터가 필요합니다.");

            byte[] bytes = new byte[4];
            bytes[0] = (byte)(registers[1] & 0xff);
            bytes[1] = (byte)(registers[1] >> 8);
            bytes[2] = (byte)(registers[0] & 0xff);
            bytes[3] = (byte)(registers[0] >> 8);

            return BitConverter.ToSingle(bytes, 0);
        }

        public static int ToInt32(ushort[] registers)
        {
            if (registers.Length < 2)
                throw new ArgumentException("Int32 변환에는 2개의 레지스터가 필요합니다.");

            return (registers[0] << 16) | registers[1];
        }

        public static short ToInt16(ushort register)
        {
            return (short)register;
        }

        public static ushort[] FromFloat(float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            return new ushort[]
            {
                (ushort)((bytes[3] << 8) | bytes[2]),
                (ushort)((bytes[1] << 8) | bytes[0])
            };
        }
    }

   
}
