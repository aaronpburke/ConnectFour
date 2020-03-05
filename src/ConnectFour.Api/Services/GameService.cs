using ConnectFour.Api.Models;
using ConnectFour.Api.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConnectFour.Api.Services
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
                throw new ArgumentNullException(nameof(newGameDetails));

            var game = new Game()
            {
                Id = Guid.NewGuid().ToString(),
                Columns = newGameDetails.Columns,
                Rows = newGameDetails.Rows,
                Players = newGameDetails.Players,
                State = Game.GameState.IN_PROGRESS
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
                throw new ArgumentException(nameof(gameId));
            if (moveNumber < 0)
                throw new ArgumentOutOfRangeException(nameof(moveNumber));

            var game = Get(gameId);
            if (game == null)
                return null;

            return game.Moves.ElementAtOrDefault(moveNumber);
        }

        /// <inheritdoc />
        public IEnumerable<GameMove> GetMoves(string gameId)
        {
            if (string.IsNullOrWhiteSpace(gameId))
                throw new ArgumentException(nameof(gameId));

            var game = Get(gameId);
            if (game == null)
                return null;

            return game.Moves;
        }

        /// <inheritdoc />
        public IEnumerable<GameMove> GetMoves(string gameId, int start, int until)
        {
            if (string.IsNullOrWhiteSpace(gameId))
                throw new ArgumentException(nameof(gameId));
            if (start < 0)
                throw new ArgumentOutOfRangeException(nameof(start));
            if (until < 0)
                throw new ArgumentOutOfRangeException(nameof(until));
            if (until < start)
                throw new ArgumentException("until cannot be less than start");

            var game = Get(gameId);
            if (game == null)
                return null;

            return new ArraySegment<GameMove>(game.Moves, start, until - start + 1);
        }

        /// <inheritdoc />
        public GameMove PlayMove(string gameId, string playerName, GameMove move)
        {
            if (string.IsNullOrWhiteSpace(gameId))
                throw new ArgumentException(nameof(gameId));
            if (string.IsNullOrWhiteSpace(playerName))
                throw new ArgumentException(nameof(playerName));
            if (move == null)
                throw new ArgumentNullException(nameof(move));

            var game = Get(gameId);
            if (game == null)
                return null;

            if (GetActivePlayer(game) != playerName)
            {
                throw new InvalidOperationException($"It is not {playerName}'s turn");
            }

            game.Moves.Append(move);
            move.MoveId = game.Moves.Length;

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
                throw new ArgumentNullException(nameof(game));

            // TODO: Check game state
            return game.Players.ElementAt(game.Moves.Length % game.Players.Count());
        }

        /// <inheritdoc />
        public void RemovePlayer(string gameId, string playerName)
        {
            if (string.IsNullOrWhiteSpace(gameId))
                throw new ArgumentException(nameof(gameId));
            if (string.IsNullOrWhiteSpace(playerName))
                throw new ArgumentException(nameof(playerName));

            var game = Get(gameId);
            if (game == null)
                throw new KeyNotFoundException($"{gameId} was not found");

            if (GetGameState(game) == Game.GameState.DONE)
            {
                throw new InvalidOperationException($"{gameId} has already finished; cannot remove player");
            }

            if (!game.Players.Remove(playerName))
            {
                throw new KeyNotFoundException($"{playerName} does not exist in game {gameId}");
            }
        }

        /// <inheritdoc />
        public Game.GameState GetGameState(Game game)
        {
            return string.IsNullOrEmpty(FindWinner(game)) || IsBoardFull(game)
                ? Game.GameState.DONE
                : Game.GameState.IN_PROGRESS;
        }

        /// <summary>
        /// Attempts to find a game winner, if any.
        /// </summary>
        /// <param name="game">Game for which to find the winner</param>
        /// <returns>The player name if a winner is found; otherwise null</returns>
        private string FindWinner(Game game)
        {
            string winner;

            winner = CheckColumnWin(game);
            if (winner != null)
                return winner;

            winner = CheckRowWin(game);
            if (winner != null)
                return winner;

            winner = CheckDiagonalWin(game);
            if (winner != null)
                return winner;

            return null;
        }

        /// <summary>
        /// Checks if the entire game board is full (i.e., game is over with no winner)
        /// </summary>
        /// <param name="game">Game containing the board to check</param>
        /// <returns>true if all columns are full; otherwise false</returns>
        private bool IsBoardFull(Game game)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks if a winner is found with a horizontal ("row") victory.
        /// </summary>
        /// <param name="game">Game containing the board to check</param>
        /// <returns>The player name if a winner is found; otherwise null</returns>
        private string CheckRowWin(Game game)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks if a winner is found with a vertical ("column") victory.
        /// </summary>
        /// <param name="game">Game containing the board to check</param>
        /// <returns>The player name if a winner is found; otherwise null</returns>
        private string CheckColumnWin(Game game)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks if a winner is found with a diagonal victory.
        /// </summary>
        /// <param name="game">Game containing the board to check</param>
        /// <returns>The player name if a winner is found; otherwise null</returns>
        private string CheckDiagonalWin(Game game)
        {
            throw new NotImplementedException();
        }
    }
}
