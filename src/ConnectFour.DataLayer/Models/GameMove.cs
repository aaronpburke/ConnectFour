/*
 * 98Point6 Drop-Token
 *
 * At-home interview implementation of \"98Point6 Drop-Token\" homework assignment
 *
 * OpenAPI spec version: 1.0.0
 * Contact: aaron@focuszonedevelopment.com
 * Generated by: https://github.com/swagger-api/swagger-codegen.git
 */
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace ConnectFour.DataLayer.Models
{
    /// <summary>
    /// A single move within a game
    /// </summary>
    [DataContract]
    public partial class GameMove : IEntity<int>
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Foreign key back to the containing game board
        /// </summary>
        public int GameBoardId { get; set; }

        /// <summary>
        /// Foreign key back to the player who played the move
        /// </summary>
        [Required]
        public int PlayerId { get; set; }

        /// <summary>
        /// Sequence number of the move within its parent <seealso cref="Game"/>
        /// </summary>
        public int MoveId { get; set; }

        /// <summary>
        /// Type of the player move -- e.g., "move" (drop token) or quit
        /// </summary>
        public enum MoveType
        {
            /// <summary>
            /// The player dropped a token.
            /// </summary>
            [Display(Name = "Move")]
            MOVE,

            /// <summary>
            /// The player quit the game.
            /// </summary>
            [Display(Name = "Quit")]
            QUIT
        }

        /// <summary>
        /// Type of the player move
        /// </summary>
        [Required]
        public MoveType Type { get; set; }

        /// <summary>
        /// The column the token was dropped into, if <see cref="Type"/> is <see cref="MoveType.MOVE"/>
        /// </summary>
        [Required]
        public int Column { get; set; }
    }
}
