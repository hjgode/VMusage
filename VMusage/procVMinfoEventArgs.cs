using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace VMusage
{
    public class procVMinfoEventArgs : EventArgs
    {
        //fields
        List<VMusage.procVMinfo> _procMemList;
        public List<VMusage.procVMinfo> procVMlist
        {
            get { return _procMemList; }
        }

        uint _totalMemoryInUse = 0;
        public uint totalMemoryInUse
        {
            get { return _totalMemoryInUse; }
        }
        public procVMinfoEventArgs(List<VMusage.procVMinfo> _list, uint totalMemUse)
        {
            _procMemList = _list;
            _totalMemoryInUse = totalMemUse;
        }
    }
}
