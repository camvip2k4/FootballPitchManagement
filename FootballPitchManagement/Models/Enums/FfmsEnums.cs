namespace FootballPitchManagement.Models.Enums
{
    public enum RoleType
    {
        CUSTOMER,
        STAFF,
        MANAGER,
        ADMIN
    }

    public enum AccountStatus
    {
        ACTIVE,
        INACTIVE
    }

    public enum FieldType
    {
        FIELD_5,
        FIELD_7,
        FIELD_11
    }

    public enum FieldStatus
    {
        AVAILABLE,
        OCCUPIED,
        MAINTENANCE
    }

    public enum BookingStatus
    {
        PENDING_PAYMENT,
        PAYMENT_FAILED,
        EXPIRED,
        CONFIRMED,
        CHECKED_IN,
        COMPLETED,
        CANCELLED,
        NO_SHOW
    }

    public enum PaymentStatus
    {
        PENDING,
        SUCCESS,
        FAILED
    }

    public enum PaymentMethod
    {
        ONLINE_GATEWAY,
        CASH,
        BANK_TRANSFER
    }

    public enum PaymentChannel
    {
        ONLINE,
        DIRECT
    }

    public enum SettlementStatus
    {
        UNPAID,
        PARTIALLY_PAID,
        PAID,
        UNSETTLED
    }

    public enum CancellationType
    {
        CUSTOMER,
        FACILITY
    }

    public enum RefundType
    {
        CUSTOMER_CANCEL,
        FACILITY_CANCEL,
        LATE_PAYMENT
    }

    public enum RefundStatus
    {
        PENDING,
        SUCCESS,
        FAILED
    }

    public enum DayType
    {
        WEEKDAY,
        WEEKEND,
        HOLIDAY
    }

    public enum TimeTier
    {
        NORMAL,
        PEAK
    }
}

