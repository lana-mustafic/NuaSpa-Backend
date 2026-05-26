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

        Task HandleStripeWebhookAsync(
            string requestBodyJson,
            string stripeSignatureHeader,
            CancellationToken ct);
    }
}

