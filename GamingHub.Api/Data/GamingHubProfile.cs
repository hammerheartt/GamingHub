using AutoMapper;
using GamingHub.Api.Data.Dtos.Rooms;
using GamingHub.Api.Data.Entities;

namespace GamingHub.Api.Data
{
    public class GamingHubProfile : Profile
    {
        public GamingHubProfile()
        {
            CreateMap<Room, RoomDto>();
            CreateMap<CreateRoomDto, Room>();
            CreateMap<UpdateRoomDto, Room>();
        }
    }
}
