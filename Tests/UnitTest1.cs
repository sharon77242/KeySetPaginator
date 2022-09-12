using FluentAssertions;

namespace KeySetPaginator.Tests
{
    public class Tests
    {
        public IQueryable<ExampleModel> Rows { get; set; }

        [SetUp]
        public void Setup()
        {
            var rows = new List<ExampleModel>
            {
                new ExampleModel() { StringName = "sharon2", DecimalName = 1, IntName = int.MaxValue, LongName = 2, NullableName = null },
                new ExampleModel() { StringName = "sharon3", DecimalName = 3, IntName = 3, LongName = 3, NullableName = null }
            };

            for (int i = 1; i < 10; i++)
            {
                rows.Add(new ExampleModel() { StringName = "sharon" + i, DecimalName = i, IntName = i, LongName = i, NullableName = i });
            }

            Rows = rows.AsQueryable();
        }

        [Test]
        public void TestEmptyToken_SkippingSortingFirstRows()
        {
            ExampleToken token = new();
            Rows = Rows.AddSorting(token, SortDirectionDTO.asc);

            Rows = Rows.KeySetSkip(token, SortDirectionDTO.asc);

            Rows = Rows.Take(5);

            var response = Rows.ToList();

            response.Should().BeEquivalentTo(new List<ExampleModel> {
                                new ExampleModel() { StringName = "sharon1", DecimalName = 1, IntName = 1, LongName = 1, NullableName = 1 },
                                new ExampleModel() { StringName = "sharon2", DecimalName = 1, IntName = int.MaxValue, LongName = 2, NullableName = null },
                                new ExampleModel() { StringName = "sharon2", DecimalName = 2, IntName = 2, LongName = 2, NullableName = 2 },
                                new ExampleModel() { StringName = "sharon3", DecimalName = 3, IntName = 3, LongName = 3, NullableName = null },
                                new ExampleModel() { StringName = "sharon3", DecimalName = 3, IntName = 3, LongName = 3, NullableName = 3 },
            });
        }

        [Test]
        public void TestEmptyToken_SkippingSortingLastRows()
        {
            ExampleToken token = new();
            Rows = Rows.AddSorting(token, SortDirectionDTO.desc);

            Rows = Rows.KeySetSkip(token, SortDirectionDTO.desc);

            Rows = Rows.Take(5);

            var response = Rows.ToList();

            response.Should().BeEquivalentTo(new List<ExampleModel> {
                                new ExampleModel() { StringName = "sharon9", DecimalName = 9, IntName = 9, LongName = 9, NullableName = 9 },
                                new ExampleModel() { StringName = "sharon8", DecimalName = 8, IntName = 8, LongName = 8, NullableName = 8 },
                                new ExampleModel() { StringName = "sharon7", DecimalName = 7, IntName = 7, LongName = 7, NullableName = 7 },
                                new ExampleModel() { StringName = "sharon6", DecimalName = 6, IntName = 6, LongName = 6, NullableName = 6 },
                                new ExampleModel() { StringName = "sharon5", DecimalName = 5, IntName = 5, LongName = 5, NullableName = 5 },
            });
        }

        [Test]
        public void TestEmptyToken_SkippingSortingFirstRows_DifferentDefaultFields()
        {
            ExampleToken token = new(new List<string> { "DecimalName", "NullableName" });
            Rows = Rows.AddSorting(token, SortDirectionDTO.asc);

            Rows = Rows.KeySetSkip(token, SortDirectionDTO.asc);

            Rows = Rows.Take(5);

            var response = Rows.ToList();

            response.Should().BeEquivalentTo(new List<ExampleModel> {
                                new ExampleModel() { StringName = "sharon1", DecimalName = 1, IntName = 1, LongName = 1, NullableName = 1 },
                                new ExampleModel() { StringName = "sharon2", DecimalName = 1, IntName = int.MaxValue, LongName = 2, NullableName = null },
                                new ExampleModel() { StringName = "sharon2", DecimalName = 2, IntName = 2, LongName = 2, NullableName = 2 },
                                new ExampleModel() { StringName = "sharon3", DecimalName = 3, IntName = 3, LongName = 3, NullableName = null },
                                new ExampleModel() { StringName = "sharon3", DecimalName = 3, IntName = 3, LongName = 3, NullableName = 3 },
            });
        }

        [Test]
        public void TestEmptyToken_SkippingSortingLastRows_DifferentDefaultFields()
        {
            ExampleToken token = new(new List<string> { "IntName", "NullableName" });
            Rows = Rows.AddSorting(token, SortDirectionDTO.desc);

            Rows = Rows.KeySetSkip(token, SortDirectionDTO.desc);

            Rows = Rows.Take(5);

            var response = Rows.ToList();

            response.Should().BeEquivalentTo(new List<ExampleModel> {
                                new ExampleModel() { StringName = "sharon2", DecimalName = 1, IntName = int.MaxValue, LongName = 2, NullableName = null },
                                new ExampleModel() { StringName = "sharon9", DecimalName = 9, IntName = 9, LongName = 9, NullableName = 9 },
                                new ExampleModel() { StringName = "sharon8", DecimalName = 8, IntName = 8, LongName = 8, NullableName = 8 },
                                new ExampleModel() { StringName = "sharon7", DecimalName = 7, IntName = 7, LongName = 7, NullableName = 7 },
                                new ExampleModel() { StringName = "sharon6", DecimalName = 6, IntName = 6, LongName = 6, NullableName = 6 },
            });
        }
    }
}