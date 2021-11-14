namespace Core.Model.Basket
{
    public class BasketProductDto
    {
        public int Id { get; set; }
        public int BasketId { get; set; }
        public int ProductId { get; set; }
        public int Count { get; set; }
    }
}
