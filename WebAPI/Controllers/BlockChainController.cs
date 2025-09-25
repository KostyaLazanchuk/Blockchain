using Blockchain.Core.Models;
using Blockchain.Core.Services;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Requests;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BlockchainController : ControllerBase
    {
        private readonly BlockchainService _blockchainService;
        private readonly IHttpClientFactory _httpClientFactory;

        private static string nodeIdentifier = Guid.NewGuid().ToString().Replace("-", "");
        private static List<Transaction> mempool = new List<Transaction>();

        public BlockchainController(BlockchainService blockchainService, IHttpClientFactory httpClientFactory)
        {
            _blockchainService = blockchainService;
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost("transactions/new")]
        public IActionResult NewTransaction([FromBody] Transaction transaction)
        {
            if (transaction == null || string.IsNullOrEmpty(transaction.Sender) ||
                string.IsNullOrEmpty(transaction.Recipient) || transaction.Amount <= 0)
            {
                return BadRequest("Missing values");
            }

            var index = _blockchainService.AddTransaction(transaction.Sender, transaction.Recipient, 
                transaction.Amount, mempool);
            return Ok(new { message = $"Transaction will be added to Block {index}" });
        }

        [HttpGet("mine")]
        public IActionResult Mine()
        {
            var proof = _blockchainService.MineNextProof();

            _blockchainService.AddTransaction("0", nodeIdentifier, 3, mempool);

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

        [HttpPost("nodes/register")]
        public IActionResult RegisterNodes([FromBody] RegisterNodesRequest req)
        {
            if (req?.nodes == null || req.nodes.Count == 0)
                return BadRequest("Error: Please supply a valid list of nodes");

            foreach (var node in req.nodes)
                _blockchainService.RegisterNode(node);

            return StatusCode(201, new
            {
                message = "New nodes have been added",
                total_nodes = _blockchainService.Nodes.ToList()
            });
        }

        [HttpGet("nodes/resolve")]
        public async Task<IActionResult> Resolve()
        {
            var replaced = await _blockchainService.ResolveConflictsAsync(_httpClientFactory);

            if (replaced)
            {
                return Ok(new
                {
                    message = "Our chain was replaced",
                    new_chain = _blockchainService.Chain,
                    length = _blockchainService.Chain.Count
                });
            }

            return Ok(new
            {
                message = "Our chain is authoritative",
                chain = _blockchainService.Chain,
                length = _blockchainService.Chain.Count
            });
        }
    }
}

