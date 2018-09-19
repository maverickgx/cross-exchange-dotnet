using System;
using System.Threading.Tasks;
using CrossExchange.Controller;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace CrossExchange.Tests
{
    public class PortfolioControllerTests
    {
        private readonly Mock<IPortfolioRepository> _portfolioRepositoryMock = new Mock<IPortfolioRepository>();

        private readonly Mock<ITradeRepository> _tradeRepositoryMock = new Mock<ITradeRepository>();

        private readonly Mock<IShareRepository> _shareRepositoryMock = new Mock<IShareRepository>();

        private readonly PortfolioController _portfolioController;

        public PortfolioControllerTests()
        {
            //Instrumentating Mock Data Portfolio
            List<Portfolio> portfolios = new List<Portfolio>();
            portfolios.Add(new Portfolio()
            {
                Id = 1,
                Name = "Portfolio Test"

            });
            _portfolioRepositoryMock.Setup(mr => mr.Query()).Returns(portfolios.AsQueryable());

            _portfolioController = new PortfolioController(_shareRepositoryMock.Object, _tradeRepositoryMock.Object, _portfolioRepositoryMock.Object);
        }

        [Test]
        public async Task Post_ShouldNotInsertPortfolio()
        {

            Portfolio p = new Portfolio()
            {
                Id = 1,
                Name = "Portfolio Test"
            };

            // Arrange
            _portfolioController.ModelState.AddModelError("Error", "Model State error");
            // Act
            var result = await _portfolioController.Post(p);

            // Assert
            Assert.NotNull(result);

            var okResult = result as BadRequestObjectResult;
            Assert.NotNull(okResult);

            _portfolioController.ModelState.Clear();
        }

        [Test]
        public async Task Post_ShouldInsertPortfolio()
        {
            // Arrange
            Portfolio portfolio = new Portfolio()
            {
                Id = 1,
                Name = "Portfolio Test"
            };

            // Act
            var result = await _portfolioController.Post(portfolio);

            // Assert
            Assert.NotNull(result);

            var createdResult = result as CreatedResult;
            Assert.NotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
        }

        [Test]
        public async Task Get_ShouldGetPortfolioById()
        {
            // Arrange

            // Act
            var result = await _portfolioController.GetPortfolioInfo(1) as OkObjectResult;

            // Assert
            Assert.NotNull(result);

            Portfolio portfolio = result.Value as Portfolio;
            Assert.AreEqual(1, portfolio.Id);
            
        }

    }
}
