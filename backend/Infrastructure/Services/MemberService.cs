using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PCM.Application.DTOs.Members;
using PCM.Application.Interfaces;
using PCM.Domain.Entities;
using PCM.Domain.Interfaces;

namespace PCM.Infrastructure.Services
{
    public class MemberService : IMemberService
    {
        private readonly IUnitOfWork _unitOfWork;
        public MemberService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<MemberDto?> GetByIdAsync(int id)
        {
            var member = await _unitOfWork.Members.GetByIdAsync(id);
            return member == null ? null : ToDto(member);
        }

        public async Task<MemberDto?> GetByUserIdAsync(string userId)
        {
            var member = (await _unitOfWork.Members.FindAsync(m => m.UserId == userId)).FirstOrDefault();
            return member == null ? null : ToDto(member);
        }

        public async Task<MemberDto?> UpdateProfileAsync(string userId, UpdateMemberProfileDto dto)
        {
            var member = (await _unitOfWork.Members.FindAsync(m => m.UserId == userId)).FirstOrDefault();
            if (member == null) return null;
            if (dto.FullName != null) member.FullName = dto.FullName;
            if (dto.PhoneNumber != null) member.PhoneNumber = dto.PhoneNumber;
            if (dto.DateOfBirth.HasValue) member.DateOfBirth = dto.DateOfBirth;
            if (dto.AvatarUrl != null) member.AvatarUrl = dto.AvatarUrl;
            await _unitOfWork.SaveChangesAsync();
            return ToDto(member);
        }

        public async Task<List<MemberDto>> GetTopRankingAsync(int count = 5)
        {
            var members = (await _unitOfWork.Members.GetAllAsync())
                .OrderByDescending(m => m.RankELO)
                .Take(count)
                .Select(ToDto)
                .ToList();
            return members;
        }

        public async Task<bool> UpdateRankELOAsync(int memberId, double newELO)
        {
            var member = await _unitOfWork.Members.GetByIdAsync(memberId);
            if (member == null) return false;
            member.RankELO = newELO;
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        private static MemberDto ToDto(Member m) => new MemberDto
        {
            Id = m.Id,
            UserId = m.UserId,
            FullName = m.FullName,
            Email = m.Email,
            PhoneNumber = m.PhoneNumber,
            DateOfBirth = m.DateOfBirth,
            JoinDate = m.JoinDate,
            RankELO = m.RankELO,
            WalletBalance = m.WalletBalance,
            AvatarUrl = m.AvatarUrl,
            IsActive = m.IsActive
        };
    }
}
