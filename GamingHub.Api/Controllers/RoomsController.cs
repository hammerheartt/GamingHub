using AutoMapper;
using GamingHub.Api.Data.Dtos.Rooms;
using GamingHub.Api.Data.Entities;
using GamingHub.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GamingHub.Api.Controllers
{
    ///api/rooms GET ALL 200
    ///api/rooms/{roomId} GET 200
    ///api/rooms POST 201
    ///api/rooms/{roomId} PUT 200
    ///api/rooms/{roomId} DELETE 204

    [ApiController]
    [Route("api/rooms")]
    public class RoomsController : ControllerBase
    {
        private readonly IRoomsRepository _roomsRepository;
        private readonly IMapper _mapper;
        public RoomsController(IRoomsRepository roomsRepository, IMapper mapper) 
        { 
            _roomsRepository = roomsRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IEnumerable<RoomDto>> GetAll()
        {
            return (await _roomsRepository.GetAll()).Select(o => _mapper.Map<RoomDto>(o));
        }

        [HttpGet("{roomId}")]
        public async Task<ActionResult<RoomDto>> Get(int roomId)
        {
            var room = await _roomsRepository.Get(roomId);
            if (room == null) { return NotFound(); }
            return Ok(_mapper.Map<RoomDto>(room));
        }

        [HttpPost]
        public async Task<ActionResult<RoomDto>> Create(CreateRoomDto createRoomDto)
        {
            var room = _mapper.Map<Room>(createRoomDto);
            await _roomsRepository.Create(room);

            var roomDto = _mapper.Map<RoomDto>(room);
            return CreatedAtAction(nameof(Get), new { roomId = room.Id }, roomDto);
        }

        [HttpPut("{roomId}")]
        public async Task<ActionResult<RoomDto>> Put(int roomId, UpdateRoomDto updateRoomDto)
        {
            var room = await _roomsRepository.Get(roomId);
            if (room == null) { return NotFound(); }

            _mapper.Map(updateRoomDto, room);
            await _roomsRepository.Update(room);

            return Ok(_mapper.Map<RoomDto>(room));

        }

        [HttpDelete("{roomId}")]
        public async Task<IActionResult> Delete(int roomId)
        {
            var room = await _roomsRepository.Get(roomId);
            if (room == null) { return NotFound(); }

            await _roomsRepository.Delete(room);

            return NoContent();
        }
    }
}   
