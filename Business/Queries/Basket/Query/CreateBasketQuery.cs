using Core.Model.Basket;
using Core.Model.Response;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Business.Queries.BasketQuery
{
    public class CreateBasketQuery : IRequest<ResponseModel<BasketDto>>
    {
        public Guid Guid { get; set; }
        public int CustomerId { get; set; }
        public int AddressId { get; set; }
        public int OrderId { get; set; }
        public List<CreateBasketProductListModel> ProductList { get; set; }
    }
    public class CreateBasketProductListModel
    {
        public int ProductId { get; set; }
        public int Count { get; set; }

    }
}
