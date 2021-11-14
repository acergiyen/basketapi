using Business.Queries.Product;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Core.Model.Response;
using MediatR;
using System.Threading;

namespace Business.Queries.ProductQuery
{

    public class GetPurchasableProductByIdQueryHandler : IRequestHandler<GetPurchasableProductByIdQuery , ResponseModel<bool>>
    {
        private readonly Entities db;
        public GetPurchasableProductByIdQueryHandler(Entities db)
        {
            this.db = db;
        }

        public async Task<ResponseModel<bool>> Handle(GetPurchasableProductByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var product = await db.Product.AsNoTracking().FirstOrDefaultAsync(q => q.Id == request.ProductId, cancellationToken);
                if (product.Count >= request.Count)
                {
                    return new ResponseModel<bool> { Result = true };
                }
                return new ResponseModel<bool> { Result = false };
            }
            catch (System.Exception)
            {

                return new ResponseModel<bool> { Result = false };
            }
          
        }
    }
}
