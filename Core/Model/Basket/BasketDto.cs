using System;
using System.Collections.Generic;

namespace Core.Model.Basket
{
    public class BasketDto
    {
        public BasketDto()
        {
            ProductList = new List<BasketProductDto>();
        }
        public int Id { get; set; }
        public Guid Guid { get; set; }
        public int? CustomerId { get; set; }
        public int? AddressId { get; set; }
        public int? OrderId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int StatusId { get; set; }
        public List<BasketProductDto> ProductList { get; set; }
    }
}
