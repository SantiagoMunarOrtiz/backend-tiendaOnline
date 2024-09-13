namespace TiendaOnlineAPI.Models
{
    public class WishList
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
