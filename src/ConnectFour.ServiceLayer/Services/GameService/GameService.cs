using ConnectFour.DataLayer;
using ConnectFour.DataLayer.Models;
using ConnectFour.DataLayer.Repositories.GameRepository;
using ConnectFour.ServiceLayer.Models;
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
        private readonly GameOptions _gameOptions;

        public GameService(DataContext context, GameOptions gameOptions)
            : base(context)
        {
            _gameOptions = gameOptions;
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
                GameBoard = new GameBoard(newGameDetails.Rows, newGameDetails.Columns, _gameOptions.WinningChainLength),
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
            LoadGameBoard(game);

            var details = new GameDetails()
            {
                Players = game.Players.Select(p => p.Name),
                State = GetGameState(game),
            };

            var winner = game.GameBoard.GetWinner();
            if (winner != GameBoard.NO_WINNER)
            {
                details.Winner = game.Players.FirstOrDefault(p => p.Id == winner)?.Name;
            }

            return details;
        }

        /// <inheritdoc />
        public GameMoveDetails GetMove(string gameId, int moveNumber)
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

            var move = game.GameBoard.Moves.ElementAtOrDefault(moveNumber);
            if (move == null)
            {
                return null;
            }

            return GameMoveDetails.FromEntity(game, move);
        }

        /// <inheritdoc />
        public IEnumerable<GameMoveDetails> GetMoves(string gameId)
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

            return game.GameBoard.Moves.Select(move => GameMoveDetails.FromEntity(game, move));
        }

        /// <inheritdoc />
        public IEnumerable<GameMoveDetails> GetMoves(string gameId, int start, int until)
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

            // TODO: This could be more efficient by only returning the desired rows from the database
            // instead of loading them all into memory, then converting to array, then segmenting it
            var moves = new ArraySegment<GameMove>(game.GameBoard.Moves.ToArray(), start, until - start + 1);
            return moves.Select(move => GameMoveDetails.FromEntity(game, move));
        }

        /// <inheritdoc />
        public GameMove PlayMove(string gameId, GameMoveDetails newMove)
        {
            if (string.IsNullOrWhiteSpace(gameId))
            {
                throw new ArgumentException(nameof(gameId));
            }

            if (newMove == null)
            {
                throw new ArgumentNullException(nameof(newMove));
            }

            var game = GetById(gameId);
            if (game == null)
            {
                return null;
            }

            if (GetActivePlayer(game).Name != newMove.Player)
            {
                throw new PlayerTurnException($"It is not {newMove.Player}'s turn");
            }

            if (GetGameState(game) == Game.GameState.DONE)
            {
                throw new InvalidOperationException($"Game is finished! Cannot play new moves.");
            }

            var player = game.Players.SingleOrDefault(p => p.Name == newMove.Player);
            if (player == null)
            {
                throw new PlayerNotFoundException($"{newMove.Player} does not exist in game {gameId}");
            }

            if (!game.GameBoard.DropToken(newMove.Column, player.Id))
            {
                throw new InvalidOperationException($"Could not place token in column {newMove.Column}; column is full");
            }

            var move = new GameMove()
            {
                Column = newMove.Column,
                PlayerId = player.Id,
                MoveId = game.GameBoard.Moves.Count() + 1, // TODO: This smells inefficient - is there a better way than re-counting? Check the generated SQL.
                Type = newMove.Type
            };

            game.GameBoard.Moves.Add(move);
            game.State = GetGameState(game);

            Context.SaveChanges();  // TODO: Leaky abstraction; move to data layer?

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
            // If the game is already over, there's no need to load it. It can't go back to NOT over.
            if (game.State == Game.GameState.DONE)
            {
                return game.State;
            }

            // Load the game board if we don't know whether the game is over yet.
            if (game.GameBoard == null)
            {
                LoadGameBoard(game);
            }

            return game.GameBoard.HasWinner() || game.GameBoard.IsFull()
                ? Game.GameState.DONE
                : Game.GameState.IN_PROGRESS;
        }
    }
}
