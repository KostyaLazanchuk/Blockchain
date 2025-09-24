using Blockchain.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blockchain.Core.Interfaces
{
    public interface IBlockRepository
    {
        IReadOnlyList<Block> GetChain();
        Block GetLast();
        void Append(Block block);
    }
}
