using AutoMapper;
using AutoTraderApp.Application.Features.Common.Mappings;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Domain.Enums;

namespace AutoTraderApp.Application.Features.Auth.Models;

public class UserDto : IMapFrom<User>
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public AccountStatus Status { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<User, UserDto>()
            .ForMember(d => d.Email, opt => opt.MapFrom(s => s.Email.Address));
    }
}