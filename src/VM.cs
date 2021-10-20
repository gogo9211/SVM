using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VM
{
    class callStack
    {
        public int pc;
        public int sp;
        public int ret;
        public int args;

        public callStack(int pc, int sp, int ret, int args)
        {
            this.pc = pc;
            this.sp = sp;
            this.ret = ret;
            this.args = args;
        }
    }

    class VM
    {
        private byte[] bytecode;

        private int pc = 0;
        private int sp = -1;
        private int fp = -1;

        private int[] stack = new int[100];
        private int[] memory = new int[100];
        private callStack[] callStack = new callStack[100];

        public VM(byte[] bytecode)
        {
            byte[] code = new byte[bytecode.Length - 9];

            Array.Copy(bytecode, 9, code, 0, code.Length);

            this.bytecode = code;
        }

        private int GetInt8(int index)
        {
            int b0 = bytecode[index];

            ++index;

            return b0;
        }

        private int GetInt32(int index)
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

        private void stack_push(int val)
        {
            stack[++sp] = val;
        }

        private int stack_pop()
        {
            return stack[sp--];
        }

        public void Run()
        {
            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();

            while (true)
            {
                var opcode = bytecode[pc];

                ++pc;

                switch (opcode)
                {
                    case 1: //pushi
                        {
                            stack_push(GetInt32(pc));

                            pc += sizeof(int);

                            break;
                        }

                    case 2: //pushs
                        {
                            stack_push(GetInt16(pc));

                            pc += sizeof(short);

                            break;
                        }


                    case 3: //add
                        {
                            var val0 = stack_pop();
                            var val1 = stack_pop();

                            stack_push(val1 + val0);

                            break;
                        }

                    case 4: //subt
                        {
                            var val0 = stack_pop();
                            var val1 = stack_pop();

                            stack_push(val1 - val0);

                            break;
                        }

                    case 5: //mult
                        {
                            var val0 = stack_pop();
                            var val1 = stack_pop();

                            stack_push(val1 * val0);

                            break;
                        }

                    case 6: //div
                        {
                            var val0 = stack_pop();
                            var val1 = stack_pop();

                            stack_push(val1 / val0);

                            break;
                        }

                    case 7: //xor
                        {
                            var val0 = stack_pop();
                            var val1 = stack_pop();

                            stack_push(val1 ^ val0);

                            break;
                        }

                    case 8: //inc
                        {
                            stack[sp] = stack[sp] + 1;

                            break;
                        }

                    case 9: //dec
                        {
                            stack[sp] = stack[sp] - 1;

                            break;
                        }

                    case 13: //swap
                        {
                            var val0 = stack_pop();
                            var val1 = stack_pop();

                            stack_push(val0);
                            stack_push(val1);

                            break;
                        }

                    case 14: //jmp
                        {
                            pc = GetInt16(pc);

                            break;
                        }

                    case 15: //jmpt
                        {
                            var val0 = stack_pop();

                            if (val0 == 1)
                                pc = GetInt16(pc);
                            else
                                pc += sizeof(short);

                            break;
                        }

                    case 16: //jmpf
                        {
                            var val0 = stack_pop();

                            if (val0 == 0)
                                pc = GetInt16(pc);
                            else
                                pc += sizeof(short);

                            break;
                        }


                    case 20: //eq
                        {
                            var val0 = stack_pop();
                            var val1 = stack_pop();

                            if (val1 == val0)
                                stack_push(1);
                            else
                                stack_push(0);

                            break;
                        }

                    case 21: //lesst
                        {
                            var val0 = stack_pop();
                            var val1 = stack_pop();

                            if (val1 < val0)
                                stack_push(1);
                            else
                                stack_push(0);

                            break;
                        }

                    case 22: //printin
                        {
                            Console.WriteLine(stack[sp]);

                            break;
                        }

                    case 17: //gstore
                        {
                            var val0 = stack_pop();
                            var loc = GetInt16(pc);

                            memory[loc] = val0;

                            pc += sizeof(short);

                            break;
                        }

                    case 18: //gload
                        {
                            var loc = GetInt16(pc);

                            stack_push(memory[loc]);

                            pc += sizeof(short);

                            break;
                        }

                    case 12: //call
                        {
                            var loc = GetInt16(pc);
                            var ret = GetInt16(pc + sizeof(short));
                            var args = GetInt16(pc + sizeof(short) * 2);

                            callStack[++fp] = new callStack(pc + sizeof(short) * 3, sp, ret, args);

                            pc = loc;

                            break;
                        }

                    case 11: //ret
                        {
                            var frame = callStack[fp];

                            if (frame.ret == 1)
                            {
                                var retVal = stack_pop();

                                sp = frame.sp;

                                for (var i = 0; i < frame.args; ++i)
                                    stack_pop();

                                stack_push(retVal);
                            }
                            else
                            {
                                sp = frame.sp;

                                for (var i = 0; i < frame.args; ++i)
                                    stack_pop();
                            }

                            --fp;

                            pc = frame.pc;

                            break;
                        }

                    case 19: //aload
                        {
                            var loc = GetInt16(pc);
                            var beforeArgs = callStack[fp].sp - callStack[fp].args;

                            stack_push(stack[beforeArgs + 1 + loc]);

                            pc += sizeof(short);

                            break;
                        }

                    case 10: //halt
                        {
                            stopwatch.Stop();

                            Console.WriteLine("VM Return! Execution Time: {0} ms\n", stopwatch.Elapsed.TotalMilliseconds);

                            for (var i = 0; i <= sp && sp != -1; ++i)
                                Console.WriteLine("stack[{0}] = {1}", i, stack[i]);

                            return;
                        }

                    default:
                        {
                            Console.WriteLine("Unsupported Opcode. Aborting!");

                            return;
                        }
                }
            }
        }
    }
}