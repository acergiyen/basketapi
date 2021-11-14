using Core.Model.Basket;
using Core.Model.Response;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Queries.BasketQuery
{
    public class GetBasketByIdQuery : IRequest<ResponseModel<BasketDto>>
    { 
        public int BasketId { get; set; }

    }
}
