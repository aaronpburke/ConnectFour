using System;

namespace ConnectFour.ServiceLayer
{
    /// <summary>
    /// Exception thrown when a player attempts to play a move, but it is not their turn.
    /// </summary>
    public class PlayerTurnException : Exception
    {
        /// <inheritdoc />
        public PlayerTurnException()
        {
        }

        /// <inheritdoc />
        public PlayerTurnException(string message)
            : base(message)
        {
        }

        /// <inheritdoc />
        public PlayerTurnException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    /// <summary>
    /// Exception thrown when a player attempts to play a move, but they are not in the game.
    /// </summary>
    public class PlayerNotFoundException : Exception
    {
        /// <inheritdoc />
        public PlayerNotFoundException()
        {
        }

        /// <inheritdoc />
        public PlayerNotFoundException(string message)
            : base(message)
        {
        }

        /// <inheritdoc />
        public PlayerNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
