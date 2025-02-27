﻿using InvestoAPI.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace InvestoAPI.Core.Interfaces
{
    public interface INewsService
    {
        IEnumerable<News> GetLast();
        void Add(News news);
        Task UpdateWithDataProvider(string[] symbols);
    }
}
