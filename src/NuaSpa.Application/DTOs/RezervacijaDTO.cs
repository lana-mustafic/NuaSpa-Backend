using System;
using System.Collections.Generic;
using System.Text;

namespace NuaSpa.Application.DTOs
{
    public class RezervacijaDTO
    {
        public int Id { get; set; }
        public DateTime DatumRezervacije { get; set; }
        public bool IsPotvrdjena { get; set; }
        public bool IsPlacena { get; set; }
        public bool IsOtkazana { get; set; }
        public string? RazlogOtkaza { get; set; }
        public DateTime? OtkazanaAt { get; set; }

        /// <summary>VIP tretman (admin), per rezervacija.</summary>
        public bool IsVip { get; set; }

        public int? ProstorijaId { get; set; }
        public string? ProstorijaNaziv { get; set; }
        public List<RezervacijaOpremaItemDTO> Oprema { get; set; } = new();

        /// <summary>ID klijenta (korisnika) — za povijest i CRM.</summary>
        public int KorisnikId { get; set; }

        public string? KorisnikIme { get; set; }
        public string? KorisnikTelefon { get; set; }
        public string? KorisnikEmail { get; set; }

        /// <summary>Tekst s kartice klijenta (medicinsko / tehnička napomena za tretman).</summary>
        public string? NapomenaZaTerapeuta { get; set; }

        public string? UslugaNaziv { get; set; }
        public int UslugaId { get; set; }
        public int UslugaTrajanjeMinuta { get; set; }
        public decimal UslugaCijena { get; set; }

        public int ZaposlenikId { get; set; }
        public string? ZaposlenikIme { get; set; }

        /// <summary>
        /// Heuristika VIP segmenta (&gt;= 3 uspješno plaćena termina bez otkaza).
        /// </summary>
        public bool PremiumKlijent { get; set; }
    }
}
