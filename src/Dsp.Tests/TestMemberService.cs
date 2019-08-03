namespace Dsp.Tests
{
    using Data;
    using Data.Entities;
    using Dsp.Services;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Services.Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using System.Threading.Tasks;

    [TestClass]
    public class TestMemberService
    {
        private IMemberService _memberService;

        [TestInitialize]
        public void Initialize()
        {
            var membersData = new List<Member>
            {
                new Member
                {
                    Id = 1, UserName = "jsmith",
                    FirstName = "Jon",
                    LastName = "Smith",
                    Email = "jsmith@dsp.com",
                    StatusId = 1,
                    GraduationSemester = null,
                    PledgeClassId = 1
                },
                new Member
                {
                    Id = 2,
                    UserName = "tblack",
                    FirstName = "Tom",
                    LastName = "Black",
                    Email = "tblack@dsp.com",
                    StatusId = 2,
                    GraduationSemester = null,
                    PledgeClassId = 1
                },
                new Member
                {
                    Id = 3,
                    UserName = "rjames",
                    FirstName = "Rob",
                    LastName = "James",
                    Email = "rjames@dsp.com",
                    StatusId = 3,
                    GraduationSemester = null,
                    PledgeClassId = 1
                }
            }.AsQueryable();
            var memberStatusesData = new List<MemberStatus>
            {
                new MemberStatus { StatusId = 1, StatusName = "New" },
                new MemberStatus { StatusId = 2, StatusName = "Neophyte" },
                new MemberStatus { StatusId = 3, StatusName = "Active" },
                new MemberStatus { StatusId = 4, StatusName = "Alumnus" },
                new MemberStatus { StatusId = 5, StatusName = "Affiliate" },
                new MemberStatus { StatusId = 6, StatusName = "Released" }
            }.AsQueryable();
            var semestersData = new List<Semester>
            {
                new Semester { Id = 1, DateStart = new DateTime(2015, 1, 15), DateEnd = new DateTime(2015, 5, 15) },
                new Semester { Id = 2, DateStart = new DateTime(2015, 1, 15), DateEnd = new DateTime(2015, 5, 15) }
            }.AsQueryable();
            // TODO: Finish semesters and add pledge classes to test roster.

            var mockContext = new Mock<SphinxDbContext>();
            var memberMockSet = new Mock<DbSet<Member>>();
            memberMockSet.As<IDbAsyncEnumerable<Member>>()
                .Setup(e => e.GetAsyncEnumerator())
                .Returns(new TestDbAsyncEnumerator<Member>(membersData.GetEnumerator()));
            memberMockSet.As<IQueryable<Member>>()
                .Setup(e => e.Provider)
                .Returns(new TestDbAsyncQueryProvider<Member>(membersData.Provider));
            memberMockSet.As<IQueryable<Member>>().Setup(e => e.Expression).Returns(membersData.Expression);
            memberMockSet.As<IQueryable<Member>>().Setup(e => e.ElementType).Returns(membersData.ElementType);
            memberMockSet.As<IQueryable<Member>>().Setup(e => e.GetEnumerator()).Returns(() => membersData.GetEnumerator());

            mockContext.Setup(e => e.Users).Returns(memberMockSet.Object);

            var memberStatusMockSet = new Mock<DbSet<MemberStatus>>();
            memberStatusMockSet.As<IDbAsyncEnumerable<MemberStatus>>()
                .Setup(e => e.GetAsyncEnumerator())
                .Returns(new TestDbAsyncEnumerator<MemberStatus>(memberStatusesData.GetEnumerator()));
            memberStatusMockSet.As<IQueryable<MemberStatus>>()
                .Setup(e => e.Provider)
                .Returns(new TestDbAsyncQueryProvider<MemberStatus>(memberStatusesData.Provider));
            memberStatusMockSet.As<IQueryable<MemberStatus>>().Setup(e => e.Expression).Returns(memberStatusesData.Expression);
            memberStatusMockSet.As<IQueryable<MemberStatus>>().Setup(e => e.ElementType).Returns(memberStatusesData.ElementType);
            memberStatusMockSet.As<IQueryable<MemberStatus>>().Setup(e => e.GetEnumerator()).Returns(() => memberStatusesData.GetEnumerator());

            mockContext.Setup(e => e.MemberStatuses).Returns(memberStatusMockSet.Object);

            _memberService = new MemberService(mockContext.Object);
        }

        [TestMethod]
        public async Task MemberService_GetMemberByIdAsync_Found()
        {
            var entity = await _memberService.GetMemberByIdAsync(1);
            Assert.AreEqual(entity.Id, 1);

            entity = await _memberService.GetMemberByIdAsync(2);
            Assert.AreEqual(entity.Id, 2);

            entity = await _memberService.GetMemberByIdAsync(3);
            Assert.AreEqual(entity.Id, 3);
        }

        [TestMethod]
        public async Task MemberService_GetMemberByUserNameAsync_Found()
        {
            var entity = await _memberService.GetMemberByUserNameAsync("jsmith");
            Assert.AreEqual(entity.UserName, "jsmith");

            entity = await _memberService.GetMemberByUserNameAsync("tblack");
            Assert.AreEqual(entity.UserName, "tblack");

            entity = await _memberService.GetMemberByUserNameAsync("rjames");
            Assert.AreEqual(entity.UserName, "rjames");
        }

        [TestMethod]
        public async Task MemberService_GetRosterForSemester_All()
        {
            var semester = new Semester();
            var entity = await _memberService.GetRosterForSemesterAsync(semester);
            Assert.AreEqual(1, 1);
        }
    }
}
