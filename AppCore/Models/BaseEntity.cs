using System;

namespace AppCore.Models
{
    /// <summary>
    /// Base entity class that all domain entities will inherit from
    /// </summary>
    public abstract class BaseEntity : IEntity
    {
        /// <summary>
        /// Unique identifier for the entity
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Date and time when the entity was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Date and time when the entity was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
