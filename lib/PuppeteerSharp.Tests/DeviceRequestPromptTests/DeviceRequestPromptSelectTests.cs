using System.Threading.Tasks;
using NUnit.Framework;
using PuppeteerSharp.Cdp.Messaging;
using PuppeteerSharp.Nunit;

namespace PuppeteerSharp.Tests.DeviceRequestPromptTests;

public class DeviceRequestPromptSelectTests : PuppeteerPageBaseTest
{
    [Test, PuppeteerTest("DeviceRequestPrompt.test.ts", "DeviceRequestPrompt.select", "should succeed with listed device")]
    public async Task ShouldSucceedWithListedDevice()
    {
        var client = new MockCDPSession();
        var timeoutSettings = new TimeoutSettings();
        var prompt = new DeviceRequestPrompt(
            client,
            timeoutSettings,
            new DeviceAccessDeviceRequestPromptedResponse() { Id = "000" });

        var deviceTask = prompt.WaitForDeviceAsync(device => device.Name == "My Device 1");

        var promptData = new DeviceAccessDeviceRequestPromptedResponse()
        {
            Id = "000",
            Devices = new[]
            {
                new DeviceAccessDeviceRequestPromptedResponse.DeviceAccessDevice()
                {
                    Name = "My Device 1", Id = "0000",
                }
            }
        };

        client.OnMessage(new ConnectionResponse()
        {
            Method = "DeviceAccess.deviceRequestPrompted",
            Params = promptData.ToJsonElement(),
        });

        var device = await deviceTask;
        await prompt.SelectAsync(device);
    }

    [Test, PuppeteerTest("DeviceRequestPrompt.test.ts", "DeviceRequestPrompt.select", "should error for device not listed in devices")]
    public void ShouldErrorForDeviceNotListedInDevices()
    {
        var client = new MockCDPSession();
        var timeoutSettings = new TimeoutSettings();
        var prompt = new DeviceRequestPrompt(
            client,
            timeoutSettings,
            new DeviceAccessDeviceRequestPromptedResponse() { Id = "000" });

        var exception = Assert.ThrowsAsync<PuppeteerException>(() =>
            prompt.SelectAsync(new DeviceRequestPromptDevice("My Device 2", "0001")));

        Assert.That(exception!.Message, Is.EqualTo("Cannot select unknown device!"));
    }

    [Test, PuppeteerTest("DeviceRequestPrompt.test.ts", "DeviceRequestPrompt.select", "should fail when selecting prompt twice")]
    public async Task ShouldFailWhenSelectingPromptTwice()
    {
        var client = new MockCDPSession();
        var timeoutSettings = new TimeoutSettings();
        var prompt = new DeviceRequestPrompt(
            client,
            timeoutSettings,
            new DeviceAccessDeviceRequestPromptedResponse() { Id = "000" });

        var deviceTask = prompt.WaitForDeviceAsync(device => device.Name == "My Device 1");

        var promptData = new DeviceAccessDeviceRequestPromptedResponse()
        {
            Id = "000",
            Devices = new[]
            {
                new DeviceAccessDeviceRequestPromptedResponse.DeviceAccessDevice()
                {
                    Name = "My Device 1", Id = "0000",
                }
            }
        };

        client.OnMessage(new ConnectionResponse()
        {
            Method = "DeviceAccess.deviceRequestPrompted",
            Params = promptData.ToJsonElement(),
        });

        var device = await deviceTask;
        await prompt.SelectAsync(device);

        var exception = Assert.ThrowsAsync<PuppeteerException>(() => prompt.SelectAsync(device));
        Assert.That(exception!.Message, Is.EqualTo("Cannot select DeviceRequestPrompt which is already handled!"));
    }
}
