using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VM
{
    class Assembler
    {
        struct Label
        {
            public string labelName;
            public int codeIndex;

            public Label(string name, int index)
            {
                labelName = name;
                codeIndex = index;
            }
        }

        struct RelocInst
        {
            public string jmpLabelName;
            public int jmpLocation;
            public int instVal;

            public RelocInst(string name, int index, int val)
            {
                jmpLabelName = name;
                jmpLocation = index;
                instVal = val;
            }
        }

        class Opcode
        {
            public string opcodeName;
            public byte size;
            public byte opcodeVal;
            public byte opA;

            public Opcode(string name, byte val, byte opA, byte size)
            {
                opcodeName = name;
                opcodeVal = val;
                this.opA = opA;
                this.size = size;
            }
        }

        private string asm;
        private List<byte> stream;
        private List<Label> labels;
        private List<RelocInst> relocList;

        List<Opcode> opcodes = new List<Opcode>()
        {
            new Opcode("pushi", 1, 1, 4),
            new Opcode("pushs", 2, 1, 2),
            new Opcode("add", 3, 0, 0),
            new Opcode("subt", 4, 0, 0),
            new Opcode("mult", 5, 0, 0),
            new Opcode("div", 6, 0, 0),
            new Opcode("xor", 7, 0, 0),
            new Opcode("inc", 8, 0, 0),
            new Opcode("dec", 9, 0, 0),
            new Opcode("halt", 10, 0, 0),
            new Opcode("ret", 11, 0, 0),
            new Opcode("call", 12, 3, 2),
            new Opcode("swap", 13, 0, 0),
            new Opcode("jmp", 14, 1, 2),
            new Opcode("jmpt", 15, 1, 2),
            new Opcode("jmpf", 16, 1, 2),
            new Opcode("gstore", 17, 1, 2),
            new Opcode("gload", 18, 1, 2),
            new Opcode("aload", 19, 1, 2),
            new Opcode("eq", 20, 0, 0),
            new Opcode("lesst", 21, 0, 0),
            new Opcode("printin", 22, 0, 0),
        };

        private void WriteBytes(byte[] input)
        {
            foreach (byte b in input)
                stream.Add(b);
        }

        private void Write(byte input)
        {
            stream.Add(input);
        }

        private void Write(Int16 input)
        {
            byte[] inputBytes = BitConverter.GetBytes(input);

            WriteBytes(inputBytes);
        }

        private void Write(int input)
        {
            byte[] inputBytes = BitConverter.GetBytes(input);

            WriteBytes(inputBytes);
        }

        public void Write(string input)
        {
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);

            WriteBytes(inputBytes);
        }

        public Assembler(string asm)
        {
            stream = new List<byte>();
            labels = new List<Label>();
            relocList = new List<RelocInst>();
            this.asm = asm;
        }

        public List<byte> Assemble()
        {
            Write("\x01gasm");

            var startOfCode = stream.Count;

            foreach (var str in asm.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (str.Substring(0, 1) == "#")
                {
                    var labelName = str.Substring(1, str.Length - 1);

                    labels.Add(new Label(labelName, stream.Count - startOfCode));

                    continue;
                }

                var op = str.Split(' ');

                var opcode = opcodes.Find(item => item.opcodeName == op[0]);

                if (opcode == null)
                {
                    Console.WriteLine("Invalid Instruction: {0}", op[0]);

                    return stream;
                }

                if (opcode.opA == 0)
                   Write(opcode.opcodeVal);
                else
                {
                    Write(opcode.opcodeVal);

                    if (opcode.opcodeName == "call" || opcode.opcodeName == "jmp" || opcode.opcodeName == "jmpt" || opcode.opcodeName == "jmpf")
                    {
                        var labelName = op[1].Substring(1, op[1].Length - 1);

                        op[1] = "0";

                        relocList.Add(new RelocInst(labelName, stream.Count - startOfCode, opcode.opcodeVal));
                    }

                    if (opcode.size == sizeof(int))
                        for (var i = 1; i <= opcode.opA; ++i)
                            Write(int.Parse(op[i]));
                    else if (opcode.size == sizeof(short))
                        for (var i = 1; i <= opcode.opA; ++i)
                            Write(short.Parse(op[i]));
                }
            }

            foreach (var reloc in relocList)
            {
                var label = labels.Find(item => item.labelName == reloc.jmpLabelName);

                if (label.codeIndex < startOfCode)
                {
                    Console.WriteLine("Invalid Label: '{0}'", reloc.jmpLabelName);

                    return stream;
                }

                byte[] jmpBytes = BitConverter.GetBytes((short)label.codeIndex);

                for (var i = 0; i < jmpBytes.Length; ++i)
                    stream[startOfCode + reloc.jmpLocation + i] = jmpBytes[i];
            }

            byte[] sizeOfCode = BitConverter.GetBytes(stream.Count - startOfCode);

            for (var i = 0; i < sizeOfCode.Length; ++i)
                stream.Insert(startOfCode + i, sizeOfCode[i]);

            foreach (var label in labels)
                Console.WriteLine("Label: '{0}' at: {1}", label.labelName, label.codeIndex);

            foreach (var reloc in relocList)
                Console.WriteLine("Instruction at: {0} jmps to label: '{1}'", reloc.jmpLocation - 1, reloc.jmpLabelName);

            return stream;
        }
    }
}