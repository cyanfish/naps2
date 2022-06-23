using Moq;
using NAPS2.ImportExport;
using NAPS2.Scan;
using NAPS2.Scan.Batch;
using NAPS2.WinForms;
using Xunit;

namespace NAPS2.Sdk.Tests.WinForms;

public class BatchFormTests : ContextualTexts
{
    private readonly IProfileManager _profileManager = new StubProfileManager();
    
    [Fact]
    public void LoadUserConfig()
    {
        var ctx = CreateForm();
        ctx.Config.User.Set(c => c.BatchSettings, new BatchSettings {
                OutputType = BatchOutputType.MultipleFiles,
                SaveSeparator = SaveSeparator.PatchT,
                ScanType = BatchScanType.MultipleWithDelay,
                SavePath = "test_path",
                ScanIntervalSeconds = 2.3,
                ScanCount = 2,
                ProfileDisplayName = "test_name"
        });
        _profileManager.Mutate(new ListMutation<ScanProfile>.Append(new ScanProfile
        {
            DisplayName = "bad_name",
            IsDefault = true
        }), ListSelection.Empty<ScanProfile>());
        _profileManager.Mutate(new ListMutation<ScanProfile>.Append(new ScanProfile
        {
            DisplayName = "test_name"
        }), ListSelection.Empty<ScanProfile>());
        ctx.Form.Show();
        try
        {
            Assert.True(ctx.Form.rdSaveToMultipleFiles.Checked);
            Assert.True(ctx.Form.rdSeparateByPatchT.Checked);
            Assert.True(ctx.Form.rdMultipleScansDelay.Checked);
            Assert.Equal("test_path", ctx.Form.txtFilePath.Text);
            Assert.Equal("2.3", ctx.Form.txtTimeBetweenScans.Text);
            Assert.Equal("2", ctx.Form.txtNumberOfScans.Text);
            Assert.Equal("test_name", ctx.Form.comboProfile.Text);
        }
        finally
        {
            ctx.Form.Hide();
        }
    }

    private FormContext CreateForm()
    {
        var ctx = new FormContext
        {
            Performer = new Mock<IBatchScanPerformer>(),
            ErrorOutput = new Mock<ErrorOutput>(),
            DialogHelper = new Mock<DialogHelper>(),
            Config = ScopedConfig.Stub()
        };
        ctx.Form = new FBatchScan(ctx.Performer.Object, ctx.ErrorOutput.Object, ctx.DialogHelper.Object, _profileManager)
        {
            Config = ctx.Config
        };
        return ctx;
    }

    private class FormContext
    {
        public Mock<IBatchScanPerformer> Performer { get; set; }

        public Mock<ErrorOutput> ErrorOutput { get; set; }

        public Mock<DialogHelper> DialogHelper { get; set; }

        public ScopedConfig Config { get; set; }

        public FBatchScan Form { get; set; }
    }
}