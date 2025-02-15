using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUnit.Core;
using TUnit.Playwright;
using FluentAssertions;

namespace Visage.Tests.Unit.Frontend.Web
{
    internal class HomeTests: PageTest
    {
        [Test]
        public async Task TestHomePage()
        {
            await Page.GotoAsync("https://localhost:7150");
            var title = await Page.TitleAsync();
            title.Should().Be("Visage");
        }
    }
}
