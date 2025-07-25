using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using PuppeteerSharp.Nunit;

namespace PuppeteerSharp.Tests.ElementHandleTests
{
    public class ClickTests : PuppeteerPageBaseTest
    {
        [Test, PuppeteerTest("elementhandle.spec", "ElementHandle specs ElementHandle.click", "should work")]
        public async Task ShouldWork()
        {
            await Page.GoToAsync(TestConstants.ServerUrl + "/input/button.html");
            var button = await Page.QuerySelectorAsync("button");
            await button.ClickAsync();
            Assert.That(await Page.EvaluateExpressionAsync<string>("result"), Is.EqualTo("Clicked"));
        }

        [Test, PuppeteerTest("elementhandle.spec", "ElementHandle specs ElementHandle.click", "should work for Shadow DOM v1")]
        public async Task ShouldWorkForShadowDomV1()
        {
            await Page.GoToAsync(TestConstants.ServerUrl + "/shadow.html");
            var buttonHandle = (IElementHandle)await Page.EvaluateExpressionHandleAsync("button");
            await buttonHandle.ClickAsync();
            Assert.That(await Page.EvaluateExpressionAsync<bool>("clicked"), Is.True);
        }

        [Test, PuppeteerTest("elementhandle.spec", "ElementHandle specs ElementHandle.click", "should not work for TextNodes")]
        public async Task ShouldNotWorkForTextNodes()
        {
            await Page.GoToAsync(TestConstants.ServerUrl + "/input/button.html");
            var buttonTextNode = (IElementHandle)await Page.EvaluateExpressionHandleAsync(
                "document.querySelector('button').firstChild");
            var exception = Assert.ThrowsAsync<PuppeteerException>(async () => await buttonTextNode.ClickAsync());

            Assert.That(exception.Message, Does.Contain("is not of type HTMLElement"));
        }

        [Test, PuppeteerTest("elementhandle.spec", "ElementHandle specs ElementHandle.click", "should throw for detached nodes")]
        public async Task ShouldThrowForDetachedNodes()
        {
            await Page.GoToAsync(TestConstants.ServerUrl + "/input/button.html");
            var button = await Page.QuerySelectorAsync("button");
            await Page.EvaluateFunctionAsync("button => button.remove()", button);
            var exception = Assert.ThrowsAsync<PuppeteerException>(async () => await button.ClickAsync());
            Assert.That(exception.Message, Is.EqualTo("Node is detached from document"));
        }

        [Test, PuppeteerTest("elementhandle.spec", "ElementHandle specs ElementHandle.click", "should throw for hidden nodes")]
        public async Task ShouldThrowForHiddenNodes()
        {
            await Page.GoToAsync(TestConstants.ServerUrl + "/input/button.html");
            var button = await Page.QuerySelectorAsync("button");
            await Page.EvaluateFunctionAsync("button => button.style.display = 'none'", button);
            var exception = Assert.ThrowsAsync<PuppeteerException>(async () => await button.ClickAsync());
            Assert.That(exception.Message, Is.EqualTo("Node is either not clickable or not an Element"));
        }

        [Test, PuppeteerTest("elementhandle.spec", "ElementHandle specs ElementHandle.click", "should throw for recursively hidden nodes")]
        public async Task ShouldThrowForRecursivelyHiddenNodes()
        {
            await Page.GoToAsync(TestConstants.ServerUrl + "/input/button.html");
            var button = await Page.QuerySelectorAsync("button");
            await Page.EvaluateFunctionAsync("button => button.parentElement.style.display = 'none'", button);
            var exception = Assert.ThrowsAsync<PuppeteerException>(async () => await button.ClickAsync());
            Assert.That(exception.Message, Is.EqualTo("Node is either not clickable or not an Element"));
        }

        [Test, PuppeteerTest("elementhandle.spec", "ElementHandle specs ElementHandle.click", "should throw for <br> elements")]
        public async Task ShouldThrowForBrElements()
        {
            await Page.SetContentAsync("hello<br>goodbye");
            var br = await Page.QuerySelectorAsync("br");
            var exception = Assert.ThrowsAsync<PuppeteerException>(async () => await br.ClickAsync());
            Assert.That(exception.Message, Is.EqualTo("Node is either not clickable or not an Element"));
        }

        [Test, PuppeteerTest("elementhandle.spec", "ElementHandle specs ElementHandle.click", "should return Point data")]
        public async Task ShouldReturnPointData()
        {
            var clicks = new List<BoxModelPoint>();

            await Page.ExposeFunctionAsync("reportClick", (int x, int y) =>
            {
                clicks.Add(new BoxModelPoint { X = x, Y = y });

                return true;
            });

            await Page.EvaluateExpressionAsync(@"document.body.style.padding = '0';
                document.body.style.margin = '0';
                document.body.innerHTML = '<div style=""cursor: pointer; width: 120px; height: 60px; margin: 30px; padding: 15px;""></div>';
                document.body.addEventListener('click', e => {
                    window.reportClick(e.clientX, e.clientY);
                });");

            var divHandle = await Page.QuerySelectorAsync("div");

            await divHandle.ClickAsync();
            await divHandle.ClickAsync(new Input.ClickOptions { OffSet = new Offset(10, 15) });

            await TestUtils.ShortWaitForCollectionToHaveAtLeastNElementsAsync(clicks, 2);

            // margin + middle point offset
            Assert.That(clicks[0].X, Is.EqualTo(45 + 60));
            Assert.That(clicks[0].Y, Is.EqualTo(45 + 30));

            // margin + offset
            Assert.That(clicks[1].X, Is.EqualTo(30 + 10));
            Assert.That(clicks[1].Y, Is.EqualTo(30 + 15));
        }
    }
}
