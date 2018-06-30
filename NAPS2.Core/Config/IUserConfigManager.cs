namespace NAPS2.Config
{
    public interface IUserConfigManager
    {
        UserConfig Config { get; }

        void Load();

        void Save();
    }
}