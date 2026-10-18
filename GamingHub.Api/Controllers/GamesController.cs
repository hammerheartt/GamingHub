using AutoMapper;
using GamingHub.Api.Data.Dtos.Games;
using GamingHub.Api.Data.Entities;
using GamingHub.Api.Repositories;
using GamingHub.Api.Data.Paging;
using Microsoft.AspNetCore.Mvc;

namespace GamingHub.Api.Controllers
{
    ///api/games GET ALL 200 (optional filters: genre, search)
    ///api/games/{gameId} GET 200
    ///api/games POST 201
    ///api/games/{gameId} PUT 200
    ///api/games/{gameId} DELETE 204
    ///api/rooms/{roomId}/stations/{stationId}/games GET ALL 200
    ///api/rooms/{roomId}/stations/{stationId}/games/{gameId} PUT 204
    ///api/rooms/{roomId}/stations/{stationId}/games/{gameId} DELETE 204

    [ApiController]
    [Route("api/games")]
    public class GamesController : ControllerBase
    {
        private readonly IGamesRepository _gamesRepository;
        private readonly IStationsRepository _stationsRepository;
        private readonly IMapper _mapper;
        private readonly LinkGenerator _linkGenerator;
        public GamesController(IGamesRepository gamesRepository, IStationsRepository stationsRepository, 
                               IMapper mapper, LinkGenerator linkGenerator)
        {
            _gamesRepository = gamesRepository;
            _stationsRepository = stationsRepository;
            _mapper = mapper;
            _linkGenerator = linkGenerator;
        }

        [HttpGet(Name = "GetGames")]
        public async Task<ActionResult<IEnumerable<GameDto>>> GetAll(string? genre, string? search, 
                                                                    [FromQuery] SearchParameters searchParameters)
        {
            var noFilters = string.IsNullOrWhiteSpace(genre) && string.IsNullOrWhiteSpace(search);
            var games = noFilters
                ? await _gamesRepository.GetAll(searchParameters)
                : await _gamesRepository.GetFiltered(genre, search, searchParameters);
            Response.AddPaginationHeader(games.CreatePaginationMetadata(_linkGenerator, HttpContext, "GetGames", new { genre, search }));

            return Ok(games.Select(o => _mapper.Map<GameDto>(o)));
        }

        [HttpGet("/api/rooms/{roomId}/stations/{stationId}/games", Name = "GetStationGames")]
        public async Task<ActionResult<IEnumerable<GameDto>>> GetByStation(int roomId, int stationId, 
                                                                           [FromQuery] SearchParameters searchParameters)
        {
            var station = await _stationsRepository.Get(roomId, stationId);
            if (station == null) { return NotFound(); }

            var games = await _gamesRepository.GetByStation(stationId, searchParameters);
            Response.AddPaginationHeader(games.CreatePaginationMetadata(
                                                            _linkGenerator, HttpContext, "GetStationGames", new { roomId, stationId }));

            return Ok(games.Select(o => _mapper.Map<GameDto>(o)));
        }

        [HttpGet("{gameId}")]
        public async Task<ActionResult<GameDto>> Get(int gameId)
        {
            var game = await _gamesRepository.Get(gameId);
            if (game == null) { return NotFound(); }

            return Ok(_mapper.Map<GameDto>(game));
        }

        [HttpPost()]
        public async Task<ActionResult<GameDto>> Create(CreateGameDto dto)
        {
            if (await _gamesRepository.TitleExists(dto.Title))
                return Problem(detail: $"A game titled '{dto.Title}' already exists.", statusCode: 409);

            var game = _mapper.Map<Game>(dto);
            await _gamesRepository.Create(game);

            return CreatedAtAction(nameof(Get), new { gameId = game.Id }, _mapper.Map<GameDto>(game));
        }

        [HttpPut("{gameId}")]
        public async Task<ActionResult<GameDto>> Put(int gameId, UpdateGameDto dto)
        {
            var game = await _gamesRepository.Get(gameId);
            if (game == null) { return NotFound(); }

            if (await _gamesRepository.TitleExists(dto.Title, excludeGameId: gameId))
                return Problem(detail: $"A game titled '{dto.Title}' already exists.", statusCode: 409);

            _mapper.Map(dto, game);
            await _gamesRepository.Update(game);

            return Ok(_mapper.Map<GameDto>(game));
        }

        [HttpDelete("{gameId}")]
        public async Task<IActionResult> Delete(int gameId)
        {
            var game = await _gamesRepository.Get(gameId);
            if (game == null) { return NotFound(); }

            await _gamesRepository.Delete(game);
            return NoContent();
        }

        [HttpPut("/api/rooms/{roomId}/stations/{stationId}/games/{gameId}")]
        public async Task<IActionResult> Install(int roomId, int stationId, int gameId)
        {
            var station = await _stationsRepository.GetWithGames(roomId, stationId);
            if (station == null) { return NotFound(); }

            var game = await _gamesRepository.Get(gameId);
            if (game == null) { return NotFound(); }

            if (!station.Games.Any(o => o.Id == gameId))
            {
                station.Games.Add(game);
                await _stationsRepository.Save();
            }

            return NoContent();
        }

        [HttpDelete("/api/rooms/{roomId}/stations/{stationId}/games/{gameId}")]
        public async Task<IActionResult> Uninstall(int roomId, int stationId, int gameId)
        {
            var station = await _stationsRepository.GetWithGames(roomId, stationId);
            if (station == null) { return NotFound(); }

            var game = station.Games.FirstOrDefault(o => o.Id == gameId);
            if (game == null) { return NotFound(); }

            station.Games.Remove(game);
            await _stationsRepository.Save();

            return NoContent();
        }
    }
}
