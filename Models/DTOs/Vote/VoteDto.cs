using Org.BouncyCastle.Bcpg;
using StackOverFlowClone.Models.Enum;

namespace StackOverFlowClone.Models.DTOs.Vote
{
    public class VoteDto
    {      
        public int UserId { get; set; }
        public int TargetId { get; set; }           // ✅ Fixed typo from "TrgetId"
        public TargetType TargetType { get; set; }  // Question or Answer
        public VoteType VoteType { get; set; }      // UpVote or DownVote
    }
}
