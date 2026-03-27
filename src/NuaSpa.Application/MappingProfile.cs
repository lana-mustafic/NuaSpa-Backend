using AutoMapper;
using NuaSpa.Domain.Entities;
using NuaSpa.Application.DTOs; // Koristimo tvoj folder DTOs

namespace NuaSpa.Application
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // --- 1. Jednostavna mapiranja (Ista imena polja) ---
            CreateMap<Uloga, UlogaDTO>();
            CreateMap<Usluga, UslugaDTO>();
            CreateMap<Proizvod, ProizvodDTO>();
            CreateMap<Popust, PopustDTO>(); // Ako si napravila PopustDTO

            // --- 2. Napredna mapiranja (Sa spajanjem podataka) ---

            // Korisnik: Izvlačimo naziv uloge iz povezane tabele
            CreateMap<Korisnik, KorisnikDTO>()
                .ForMember(dest => dest.UlogaNaziv, opt => opt.MapFrom(src => src.Uloga.Naziv));

            // Rezervacija: Spajamo Ime i Prezime korisnika i naziv usluge
            CreateMap<Rezervacija, RezervacijaDTO>()
                .ForMember(dest => dest.KorisnikIme, opt => opt.MapFrom(src => src.Korisnik.Ime + " " + src.Korisnik.Prezime))
                .ForMember(dest => dest.UslugaNaziv, opt => opt.MapFrom(src => src.Usluga.Naziv))
                .ForMember(dest => dest.ZaposlenikIme, opt => opt.MapFrom(src => src.Zaposlenik.Ime));

            // Skladište: Izvlačimo naziv proizvoda
            CreateMap<Skladiste, SkladisteDTO>()
                .ForMember(dest => dest.ProizvodNaziv, opt => opt.MapFrom(src => src.Proizvod.Naziv));

            // Recenzija: Izvlačimo ko je pisao i za koju uslugu
            CreateMap<Recenzija, RecenzijaDTO>()
                .ForMember(dest => dest.KorisnikIme, opt => opt.MapFrom(src => src.Korisnik.KorisnickoIme))
                .ForMember(dest => dest.UslugaNaziv, opt => opt.MapFrom(src => src.Usluga.Naziv));

            // Zaposlenik
            CreateMap<Zaposlenik, ZaposlenikDTO>();
        }
    }
}