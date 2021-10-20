using System;
using System.Collections.Generic;
using System.Text;

namespace VM
{
    class Program
    {
        static void Main(string[] args)
        {
            Assembler assembler = new Assembler(
@"
pushs 1
gstore 1

#main
gload 1
call #func 1 1
pushs 11
gload 1
inc
gstore 1
gload 1
eq
jmpf #main
halt

#func
aload 0
pushs 3
lesst
jmpt #end
aload 0
dec
call #func 1 1
aload 0
pushs 2
subt
call #func 1 1
add
ret

#end
pushs 1
ret
");
            var bytecode = assembler.Assemble();
            var bytecodeArray = bytecode.ToArray();

            StringBuilder hex = new StringBuilder(bytecode.Count);
            foreach (var b in bytecode)
                hex.AppendFormat("{0:x2}" + " ", b);

            Console.WriteLine("\nBytecode: " + hex.ToString() + "\n");

            Disassembler disassembler = new Disassembler(bytecodeArray);
            VM vm = new VM(bytecodeArray);

            disassembler.Dissasemble();
            vm.Run();

            Console.ReadKey();
        }
    }
}