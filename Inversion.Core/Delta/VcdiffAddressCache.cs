using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inversion.Delta
{
    internal class VcdiffAddressCache
    {
        private int _nearP;
        
        private long[] _near;
        private long[] _same;

        public VcdiffAddressCache(int nearTableLength, int sameTableLength)
        {
            _near = new long[nearTableLength];
            _same = new long[sameTableLength * 256];
        }

        public long GetNear(int index)
        {
            return _near[index];
        }

        public long GetSame(int table, byte offset)
        {
            return _same[(table * 256) + offset];
        }

        public void Update(long address)
        {
            if (_near.Length > 0)
            {
                _near[_nearP] = address;
                _nearP = (_nearP + 1) % _near.Length;
            }
            if (_same.Length > 0)
            {
                _same[address % (_same.Length * 256)] = address;
            }
        }
    }
}
