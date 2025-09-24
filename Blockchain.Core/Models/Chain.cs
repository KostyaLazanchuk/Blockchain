namespace Blockchain.Core.Models
{
    public sealed class Chain
    {
        public List<Block>? chain { get; set; }
        public int length { get; set; }
    }
}
