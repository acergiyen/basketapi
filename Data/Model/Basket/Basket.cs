using System;
using System.Collections.Generic;

namespace Data.Model.Basket
{
    public class Basket
    {
        public Basket()
        {
            BasketProduct = new HashSet<BasketProduct>();
        }
        public int Id { get; set; }
        public Guid Guid { get; set; }
        public int? CustomerId { get; set; }
        public int? AddressId { get; set; }
        public int? OrderId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int StatusId { get; set; }
        public ICollection<BasketProduct> BasketProduct { get; set; }
    }
}
