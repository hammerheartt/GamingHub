using AutoMapper;
using GamingHub.Api.Data.Dtos.Rooms;
using GamingHub.Api.Data.Entities;
using GamingHub.Api.Repositories;
using GamingHub.Api.Data.Paging;
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
        private readonly LinkGenerator _linkGenerator;

        public RoomsController(IRoomsRepository roomsRepository, IMapper mapper, LinkGenerator linkGenerator) 
        { 
            _roomsRepository = roomsRepository;
            _mapper = mapper;
            _linkGenerator = linkGenerator;
        }

        [HttpGet(Name = "GetRooms")]
        public async Task<ActionResult<IEnumerable<RoomDto>>> GetAll([FromQuery] SearchParameters searchParameters)
        {
            var rooms = await _roomsRepository.GetAll(searchParameters);
            Response.AddPaginationHeader(rooms.CreatePaginationMetadata(_linkGenerator, HttpContext, "GetRooms"));

            return Ok(rooms.Select(o => _mapper.Map<RoomDto>(o)));
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
            if (room.Type != updateRoomDto.Type && await _roomsRepository.HasStations(roomId))
                return Problem(detail: "Room type cannot be changed while the room has stations.", statusCode: 409);

            _mapper.Map(updateRoomDto, room);
            await _roomsRepository.Update(room);

            return Ok(_mapper.Map<RoomDto>(room));

        }

        [HttpDelete("{roomId}")]
        public async Task<IActionResult> Delete(int roomId)
        {
            var room = await _roomsRepository.Get(roomId);
            if (room == null) { return NotFound(); }
            if (await _roomsRepository.HasStations(roomId))
                return Problem(detail: "Room cannot be deleted while it has stations.", statusCode: 409);

            await _roomsRepository.Delete(room);

            return NoContent();
        }
    }
}   
