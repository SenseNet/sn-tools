using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SenseNet.Tools.CommandLineArguments;
// ReSharper disable ArgumentsStyleStringLiteral
// ReSharper disable ArgumentsStyleLiteral

namespace SenseNet.Tools.Tests
{
    [TestClass]
    public class CommandLineArgumentParserTests
    {
        [TestMethod]
        public void CmdArgs_Acceptance1()
        {
            var args = new[] { "/a", "/string:asdf", "/int:42" };
            var settings = new Args1();
            var settingsAcc = new PrivateObject(settings);

            ArgumentParser.Parse(args, settings);

            Assert.AreEqual(true, settings.A);
            Assert.AreEqual(false, settings.B);
            Assert.AreEqual(false, (bool)settingsAcc.GetProperty("C"));
            Assert.AreEqual(null, settings.Source);
            Assert.AreEqual(null, settings.Target);
            Assert.AreEqual("asdf", (string)settingsAcc.GetFieldOrProperty("StringParam1"));
            Assert.AreEqual(42, settings.IntParam1);
        }
        [TestMethod]
        public void CmdArgs_Acceptance2()
        {
            var args = new[] { "--string=asdf", "-int:42" };
            var settings = new Args1();
            var settingsAcc = new PrivateObject(settings);

            ArgumentParser.Parse(args, settings);

            Assert.AreEqual(false, settings.A);
            Assert.AreEqual(false, settings.B);
            Assert.AreEqual(false, settings.A);
            Assert.AreEqual(null, settings.Source);
            Assert.AreEqual(null, settings.Target);
            Assert.AreEqual("asdf", (string)settingsAcc.GetFieldOrProperty("StringParam1"));
            Assert.AreEqual(42, settings.IntParam1);
        }
        [TestMethod]
        public void CmdArgs_Acceptance3()
        {
            var args = new[] { "/STRING:asdf", "-INT=42" };
            var settings = new Args1();
            var settingsAcc = new PrivateObject(settings);

            ArgumentParser.Parse(args, settings);

            Assert.AreEqual(false, settings.A);
            Assert.AreEqual(false, settings.B);
            Assert.AreEqual(false, settings.A);
            Assert.AreEqual(null, settings.Source);
            Assert.AreEqual(null, settings.Target);
            Assert.AreEqual("asdf", (string)settingsAcc.GetFieldOrProperty("StringParam1"));
            Assert.AreEqual(42, settings.IntParam1);
        }
        [TestMethod]
        public void CmdArgs_Acceptance4()
        {
            var args = new[] { "-s=asdf", "-i:42" };
            var settings = new Args1();
            var settingsAcc = new PrivateObject(settings);

            ArgumentParser.Parse(args, settings);

            Assert.AreEqual(false, settings.A);
            Assert.AreEqual(false, settings.B);
            Assert.AreEqual(false, settings.A);
            Assert.AreEqual(null, settings.Source);
            Assert.AreEqual(null, settings.Target);
            Assert.AreEqual("asdf", (string)settingsAcc.GetFieldOrProperty("StringParam1"));
            Assert.AreEqual(42, settings.IntParam1);
        }
        [TestMethod]
        public void CmdArgs_Acceptance5()
        {
            var args = new[] { "-STRING", "asdf", "-INT", "42" };
            var settings = new Args1();
            var settingsAcc = new PrivateObject(settings);

            ArgumentParser.Parse(args, settings);

            Assert.AreEqual(false, settings.A);
            Assert.AreEqual(false, settings.B);
            Assert.AreEqual(false, settings.A);
            Assert.AreEqual(null, settings.Source);
            Assert.AreEqual(null, settings.Target);
            Assert.AreEqual("asdf", (string)settingsAcc.GetFieldOrProperty("StringParam1"));
            Assert.AreEqual(42, settings.IntParam1);
        }
        [TestMethod]
        public void CmdArgs_Acceptance6()
        {
            var args = new[] { "/STRING", "asdf", "/INT", "42" };
            var settings = new Args1();
            var settingsAcc = new PrivateObject(settings);

            ArgumentParser.Parse(args, settings);

            Assert.AreEqual(false, settings.A);
            Assert.AreEqual(false, settings.B);
            Assert.AreEqual(false, settings.A);
            Assert.AreEqual(null, settings.Source);
            Assert.AreEqual(null, settings.Target);
            Assert.AreEqual("asdf", (string)settingsAcc.GetFieldOrProperty("StringParam1"));
            Assert.AreEqual(42, settings.IntParam1);
        }
        [TestMethod]
        public void CmdArgs_Acceptance7()
        {
            var args = new[] { "source", "/STRING", "asdf", "target", "-i:42" };
            var settings = new Args1();
            var settingsAcc = new PrivateObject(settings);

            ArgumentParser.Parse(args, settings);

            Assert.AreEqual(false, settings.A);
            Assert.AreEqual(false, settings.B);
            Assert.AreEqual(false, settings.A);
            Assert.AreEqual("source", settings.Source);
            Assert.AreEqual("target", settings.Target);
            Assert.AreEqual("asdf", (string)settingsAcc.GetFieldOrProperty("StringParam1"));
            Assert.AreEqual(42, settings.IntParam1);
        }
        [TestMethod]
        public void CmdArgs_Acceptance8()
        {
            var defaultSource = "defaultSource";
            var defaultInt = 444;

            var args = new string[0];
            var settings = new Args1 { IntParam1 = defaultInt, Source = defaultSource };
            var settingsAcc = new PrivateObject(settings);

            ArgumentParser.Parse(args, settings);

            Assert.AreEqual(false, settings.A);
            Assert.AreEqual(false, settings.B);
            Assert.AreEqual(false, settings.A);
            Assert.AreEqual(defaultSource, settings.Source);
            Assert.AreEqual(null, settings.Target);
            Assert.AreEqual(null, (string)settingsAcc.GetFieldOrProperty("StringParam1"));
            Assert.AreEqual(defaultInt, settings.IntParam1);
        }

        [TestMethod]
        public void CmdArgs_Help1()
        {
            var argsArray = new[] {
                new [] { "?" },
                new [] { "?", "asdf" },
                new [] { "-?" },
                new [] { "-?", "asdf" },
                new [] { "-h" },
                new [] { "-h", "asdf" },
                new [] { "-H" },
                new [] { "-H", "asdf" },
                new [] { "/?" },
                new [] { "/?", "asdf" },
                new [] { "/h" },
                new [] { "/h", "asdf" },
                new [] { "/H" },
                new [] { "/H", "asdf" },
                new [] { "-help" },
                new [] { "-help", "asdf" },
                new [] { "--help" },
                new [] { "--help", "asdf" }
            };

            foreach (var args in argsArray)
            {
                var settings = new Args1();

                var parser = ArgumentParser.Parse(args, settings);

                Assert.IsTrue(parser.IsHelp);
            }
        }
        [TestMethod]
        public void CmdArgs_Help2()
        {
            var settings = new Args2();

            var parser = ArgumentParser.Parse(new[] { "?" }, settings);

            Assert.IsTrue(parser.IsHelp);
            var help = parser.GetHelpText();

            Assert.AreEqual(Args2.GetExpectedHelpText().Trim(), help.Trim());
        }

        [TestMethod]
        public void CmdArgs_Usage1()
        {
            var settings = new Args1();

            var parser = ArgumentParser.Parse(new[] { "?" }, settings);

            var expected = "SenseNet.Tools.Tests [Arg1] [Arg2] [-A:Boolean] [-B:Boolean] [-C:Boolean] [-INT:Int32] [-STRING:String] [?]";
            var usage = parser.GetUsage();

            Assert.AreEqual(expected, usage);
        }
        [TestMethod]
        public void CmdArgs_Usage2()
        {
            var args = new string[0];
            var settings = new Args2();

            ArgumentParser parser = null;
            try
            {
                parser = ArgumentParser.Parse(args, settings);
            }
            catch(ParsingException e)
            {
                // skip any errors
                parser = e.Result;
            }

            var expected = "SenseNet.Tools.Tests <source> <target> [-A:Boolean] [-B:Boolean] [-C:Boolean] [-INT:Int32] <-STRING:String> [?]";
            var usage = parser.GetUsage();

            Assert.AreEqual(expected, usage);
        }

        [TestMethod]
        public void CmdArgs_Error_InvalidType()
        {
            var args = new[] { "-INT:42asdf" };
            var settings = new Args1();

            ParsingException ex = null;
            try
            {
                ArgumentParser.Parse(args, settings);
                Assert.Fail(); // must have an error
            }
            catch (ParsingException e)
            {
                ex = e;
            }

            Assert.AreEqual(ResultState.InvalidType, ex.ErrorCode);
        }
        [TestMethod]
        public void CmdArgs_Error_UnknownArgument()
        {
            var args = new[] { "-UNKNOWN:asdf" };
            var settings = new Args1();

            ParsingException ex = null;
            try
            {
                ArgumentParser.Parse(args, settings);
                Assert.Fail(); // must have an error
            }
            catch (ParsingException e)
            {
                ex = e;
            }

            Assert.AreEqual(ResultState.UnknownArgument, ex.ErrorCode);
        }
        [TestMethod]
        public void CmdArgs_Error_MissingArgumentName()
        {
            var args = new[] { "source", "target", "thirdValue" };
            var settings = new Args1();

            ParsingException ex = null;
            try
            {
                ArgumentParser.Parse(args, settings);
                Assert.Fail(); // must have an error
            }
            catch (ParsingException e)
            {
                ex = e;
            }

            Assert.AreEqual(ResultState.MissingArgumentName, ex.ErrorCode);
        }
        [TestMethod]
        public void CmdArgs_Error_MissingValue()
        {
            var args = new[] { "-INT" };
            var settings = new Args1();

            ParsingException ex = null;
            try
            {
                ArgumentParser.Parse(args, settings);
                Assert.Fail(); // must have an error
            }
            catch (ParsingException e)
            {
                ex = e;
            }

            Assert.AreEqual(ResultState.MissingValue, ex.ErrorCode);
        }

        [TestMethod]
        public void CmdArgs_Error_MissingArgs()
        {
            var args = new[] { "-PROFILE:asdf:5,qwer:3", "-u", "asdf" };
            var settings = new Args3();

            ParsingException ex = null;
            try
            {
                ArgumentParser.Parse(args, settings);
                Assert.Fail(); // must have an error
            }
            catch (ParsingException e)
            {
                ex = e;
            }

            Assert.AreEqual(ResultState.MissingArgument, ex.ErrorCode);
            Assert.IsTrue(ex.Message.Contains("Site"));
            Assert.IsTrue(ex.Message.Contains("Password"));
            Assert.IsFalse(ex.Message.Contains("Profile"));
            Assert.IsFalse(ex.Message.Contains("UserName"));
        }
        [TestMethod]
        public void CmdArgs_Error_HelpFromException()
        {
            var args = new[] { "-PROFILE:asdf:5,qwer:3", "-u", "asdf" };
            var settings = new Args2();

            try
            {
                ArgumentParser.Parse(args, settings);
                Assert.Fail(); // must have an error
            }
            catch (ParsingException e)
            {
                Assert.AreEqual(Args2.GetExpectedHelpText().Trim(), e.Result.GetHelpText().Trim());
            }
        }
    }

    internal class Args1
    {
        [NoNameOption(0, false, null, "Description source")]
        public string Source { get; set; }
        [NoNameOption(1, false, null, "Description target")]
        public string Target { get; set; }
        [CommandLineArgument(helpText: "Description A")]
        public bool A { get; set; }
        [CommandLineArgument(helpText: "Description B")]
        internal bool B { get; set; }
        [CommandLineArgument(helpText: "Description C")]
        // ReSharper disable once UnusedMember.Local
        private bool C { get; set; }
        [CommandLineArgument(name: "STRING", aliases: "s", helpText: "Description STRING")]
        // ReSharper disable once UnusedMember.Local
        private string StringParam1 { get; set; }
        [CommandLineArgument(name: "INT", aliases: "i", helpText: "Description INT")]
        internal int IntParam1 { get; set; }
    }
    internal class Args2
    {
        [NoNameOption(0, true, "source", "Description source")]
        public string Source { get; set; }
        [NoNameOption(1, true, "target", "Description target")]
        public string Target { get; set; }
        [CommandLineArgument(helpText: "Description A")]
        public bool A { get; set; }
        [CommandLineArgument(helpText: "Description B")]
        internal bool B { get; set; }
        [CommandLineArgument(helpText: "Description C")]
        // ReSharper disable once UnusedMember.Local
        private bool C { get; set; }
        [CommandLineArgument(name: "STRING", aliases: "s", required: true, helpText: "Description STRING")]
        public string StringParam1 { get; set; }
        [CommandLineArgument(name: "INT", aliases: "i", helpText: "Description INT")]
        internal int IntParam1 { get; set; }

        internal static string GetExpectedHelpText()
        {
            var asmName = Assembly.GetExecutingAssembly().GetName();
            return $@"{asmName.Name} {asmName.Version}

Usage:
SenseNet.Tools.Tests <source> <target> [-A:Boolean] [-B:Boolean] [-C:Boolean] [-INT:Int32] <-STRING:String> [?]

<source> (required)
    Description source

<target> (required)
    Description target

[-A:Boolean] (optional)
    Description A

[-B:Boolean] (optional)
    Description B

[-C:Boolean] (optional)
    Description C

[-INT:Int32] (optional)
    Alias: i
    Description INT

<-STRING:String> (required)
    Alias: s
    Description STRING

[?, -?, /?, -h, -H, /h /H -help --help] (optional)
    Display this text.
";
        }
    }
    internal class Args3
    {
        [CommandLineArgument(name: "Profile", required: true, aliases: "P", helpText: "Comma separated name:count pairs (e.g.: 'Profile1:5,Profile2:1')")]
        public string ProfileArg { get; set; }

        [CommandLineArgument(name: "Site", required: true, aliases: "S", helpText: "Comma separated url list (e.g.: 'http://mysite1,http://mysite1')")]
        public string SiteUrlArg { get; set; }

        [CommandLineArgument(name: "UserName", required: true, aliases: "U", helpText: "Username and domain (e.g. 'Admin' or 'demo\\someone")]
        public string UserName { get; set; }

        [CommandLineArgument(name: "Password", required: true, aliases: "P", helpText: "Password")]
        public string Password { get; set; }
    }
}
