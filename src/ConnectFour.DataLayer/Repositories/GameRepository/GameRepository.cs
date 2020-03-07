using ConnectFour.DataLayer.Models;

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
                .Collection(d => d.Players)
                .Load();

            return game;
        }
    }
}
