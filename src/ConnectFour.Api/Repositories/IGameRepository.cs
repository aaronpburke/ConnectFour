using ConnectFour.Api.Models;
using System.Linq;

namespace ConnectFour.Api.Repositories
{
    public interface IGameRepository
    {
        bool Create(Game item);
        bool Delete(string id);
        IQueryable<Game> GetAll();
        Game Get(string id);
    }
}
