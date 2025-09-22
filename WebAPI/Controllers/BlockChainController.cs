using Blockchain.Core.Infrastructure;
using Blockchain.Core.Models;
using Blockchain.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BlockchainController : ControllerBase
    {
        private readonly BlockchainService _blockchainService;
        private static List<Transaction> mempool = new List<Transaction>();
        private static string nodeIdentifier = Guid.NewGuid().ToString().Replace("-", "");

        public BlockchainController(BlockchainService blockchainService)
        {
            _blockchainService = blockchainService;
        }

        [HttpPost("transactions/new")]
        public IActionResult NewTransaction([FromBody] Transaction transaction)
        {
            if (transaction == null || string.IsNullOrEmpty(transaction.Sender) ||
                string.IsNullOrEmpty(transaction.Recipient) || transaction.Amount <= 0)
            {
                return BadRequest("Missing values");
            }

            var index = _blockchainService.AddTransaction(transaction.Sender, transaction.Recipient, transaction.Amount, mempool);
            return Ok(new { message = $"Transaction will be added to Block {index}" });
        }

        [HttpGet("mine")]
        public IActionResult Mine()
        {
            var proof = _blockchainService.MineNextProof();

            _blockchainService.AddTransaction("0", nodeIdentifier, 1, mempool);

            var block = _blockchainService.CreateBlock(proof, mempool);

            return Ok(new
            {
                message = "New Block Forged",
                index = block.Index,
                transactions = block.Transactions,
                proof = block.Proof,
                previous_hash = block.PreviousHash
            });
        }

        [HttpGet("chain")]
        public IActionResult FullChain()
        {
            var chain = _blockchainService.Chain;
            return Ok(new
            {
                chain,
                length = chain.Count
            });
        }
    }
}

