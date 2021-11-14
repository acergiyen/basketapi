using Business.Queries.Product;
using Core.Enum;
using Core.Model.Basket;
using Core.Model.Response;
using Data.Entities;
using Data.Model.Basket;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Business.Queries.BasketQuery
{
    public class CreateBasketQueryHandler : IRequestHandler<CreateBasketQuery, ResponseModel<BasketDto>>
    {
        private readonly Entities db;
        private readonly IMediator mediator;
        public CreateBasketQueryHandler(Entities db, IMediator mediator)
        {
            this.db = db;
            this.mediator = mediator;
        }

        public async Task<ResponseModel<BasketDto>> Handle(CreateBasketQuery request, CancellationToken cancellationToken)
        {
            Basket basket = await db.Basket.Include(x => x.BasketProduct).FirstOrDefaultAsync(x => x.Guid == request.Guid, cancellationToken);
            if (basket == null)
            {
                return await InsertProductAsync(request, cancellationToken);
            }
            return await UpdateBasketAsync(basket, request, cancellationToken);
        }
        /// <summary>
        /// Insert Product 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<ResponseModel<BasketDto>> InsertProductAsync(CreateBasketQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var basket = new Basket
                {
                    AddressId = request.AddressId,
                    CreateDate = DateTime.Now,
                    CustomerId = request.CustomerId,
                    OrderId = request.OrderId,
                    StatusId = (int)BasketStatus.Active,
                    Guid = Guid.NewGuid()
                };

                foreach (var product in request.ProductList)
                {
                    var basketProduct = new Data.Model.Basket.BasketProduct
                    {
                        Count = product.Count,
                        ProductId = product.ProductId,
                        BasketId = basket.Id
                    };
                    basket.BasketProduct.Add(basketProduct);
                }
                await db.Basket.AddAsync(basket, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return await mediator.Send(new GetBasketByIdQuery
                {
                    BasketId = basket.Id
                });
            }
            catch (Exception)
            {
                return null;
            }


        }
        /// <summary>
        /// Update Basket
        /// </summary>
        /// <param name="basket"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<ResponseModel<BasketDto>> UpdateBasketAsync(Data.Model.Basket.Basket basket, CreateBasketQuery request, CancellationToken cancellationToken)
        {
            try
            {
                basket.AddressId = request.AddressId;
                basket.CustomerId = request.CustomerId;
                basket.OrderId = request.OrderId;
                basket.StatusId = (int)BasketStatus.Edited;
                basket.UpdateDate = DateTime.Now;

                var basketProducts = db.BasketProduct.Where(q => q.BasketId == basket.Id).ToList();
                // insufficient stock
                foreach (var basketproduct in basketProducts)
                {
                    foreach(var product in request.ProductList)
                    {
                        if (basketproduct.ProductId == product.ProductId)
                        {
                            var _product = await db.Product.Where(x => x.Id == product.ProductId).FirstOrDefaultAsync(cancellationToken);
                            if(basketproduct.Count+ product.Count <= _product.Count)
                            {
                                continue;
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }

                }
                db.BasketProduct.RemoveRange(basketProducts);

                foreach (var product in request.ProductList)
                {
                    var basketProduct = new BasketProduct
                    {
                        ProductId = product.ProductId,
                        Count = product.Count
                    };

                    basket.BasketProduct.Add(basketProduct);
                }

                await db.SaveChangesAsync(cancellationToken);

                return await mediator.Send(new GetBasketByIdQuery
                {
                    BasketId = basket.Id
                });
            }
            catch (Exception ex)
            {

                return null;
            }
           
        }
        /// <summary>
        /// validator
        /// </summary>
        public class BasketSaveValidation : AbstractValidator<CreateBasketQuery>
        {
            private readonly IMediator mediator;
            public BasketSaveValidation(IMediator mediator)
            {
                this.mediator = mediator;
                RuleFor(q => q.ProductList).NotNull().WithMessage("product list cannot be empty");
                RuleFor(q => q.ProductList).MustAsync(CheckProductInDb).When(q => q.ProductList != null).WithMessage("There are not enough products.");
            }

            private async Task<bool> CheckProductInDb(List<CreateBasketProductListModel> ProductList, CancellationToken cancellationToken)
            {
                foreach (var product in ProductList)
                {
                    var isPurchasable = await mediator.Send(new GetPurchasableProductByIdQuery { Count = product.Count, ProductId = product.ProductId });
                    if (!isPurchasable.Result)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
    }
}
