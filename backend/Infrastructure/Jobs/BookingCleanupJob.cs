using System;
using System.Linq;
using System.Threading.Tasks;
using PCM.Domain.Enums;
using PCM.Domain.Interfaces;

namespace PCM.Infrastructure.Jobs
{
    public class BookingCleanupJob
    {
        private readonly IUnitOfWork _unitOfWork;

        public BookingCleanupJob(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task ExecuteAsync()
        {
            var cutoff = DateTime.UtcNow.AddMinutes(-15);
            var pending = await _unitOfWork.Bookings.FindAsync(b =>
                b.Status == BookingStatus.PendingPayment && b.CreatedDate <= cutoff);

            var list = pending.ToList();
            if (!list.Any())
                return;

            foreach (var booking in list)
            {
                booking.Status = BookingStatus.Cancelled;
                _unitOfWork.Bookings.Update(booking);
            }

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
