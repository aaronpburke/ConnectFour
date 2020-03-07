using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace ConnectFour.DataLayer.Models
{
    /// <summary>
    /// Player in one or more <see cref="Game"/>
    /// </summary>
    [DataContract]
    public class Player : IEntity<int>
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Player name
        /// </summary>
        [Required]
        public string Name { get; set; }
    }
}
