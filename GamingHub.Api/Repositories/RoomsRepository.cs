using GamingHub.Api.Data.Entities;

namespace GamingHub.Api.Repositories
{
    public interface IRoomsRepository
    {
        Task<IEnumerable<Room>> GetAll();
        Task<Room?> Get(int roomId);
        Task<Room> Create(Room room);
        Task<Room> Update(Room room);
        Task Delete(Room room);
    }

    public class RoomsRepository : IRoomsRepository
    {
        public async Task<IEnumerable<Room>> GetAll()
        {
            return new List<Room>
            {
                new Room { Id = 1, Name = "Room 1", Type = RoomType.PC, Description = "A PC gaming room", HourlyRate = 10.0m },
                new Room { Id = 2, Name = "Room 2", Type = RoomType.Console, Description = "A console gaming room", HourlyRate = 15.0m }
            };
        }

        public async Task<Room?> Get(int roomId)
        {
            return new Room { Id = roomId, Name = "Room 1", Type = RoomType.PC, Description = "A PC gaming room", HourlyRate = 10.0m };
        }

        public async Task<Room> Create(Room room)
        {
            room.Id = 3;
            return room;
        }

        public async Task<Room> Update(Room room)
        {
            return room;
        }

        public async Task Delete(Room room)
        {

        }

    }
}
