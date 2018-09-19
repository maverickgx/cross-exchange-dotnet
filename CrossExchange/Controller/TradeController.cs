using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace CrossExchange.Controller
{
    [Route("api/Trade")]
    public class TradeController : ControllerBase
    {
        private IShareRepository _shareRepository { get; set; }
        private ITradeRepository _tradeRepository { get; set; }
        private IPortfolioRepository _portfolioRepository { get; set; }

        public TradeController(IShareRepository shareRepository, ITradeRepository tradeRepository, IPortfolioRepository portfolioRepository)
        {
            _shareRepository = shareRepository;
            _tradeRepository = tradeRepository;
            _portfolioRepository = portfolioRepository;
        }

        [HttpGet("{portfolioid}")]
        public async Task<IActionResult> GetAllTradings([FromRoute]int portFolioid)
        {
            var trade = await _tradeRepository.GetTradingsByPortfolioId(portFolioid);
            return Ok(trade);
        }

        /*************************************************************************************************************************************
        For a given portfolio, with all the registered shares you need to do a trade which could be either a BUY or SELL trade. For a particular trade keep following conditions in mind:
		BUY:
        OK a) The rate at which the shares will be bought will be the latest price in the database.
		OK b) The share specified should be a registered one otherwise it should be considered a bad request. 
		OK c) The Portfolio of the user should also be registered otherwise it should be considered a bad request. 
                
        SELL:
            a) The share should be there in the portfolio of the customer.
		OK  b) The Portfolio of the user should be registered otherwise it should be considered a bad request. 
		    c) The rate at which the shares will be sold will be the latest price in the database.
            d) The number of shares should be sufficient so that it can be sold. 
        Hint: You need to group the total shares bought and sold of a particular share and see the difference to figure out if there are sufficient quantities available for SELL. 

        *************************************************************************************************************************************/

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]TradeModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var trade = new Trade
            {
                Action = model.Action,
                NoOfShares = model.NoOfShares,
                Price = 0,
                Symbol = model.Symbol,
                PortfolioId = model.PortfolioId
            };

            Portfolio portfolio = await _portfolioRepository.GetAsync(1);
            HourlyShareRate share = await _shareRepository.GetLastSharesBySymbol(trade.Symbol);//).First();

            //Validate portfolio and Share exists
            //Also get last share rate
            if (portfolio != null && share != null) 
            {
                if (model.Action.Equals("SELL"))
                {
                    //Check if portfolio has enough shares and therefore exists in it
                    var tradesByPortfolio = _tradeRepository.Query()
                                                                .Where(x => x.PortfolioId.Equals(model.PortfolioId) && x.Symbol.Equals(model.Symbol))
                                                                .ToList();
                    int boughtShares = tradesByPortfolio.Where(a => a.Action.Equals("BUY")).Sum(b => b.NoOfShares);
                    int soldShares = tradesByPortfolio.Where(a => a.Action.Equals("SELL")).Sum(b => b.NoOfShares);

                    if (model.NoOfShares > (boughtShares - soldShares))
                    {
                        return BadRequest("Insufficient shares to sell!");
                    }
                }
                //Otherwise (BUY) 2 constraints have already been checked
                //If more options were needed I would used a Switch clause

                //Prepare trade to persist
                //HourlyShareRate share = shares.FirstOrDefault();
                trade.Price = share.Rate;
                await _tradeRepository.InsertAsync(trade);
                return Created("Trade", trade); 
            }
            else
            {
                return BadRequest("Portfolio or Share doesn't exist");
            }
        }
    }
}
