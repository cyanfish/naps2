using CommandLine;
using NAPS2.Tools.Localization;
using NAPS2.Tools.Project;
using NAPS2.Tools.Project.Installation;
using NAPS2.Tools.Project.Packaging;
using NAPS2.Tools.Project.Releasing;
using NAPS2.Tools.Project.Verification;
using NAPS2.Tools.Project.Workflows;

namespace NAPS2.Tools;

public static class Program
{
    // TODO: Add a "testpo"/"testlang" command that:
    // - Takes the URL/path of a .po file as input
    // - Downloads it
    // - Replaces the corresponding .po file
    // - Updates language resources for that language
    // - Possibly then runs "pkg zip --name test-{lang}"

    // TODO: Add a "setver" command that updates version targets, Info.plist, and anything else that needs a version

    public static int Main(string[] args)
    {
        var commands = new CommandList()
            .Add<CleanOptions, CleanCommand>()
            .Add<BuildOptions, BuildCommand>()
            .Add<TestOptions, TestCommand>()
            .Add<PackageOptions, PackageCommand>()
            .Add<InstallOptions, InstallCommand>()
            .Add<VerifyOptions, VerifyCommand>()
            .Add<PublishOptions, PublishCommand>()
            .Add<VirusScanOptions, VirusScanCommand>()
            .Add<ShareOptions, ShareCommand>()
            .Add<TemplatesOptions, TemplatesCommand>()
            .Add<ResxOptions, ResxCommand>()
            .Add<PushTemplatesOptions, PushTemplatesCommand>()
            .Add<PullTranslationsOptions, PullTranslationsCommand>()
            .Add<SetVersionOptions, SetVersionCommand>()
            .Add<SaneOptsOptions, SaneOptsCommand>();

        var result = Parser.Default.ParseArguments(args, commands.OptionTypes);
        if (result.Errors.Any())
        {
            return 1;
        }
        var options = (OptionsBase) result.Value;
        Output.EnableVerbose = options.Verbose;
        var commandType = commands.GetCommandType(options.GetType());
        var command = Activator.CreateInstance(commandType);
        var run = commandType.GetMethod("Run") ?? throw new InvalidOperationException();
        run.Invoke(command, new object?[] { options });
        return 0;
    }

    public class CommandList
    {
        private readonly List<Type> _optionTypes = new();
        private readonly Dictionary<Type, Type> _optionTypeToCommandType = new();

        public CommandList Add<TOption, TCommand>() where TOption : OptionsBase where TCommand : ICommand<TOption>
        {
            _optionTypes.Add(typeof(TOption));
            _optionTypeToCommandType.Add(typeof(TOption), typeof(TCommand));
            return this;
        }

        public Type[] OptionTypes => _optionTypes.ToArray();

        public Type GetCommandType(Type optionType)
        {
            return _optionTypeToCommandType[optionType];
        }
    }
}