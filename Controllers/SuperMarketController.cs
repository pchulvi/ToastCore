using System;
using Microsoft.AspNetCore.Mvc;

namespace ToastCore.Controllers
{
    [Produces("application/json")]
    [Route("api/SuperMarket")]
    public class SuperMarketController : Controller
    {
        /// <summary>
        /// Action of selling bread, from Supermarket (see api/pantry/breads/buy/nBreads)
        /// This method only returns a number of breads. 
        /// </summary>
        /// <param name="nBreads">Number of breads. It can't be more than 60 breads</param>
        /// <returns></returns>
        [HttpGet]
        public int SellBread(int nBreads)
        {
            //if (nBreads < 1) throw new Exception("I can't sell less 1 bread");
            if (nBreads > 60) throw new Exception("I can't sell more than 60 breads");
            return nBreads;
        }
    }
}