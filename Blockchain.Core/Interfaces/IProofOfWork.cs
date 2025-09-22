using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blockchain.Core.Interfaces
{
    public interface IProofOfWork
    {
        int FindProof(int lastProof);
        bool IsValid(int lastProof, int proof);
    }
}
