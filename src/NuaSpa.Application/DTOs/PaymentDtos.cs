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

