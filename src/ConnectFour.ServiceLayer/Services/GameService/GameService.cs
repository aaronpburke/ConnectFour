using ConnectFour.DataLayer.Models;
using ConnectFour.DataLayer.Repositories.GameRepository;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConnectFour.ServiceLayer.GameService
{
    /// <summary>
    /// Provides business-layer logic for maintaining a <see cref="Game"/>.
    /// </summary>
    public class GameService : GameRepository, IGameService
    {
        /// <inheritdoc />
        public string CreateNewGame(NewGameDetails newGameDetails)
        {
            if (newGameDetails == null)
            {
                throw new ArgumentNullException(nameof(newGameDetails));
            }

            var game = new Game()
            {
                Id = Guid.NewGuid().ToString(),
                Players = newGameDetails.Players,
                State = Game.GameState.IN_PROGRESS,
                Board = new GameBoard(newGameDetails.Rows, newGameDetails.Columns)
            };

            if (!Add(game))
            {
                throw new InvalidOperationException("could not add new game!");
            }

            return game.Id;
        }

        /// <inheritdoc />
        public GameMove GetMove(string gameId, int moveNumber)
        {
            if (string.IsNullOrWhiteSpace(gameId))
            {
                throw new ArgumentException(nameof(gameId));
            }

            if (moveNumber < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(moveNumber));
            }

            var game = Get(gameId);
            if (game == null)
            {
                return null;
            }

            return game.Moves.ElementAtOrDefault(moveNumber);
        }

        /// <inheritdoc />
        public IEnumerable<GameMove> GetMoves(string gameId)
        {
            if (string.IsNullOrWhiteSpace(gameId))
            {
                throw new ArgumentException(nameof(gameId));
            }

            var game = Get(gameId);
            if (game == null)
            {
                return null;
            }

            return game.Moves;
        }

        /// <inheritdoc />
        public IEnumerable<GameMove> GetMoves(string gameId, int start, int until)
        {
            if (string.IsNullOrWhiteSpace(gameId))
            {
                throw new ArgumentException(nameof(gameId));
            }

            if (start < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(start));
            }

            if (until < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(until));
            }

            if (until < start)
            {
                throw new ArgumentException("until cannot be less than start");
            }

            var game = Get(gameId);
            if (game == null)
            {
                return null;
            }

            return new ArraySegment<GameMove>(game.Moves, start, until - start + 1);
        }

        /// <inheritdoc />
        public GameMove PlayMove(string gameId, string playerName, GameMove move)
        {
            if (string.IsNullOrWhiteSpace(gameId))
            {
                throw new ArgumentException(nameof(gameId));
            }

            if (string.IsNullOrWhiteSpace(playerName))
            {
                throw new ArgumentException(nameof(playerName));
            }

            if (move == null)
            {
                throw new ArgumentNullException(nameof(move));
            }

            var game = Get(gameId);
            if (game == null)
            {
                return null;
            }

            if (GetActivePlayer(game) != playerName)
            {
                throw new PlayerTurnException($"It is not {playerName}'s turn");
            }

            // TODO: Consider making this a HashSet with faster lookup if we think we might have a LOT of players
            var playerId = game.Players.IndexOf(playerName);
            if (playerId == -1)
            {
                throw new PlayerNotFoundException($"{playerName} does not exist in game {gameId}");
            }
            if (!game.Board.DropToken(move.Column, playerId))
            {
                throw new InvalidOperationException($"Could not place token in column {move.Column}; column is full");
            }

            game.Moves.Append(move);
            move.MoveId = game.Moves.Length;

            // TODO: Check if winner and update game state

            return move;
        }

        /// <summary>
        /// Gets the active player for the specified <paramref name="game"/>.
        /// </summary>
        /// <param name="game">Game for which to retrieve the currently active player</param>
        /// <returns>The name of the currently active player</returns>
        private string GetActivePlayer(Game game)
        {
            if (game == null)
            {
                throw new ArgumentNullException(nameof(game));
            }

            // TODO: Check game state

            return game.Players.ElementAt(game.Moves.Length % game.Players.Count());
        }

        /// <inheritdoc />
        public void RemovePlayer(string gameId, string playerName)
        {
            if (string.IsNullOrWhiteSpace(gameId))
            {
                throw new ArgumentException(nameof(gameId));
            }

            if (string.IsNullOrWhiteSpace(playerName))
            {
                throw new ArgumentException(nameof(playerName));
            }

            var game = Get(gameId);
            if (game == null)
            {
                throw new KeyNotFoundException($"{gameId} was not found");
            }

            if (GetGameState(game) == Game.GameState.DONE)
            {
                throw new InvalidOperationException($"{gameId} has already finished; cannot remove player");
            }

            if (!game.Players.Remove(playerName))
            {
                throw new PlayerNotFoundException($"{playerName} does not exist in game {gameId}");
            }
        }

        /// <inheritdoc />
        public Game.GameState GetGameState(Game game)
        {
            return game.Board.HasWinner() || game.Board.IsFull()
                ? Game.GameState.DONE
                : Game.GameState.IN_PROGRESS;
        }
    }
}
