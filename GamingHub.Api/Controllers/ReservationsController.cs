using AutoMapper;
using GamingHub.Api.Data.Dtos.Reservations;
using GamingHub.Api.Data.Entities;
using GamingHub.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GamingHub.Api.Controllers
{
    ///api/reservations GET ALL 200 (filters: date, status, roomId, customerEmail)
    ///api/rooms/{roomId}/stations/{stationId}/reservations GET ALL 200
    ///api/rooms/{roomId}/stations/{stationId}/reservations/{reservationId} GET 200
    ///api/rooms/{roomId}/stations/{stationId}/reservations POST 201
    ///api/rooms/{roomId}/stations/{stationId}/reservations/{reservationId} PUT 200
    ///api/rooms/{roomId}/stations/{stationId}/reservations/{reservationId} PATCH 200
    ///api/rooms/{roomId}/stations/{stationId}/reservations/{reservationId} DELETE 204

    [Route("api/rooms/{roomId}/stations/{stationId}/reservations")]
    [ApiController]
    public class ReservationsController : ControllerBase
    {
        private readonly IRoomsRepository _roomsRepository;
        private readonly IStationsRepository _stationsRepository;
        private readonly IReservationsRepository _reservationsRepository;
        private readonly IMapper _mapper;

        public ReservationsController(IRoomsRepository roomsRepository, IStationsRepository stationsRepository, IReservationsRepository reservationsRepository, IMapper mapper)
        {
            _roomsRepository = roomsRepository;
            _stationsRepository = stationsRepository;
            _reservationsRepository = reservationsRepository;
            _mapper = mapper;
        }

        // List across all stations
        [HttpGet("/api/reservations")]
        public async Task<ActionResult<IEnumerable<ReservationDto>>> GetFiltered(
                         [FromQuery] DateOnly? date, [FromQuery] ReservationStatus? status,
                         [FromQuery] int? roomId, [FromQuery] string? customerEmail)
        {
            var reservations = await _reservationsRepository.GetFiltered(date, status, roomId, customerEmail);
            return Ok(reservations.Select(r => _mapper.Map<ReservationDto>(r)));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReservationDto>>> GetAll(int roomId, int stationId)
        {
            var station = await _stationsRepository.Get(roomId, stationId);
            if (station == null) { return NotFound(); }

            var reservations = await _reservationsRepository.GetAll(stationId);
            return Ok(reservations.Select(r => _mapper.Map<ReservationDto>(r)));
        }

        [HttpGet("{reservationId}")]
        public async Task<ActionResult<ReservationDto>> Get(int roomId, int stationId, int reservationId)
        {
            var station = await _stationsRepository.Get(roomId, stationId);
            if (station == null) { return NotFound(); }

            var reservation = await _reservationsRepository.Get(stationId, reservationId);
            if (reservation == null) { return NotFound(); }

            return Ok(_mapper.Map<ReservationDto>(reservation));
        }

        [HttpPost]
        public async Task<ActionResult<ReservationDto>> Create(int roomId, int stationId, CreateReservationDto dto)
        {
            var room = await _roomsRepository.Get(roomId);
            if (room == null) return NotFound();

            var station = await _stationsRepository.Get(roomId, stationId);
            if (station == null) return NotFound();

            var start = dto.StartTime!.Value.ToUniversalTime();
            var end = dto.EndTime!.Value.ToUniversalTime();

            ValidateTimes(start, end);
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            if (!station.IsActive)
                return Problem(detail: "This station is not active and cannot be reserved.", statusCode: 409);

            if (await _reservationsRepository.HasOverlap(stationId, start, end))
                return Problem(detail: "The station is already reserved for this time.", statusCode: 409);

            var reservation = new Reservation
            {
                StationId = stationId,
                CustomerName = dto.CustomerName,
                CustomerEmail = dto.CustomerEmail,
                StartTime = start,
                EndTime = end,
                TotalPrice = CalculatePrice(room.HourlyRate, start, end),
                Status = ReservationStatus.Upcoming,
                CreatedAt = DateTimeOffset.UtcNow
            };
            await _reservationsRepository.Create(reservation);

            return CreatedAtAction(nameof(Get), new { roomId, stationId, reservationId = reservation.Id },
                _mapper.Map<ReservationDto>(reservation));
        }

        [HttpPut("{reservationId}")]
        public async Task<ActionResult<ReservationDto>> Put(int roomId, int stationId, int reservationId, UpdateReservationDto dto)
        {
            var room = await _roomsRepository.Get(roomId);
            if (room == null) return NotFound();

            var station = await _stationsRepository.Get(roomId, stationId);
            if (station == null) return NotFound();

            var reservation = await _reservationsRepository.Get(stationId, reservationId);
            if (reservation == null) return NotFound();

            if (reservation.Status != ReservationStatus.Upcoming)
                return Problem(detail: "Only upcoming reservations can be changed.", statusCode: 409);

            var start = dto.StartTime!.Value.ToUniversalTime();
            var end = dto.EndTime!.Value.ToUniversalTime();

            ValidateTimes(start, end);
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            if (await _reservationsRepository.HasOverlap(stationId, start, end, excludeReservationId: reservationId))
                return Problem(detail: "The station is already reserved for this time.", statusCode: 409);

            reservation.CustomerName = dto.CustomerName;
            reservation.CustomerEmail = dto.CustomerEmail;
            reservation.StartTime = start;
            reservation.EndTime = end;
            reservation.TotalPrice = CalculatePrice(room.HourlyRate, start, end);
            await _reservationsRepository.Update(reservation);

            return Ok(_mapper.Map<ReservationDto>(reservation));
        }

        [HttpPatch("{reservationId}")]
        public async Task<ActionResult<ReservationDto>> Patch(int roomId, int stationId, int reservationId, UpdateReservationStatusDto dto)
        {
            var station = await _stationsRepository.Get(roomId, stationId);
            if (station == null) return NotFound();

            var reservation = await _reservationsRepository.Get(stationId, reservationId);
            if (reservation == null) return NotFound();

            var newStatus = dto.Status!.Value;
            if (!AllowedTransitions[reservation.Status].Contains(newStatus))
                return Problem(detail: $"Cannot change status from {reservation.Status} to {newStatus}.", statusCode: 409);

            reservation.Status = newStatus;
            await _reservationsRepository.Update(reservation);

            return Ok(_mapper.Map<ReservationDto>(reservation));
        }

        [HttpDelete("{reservationId}")]
        public async Task<IActionResult> Delete(int roomId, int stationId, int reservationId)
        {
            var station = await _stationsRepository.Get(roomId, stationId);
            if (station == null) return NotFound();

            var reservation = await _reservationsRepository.Get(stationId, reservationId);
            if (reservation == null) return NotFound();

            await _reservationsRepository.Delete(reservation);
            return NoContent();
        }



        private static readonly Dictionary<ReservationStatus, ReservationStatus[]> AllowedTransitions = new()
        {
            [ReservationStatus.Upcoming] = new[] { ReservationStatus.Started, ReservationStatus.Cancelled, ReservationStatus.NoShow },
            [ReservationStatus.Started] = new[] { ReservationStatus.Finished },
            [ReservationStatus.Finished] = Array.Empty<ReservationStatus>(),
            [ReservationStatus.Cancelled] = Array.Empty<ReservationStatus>(),
            [ReservationStatus.NoShow] = Array.Empty<ReservationStatus>(),
        };
        private void ValidateTimes(DateTimeOffset start, DateTimeOffset end)
        {
            if (start <= DateTimeOffset.UtcNow)
                ModelState.AddModelError("StartTime", "StartTime must be in the future.");
            if (end <= start)
                ModelState.AddModelError("EndTime", "EndTime must be after StartTime.");
            else if (end - start < TimeSpan.FromMinutes(30))
                ModelState.AddModelError("EndTime", "A reservation must be at least 30 minutes long.");
            else if (end - start > TimeSpan.FromHours(12))
                ModelState.AddModelError("EndTime", "A reservation can be at most 12 hours long.");
        }

        private static decimal CalculatePrice(decimal hourlyRate, DateTimeOffset start, DateTimeOffset end) 
        { 
            return Math.Round(hourlyRate * (decimal)(end - start).TotalHours, 2); 
        }
    }
}