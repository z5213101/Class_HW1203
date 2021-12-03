using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading.Tasks;


namespace Unpacking
{
    class Program
    {
        private static byte[] m_PacketData;
        private static int length;
        private static byte[] m_Length;
        private static byte[] m_ID;
        private static uint m_Pos;
        private static byte[] m_ReadData;
        private static uint m_ReadPos;
        private static int m_Count;
        //private static string m_Separater;
        static void Main(string[] args)
        {
            m_PacketData = new byte[1024];
            m_Length = new byte[4];
            m_ID = new byte[4];
            m_Count = 0;
            length = 0;
            m_Pos = 0;
            Write(109);
            Write(109.99f);
            Write("Hello!");
            Console.Write($"Output Byte array(length:{m_Pos}): ");
            for (var i = 0; i < m_Pos; i++)
            {
                Console.Write(m_PacketData[i] + ", ");
            }
            //Start unpacking            
            Console.WriteLine("\n\nNow start reading......");
            m_ReadPos = 0;
            for (var i = 0; i < m_Count; i++)
            {
                Read(m_PacketData);
            }
            Console.ReadLine();
        }
        //Read one message at once
        static string Read(byte[] byteData)
        {
            string msg = "";
            int msgLength = GetLength(byteData);
            //Console.Write($"Message (length:{msgLength}): ");
            m_ReadData = new byte[msgLength];
            int ID = GetID(byteData);
            Array.ConstrainedCopy(byteData, (int)m_ReadPos, m_ReadData, 0, msgLength);
            _Read(m_ReadData, ID);
            //msg = ASCIIEncoding.ASCII.GetString(m_ReadData);
            m_ReadPos += (uint)msgLength;
            return msg;
        }
        static int GetLength(byte[] byteData)
        {
            int l = 0;
            int pos = (int)m_ReadPos;
            byte[] readLength = new byte[] { byteData[pos], byteData[pos + 1], byteData[pos + 2], byteData[pos + 3] };
            m_ReadPos += (uint)(4);   //讀了長度
            if (BitConverter.IsLittleEndian)
                Array.Reverse(readLength);
            l = BitConverter.ToInt32(readLength, 0);
            Console.WriteLine("length=" + l);
            return l;
        }
        static int GetID(byte[] byteData)
        {
            int ID = 0;
            int pos = (int)m_ReadPos;
            byte[] readID = new byte[] { byteData[pos], byteData[pos + 1], byteData[pos + 2], byteData[pos + 3] };
            m_ReadPos += (uint)(4);   //讀了ID
            if (BitConverter.IsLittleEndian)
                Array.Reverse(readID);
            ID = BitConverter.ToInt32(readID, 0);
            Console.WriteLine("ID=" + ID);
            return ID;
        }
        static void _Read(byte[] msg, int ID)
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(msg);
            switch (ID)
            {
                case 1:
                    int output_i = BitConverter.ToInt32(msg, 0);
                    Console.WriteLine("message = " + output_i);
                    break;
                case 2:
                    float output_f = BitConverter.ToSingle(msg, 0);
                    Console.WriteLine("message = " + output_f);
                    break;
                case 3:
                    string output_s = ASCIIEncoding.Unicode.GetString(msg);
                    Console.WriteLine("message = " + output_s);
                    break;
                default:
                    Console.WriteLine("ID Error");
                    break;
            }
        }

        //以下為老師範例的修改
        private static bool Write(int i)
        {
            int ID = 1;
            var bytes = BitConverter.GetBytes(i);
            _Write(bytes, ID);
            return true;
        }

        // write a float into a byte array
        private static bool Write(float f)
        {
            int ID = 2;
            var bytes = BitConverter.GetBytes(f);
            _Write(bytes, ID);
            return true;
        }

        // write a string into a byte array
        private static bool Write(string s)
        {
            int ID = 3;
            var bytes = Encoding.Unicode.GetBytes(s);
            // write byte array length to packet's byte array
            //if (Write(bytes.Length) == false)
            //{
            //    return false;
            //}

            _Write(bytes, ID);
            return true;
        }

        // write a byte array into packet's byte array
        private static void _Write(byte[] byteData, int ID)
        {
            // converter little-endian to network's big-endian
            //Console.WriteLine(ID);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(byteData);
            }
            LengthRecorder(byteData, ID);
            byteData.CopyTo(m_PacketData, m_Pos);
            m_Pos += (uint)byteData.Length;
            m_Count += 1;
        }
        //Calculate mes length and record(int is 4 byte)
        private static void LengthRecorder(byte[] mes, int ID)
        {
            length = mes.Length;
            m_Length = BitConverter.GetBytes(length);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(m_Length);
            m_Length.CopyTo(m_PacketData, m_Pos);
            m_Pos += (uint)4;
            //Console.WriteLine($"This message length is{length}");

            //Also record messageID
            m_ID = BitConverter.GetBytes(ID);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(m_ID);
            m_ID.CopyTo(m_PacketData, m_Pos);
            m_Pos += (uint)4;
        }
    }
}
