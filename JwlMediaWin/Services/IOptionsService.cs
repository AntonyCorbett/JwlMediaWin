namespace JwlMediaWin.Services
{
    using Models;

#pragma warning disable U2U1005
    internal interface IOptionsService
#pragma warning restore U2U1005
    {
        Options Options { get; }

        void Save();
    }
}
