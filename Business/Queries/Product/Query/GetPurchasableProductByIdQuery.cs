using Core.Model.Product;
using Core.Model.Response;
using MediatR;


namespace Business.Queries.Product
{
    public class GetPurchasableProductByIdQuery : IRequest<ResponseModel<bool>>
    {
        public int ProductId { get; set; }
        public int Count { get; set; }
    }
}
