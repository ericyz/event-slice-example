using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bolt.Common.Extensions;
using Bolt.Logger;
using Bolt.RequestBus;
using Bolt.RestClient;
using Bolt.RestClient.Builders;
using Bolt.RestClient.Extensions;
using BookWorm.Api;
using BookWorm.Web.Features.Shared.SavedBooks;
using Microsoft.Extensions.Options;
using Src.Features.Shared.Settings;
using Src.Infrastructure.Attributes;
using Src.Infrastructure.ErrorSafeHelpers;
using Src.Infrastructure.Stores;

namespace BookWorm.Web.Features.Home.BooksOfTheWeek
{

    public interface IBooksOfTheWeekProvider
    {
        IEnumerable<BookDto> Get();
        void Set(IEnumerable<BookDto> value);
    }

    public class LoadBooksOfTheWeekOnPageLoadEventHandler : IAsyncEventHandler<HomePageRequestedEvent>
    {
        private readonly ILogger logger;
        private readonly IBooksOfTheWeekProvider provider;
        private readonly IOptions<ApiSettings> settings;
        private readonly IRestClient restClient;

        public LoadBooksOfTheWeekOnPageLoadEventHandler(ILogger logger,
            IBooksOfTheWeekProvider provider, 
            IOptions<ApiSettings> settings, 
            IRestClient restClient)
        {
            this.logger = logger;
            this.provider = provider;
            this.settings = settings;
            this.restClient = restClient;
        }

        public async Task HandleAsync(HomePageRequestedEvent eEvent)
        {
            var response = await ErrorSafe.WithLogger(logger)
                .ExecuteAsync(() => restClient.For(UrlBuilder.Host(settings.Value.BaseUrl).Route("books/featured"))
                    .AcceptJson()
                    .Timeout(1000)
                    .RetryOnFailure(2)
                    .GetAsync<IEnumerable<BookDto>>());

            provider.Set(response.Value?.Output);
        }
    }


    [AutoBind]
    public class BooksOfTheWeekProvider : IBooksOfTheWeekProvider
    {
        private readonly IContextStore context;
        private readonly ISavedItemsProvider savedItemsProvider;
        private const string Key = "BooksOfTheWeekProvider:Get";

        public BooksOfTheWeekProvider(IContextStore context, ISavedItemsProvider savedItemsProvider)
        {
            this.context = context;
            this.savedItemsProvider = savedItemsProvider;
        }

        public IEnumerable<BookDto> Get()
        {
            var savedIds = savedItemsProvider.Get();
            return context.Get<IEnumerable<BookDto>>(Key)
                .NullSafe()
                .Select(x =>
                {
                    x.IsSaved = savedIds.Any(id => id == x.Id);
                    return x;
                });
        }

        public void Set(IEnumerable<BookDto> value)
        {
            context.Set(Key, value);
        }
    }
}