using CrossExchange.Controller;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Linq;

namespace CrossExchange.Tests
{
    public class TradeControllerTests
    {
        private readonly TradeController _tradeController;

        private readonly Mock<ITradeRepository> _tradeRepositoryMock = new Mock<ITradeRepository>();
        private readonly Mock<IShareRepository> _shareRepositoryMock = new Mock<IShareRepository>();
        private readonly Mock<IPortfolioRepository> _portfolioRepositoryMock = new Mock<IPortfolioRepository>();

        //Valid data
        static int validPortfolioId = 1;
        String portfolioName = "Portfolio test";
        static String share = "CBI";
        HourlyShareRate rate1 = new HourlyShareRate(){ Id = 1, TimeStamp = new DateTime(2018, 09, 17, 0, 0, 0), Symbol = share, Rate = 100.0M };
        HourlyShareRate rate2 = new HourlyShareRate() { Id = 2, TimeStamp = new DateTime(2018, 09, 18, 0, 0, 0), Symbol = share, Rate = 150.0M };
        HourlyShareRate lastRate = new HourlyShareRate() { Id = 2, TimeStamp = DateTime.Now, Symbol = share, Rate = 150.0M };

        Trade trade1 = new Trade() { Id = 1, Symbol = share, NoOfShares = 200, PortfolioId = validPortfolioId, Action = "BUY", Price =100.0M };
        Trade trade2 = new Trade() { Id = 2, Symbol = share, NoOfShares = 100, PortfolioId = validPortfolioId, Action = "SELL", Price = 150.0M };
        
        //Invalid data
        int invalidPortfolioId = 0;

        TradeModel tradeToTest = new TradeModel()
        {
            Symbol = "CBI",
            Action = "SELL",
            NoOfShares = 5,
            PortfolioId = 1
        };

        //Instrumenting Mock Data
        public TradeControllerTests()
        {
            //Instrumentating Mock Data Portfolio
            _portfolioRepositoryMock
                .Setup(x => x.GetAsync(It.Is<int>(id => id == validPortfolioId)))
                .Returns<int>(x => Task.FromResult(
                                            new Portfolio() {
                                            Id = validPortfolioId,
                                            Name = portfolioName
                                            })
                                );
            List<Portfolio> portfolios = new List<Portfolio>();
            portfolios.Add(new Portfolio()
            {
                Id = validPortfolioId,
                Name = portfolioName

            });
            _portfolioRepositoryMock.Setup(mr => mr.Query()).Returns(portfolios.AsQueryable());

            //Instrumentating Mock Data Shares
            List<HourlyShareRate> rates = new List<HourlyShareRate>();
            rates.Add(rate1);
            rates.Add(rate2);
            _shareRepositoryMock.Setup(mr => mr.Query()).Returns(rates.AsQueryable());

            _shareRepositoryMock.Setup(x => x.GetLastSharesBySymbol(It.Is<string>(s => s.Equals(share))))
                .Returns(Task.FromResult(lastRate));

            //Instrumentating Mock Data Trades
            List<Trade> trades = new List<Trade>();
            trades.Add(trade1);
            trades.Add(trade2);
            _tradeRepositoryMock.Setup(tr => tr.Query()).Returns(trades.AsQueryable());

            _tradeRepositoryMock.Setup(x => x.GetTradingsByPortfolioId(validPortfolioId))
                .Returns(Task.FromResult(trades));
                
            _tradeController = new TradeController(_shareRepositoryMock.Object, _tradeRepositoryMock.Object, _portfolioRepositoryMock.Object);
        }
        


        [Test]
        public async Task Get_TradingsByPortfolio_Found()
        {
            // Arrange -> Take initial instrumentation
            
            // Act
            var result = await _tradeController.GetAllTradings(validPortfolioId) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            
            List<Trade> lTrades = result.Value as List<Trade>;
            Assert.AreEqual(2, lTrades.Count);
        }

        [Test]
        public async Task Post_Trade_InvalidModelState()
        {
            // Arrange
            _tradeController.ModelState.AddModelError("Error", "Model State Error");
            TradeModel trade = new TradeModel();

            // Act
            var result = await _tradeController.Post(trade) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual((int)HttpStatusCode.BadRequest, result.StatusCode);
            _tradeController.ModelState.Clear();
        }

        [Test]
        public async Task Post_Trade_PortfolioNotExist()
        {
            // Arrange
            TradeModel trade = new TradeModel()
            {
                Action = "SELL",
                NoOfShares = 5,
                Symbol = "***",
                PortfolioId = 0
            };

            // Act
            var result = await _tradeController.Post(trade) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual((int)HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task Post_Trade_ShareNotExist()
        {
            // Arrange
            //_portfolioRepositoryMock.Setup(m => m.GetAsync(1)).Returns(Task.FromResult<Portfolio>(null));

            TradeModel trade = new TradeModel()
            {
                Action = "SELL",
                NoOfShares = 5,
                Symbol = "XXX",
                PortfolioId = 1
            };

            // Act
            var result = await _tradeController.Post(trade) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual((int)HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task Post_Trade_PortFolioBuy()
        {
            TradeModel trade = new TradeModel()
            {
                Action = "BUY",
                NoOfShares = 5,
                Symbol = "CBI",
                PortfolioId = 1
            };
            var tradeController = new TradeController(_shareRepositoryMock.Object, _tradeRepositoryMock.Object, _portfolioRepositoryMock.Object);
            // Act
            
            var result = await tradeController.Post(trade) as CreatedResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual((int)HttpStatusCode.Created, result.StatusCode);
        }

        [Test]
        public async Task Post_Trade_SharesNotEnough()
        {
            // Arrange
            TradeModel trade = new TradeModel()
            {
                Action = "SELL",
                NoOfShares = 1000,
                Symbol = "CBI",
                PortfolioId = 1
            };

            // Act
            var result = await _tradeController.Post(trade) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual((int)HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task Post_Trade_PortFolioSell()
        {
            // Arrange
            TradeModel trade = new TradeModel()
            {
                Action = "SELL",
                NoOfShares = 5,
                Symbol = "CBI",
                PortfolioId = 1
            };

            var tradeController = new TradeController(_shareRepositoryMock.Object, _tradeRepositoryMock.Object, _portfolioRepositoryMock.Object);

            // Act
            var result = await tradeController.Post(trade) as CreatedResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual((int)HttpStatusCode.Created, result.StatusCode);
        }
    }
}
