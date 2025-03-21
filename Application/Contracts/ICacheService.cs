﻿
using Commons.Classes;
using Models.Entities;

namespace Application.Contracts
{
    public interface ICacheService
    {
        T? Get<T>(string key);
        void Set<T>(string key, T value, TimeSpan? expiration = null);
        void Remove(string key);
        void RemoveAll();
    }
}

