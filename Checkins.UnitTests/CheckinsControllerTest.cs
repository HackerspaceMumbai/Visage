using Xunit;
using Moq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Checkins.API;
using Bogus;
using System;
using System.Linq;

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
            var result =  await checkinsController.GetAsync("test");
            
            //Assert
            var attendees = Assert.IsType<List<Attendee>>(result);
            Assert.Equal(expectedNumberofAttendees, attendees.Count);

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
            var result = await checkinsController.GetAsync("test", "checkedin");

            //Assert
            var attendees = Assert.IsType<List<Attendee>>(result);
            Assert.Equal(expected_checkedin_attendees, attendees.Count);

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
            var result = await checkinsController.GetAsync("test", "noshow");

            //Assert
            var attendees = Assert.IsType<List<Attendee>>(result);
            Assert.Equal(expected_noshow_attendees, attendees.Count);

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
