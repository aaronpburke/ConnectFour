using ConnectFour.Api.Models;
using System.Linq;

namespace ConnectFour.Api.Repositories
{
    /// <summary>
    /// Provides data-level access to all <seealso cref="Game"/>s.
    /// </summary>
    public interface IGameRepository
    {
        /// <summary>
        /// Adds a new game to the collection.
        /// </summary>
        /// <param name="game">New game to add.</param>
        /// <returns>true if the game was added; otherwise false (e.g., an game with that key already exists)</returns>
        bool Add(Game game);

        /// <summary>
        /// Removes an game from the collection.
        /// </summary>
        /// <param name="gameId">ID of the game to remove.</param>
        /// <returns>true if the game was removed; otherwise false (e.g., the game did not exist)</returns>
        bool Delete(string gameId);

        /// <summary>
        /// Gets a game from the collection by name.
        /// </summary>
        /// <param name="gameId">Name of the game to return.</param>
        /// <returns>game if it exists in the collection; otherwise null.</returns>
        Game Get(string gameId);

        /// <summary>
        /// Gets all games currently in the collection.
        /// </summary>
        /// <returns>All games in the collection.</returns>
        IQueryable<Game> GetAll();

        /// <summary>
        /// Updates the collection and replaces an game element.
        /// </summary>
        /// <param name="game">Replacement game.</param>
        /// <returns>true if the game previously existed and was updated; otherwise false (i.e., the game did not previously exist).</returns>
        bool Update(Game game);
    }
}
