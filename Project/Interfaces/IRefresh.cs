using System;

namespace Project.Interfaces
{
    public interface IRefresh
    {
        event Action RefreshRequested;
    }
}