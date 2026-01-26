using System.Collections.Generic;
using System.Threading.Tasks;
using PCM.Application.DTOs.Members;

namespace PCM.Application.Interfaces
{
    public interface IMemberService
    {
        Task<MemberDto?> GetByIdAsync(int id);
        Task<MemberDto?> GetByUserIdAsync(string userId);
        Task<List<MemberDto>> GetAllAsync();
        Task<MemberDto?> UpdateProfileAsync(string userId, UpdateMemberProfileDto dto);
        Task<MemberDto?> UpdateMemberAsync(int id, UpdateMemberProfileDto dto);
        Task<List<MemberDto>> GetTopRankingAsync(int count = 5);
        Task<bool> UpdateRankELOAsync(int memberId, double newELO);
    }
}
