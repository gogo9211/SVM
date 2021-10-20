using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VM
{
    class Disassembler
    {
        private int index;
        private byte[] bytecode;

        public Disassembler(byte[] bytecode)
        {
            this.bytecode = bytecode;
        }

        private int GetInt8()
        {
            int b0 = bytecode[index];

            ++index;

            return b0;
        }

        private int GetInt32()
        {
            int res = BitConverter.ToInt32(bytecode, index);

            index += sizeof(int);

            return res;
        }

        private int GetInt16(int index)
        {
            int res = BitConverter.ToInt16(bytecode, index);

            index += sizeof(short);

            return res;
        }

        private string GetString(int strLen)
        {
            char[] chars = new char[strLen];

            for (var i = 0; i < strLen; ++i)
                chars[i] = Convert.ToChar(bytecode[index + i]);

            string str = new string(chars);

            index += strLen;

            return str;
        }

        public void Dissasemble()
        {
            var version = GetInt8();

            var magic = GetString(4);

            var sizeOfCode = GetInt32();

            Console.WriteLine("---Disassembly---");

            Console.WriteLine("String: {0} | Version: {1} | Size of Code: {2}\n", magic, version, sizeOfCode);
        }
    }
}