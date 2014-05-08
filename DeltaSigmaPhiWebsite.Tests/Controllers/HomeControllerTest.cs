namespace DeltaSigmaPhiWebsite.Tests.Controllers
{
    using Data.UnitOfWork;
    using DeltaSigmaPhiWebsite.Controllers;
    using Moq;
    using NUnit.Framework;
    using System.Transactions;
    using System.Web.Mvc;

    [TestFixture]
    public class HomeControllerTest
    {
        private TransactionScope scope;

        [SetUp]
        public void SetUp()
        {
            scope = new TransactionScope();
        }

        [TearDown]
        public void TearDown()
        {
            scope.Dispose();
        }

        [Test]
        public void HomeIndexReturnsActionResult()
        {
            var uowMock = new Mock<IUnitOfWork>();
            // Arrange
            var controller = new HomeController(uowMock.Object);
            // Act
            var actual = controller.Index() as ViewResult;
            // Assert
            Assert.IsInstanceOf<ActionResult>(actual);
        }
        [Test]
        public void HomeAboutReturnsActionResult()
        {
            var uowMock = new Mock<IUnitOfWork>();
            // Arrange
            var controller = new HomeController(uowMock.Object);
            // Act
            var actual = controller.About() as ViewResult;
            // Assert
            Assert.IsInstanceOf<ActionResult>(actual);
        }
        [Test]
        public void HomeContactReturnsActionResult()
        {
            var uowMock = new Mock<IUnitOfWork>();
            // Arrange
            var controller = new HomeController(uowMock.Object);
            // Act
            var actual = controller.Contact() as ViewResult;
            // Assert
            Assert.IsInstanceOf<ActionResult>(actual);
        }
        [Test]
        public void HomeHowToJoinReturnsActionResult()
        {
            var uowMock = new Mock<IUnitOfWork>();
            // Arrange
            var controller = new HomeController(uowMock.Object);
            // Act
            var actual = controller.HowToJoin() as ViewResult;
            // Assert
            Assert.IsInstanceOf<ActionResult>(actual);
        }
        [Test]
        public void HomeScholarshipReturnsActionResult()
        {
            var uowMock = new Mock<IUnitOfWork>();
            // Arrange
            var controller = new HomeController(uowMock.Object);
            // Act
            var actual = controller.BuildingBetterMenScholarship() as ViewResult;
            // Assert
            Assert.IsInstanceOf<ActionResult>(actual);
        }
        [Test]
        public void HomeAcademicsReturnsActionResult()
        {
            var uowMock = new Mock<IUnitOfWork>();
            // Arrange
            var controller = new HomeController(uowMock.Object);
            // Act
            var actual = controller.Academics() as ViewResult;
            // Assert
            Assert.IsInstanceOf<ActionResult>(actual);
        }
        [Test]
        public void HomeOfficersReturnsActionResult()
        {
            var uowMock = new Mock<IUnitOfWork>();
            // Arrange
            var controller = new HomeController(uowMock.Object);
            // Act
            var actual = controller.Officers() as ViewResult;
            // Assert
            Assert.IsInstanceOf<ActionResult>(actual);
        }
        [Test]
        public void HomeChairmenReturnsActionResult()
        {
            var uowMock = new Mock<IUnitOfWork>();
            // Arrange
            var controller = new HomeController(uowMock.Object);
            // Act
            var actual = controller.Chairmen() as ViewResult;
            // Assert
            Assert.IsInstanceOf<ActionResult>(actual);
        }
    }
}
