namespace DeltaSigmaPhiWebsite.Tests.Controllers
{
    using System;
    using System.Linq.Expressions;
    using System.Security.Policy;
    using System.Web.Mvc;
    using Data.Interfaces;
    using Data.UnitOfWork;
    using DeltaSigmaPhiWebsite.Controllers;
    using Models;
    using Moq;
    using NUnit.Framework;
    using System.Linq;
    using System.Transactions;

    [TestFixture]
    public class AccountControllerTest
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
        public void Index()
        {
            var uowMock = new Mock<IUnitOfWork>();
            var wsMock = new Mock<IWebSecurity>();
            var oawsMock = new Mock<IOAuthWebSecurity>();
            var repMock = new Mock<IMembersRepository>();
            var conMock = new Mock<AccountController>(uowMock.Object, wsMock.Object, oawsMock.Object) { CallBase = true };

            // Arrange
            repMock.Setup(m => m.Get(It.IsAny< Expression<Func<Member, bool>>>())).Returns(new Member
            {
                UserId = 1, FirstName = "John", LastName = "Doe", UserName = "johndoe"
            });
            uowMock.Setup(m => m.MemberRepository).Returns(repMock.Object);
            wsMock.Setup(x => x.CurrentUser.Identity.Name).Returns("johndoe");
            conMock.Setup(x => x.GetBigPictureUrl(It.IsAny<string>())).Returns("johndoe");

            // Act
            var actual = conMock.Object.Index(null);

            // Assert
            Assert.IsInstanceOf<ActionResult>(actual);
        }

        [Test]
        public void Login()
        {
            var uowMock = new Mock<IUnitOfWork>();
            var ws = new Mock<IWebSecurity>();
            var oaws = new Mock<IOAuthWebSecurity>();
            // Arrange
            var controller = new AccountController(uowMock.Object, ws.Object, oaws.Object);
            // Act
            var actual = controller.Login() as ViewResult;
            // Assert
            Assert.IsInstanceOf<ActionResult>(actual);
        }
    }
}
