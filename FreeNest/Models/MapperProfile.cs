using AutoMapper;
using DATA.Models;
using FreeNest.Models.ViewModels;

namespace FreeNest.Models
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<User, UserSessionModel>();

            CreateMap<User, UserProfileDtoModel>();
            CreateMap<UserProfileDtoModel, User>()
                .ForMember(d => d.Id, option => { option.Ignore(); })
                .ForMember(d => d.Email, option => { option.Ignore(); })
                .ForMember(d => d.Username, option => { option.Ignore(); })
                .ForMember(p => p.PasswordHash, option => { option.PreCondition(a => a.PasswordHash is null); option.Ignore(); });

            CreateMap<Link, LinkDtoModel>();
            CreateMap<LinkDtoModel, Link>();

            CreateMap<LinkClick, LinkClickDtoModel>();
            CreateMap<LinkClickDtoModel, LinkClick>();

            CreateMap<User, UserDtoModel>();
            CreateMap<UserDtoModel, User>()
                .ForMember(p => p.PasswordHash, option => { option.PreCondition(a => a.PasswordHash is null); option.Ignore(); });
        }
    }
}
