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
                .ForMember(p => p.PasswordHash, option => { option.PreCondition(a => a.PasswordHash is null); option.Ignore(); })
                .ForMember(dest => dest.AvatarUrl, opt => opt.Ignore());

            CreateMap<Link, LinkDtoModel>();
            CreateMap<LinkDtoModel, Link>();

            CreateMap<LinkClick, LinkClickDtoModel>();
            CreateMap<LinkClickDtoModel, LinkClick>();

            CreateMap<User, UserDtoModel>();
            CreateMap<UserDtoModel, User>()
                .ForMember(p => p.PasswordHash, option => { option.PreCondition(a => a.PasswordHash is null); option.Ignore(); });

            CreateMap<UserRegisterModel, User>()
            .ForMember(dest => dest.AvatarUrl, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
        }
    }
}
