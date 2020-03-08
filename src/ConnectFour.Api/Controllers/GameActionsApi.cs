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
using ConnectFour.DataLayer.Models;
using ConnectFour.ServiceLayer;
using ConnectFour.ServiceLayer.GameService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ConnectFour.Api.Controllers
{
    /// <summary>
    /// The GameActionsApiController is responsible for all operations that progress a single <seealso cref="Game"/>.
    /// </summary>
    [ApiController]
    public class GameActionsApiController : ControllerBase
    {
        private readonly IGameService _gameService;

        /// <summary>
        /// Dependency injection constructor
        /// </summary>
        /// <param name="gameService"><see cref="IGameService"/> used to manage individual games</param>
        public GameActionsApiController(IGameService gameService)
        {
            _gameService = gameService;
        }

        /// <summary>
        /// Get a previously played move.
        /// </summary>
        /// <param name="gameId">ID of the game to get move from</param>
        /// <param name="moveNumber">Move sequence number to get</param>
        /// <response code="200">Returns the requested game move.</response>
        /// <response code="400">Malformed request.</response>
        /// <response code="404">Game/move not found</response>
        [HttpGet]
        [Route("/drop-token/{gameId}/moves/{moveNumber}")]
        [ValidateModelState]
        [SwaggerOperation("GetMove")]
        [SwaggerResponse(statusCode: 200, type: typeof(GameMove), description: "Returns the requested game move.")]
        public virtual IActionResult GetMove([FromRoute][Required]string gameId, [FromRoute][Required]int moveNumber)
        {
            var move = _gameService.GetMove(gameId, moveNumber);
            if (move == null)
            {
                return NotFound();
            }

            return Ok(move);
        }

        /// <summary>
        /// Get a (sub-)list of the moves played.
        /// </summary>
        /// <param name="gameId">ID of the game to get moves from</param>
        /// <param name="start">Starting move to return (inclusive). If specified, <paramref name="until"/> must also be specified.</param>
        /// <param name="until">Ending move to return (inclusive). If specified, <paramref name="start"/> must also be specified.</param>
        /// <response code="200">Returns the requested game moves.</response>
        /// <response code="400">Malformed request.</response>
        /// <response code="404">Game/moves not found</response>
        [HttpGet]
        [Route("/drop-token/{gameId}/moves")]
        [ValidateModelState]
        [SwaggerOperation("GetMoves")]
        [SwaggerResponse(statusCode: 200, type: typeof(List<GameMove>), description: "Returns the requested game moves.")]
        public virtual IActionResult GetMoves([FromRoute][Required]string gameId, [FromQuery]int? start, [FromQuery]int? until)
        {
            IEnumerable<GameMove> moves;
            if (start.HasValue && until.HasValue)
            {
                moves = _gameService.GetMoves(gameId, start.Value, until.Value);
            }
            // If start or until is specified, the other must be specified too
            else if (start.HasValue || until.HasValue)
            {
                return BadRequest();
            }
            else
            {
                moves = _gameService.GetMoves(gameId);
            }

            if (moves == null)
            {
                return NotFound();
            }

            return Ok(moves);
        }

        /// <summary>
        /// Play a new move.
        /// </summary>
        /// <param name="gameId">ID of the game to play move on</param>
        /// <param name="playerName">Name of the player playing the move</param>
        /// <param name="gameMove">Move to play</param>
        /// <response code="200">Returns the new move.</response>
        /// <response code="400">Malformed input. Illegal move.</response>
        /// <response code="404">Game not found or player is not a part of it.</response>
        /// <response code="409">Player tried to act when it's not their turn.</response>
        [HttpPost]
        [Route("/drop-token/{gameId}/moves/{playerName}")]
        [ValidateModelState]
        [SwaggerOperation("PlayMove")]
        [SwaggerResponse(statusCode: 200, type: typeof(GameMove), description: "Returns the new move.")]
        public virtual IActionResult PlayMove([FromRoute][Required]string gameId, [FromRoute][Required]string playerName, [FromBody][Required]GameMove gameMove)
        {
            try
            {
                var move = _gameService.PlayMove(gameId, playerName, gameMove);
                if (move == null)
                {
                    return NotFound();
                }

                return CreatedAtAction(nameof(GetMove), new { gameId, moveNumber = move.MoveId }, move);
            }
            catch (PlayerTurnException)
            {
                return Conflict();
            }
            catch (PlayerNotFoundException)
            {
                return NotFound();
            }
            // InvalidOperationException means the play was invalid
            catch (InvalidOperationException)
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// Quit player from the game.
        /// </summary>
        /// <param name="gameId">ID of the game to quit</param>
        /// <param name="playerName">Name of the player quitting the game</param>
        /// <response code="202">Player quit.</response>
        /// <response code="404">Game not found or player is not a part of it.</response>
        /// <response code="410">Game is already in DONE state.</response>
        [HttpDelete]
        [Route("/drop-token/{gameId}/{playerName}")]
        [ValidateModelState]
        [SwaggerOperation("PlayerQuit")]
        public virtual IActionResult PlayerQuit([FromRoute][Required]string gameId, [FromRoute][Required]string playerName)
        {
            try
            {
                _gameService.RemovePlayer(gameId, playerName);
            }
            // KeyNotFoundException means the player did not exist in the requested game
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (PlayerNotFoundException)
            {
                return NotFound();
            }
            // InvalidOperationException means the game is already completed, so the player cannot quit
            catch (InvalidOperationException)
            {
                return StatusCode(StatusCodes.Status410Gone);
            }

            return Accepted();
        }
    }
}
