/*
 * 98Point6 Drop-Token
 *
 * At-home interview implementation of \"98Point6 Drop-Token\" homework assignment
 *
 * OpenAPI spec version: 1.0.0
 * Contact: aaron@focuszonedevelopment.com
 * Generated by: https://github.com/swagger-api/swagger-codegen.git
 */
using ConnectFour.Api.Attributes;
using ConnectFour.Api.Models.DTOs;
using ConnectFour.ServiceLayer.GameService;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ConnectFour.Api.Controllers
{
    /// <summary>
    /// The GameManagementApiController is responsible for overall game management 
    /// (e.g., creating a game, retrieving a game in progress).
    /// </summary>
    [ApiController]
    public class GameManagementApiController : ControllerBase
    {
        private readonly IGameService _gameService;

        /// <summary>
        /// Dependency injection constructor
        /// </summary>
        /// <param name="gameService"><see cref="IGameService"/> used to manage individual games</param>
        public GameManagementApiController(IGameService gameService)
        {
            _gameService = gameService;
        }

        /// <summary>
        /// Get all in-progress games.
        /// </summary>
        /// <response code="200">Returns all in-progress games.</response>
        [HttpGet]
        [Route("/drop-token/")]
        [ValidateModelState]
        [SwaggerOperation("GetAllGames")]
        [SwaggerResponse(statusCode: 200, type: typeof(List<string>), description: "Returns all in-progress games.")]
        public virtual IActionResult GetAllGames()
        {
            return Ok(_gameService.GetAll().Select(g => g.Id));
        }

        /// <summary>
        /// Get the state of a game.
        /// </summary>
        /// <param name="gameId">ID of the game to get the state of</param>
        /// <response code="200">Returns the specified game state.</response>
        /// <response code="400">Malformed request.</response>
        /// <response code="404">Game not found.</response>
        [HttpGet]
        [Route("/drop-token/{gameId}")]
        [ValidateModelState]
        [SwaggerOperation("GetGame")]
        [SwaggerResponse(statusCode: 200, type: typeof(GameDetails), description: "Returns the specified game state.")]
        public virtual IActionResult GetGame([FromRoute][Required]string gameId)
        {
            var game = _gameService.Get(gameId);
            if (game == null)
                return NotFound();

            var details = new GameDetails()
            {
                Players = game.Players,
                // TODO: Determine game state and winner, if applicable
                State = _gameService.GetGameState(game),
                Winner = ""
            };
            return Ok(details);
        }

        /// <summary>
        /// Start a new game
        /// </summary>
        /// <param name="newGameDetails">Game configuration, e.g., player names, board size</param>
        /// <response code="200">Returns the new game resource.</response>
        /// <response code="400">Malformed request.</response>
        [HttpPost]
        [Route("/drop-token/")]
        [ValidateModelState]
        [SwaggerOperation("StartNewGame")]
        [SwaggerResponse(statusCode: 200, type: typeof(string), description: "Returns the new game resource.")]
        public virtual IActionResult StartNewGame([FromBody]NewGameDetails newGameDetails)
        {
            var newGameId = _gameService.CreateNewGame(newGameDetails);
            return CreatedAtAction(nameof(GetGame), new { gameId = newGameId }, newGameId);
        }
    }
}
