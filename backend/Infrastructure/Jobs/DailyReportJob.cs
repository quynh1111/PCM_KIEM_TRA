using System;
using System.Linq;
using System.Threading.Tasks;
using PCM.Domain.Interfaces;

namespace PCM.Infrastructure.Jobs
{
    public class DailyReportJob
    {
        private readonly IUnitOfWork _unitOfWork;

        public DailyReportJob(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task ExecuteAsync()
        {
            var today = DateTime.UtcNow.Date;
            var transactions = await _unitOfWork.TreasuryTransactions.FindAsync(t => t.Date >= today);

            var totalIncome = transactions.Where(t => t.Amount > 0).Sum(t => t.Amount);
            var totalExpense = transactions.Where(t => t.Amount < 0).Sum(t => Math.Abs(t.Amount));
            var balance = transactions.Sum(t => t.Amount);

            Console.WriteLine($"[DailyReport] Income: {totalIncome}, Expense: {totalExpense}, Balance: {balance}");
        }
    }
}
