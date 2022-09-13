namespace KeySetPaginator.Tests
{
    public class SearchAction
    {
        public IQueryable<ExampleModel> Rows { get; set; }

        public SearchAction()
        {
            var rows = new List<ExampleModel>
            {
                new ExampleModel() { StringName = "sharon2", DecimalName = 1, IntName = int.MaxValue, LongName = 2, NullableName = null },
                new ExampleModel() { StringName = "sharon2", DecimalName = 1, IntName = 2, LongName = 2, NullableName = 1 },
                new ExampleModel() { StringName = "sharon3", DecimalName = 3, IntName = 3, LongName = 3, NullableName = null }
            };

            for (int i = 1; i < 10; i++)
            {
                rows.Add(new ExampleModel() { StringName = "sharon" + i, DecimalName = i, IntName = i, LongName = i, NullableName = i });
            }

            Rows = rows.AsQueryable();
        }

        public virtual Task AfterSearchEmpty(List<ExampleModel> a) => Task.CompletedTask;

        public virtual Task<bool> AfterSearchBool(List<ExampleModel> a) => Task.FromResult(true);


        public Task<List<ExampleModel>> SearchActionExample(ExampleRequest request, KeySetPagingRequest<ExampleToken> pagingRequest)
        {
            IQueryable<ExampleModel> Query = Rows;

            if (request.NullableName != null)
                Query = Query.Where(x => x.NullableName == request.NullableName);
            if (request.IntName != null)
                Query = Query.Where(x => x.IntName == request.IntName);
            if (request.LongName != null)
                Query = Query.Where(x => x.LongName == request.LongName);
            if (request.StringName != null)
                Query = Query.Where(x => x.StringName == request.StringName);
            if (request.DecimalName != null)
                Query = Query.Where(x => x.DecimalName == request.DecimalName);
            if (request.LongName != null)
                Query = Query.Where(x => x.LongName == request.LongName);

            Query = Query.AddSorting(pagingRequest.KeySetToken, pagingRequest.SortDirection);
            Query = Query.KeySetSkip(pagingRequest.KeySetToken, pagingRequest.SortDirection);
            Query = Query.Take(pagingRequest.PageSize);

            return Task.FromResult(Query.ToList());
        }
    }
}
