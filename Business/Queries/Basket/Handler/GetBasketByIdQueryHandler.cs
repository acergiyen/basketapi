using Core.Model.Basket;
using Core.Model.Response;
using Data.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Business.Queries.BasketQuery
{
    public class GetBasketByIdQueryHandler : IRequestHandler<GetBasketByIdQuery, ResponseModel<BasketDto>>
    {
        private readonly Entities db;
        public GetBasketByIdQueryHandler(Entities db)
        {
            this.db = db;
        }

        public async Task<ResponseModel<BasketDto>> Handle(GetBasketByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var basket = await db.Basket.Include(q => q.BasketProduct).AsNoTracking().FirstOrDefaultAsync(q => q.Id == request.BasketId, cancellationToken);

                if (basket == null)
                {
                    return null;
                }
                BasketDto basketDto = new BasketDto();
                basketDto.Id = basket.Id;
                basketDto.OrderId = basket.OrderId;
                basketDto.Guid = basket.Guid;
                basketDto.AddressId = basket.AddressId;
                basketDto.CreateDate = basket.CreateDate;
                basketDto.CustomerId = basket.CustomerId;
                basketDto.StatusId = basket.StatusId;
                basketDto.UpdateDate = basket.UpdateDate;  
                foreach (var product in basket.BasketProduct)
                {
                    var basketProduct = new BasketProductDto
                    {
                        ProductId = product.ProductId,
                        Count = product.Count
                    };
                    basketDto.ProductList.Add(basketProduct);
                }

                return new ResponseModel<BasketDto>
                {
                    Result = basketDto
                };
            }
            catch (Exception ex)
            {

                return null;
            }

        }
    }
}
