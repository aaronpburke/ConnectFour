using ConnectFour.DataLayer;
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
        public GameService(DataContext context)
            : base(context)
        {
        }

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
                State = Game.GameState.IN_PROGRESS,
                GameBoard = new GameBoard(newGameDetails.Rows, newGameDetails.Columns),
                Players = new List<Player>(newGameDetails.Players.Select(playerName => new Player() { Name = playerName }))
            };

            Create(game);

            return game.Id;
        }

        public GameDetails GetGameDetails(string gameId)
        {
            if (string.IsNullOrWhiteSpace(gameId))
            {
                throw new ArgumentException(nameof(gameId));
            }

            var game = GetById(gameId);
            if (game == null)
            {
                return null;
            }

            LoadPlayers(game);

            var details = new GameDetails()
            {
                Players = game.Players.Select(p => p.Name),
                // TODO: Determine game state and winner, if applicable
                State = Game.GameState.IN_PROGRESS, //GetGameState(game),
                Winner = ""
            };

            return details;
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

            var game = GetById(gameId);
            if (game == null)
            {
                return null;
            }

            LoadGameBoard(game);
            LoadGameMoves(game.GameBoard);

            return game.GameBoard.Moves.ElementAtOrDefault(moveNumber);
        }

        /// <inheritdoc />
        public IEnumerable<GameMove> GetMoves(string gameId)
        {
            if (string.IsNullOrWhiteSpace(gameId))
            {
                throw new ArgumentException(nameof(gameId));
            }

            var game = GetById(gameId);
            if (game == null)
            {
                return null;
            }

            LoadGameBoard(game);
            LoadGameMoves(game.GameBoard);

            return game.GameBoard.Moves;
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

            var game = GetById(gameId);
            if (game == null)
            {
                return null;
            }

            LoadGameBoard(game);
            LoadGameMoves(game.GameBoard);

            // TODO: This could be more efficient by only returning the desired rows from the database
            // instead of loading them all into memory, then converting to array, then segmenting it
            return new ArraySegment<GameMove>(game.GameBoard.Moves.ToArray(), start, until - start + 1);
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

            var game = GetById(gameId);
            if (game == null)
            {
                return null;
            }

            if (GetActivePlayer(game).Name != playerName)
            {
                throw new PlayerTurnException($"It is not {playerName}'s turn");
            }

            var player = game.Players.SingleOrDefault(p => p.Name == playerName);
            if (player == null)
            {
                throw new PlayerNotFoundException($"{playerName} does not exist in game {gameId}");
            }
            if (!game.GameBoard.DropToken(move.Column, player.Id))
            {
                throw new InvalidOperationException($"Could not place token in column {move.Column}; column is full");
            }

            game.GameBoard.Moves.Add(move);
            move.MoveId = game.GameBoard.Moves.Count(); // TODO: This smells inefficient - is there a better way than re-counting? Check the generated SQL.
            Context.SaveChanges();  // TODO: Leaky abstraction; move to data layer?

            // TODO: Check if winner and update game state

            return move;
        }

        /// <summary>
        /// Gets the active player for the specified <paramref name="game"/>.
        /// </summary>
        /// <param name="game">Game for which to retrieve the currently active player</param>
        /// <returns>The currently active <see cref="Player"/></returns>
        private Player GetActivePlayer(Game game)
        {
            if (game == null)
            {
                throw new ArgumentNullException(nameof(game));
            }

            LoadPlayers(game);
            LoadGameBoard(game);
            LoadGameMoves(game.GameBoard);

            // TODO: Check game state

            return game.Players.ElementAt(game.GameBoard.Moves.Count() % game.Players.Count());
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

            var game = GetById(gameId);
            if (game == null)
            {
                throw new KeyNotFoundException($"{gameId} was not found");
            }

            LoadPlayers(game);

            if (GetGameState(game) == Game.GameState.DONE)
            {
                throw new InvalidOperationException($"{gameId} has already finished; cannot remove player");
            }

            var player = game.Players.FirstOrDefault(p => p.Name == playerName);
            if (player == null)
            {
                throw new PlayerNotFoundException($"{playerName} does not exist in game {gameId}");
            }

            game.Players.Remove(player);
            Context.SaveChanges();  // TODO: Leaky abstraction; move to data layer?
        }

        /// <inheritdoc />
        public Game.GameState GetGameState(Game game)
        {
            return game.GameBoard.HasWinner() || game.GameBoard.IsFull()
                ? Game.GameState.DONE
                : Game.GameState.IN_PROGRESS;
        }
    }
}
