using AutoMapper;
using GamingHub.Api.Data.Dtos.Rooms;
using GamingHub.Api.Data.Dtos.Stations;
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

            CreateMap<Station, StationDto>();
            CreateMap<CreateStationDto, Station>().ForMember(d => d.IsActive, o => o.MapFrom(s => s.IsActive ?? true));
            CreateMap<UpdateStationDto, Station>().ForMember(d => d.IsActive, o => o.MapFrom(s => s.IsActive ?? true));
        }
    }
}
