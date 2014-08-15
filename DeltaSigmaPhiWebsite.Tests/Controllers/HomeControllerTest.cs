namespace DeltaSigmaPhiWebsite.Tests.Controllers
{
    using Data.UnitOfWork;
    using DeltaSigmaPhiWebsite.Controllers;
    using Models;
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
            var ws = new Mock<IWebSecurity>();
            var oaws = new Mock<IOAuthWebSecurity>();
            // Arrange
            var controller = new HomeController(uowMock.Object, ws.Object, oaws.Object);
            // Act
            var actual = controller.Index() as ViewResult;
            // Assert
            Assert.IsInstanceOf<ActionResult>(actual);
        }

        [Test]
        public void HomeContactReturnsActionResult()
        {
            var uowMock = new Mock<IUnitOfWork>();
            var ws = new Mock<IWebSecurity>();
            var oaws = new Mock<IOAuthWebSecurity>();
            // Arrange
            var controller = new HomeController(uowMock.Object, ws.Object, oaws.Object);
            // Act
            var actual = controller.Contacts() as ViewResult;
            // Assert
            Assert.IsInstanceOf<ActionResult>(actual);
        }
        [Test]
        public void HomeRecruitmentReturnsActionResult()
        {
            var uowMock = new Mock<IUnitOfWork>();
            var ws = new Mock<IWebSecurity>();
            var oaws = new Mock<IOAuthWebSecurity>();
            // Arrange
            var controller = new HomeController(uowMock.Object, ws.Object, oaws.Object);
            // Act
            var actual = controller.Recruitment() as ViewResult;
            // Assert
            Assert.IsInstanceOf<ActionResult>(actual);
        }
        [Test]
        public void HomeScholarshipReturnsActionResult()
        {
            var uowMock = new Mock<IUnitOfWork>();
            var ws = new Mock<IWebSecurity>();
            var oaws = new Mock<IOAuthWebSecurity>();
            // Arrange
            var controller = new HomeController(uowMock.Object, ws.Object, oaws.Object);
            // Act
            var actual = controller.Scholarships() as ViewResult;
            // Assert
            Assert.IsInstanceOf<ActionResult>(actual);
        }
        [Test]
        public void HomeAcademicsReturnsActionResult()
        {
            var uowMock = new Mock<IUnitOfWork>();
            var ws = new Mock<IWebSecurity>();
            var oaws = new Mock<IOAuthWebSecurity>();
            // Arrange
            var controller = new HomeController(uowMock.Object, ws.Object, oaws.Object);
            // Act
            var actual = controller.Academics() as ViewResult;
            // Assert
            Assert.IsInstanceOf<ActionResult>(actual);
        }
    }
}
