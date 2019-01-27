namespace JwlMediaWin.Services
{
    using JwlMediaWin.Models;

    internal interface IOptionsService
    {
        Options Options { get; }

        void Save();
    }
}
