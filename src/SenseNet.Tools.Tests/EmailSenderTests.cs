using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SenseNet.Extensions.DependencyInjection;
using SenseNet.Tools.Mail;

namespace SenseNet.Tools.Tests
{
    [TestClass]
    public class EmailSenderTests
    {
        // Disabled to avoid sending emails during tests
        //[TestMethod]
        public async Task EmailSender_Valid()
        {
            var es = GetEmailSender();

            await es.SendAsync("sensenettest@example.com", "SN Test", "test", "test message",
                CancellationToken.None);

            await es.SendAsync(new EmailData
                {
                    ToAddresses = new[]
                    {
                        new EmailAddress("email1@example.com", "Recipient 1"),
                        new EmailAddress("email2@example.com", "Recipient 2")
                    },
                    Subject = "test",
                    Body = "test message",
                    FromAddress = "customfrom@example.com",
                    FromName = "Custom From"
                },
                CancellationToken.None);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public async Task EmailSender_EmptyEmail()
        {
            var es = GetEmailSender();

            // empty email should result in ArgumentNullException
            await es.SendAsync(null, "SN Test", "test", "test message",
                CancellationToken.None);
        }

        private static IEmailSender GetEmailSender()
        {
            var config = new ConfigurationBuilder()
                .AddUserSecrets<EmailSenderTests>()
                .Build();

            // registers the default email sender that sends real emails
            var services = new ServiceCollection()
                .AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Trace);
                })
                .AddSenseNetEmailSender(options =>
                {
                    config.GetSection("sensenet:Email").Bind(options);
                })
                .BuildServiceProvider();

            return services.GetRequiredService<IEmailSender>();
        }
    }
}
