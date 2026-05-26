using System.Threading;
using System.Threading.Tasks;
using NuaSpa.Application.DTOs;

namespace NuaSpa.Application.Interfaces
{
    public interface IPaymentService
    {
        Task<CreatePaymentIntentResponseDto> CreatePaymentIntentAsync(
            int rezervacijaId,
            int userId,
            bool isAdminBooking,
            CancellationToken ct);

        Task<ConfirmPaymentResponseDto> ConfirmPaymentAsync(
            string paymentIntentId,
            int userId,
            bool isAdminBooking,
            CancellationToken ct);

        Task<RefundPaymentResponseDto> RefundPaymentAsync(
            int rezervacijaId,
            int userId,
            CancellationToken ct);

        /// <summary>
        /// Vraća refund ako postoji završeno Stripe plaćanje; null ako nije bilo online plaćanja.
        /// Idempotentan za već refundirana plaćanja.
        /// </summary>
        Task<RefundPaymentResponseDto?> RefundIfPaidAsync(
            int rezervacijaId,
            CancellationToken ct);

        Task HandleStripeWebhookAsync(
            string requestBodyJson,
            string stripeSignatureHeader,
            CancellationToken ct);
    }
}
