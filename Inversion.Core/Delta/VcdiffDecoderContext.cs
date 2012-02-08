using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inversion.Utils;

namespace Inversion.Delta
{
    internal class VcdiffDecoderContext
    {
        private VcdiffDecoderInstruction _next;

        public Stream Data { get; set; }
        public BinaryReader Instructions { get; set; }
        public BinaryReader Addresses { get; set; }
        public VcdiffCodeTable CodeTable { get; set; }
        
        public VcdiffDecoderInstruction GetNextOperation()
        {
            if (Instructions.BaseStream.Position >= Instructions.BaseStream.Length)
            {
                return null;
            }

            VcdiffDecoderInstruction op;
            if (_next != null)
            {
                op = _next;
                _next = null;
            }
            else
            {
                byte index = Instructions.ReadByte();
                VcdiffCodeTableEntry inst = CodeTable[index];
                op = ReadSingleInstruction(inst.Operation1, inst.Size1, inst.Mode1);
                if (inst.Operation2 != VcdiffOperation.NoOp)
                {
                    _next = ReadSingleInstruction(inst.Operation2, inst.Size2, inst.Mode2);
                }
            }
            return op;
        }

        private VcdiffDecoderInstruction ReadSingleInstruction(VcdiffOperation op, byte size, VcdiffCopyMode mode)
        {
            long actualSize;
            if (size == 0)
            {
                actualSize = Instructions.ReadVarInteger();
            }
            else
            {
                actualSize = size;
            }

            return new VcdiffDecoderInstruction()
            {
                Context = this,
                Operation = op,
                Size = actualSize,
                CopyMode = mode
            };
        }
    }
}
