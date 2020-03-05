using ConnectFour.Api.Models;

namespace ConnectFour.Api.Repositories
{
    /// <summary>
    /// Collection of all games
    /// </summary>
    public class GameRepository : LocalCrudRepository<Game>, IGameRepository
    {
        /// <inheritdoc />
        protected override string GetKeyForItem(Game item)
        {
            return item.Id;
        }
    }
}
