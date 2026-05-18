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

            CreateMap<Usluga, UslugaDTO>()
                .ForMember(dest => dest.KategorijaNaziv, opt => opt.MapFrom(src => src.KategorijaUsluga.Naziv))
                .ForMember(dest => dest.TrajanjeTekst, opt => opt.MapFrom(src => $"{src.TrajanjeMinuta} min"))
                .ForMember(dest => dest.SlikaUrl, opt => opt.MapFrom(src => src.SlikaUrl));
            CreateMap<UslugaDTO, Usluga>()
                .ForMember(dest => dest.KategorijaUsluga, opt => opt.Ignore());
            CreateMap<Proizvod, ProizvodDTO>().ReverseMap();
            CreateMap<Popust, PopustDTO>().ReverseMap();
            CreateMap<Zaposlenik, ZaposlenikDTO>()
                .ForMember(dest => dest.KategorijaUslugaNaziv,
                    opt => opt.MapFrom(src =>
                        src.KategorijaUsluga != null ? src.KategorijaUsluga.Naziv : null));
            CreateMap<ZaposlenikDTO, Zaposlenik>()
                .ForMember(dest => dest.KategorijaUsluga, opt => opt.Ignore())
                .ForMember(dest => dest.DatumZaposlenja, opt => opt.Ignore());
            CreateMap<Skladiste, SkladisteDTO>().ReverseMap();
            CreateMap<Recenzija, RecenzijaDTO>().ReverseMap();
            CreateMap<Korisnik, KorisnikDTO>().ReverseMap();
            CreateMap<KategorijaUslugaDTO, KategorijaUsluga>().ReverseMap();
            CreateMap<SpaCentar, SpaCentarDTO>().ReverseMap();
            CreateMap<Prostorija, ProstorijaDTO>().ReverseMap();
            CreateMap<Oprema, OpremaDTO>().ReverseMap();
            CreateMap<RadnoVrijeme, RadnoVrijemeDTO>().ReverseMap();

            // --- 2. Specifična pravila za polja (Identity usklađivanje) ---

            // Mapiranje Korisnika (UserName -> KorisnickoIme)
            CreateMap<Korisnik, KorisnikDTO>()
                .ForMember(dest => dest.KorisnickoIme, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.UlogaNaziv, opt => opt.Ignore()) // Ignorišemo jer nemaš direktnu relaciju više
                .ReverseMap()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.KorisnickoIme));

            // Mapiranje Rezervacije (Spajanje imena i prezimena)
            CreateMap<Rezervacija, RezervacijaDTO>()
                .ForMember(dest => dest.KorisnikId, opt => opt.MapFrom(src => src.KorisnikId))
                .ForMember(dest => dest.KorisnikTelefon, opt => opt.MapFrom(src => src.Korisnik.PhoneNumber))
                .ForMember(dest => dest.KorisnikEmail, opt => opt.MapFrom(src => src.Korisnik.Email))
                .ForMember(dest => dest.NapomenaZaTerapeuta,
                    opt => opt.MapFrom(src => src.Korisnik.NapomenaZaTerapeuta))
                .ForMember(dest => dest.UslugaTrajanjeMinuta,
                    opt => opt.MapFrom(src => src.Usluga.TrajanjeMinuta))
                .ForMember(dest => dest.UslugaCijena, opt => opt.MapFrom(src => src.Usluga.Cijena))
                .ForMember(dest => dest.KorisnikIme, opt => opt.MapFrom(src => src.Korisnik.Ime + " " + src.Korisnik.Prezime))
                .ForMember(dest => dest.UslugaNaziv, opt => opt.MapFrom(src => src.Usluga.Naziv))
                .ForMember(dest => dest.ZaposlenikIme, opt => opt.MapFrom(src => src.Zaposlenik.Ime + " " + src.Zaposlenik.Prezime))
                .ForMember(dest => dest.ProstorijaNaziv,
                    opt => opt.MapFrom(src => src.Prostorija != null ? src.Prostorija.Naziv : null))
                .ForMember(dest => dest.Oprema, opt => opt.MapFrom(src =>
                    src.RezervacijaOprema.Select(x => new RezervacijaOpremaItemDTO
                    {
                        OpremaId = x.OpremaId,
                        Kolicina = x.Kolicina
                    }).ToList()
                ))
                .ForMember(dest => dest.PremiumKlijent, opt => opt.Ignore());

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