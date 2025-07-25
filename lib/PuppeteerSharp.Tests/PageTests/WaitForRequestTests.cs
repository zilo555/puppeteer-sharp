using System;
using System.Threading.Tasks;
using NUnit.Framework;
using PuppeteerSharp.Nunit;

namespace PuppeteerSharp.Tests.PageTests
{
    public class WaitForRequestTests : PuppeteerPageBaseTest
    {
        [Test, PuppeteerTest("page.spec", "Page Page.waitForRequest", "should work")]
        public async Task ShouldWork()
        {
            await Page.GoToAsync(TestConstants.EmptyPage);
            var task = Page.WaitForRequestAsync(TestConstants.ServerUrl + "/digits/2.png");

            await Task.WhenAll(
                task,
                Page.EvaluateFunctionAsync(@"() => {
                    fetch('/digits/1.png');
                    fetch('/digits/2.png');
                    fetch('/digits/3.png');
                }")
            );
            Assert.That(task.Result.Url, Is.EqualTo(TestConstants.ServerUrl + "/digits/2.png"));
        }

        [Test, PuppeteerTest("page.spec", "Page Page.waitForRequest", "should work with predicate")]
        public async Task ShouldWorkWithPredicate()
        {
            await Page.GoToAsync(TestConstants.EmptyPage);
            var task = Page.WaitForRequestAsync(request => request.Url == TestConstants.ServerUrl + "/digits/2.png");

            await Task.WhenAll(
            task,
            Page.EvaluateFunctionAsync(@"() => {
                fetch('/digits/1.png');
                fetch('/digits/2.png');
                fetch('/digits/3.png');
            }")
            );
            Assert.That(task.Result.Url, Is.EqualTo(TestConstants.ServerUrl + "/digits/2.png"));
        }

        [Test, PuppeteerTest("page.spec", "Page Page.waitForRequest", "should respect timeout")]
        public async Task ShouldRespectTimeout()
        {
            await Page.GoToAsync(TestConstants.EmptyPage);
            var exception = Assert.ThrowsAsync<TimeoutException>(async () =>
                await Page.WaitForRequestAsync(_ => false, new WaitForOptions(1)));

            Assert.That(exception.Message, Does.Contain("Timeout of 1 ms exceeded"));
        }

        [Test, PuppeteerTest("page.spec", "Page Page.waitForRequest", "should respect default timeout")]
        public async Task ShouldRespectDefaultTimeout()
        {
            await Page.GoToAsync(TestConstants.EmptyPage);
            Page.DefaultTimeout = 1;
            var exception = Assert.ThrowsAsync<TimeoutException>(async () =>
                await Page.WaitForRequestAsync(_ => false));

            Assert.That(exception.Message, Does.Contain("Timeout of 1 ms exceeded"));
        }

        [Test, PuppeteerTest("page.spec", "Page Page.waitForRequest", "should work with no timeout")]
        public async Task ShouldWorkWithNoTimeout()
        {
            await Page.GoToAsync(TestConstants.EmptyPage);
            var task = Page.WaitForRequestAsync(TestConstants.ServerUrl + "/digits/2.png", new WaitForOptions(0));

            await Task.WhenAll(
                task,
                Page.EvaluateFunctionAsync(@"() => setTimeout(() => {
                    fetch('/digits/1.png');
                    fetch('/digits/2.png');
                    fetch('/digits/3.png');
                }, 50)")
            );
            Assert.That(task.Result.Url, Is.EqualTo(TestConstants.ServerUrl + "/digits/2.png"));
        }
    }
}
