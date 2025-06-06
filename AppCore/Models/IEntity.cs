namespace AppCore.Models
{
    /// <summary>
    /// Interface for entities that can be stored in a repository
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// Unique identifier for the entity
        /// </summary>
        int Id { get; set; }
    }
}
