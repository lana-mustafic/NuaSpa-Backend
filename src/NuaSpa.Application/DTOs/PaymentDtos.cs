namespace NuaSpa.Application.DTOs;

public class CreatePaymentIntentRequestDto
{
    public int RezervacijaId { get; set; }
}

public class CreatePaymentIntentResponseDto
{
    public string ClientSecret { get; set; } = string.Empty;
    public string PaymentIntentId { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public long Amount { get; set; }
}

public class ConfirmPaymentRequestDto
{
    public string PaymentIntentId { get; set; } = string.Empty;
}

public class ConfirmPaymentResponseDto
{
    public bool IsPlacena { get; set; }
    public bool IsPaid { get; set; }
    public bool AlreadyCompleted { get; set; }
    public decimal ChargedAmount { get; set; }
}

public class RefundPaymentRequestDto
{
    public int RezervacijaId { get; set; }
}

public class RefundPaymentResponseDto
{
    public string RefundId { get; set; } = string.Empty;
    public decimal RefundedAmount { get; set; }
    public bool IsRefunded { get; set; }
}
