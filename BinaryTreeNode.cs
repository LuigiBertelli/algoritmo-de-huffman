using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuffmanComp
{
    public class BinaryTreeNode
    {
        public BinaryTreeNode? Left = null;
        public BinaryTreeNode? Right = null;
        public int Freq { get; private set; }
        public byte? Val { get; private set; }

        public BinaryTreeNode(int freq, byte? val = null) 
        {
            this.Freq = freq;
            this.Val = val;
        }
    }

}
