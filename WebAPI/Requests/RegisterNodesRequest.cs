using System.ComponentModel.DataAnnotations;

namespace WebAPI.Requests
{
    public sealed class RegisterNodesRequest
    {
        [Required]
        public List<string>? nodes { get; set; }
    }
}
