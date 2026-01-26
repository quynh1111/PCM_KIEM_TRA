using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PCM.Application.DTOs.Courts;
using PCM.Application.Interfaces;
using PCM.Domain.Entities;
using PCM.Domain.Interfaces;

namespace PCM.Infrastructure.Services
{
    public class CourtService : ICourtService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CourtService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CourtDto>> GetAllAsync(bool includeInactive = false)
        {
            var courts = await _unitOfWork.Courts.GetAllAsync();
            var filtered = includeInactive ? courts : courts.Where(c => c.IsActive);
            return filtered.OrderBy(c => c.Name).Select(ToDto).ToList();
        }

        public async Task<CourtDto?> GetByIdAsync(int id)
        {
            var court = await _unitOfWork.Courts.GetByIdAsync(id);
            return court == null ? null : ToDto(court);
        }

        public async Task<CourtDto> CreateAsync(CourtCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new Exception("Court name is required");

            if (dto.HourlyRate <= 0)
                throw new Exception("Hourly rate must be greater than 0");

            var court = new Court
            {
                Name = dto.Name.Trim(),
                Description = dto.Description,
                HourlyRate = dto.HourlyRate,
                IsActive = dto.IsActive
            };

            await _unitOfWork.Courts.AddAsync(court);
            await _unitOfWork.SaveChangesAsync();

            return ToDto(court);
        }

        public async Task<CourtDto?> UpdateAsync(int id, CourtUpdateDto dto)
        {
            var court = await _unitOfWork.Courts.GetByIdAsync(id);
            if (court == null)
                return null;

            if (dto.Name != null)
                court.Name = dto.Name.Trim();

            if (dto.Description != null)
                court.Description = dto.Description;

            if (dto.HourlyRate.HasValue)
            {
                if (dto.HourlyRate.Value <= 0)
                    throw new Exception("Hourly rate must be greater than 0");

                court.HourlyRate = dto.HourlyRate.Value;
            }

            if (dto.IsActive.HasValue)
                court.IsActive = dto.IsActive.Value;

            _unitOfWork.Courts.Update(court);
            await _unitOfWork.SaveChangesAsync();

            return ToDto(court);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var court = await _unitOfWork.Courts.GetByIdAsync(id);
            if (court == null)
                return false;

            court.IsActive = false;
            _unitOfWork.Courts.Update(court);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        private static CourtDto ToDto(Court court)
        {
            return new CourtDto
            {
                Id = court.Id,
                Name = court.Name,
                Description = court.Description,
                HourlyRate = court.HourlyRate,
                IsActive = court.IsActive
            };
        }
    }
}
