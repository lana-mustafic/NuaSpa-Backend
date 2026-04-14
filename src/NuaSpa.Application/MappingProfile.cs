using AutoMapper;
using NuaSpa.Domain.Entities;
using NuaSpa.Application.DTOs;

namespace NuaSpa.Application
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // --- 1. Mapiranja za DTOs koje imaš u folderu ---
            // Koristimo "DTO" (velika slova) jer su ti takvi nazivi fajlova na slici

            CreateMap<Uloga, UlogaDTO>().ReverseMap();
            CreateMap<Usluga, UslugaDTO>().ReverseMap();
            CreateMap<Proizvod, ProizvodDTO>().ReverseMap();
            CreateMap<Popust, PopustDTO>().ReverseMap();
            CreateMap<Zaposlenik, ZaposlenikDTO>().ReverseMap();
            CreateMap<Skladiste, SkladisteDTO>().ReverseMap();
            CreateMap<Recenzija, RecenzijaDTO>().ReverseMap();
            CreateMap<Rezervacija, RezervacijaDTO>().ReverseMap();
            CreateMap<Korisnik, KorisnikDTO>().ReverseMap();

            // --- 2. Specifična pravila za polja (Identity usklađivanje) ---

            // Mapiranje Korisnika (UserName -> KorisnickoIme)
            CreateMap<Korisnik, KorisnikDTO>()
                .ForMember(dest => dest.KorisnickoIme, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.UlogaNaziv, opt => opt.Ignore()) // Ignorišemo jer nemaš direktnu relaciju više
                .ReverseMap()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.KorisnickoIme));

            // Mapiranje Rezervacije (Spajanje imena i prezimena)
            CreateMap<Rezervacija, RezervacijaDTO>()
                .ForMember(dest => dest.KorisnikIme, opt => opt.MapFrom(src => src.Korisnik.Ime + " " + src.Korisnik.Prezime))
                .ForMember(dest => dest.UslugaNaziv, opt => opt.MapFrom(src => src.Usluga.Naziv))
                .ForMember(dest => dest.ZaposlenikIme, opt => opt.MapFrom(src => src.Zaposlenik.Ime));

            // Mapiranje Skladišta (Izvlačenje naziva proizvoda)
            CreateMap<Skladiste, SkladisteDTO>()
                .ForMember(dest => dest.ProizvodNaziv, opt => opt.MapFrom(src => src.Proizvod.Naziv));

            // Mapiranje Recenzije (KorisnikIme dolazi iz UserName-a)
            CreateMap<Recenzija, RecenzijaDTO>()
                .ForMember(dest => dest.KorisnikIme, opt => opt.MapFrom(src => src.Korisnik.UserName))
                .ForMember(dest => dest.UslugaNaziv, opt => opt.MapFrom(src => src.Usluga.Naziv));
        }
    }
}