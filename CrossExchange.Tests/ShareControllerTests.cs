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
    public class ShareControllerTests
    {
        private readonly Mock<IShareRepository> _shareRepositoryMock = new Mock<IShareRepository>();

        private readonly ShareController _shareController;

        static String shareSymbol = "CBI";
        HourlyShareRate rate1 = new HourlyShareRate() { Id = 1, TimeStamp = new DateTime(2018, 09, 17, 0, 0, 0), Symbol = shareSymbol, Rate = 100.0M };
        HourlyShareRate rate2 = new HourlyShareRate() { Id = 2, TimeStamp = new DateTime(2018, 09, 18, 0, 0, 0), Symbol = shareSymbol, Rate = 150.0M };
        HourlyShareRate lastRate = new HourlyShareRate() { Id = 2, TimeStamp = DateTime.Now, Symbol = shareSymbol, Rate = 150.0M };

        public ShareControllerTests()
        {
            //Instrumentating Mock Data Shares
            List<HourlyShareRate> rates = new List<HourlyShareRate>();
            rates.Add(rate1);
            rates.Add(rate2);
            _shareRepositoryMock.Setup(mr => mr.Query()).Returns(rates.AsQueryable());
            /*
            _shareRepositoryMock.Setup(x => x.GetLastSharesBySymbol(It.Is<string>(s => s.Equals(shareSymbol))))
                .Returns(Task.FromResult(lastRate));
                */
            _shareController = new ShareController(_shareRepositoryMock.Object);
        }

        [Test]
        public async Task Get_ShouldGetShareBySymbol()
        {
            // Act

            var result = await _shareController.Get(shareSymbol) as OkObjectResult;
            // Assert
            Assert.NotNull(result);

            List<HourlyShareRate> lshares = result.Value as List<HourlyShareRate>;
            Assert.AreEqual(2, lshares.Count);
        }

        [Test]
        public async Task Get_ShouldGetLastestPrice()
        {
            // Act

            var result = await _shareController.GetLatestPrice(shareSymbol) as OkObjectResult;
            // Assert
            Assert.NotNull(result);

            //HourlyShareRate share = result.Value as HourlyShareRate;
            Assert.AreEqual(150, result.Value);
        }

        [Test]
        public async Task Post_ShouldNotInsertHourlySharePrice()
        {
            var hourRate = new HourlyShareRate
            {
                Symbol = "CBI",
                Rate = 330.0M,
                TimeStamp = new DateTime(2018, 08, 17, 5, 0, 0)
            };

            // Act
            _shareController.ModelState.AddModelError("Error", "Model state error");

            var result = await _shareController.Post(hourRate) as BadRequestObjectResult;
            // Assert
            Assert.NotNull(result);

            Assert.AreEqual((int)HttpStatusCode.BadRequest, result.StatusCode);
            _shareController.ModelState.ClearValidationState("Error");
        }

        [Test]
        public async Task Post_ShouldInsertHourlySharePrice()
        {
            var hourRate = new HourlyShareRate
            {
                Symbol = "CBI",
                Rate = 330.0M,
                TimeStamp = new DateTime(2018, 08, 17, 5, 0, 0)
            };

            // Arrange

            // Act
            var result = await _shareController.Post(hourRate);

            // Assert
            Assert.NotNull(result);

            var createdResult = result as CreatedResult;
            Assert.NotNull(createdResult);
            Assert.AreEqual((int)HttpStatusCode.Created, createdResult.StatusCode);
        }
        
        

        
    }
}
