using ConnectFour.DataLayer;
using ConnectFour.DataLayer.Models;
using ConnectFour.DataLayer.Repositories.GameRepository;
using System;
using System.Collections.Generic;

namespace ConnectFour.ServiceLayer.GameService
{
    /// <summary>
    /// Provides business-layer logic for maintaining a <see cref="Game"/>.
    /// </summary>
    public interface IGameService : IGenericRepository<string, Game>
    {
        /// <summary>
        /// Creates a new game with the specified <paramref name="newGameDetails"/>.
        /// </summary>
        /// <param name="newGameDetails">New game details</param>
        /// <returns>The ID of the newly created game</returns>
        /// <exception cref="ArgumentNullException"> if <paramref name="newGameDetails"/> is null</exception>
        /// <exception cref="InvalidOperationException"> if game could not be created</exception>
        string CreateNewGame(NewGameDetails newGameDetails);

        /// <summary>
        /// Returns the <paramref name="moveNumber"/> sequenced <see cref="GameMove"/> action of the <paramref name="gameId"/>.
        /// Returns null if the game or move number does not exist.
        /// </summary>
        /// <param name="gameId">ID of the game to get the move from</param>
        /// <param name="moveNumber">Sequence number of the move to return (0-based)</param>
        /// <returns>Sequenced <see cref="GameMove"/>, or null if the game or move does not exist</returns>
        GameMove GetMove(string gameId, int moveNumber);

        /// <summary>
        /// Returns the entire sequence of <see cref="GameMove"/>s of the <paramref name="gameId"/>.
        /// Returns null if the game does not exist.
        /// </summary>
        /// <param name="gameId">ID of the game to get the move from</param>
        /// <returns>Sequenced <see cref="GameMove"/>, or null if the game or move does not exist</returns>
        IEnumerable<GameMove> GetMoves(string gameId);

        /// <summary>
        /// Returns the (sub-) sequence of <see cref="GameMove"/>s of the <paramref name="gameId"/>.
        /// Returns null if the game does not exist.
        /// </summary>
        /// <param name="gameId">ID of the game to get the move from</param>
        /// <param name="start">Starting move to return (inclusive)</param>
        /// <param name="until">Ending move to return (inclusive)</param>
        /// <returns>Sequenced <see cref="GameMove"/>, or null if the game or move does not exist</returns>
        IEnumerable<GameMove> GetMoves(string gameId, int start, int until);

        /// <summary>
        /// Plays a <paramref name="move"/> within the specified <paramref name="gameId"/>.
        /// </summary>
        /// <param name="gameId">Game ID to play the move within</param>
        /// <param name="playerName">Name of the player playing the move</param>
        /// <param name="move">New move to play</param>
        /// <returns>The <paramref name="move"/> with its sequence ID assigned</returns>
        GameMove PlayMove(string gameId, string playerName, GameMove move);

        /// <summary>
        /// Removes the <paramref name="playerName"/> from the specified <paramref name="gameId"/>.
        /// </summary>
        /// <param name="gameId">Game from which to remove the player</param>
        /// <param name="playerName">Name of the player to remove</param>
        void RemovePlayer(string gameId, string playerName);

        /// <summary>
        /// Gets the game completion state of the specified <paramref name="game"/>.
        /// </summary>
        /// <param name="game"><see cref="Game"/> for which to retrieve the game completion state</param>
        /// <returns><see cref="Game.GameState.DONE"/> if a winner is found; otherwise <see cref="Game.GameState.IN_PROGRESS"/></returns>
        Game.GameState GetGameState(Game game);
    }
}
