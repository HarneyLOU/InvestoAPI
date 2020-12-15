using InvestoAPI.Core.Entities;
using InvestoAPI.Core.Interfaces;
using InvestoAPI.Infrastructure.Services.StockProviders;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace InvestoAPI.Infrastructure.Services
{
    public class NewsService : INewsService
    {
        private readonly ILogger _logger;
        private readonly ApplicationContext _context;

        public NewsService(
            ILogger<NewsService> logger,
            ApplicationContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IEnumerable<News> GetLast()
        {
            return _context.News.OrderByDescending(n => n.PublishedDate).Take(10);
        }

        public async Task UpdateWithDataProvider(string[] symbols)
        {
            try
            {
                var dataProvider = new FMProvider();
                var fmNews = await dataProvider.GetNews(symbols);
                var newNews = new List<News>();
                foreach (var fmN in fmNews)
                {
                    newNews.Add(new News()
                    {
                        Symbol = fmN.symbol,
                        PublishedDate = DateTime.Parse(fmN.publishedDate),
                        Title = fmN.title,
                        Image = fmN.image,
                        Site = fmN.site,
                        Text = fmN.text,
                        Url = fmN.url
                    });
                }
                DateTimeOffset minDate = newNews.Min(d => d.PublishedDate);
                var news = _context.News.Where(d => d.PublishedDate >= minDate).ToList();
                foreach (var n in newNews)
                {
                    if (news.Where(e => e.Symbol == n.Symbol && e.PublishedDate == n.PublishedDate && e.Title == n.Title).Count() == 0)
                    {
                        Add(n);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("ERROR " + ex.Message);
            } 
        }

        public void Add(News news)
        {
            _context.News.Add(news);
            _context.SaveChanges();
        }

    }
}
