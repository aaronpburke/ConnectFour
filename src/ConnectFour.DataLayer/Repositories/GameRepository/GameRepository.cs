using ConnectFour.DataLayer.Models;
using System;
using System.Linq;

namespace ConnectFour.DataLayer.Repositories.GameRepository
{
    public interface IGameRepository : IGenericRepository<string, Game> { }
    public class GameRepository : GenericRepository<string, Game>, IGameRepository
    {
        public GameRepository(DataContext context)
            : base(context)
        {

        }

        /// <summary>
        /// Loads <see cref="Player"/> information into the <paramref name="game"/> from the backing store.
        /// </summary>
        /// <param name="game"><see cref="Game"/> for which to load the players</param>
        /// <returns><see cref="Game"/> with the <seealso cref="Player"/> information loaded.</returns>
        // TODO: Can this be a Game.WithPlayers extension method instead of a method on the repository? This would be more natural and allow easy chains:
        // _repository.GetById(id).WithPlayers() /* With...() */;
        public Game LoadPlayers(Game game)
        {
            if (game == null)
            {
                return null;
            }

            Context.Entry(game)
                .Collection(g => g.Players)
                .Load();

            return game;
        }

        /// <summary>
        /// Loads <see cref="GameBoard"/> information into the <paramref name="game"/> from the backing store.
        /// </summary>
        /// <param name="game"><see cref="Game"/> for which to load the game board</param>
        /// <returns><see cref="Game"/> with the <seealso cref="GameBoard"/> information loaded.</returns>
        // TODO: Can this be a Game.WithBoard extension method instead of a method on the repository? This would be more natural and allow easy chains:
        // _repository.GetById(id).WithBoard() /* With...() */;
        public Game LoadGameBoard(Game game)
        {
            if (game == null)
            {
                return null;
            }
            // TODO: How to optimize this into a single query in EF?

            Context.Entry(game)
                .Reference(g => g.GameBoard)
                .Load();

            Context.Entry(game)
                .Collection(g => g.Players)
                .Load();

            Context.Entry(game.GameBoard)
                .Collection(gb => gb.Moves)
                .Load();

            // Play each move to load in-memory state
            foreach (var move in game.GameBoard.Moves)
            {
                game.GameBoard.DropToken(move.Column, move.PlayerId);
            }

            return game;
        }
    }
}
