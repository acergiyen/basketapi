namespace Data.Model.Basket
{
    public class BasketProduct
    {
        public int Id { get; set; }
        public int BasketId { get; set; }
        public int ProductId { get; set; }
        public int Count { get; set; }
        public Basket Basket { get; set; }
    }
}
