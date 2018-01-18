using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ToastCore.Models;

namespace ToastCore.Controllers
{
    [Produces("application/json")]
    
    public class SuperMarketController : Controller
    {
        /// <summary>
        /// Action of selling bread, from Supermarket (see api/pantry/breads/buy/nBreads)
        /// This method only returns a number of breads. 
        /// </summary>
        /// <param name="nBreads">Number of breads. It can't be more than 60 breads</param>
        /// <returns></returns>

        [HttpGet("/api/supermarket/breads/sell/{nBreads}")]
        public IActionResult SellBread(int nBreads)
        {
            //if (nBreads < 1) throw new Exception("I can't sell less 1 bread");

            if (nBreads > 60)
            {
                return StatusCode(500, "I can't sell more than 60 breads");
            }

            return StatusCode(200, nBreads);
        }
    }
}