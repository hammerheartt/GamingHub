using AutoMapper;
using GamingHub.Api.Data.Dtos.Games;
using GamingHub.Api.Data.Dtos.Rooms;
using GamingHub.Api.Data.Dtos.Stations;
using GamingHub.Api.Data.Entities;
using GamingHub.Api.Data.Paging;
using GamingHub.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GamingHub.Api.Controllers
{
    ///api/rooms/{roomId}/ stations GET ALL 200
    ///api/rooms/{roomId}/ stations /{stationId} GET 200
    ///api/rooms/{roomId}/ stations POST 201
    ///api/rooms/{roomId}/ stations /{stationId} PUT 200
    ///api/rooms/{roomId}/ stations /{stationId} DELETE 204
    ///api/rooms/{roomId}/stations/{stationId}/details GET 200

    [Route("api/rooms/{roomId}/stations")]
    [ApiController]
    public class StationsController : ControllerBase
    {
        private readonly IRoomsRepository _roomsRepository;
        private readonly IStationsRepository _stationsRepository;
        private readonly IMapper _mapper;
        private readonly LinkGenerator _linkGenerator;

        public StationsController(IRoomsRepository roomsRepository, IStationsRepository stationsRepository, IMapper mapper, LinkGenerator linkGenerator)
        {
            _roomsRepository = roomsRepository;
            _stationsRepository = stationsRepository;
            _mapper = mapper;
            _linkGenerator = linkGenerator;
        }

        [HttpGet(Name = "GetStations")]
        public async Task<ActionResult<IEnumerable<StationDto>>> GetAll(int roomId, [FromQuery] SearchParameters searchParameters)
        {
            var room = await _roomsRepository.Get(roomId);
            if (room == null) { return NotFound(); }

            var stations = await _stationsRepository.GetAll(roomId, searchParameters);
            Response.AddPaginationHeader(stations.CreatePaginationMetadata(_linkGenerator, HttpContext, "GetStations", new { roomId }));

            return Ok(stations.Select(o => _mapper.Map<StationDto>(o)));
        }

        [HttpGet("{stationId}")]
        public async Task<ActionResult<StationDto>> Get(int roomId, int stationId)
        {
            var station = await _stationsRepository.Get(roomId, stationId);
            if (station == null) { return NotFound(); }

            return Ok(_mapper.Map<StationDto>(station));
        }

        [HttpGet("{stationId}/details")]
        public async Task<ActionResult<StationDetailsDto>> GetDetails(int roomId, int stationId)
        {
            var station = await _stationsRepository.GetDetails(roomId, stationId);
            if (station == null) { return NotFound(); }

            var details = new StationDetailsDto(
                station.Id, station.Name,
                station.Cpu, station.Gpu, station.RamGb,
                station.Platform, station.Seats,
                station.Screen, station.IsActive,
                _mapper.Map<RoomDto>(station.Room),
                station.Games.Select(o => _mapper.Map<GameDto>(o)),
                station.Reservations.Select(o => new ReservationSlotDto(o.StartTime, o.EndTime)));

            return Ok(details);
        }

        [HttpPost]
        public async Task<ActionResult<StationDto>> Create(int roomId, CreateStationDto dto) 
        { 
            var room = await _roomsRepository.Get(roomId);
            if (room == null) { return NotFound(); }

            ValidateForRoomType(room.Type, dto.Cpu, dto.Gpu, dto.RamGb, dto.Platform, dto.Seats);
            if (!ModelState.IsValid) { return ValidationProblem(ModelState); }

            var station = _mapper.Map<Station>(dto);
            station.RoomId = roomId;
            await _stationsRepository.Create(station);

            return CreatedAtAction(nameof(Get), new { roomId, stationId = station.Id }, _mapper.Map<StationDto>(station));
        }

        [HttpPut("{stationId}")]
        public async Task<ActionResult<StationDto>> Update(int roomId, int stationId, UpdateStationDto dto) 
        {
            var room = await _roomsRepository.Get(roomId);
            if (room == null) return NotFound();

            var station = await _stationsRepository.Get(roomId, stationId);
            if (station == null) return NotFound();

            ValidateForRoomType(room.Type, dto.Cpu, dto.Gpu, dto.RamGb, dto.Platform, dto.Seats);
            if (!ModelState.IsValid) { return ValidationProblem(ModelState); }

            _mapper.Map(dto, station);
            await _stationsRepository.Update(station);

            return Ok(_mapper.Map<StationDto>(station));
        }

        [HttpDelete("{stationId}")]
        public async Task<IActionResult> Delete(int roomId, int stationId)
        {
            var station = await _stationsRepository.Get(roomId, stationId);
            if (station == null) return NotFound();

            if (await _stationsRepository.HasActiveReservations(stationId))
                return Problem(detail: "Station has upcoming or ongoing reservations.", statusCode: 409);

            await _stationsRepository.Delete(station);
            return NoContent();
        }

        private void ValidateForRoomType(RoomType type, string? cpu, string? gpu, int? ramGb, StationPlatform? platform, int? seats)
        {
            if (type == RoomType.PC)
            {
                if (string.IsNullOrWhiteSpace(cpu)) ModelState.AddModelError("Cpu", "Cpu is required for stations in a PC room.");
                if (string.IsNullOrWhiteSpace(gpu)) ModelState.AddModelError("Gpu", "Gpu is required for stations in a PC room.");
                if (ramGb == null) ModelState.AddModelError("RamGb", "RamGb is required for stations in a PC room.");
                if (platform != null || seats != null) ModelState.AddModelError("Platform", "Platform and Seats are only for console rooms.");
            }
            else
            {
                if (platform == null) ModelState.AddModelError("Platform", "Platform is required for stations in a console room.");
                if (seats == null) ModelState.AddModelError("Seats", "Seats is required for stations in a console room.");
                if (cpu != null || gpu != null || ramGb != null) ModelState.AddModelError("Cpu", "Cpu, Gpu and RamGb are only for PC rooms.");
            }
        }
    }
}
