using System;
using OfficeOpenXml.DataValidation.Exceptions;

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
        /// <summary>
        /// 빅엔디안 방식
        /// [1]0x1234 << 16 = 0x12340000
        //  [1]0x12340000 | [2]0x5678 = 0x12345678
        //  3.	최종 결과: 0x12345678 (305,419,896)
        //

        public static int ToInt32Big(ushort[] registers)
        {
            if (registers.Length < 2)
                throw new ArgumentException("Int32 변환에는 2개의 레지스터가 필요합니다.");

            return (registers[0] << 16) | registers[1];
        }


        //리틀 엔디안 방식.
        public static int ToInt32Little(ushort[] registers)
        {
            if (registers.Length < 2)
                throw new ArgumentException("Int32 변환에는 2개의 레지스터가 필요합니다.");

            return (registers[1] << 16) | registers[0];
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
