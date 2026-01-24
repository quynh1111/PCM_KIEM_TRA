namespace PCM.Domain.Enums
{
    public enum BookingStatus
    {
        PendingPayment,  // Chờ thanh toán
        Confirmed,       // Đã xác nhận & thanh toán
        Cancelled,       // Đã hủy
        CheckedIn,       // Đã check-in
        Completed        // Hoàn thành
    }
}
