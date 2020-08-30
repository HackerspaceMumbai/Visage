using Xunit;
using Moq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Checkins.API;
using Bogus;
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Checkins.UnitTests
{
    public class CheckinsControllerTest
    {
        private readonly Mock<IEventbriteClient> _eventbriteClient;
        public CheckinsControllerTest()
        {
            _eventbriteClient = new Mock<IEventbriteClient>();
        }

        [Fact]
        public async Task Get_all_attendees_success()
        {

            //Arrange
            var expectedNumberofAttendees = 3;

            _eventbriteClient.Setup(x => x.GetAttendees(It.IsAny<string>()))
                                          .Returns(Task.FromResult(getFakeEventbriteResonse().attendees));

            var checkinsController = new Checkins.API.Controllers.CheckinsController(_eventbriteClient.Object);

            //Act
            var Okresult =  await checkinsController.GetAsyncByEventId("test", "all") ;
            
            //Assert
            var attendees = Assert.IsType<OkObjectResult>(Okresult.Result);
            Assert.Equal(expectedNumberofAttendees, (attendees.Value as List<Attendee>).Count) ;

        } 

        [Fact]
        public async Task Get_checkedin_attendees()
        {
            //Arrange
            var expected_checkedin_attendees = 2;

            _eventbriteClient.Setup(x => x.GetAttendees(It.IsAny<string>()))
                                          .Returns(Task.FromResult(getFakeEventbriteResonse().attendees));

            var checkinsController = new Checkins.API.Controllers.CheckinsController(_eventbriteClient.Object);

            //Act
            var Okresult = await checkinsController.GetAsyncByEventId("test", "checkedin");

            //Assert
            var attendees = Assert.IsType<OkObjectResult>(Okresult.Result);
            Assert.Equal(expected_checkedin_attendees, (attendees.Value as List<Attendee>).Count);

        }

        [Fact]
        public async Task Get_noshow_attendees()
        {
            //Arrange
            var expected_noshow_attendees = 1;

            _eventbriteClient.Setup(x => x.GetAttendees(It.IsAny<string>()))
                                          .Returns(Task.FromResult(getFakeEventbriteResonse().attendees));

            var checkinsController = new Checkins.API.Controllers.CheckinsController(_eventbriteClient.Object);

            //Act
            var Okresult = await checkinsController.GetAsyncByEventId("test", "noshow");

            //Assert
            var attendees = Assert.IsType<OkObjectResult>(Okresult.Result);
            Assert.Equal(expected_noshow_attendees, (attendees.Value as List<Attendee>).Count);

        }


        [Fact]
        public async Task Get_WrongAttStatus_Failure()
        {
            //Arrange
            _eventbriteClient.Setup(x => x.GetAttendees(It.IsAny<string>()))
                                          .Returns(Task.FromResult(getFakeEventbriteResonse().attendees));

            var checkinsController = new Checkins.API.Controllers.CheckinsController(_eventbriteClient.Object);

            //Act
            var Okresult = await checkinsController.GetAsyncByEventId("test", "inValidStatus");

            //Assert
            Assert.IsType<BadRequestResult>(Okresult.Result);

        }

        private EventBriteEventResponse getFakeEventbriteResonse()
        {
            //Creating template for fake attendee

            var fakeAttendee = new Faker<Attendee>();

            var fakeAllAttendees = fakeAttendee.Generate(3)
                                                .With(2, s => s.checked_in = true);

            return new EventBriteEventResponse()
            {
                attendees = fakeAllAttendees

            };
        }
    }

    

}
    public static class AttendeeExtension
    {
        public static List<T> With<T>(this List<T> list, int count, Action<T> apply)
        {
            foreach (var item in list.Take(count))
            {
                apply(item);
            }

            return list;
        }
    };
