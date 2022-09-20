using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KeySetPaginator.Tests
{
    public class PaginatorAPITests
    {
        public List<ExampleModel> OriginalRows { get; set; }
        public SearchAction SearchAction;

        [SetUp]
        public void Setup()
        {
            SearchAction = new SearchAction();

            OriginalRows = SearchAction.Rows.ToList().OrderBy(x => x.StringName).ThenBy(x => x.NullableName).ToList();
        }

        [Test]
        public async Task TestLastResponseToToken_TokenIsNull_ShouldThrow()
        {
            ExampleToken token = null;
            var ex = Assert.Throws<ArgumentException>(() => token.LastReponseToToken<ExampleModel, ExampleToken>(null));
            Assert.That(ex.Message, Is.EqualTo("Token can't be null"));
        }

        [Test]
        public async Task TestLastResponseToToken_LastRowIsNull_ShouldReturnNullToken()
        {
            ExampleToken token = new ExampleToken();
            token.LastReponseToToken<ExampleModel, ExampleToken>(null).Should().BeNull();
        }

        [Test]
        public async Task TestLastResponseToToken()
        {
            ExampleToken token = new ExampleToken();
            var newToken = token.LastReponseToToken(new ExampleModel() { StringName = "sharon1", DecimalName = 1, IntName = 1, LongName = 1, NullableName = 1 });
            newToken.Should().BeEquivalentTo(new ExampleToken { StringName = KeySetToken.InitField("sharon1"), NullableName = KeySetToken.InitField(1M) });
        }

        [Test]
        public async Task TestEmptyRequest_GetAllResults()
        {
            var results = await PaginatorAPI.GetAllResults(
                SearchAction.SearchActionExample, new KeySetPagingRequest<ExampleToken, ExampleRequest> { PageSize = 20, SortDirection = SortDirection.asc });

            results.Should().BeEquivalentTo(OriginalRows);
        }

        [Test]
        public async Task TestEmptyRequest_GetAllResults_PageSize()
        {
            var results = await PaginatorAPI.GetAllResults(
                SearchAction.SearchActionExample, new KeySetPagingRequest<ExampleToken, ExampleRequest> { PageSize = 3, SortDirection = SortDirection.asc });

            results.Should().BeEquivalentTo(OriginalRows);
        }

        [Test]
        public async Task TestEmptyRequest_GetAllResults_AfterTokenWithPaging()
        {
            var results = await PaginatorAPI.GetAllResults(
                SearchAction.SearchActionExample,
                new KeySetPagingRequest<ExampleToken, ExampleRequest>
                {
                    PageSize = 3,
                    SortDirection = SortDirection.asc,
                    KeySetToken = new ExampleToken() { StringName = KeySetToken.InitField("sharon2"), NullableName = KeySetToken.InitField(2M) }
                });

            results.Should().BeEquivalentTo(OriginalRows.Skip(4));
        }

        [Test]
        public async Task TestEmptyRequest_GetAllResults_AfterTokenWithPagingWithAfterSearchAction()
        {
            var searchAction = new Mock<SearchAction>();
            searchAction.Setup(m => m.AfterSearchEmpty(It.IsAny<List<ExampleModel>>()));

            var results = await PaginatorAPI.GetAllResults(
                searchAction.Object.SearchActionExample,
                searchAction.Object.AfterSearchEmpty,
                new KeySetPagingRequest<ExampleToken, ExampleRequest>
                {
                    PageSize = 2,
                    SortDirection = SortDirection.asc,
                    KeySetToken = new ExampleToken() { StringName = KeySetToken.InitField("sharon3"), NullableName = KeySetToken.InitField(3M) }
                }); ;

            Assert.That(results.Count, Is.EqualTo(6));
            searchAction.Verify(m => m.AfterSearchEmpty(It.IsAny<List<ExampleModel>>()), Times.Exactly(3)); // 6 results divide by page size 2 => 6 / 2 = 3
        }

        [Test]
        public async Task TestEmptyRequest_GetAllResults_AfterTokenWithPagingWithAfterFalseSearchAction()
        {
            var searchAction = new Mock<SearchAction>();
            searchAction.Setup(m => m.AfterSearchBool(It.IsAny<List<ExampleModel>>())).Returns(Task.FromResult(false));

            var results = await PaginatorAPI.GetAllResults(
                searchAction.Object.SearchActionExample,
                searchAction.Object.AfterSearchBool,
                new KeySetPagingRequest<ExampleToken, ExampleRequest>
                {
                    PageSize = 1,
                    SortDirection = SortDirection.asc,
                    KeySetToken = new ExampleToken() { StringName = KeySetToken.InitField("sharon2"), NullableName = KeySetToken.InitField(2M) }
                });

            results.Should().BeEquivalentTo(OriginalRows.Skip(4));
            searchAction.Verify(m => m.AfterSearchBool(It.IsAny<List<ExampleModel>>()), Times.Exactly(8)); // rows numbers
        }

        [Test]
        public async Task TestEmptyRequest_GetAllResults_AfterTokenWithPagingWithAfterTrueSearchAction()
        {
            var searchAction = new Mock<SearchAction>();
            searchAction.Setup(m => m.AfterSearchBool(It.IsAny<List<ExampleModel>>())).Returns(Task.FromResult(true)); ;

            var results = await PaginatorAPI.GetAllResults(
                searchAction.Object.SearchActionExample,
                searchAction.Object.AfterSearchBool,
                new KeySetPagingRequest<ExampleToken, ExampleRequest>
                {
                    PageSize = 1,
                    SortDirection = SortDirection.asc,
                    KeySetToken = new ExampleToken() { StringName = KeySetToken.InitField("sharon2"), NullableName = KeySetToken.InitField(3M) }
                });

            searchAction.Verify(m => m.AfterSearchBool(It.IsAny<List<ExampleModel>>()), Times.Once);

            results.Should().BeEquivalentTo(new List<ExampleModel>() {
                new ExampleModel() { StringName = "sharon3", DecimalName = 3, IntName = 3, LongName = 3, NullableName = null }
            });
        }

        [Test]
        public async Task TestQueryParam()
        {
            var keySetRequest = new KeySetPagingRequest<ExampleToken, ExampleRequest>
            {
                PageSize = 20,
                SortDirection = SortDirection.asc,
                KeySetToken = new ExampleToken() { StringName = KeySetToken.InitField("sharon") },
                Timeout = 50
            };

            var param = keySetRequest.AsParams();

            param.Should().BeEquivalentTo(new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>( "PageSize", 20),
                new KeyValuePair<string, object>( "SortDirection", SortDirection.asc),
                new KeyValuePair<string, object>("Timeout", 50),
                new KeyValuePair<string, object>( "KeySetToken.StringName.Value", "sharon"),
                new KeyValuePair<string, object>( "KeySetToken.DefaultFields", "StringName"),
                new KeyValuePair<string, object>("KeySetToken.DefaultFields", "NullableName")
            });
        }
    }
}
