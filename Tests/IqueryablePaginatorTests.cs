namespace KeySetPaginator.Tests
{
    public class IqueryablePaginatorTests
    {
        public IQueryable<ExampleModel> Rows { get; set; }

        [SetUp]
        public void Setup()
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
                                new ExampleModel() { StringName = "sharon2", DecimalName = 1, IntName = 2, LongName = 2, NullableName = 1 },
                                new ExampleModel() { StringName = "sharon2", DecimalName = 2, IntName = 2, LongName = 2, NullableName = 2 },
                                new ExampleModel() { StringName = "sharon3", DecimalName = 3, IntName = 3, LongName = 3, NullableName = null }
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

            Rows = Rows.Take(6);

            var response = Rows.ToList();

            response.Should().BeEquivalentTo(new List<ExampleModel> {
                                new ExampleModel() { StringName = "sharon1", DecimalName = 1, IntName = 1, LongName = 1, NullableName = 1 },
                                new ExampleModel() { StringName = "sharon2", DecimalName = 1, IntName = int.MaxValue, LongName = 2, NullableName = null },
                                new ExampleModel() { StringName = "sharon2", DecimalName = 1, IntName = 2, LongName = 2, NullableName = 1 },
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

        [Test]
        public void TestAfterRowToken_SkippingSortingFirstRows()
        {
            ExampleToken token = new() { StringName = KeySetToken.InitField("sharon5"), NullableName = KeySetToken.InitField(5M) };
            Rows = Rows.AddSorting(token, SortDirectionDTO.asc);

            Rows = Rows.KeySetSkip(token, SortDirectionDTO.asc);

            Rows = Rows.Take(1);

            var response = Rows.ToList();

            response.Should().BeEquivalentTo(new List<ExampleModel> {
                                                new ExampleModel() { StringName = "sharon6", DecimalName = 6, IntName = 6, LongName = 6, NullableName = 6 }
            });
        }

        [Test]
        public void TestAfterRowToken_SkippingSortingLastRows()
        {
            ExampleToken token = new() { StringName = KeySetToken.InitField("sharon5"), NullableName = KeySetToken.InitField(5M) };
            Rows = Rows.AddSorting(token, SortDirectionDTO.desc);

            Rows = Rows.KeySetSkip(token, SortDirectionDTO.desc);

            Rows = Rows.Take(1);

            var response = Rows.ToList();

            response.Should().BeEquivalentTo(new List<ExampleModel> {
                                                new ExampleModel() { StringName = "sharon4", DecimalName = 4, IntName = 4, LongName = 4, NullableName = 4 }
            });
        }

        [Test]
        public void TestAfterRowToken_SkippingSortingFirstRowsSameComplexKey()
        {
            ExampleToken token = new() { StringName = KeySetToken.InitField("sharon2"), NullableName = KeySetToken.InitField(1M) };
            Rows = Rows.AddSorting(token, SortDirectionDTO.asc);

            Rows = Rows.KeySetSkip(token, SortDirectionDTO.asc);

            Rows = Rows.Take(1);

            var response = Rows.ToList();

            response.Should().BeEquivalentTo(new List<ExampleModel> {
                                                new ExampleModel() { StringName = "sharon2", DecimalName = 2, IntName = 2, LongName = 2, NullableName = 2 }
            });
        }

        [Test]
        public void TestAfterRowToken_SkippingSortingFirstRowsSameNullableComplexKey_GetAfterNull()
        {
            ExampleToken token = new() { StringName = KeySetToken.InitField("sharon2") };
            Rows = Rows.AddSorting(token, SortDirectionDTO.asc);

            Rows = Rows.KeySetSkip(token, SortDirectionDTO.asc);

            Rows = Rows.Take(1);

            var response = Rows.ToList();

            response.Should().BeEquivalentTo(new List<ExampleModel> {
                                                new ExampleModel() { StringName = "sharon2", DecimalName = 1, IntName = 2, LongName = 2, NullableName = 1 }
            });
        }

        [Test]
        public void TestAfterLastRow_SkippingSortingAscending_ShouldReturnEmpty()
        {
            ExampleToken token = new() { StringName = KeySetToken.InitField("sharon9"), NullableName = KeySetToken.InitField(9M) };
            Rows = Rows.AddSorting(token, SortDirectionDTO.asc);

            Rows = Rows.KeySetSkip(token, SortDirectionDTO.asc);

            Rows = Rows.Take(1);

            var response = Rows.ToList();

            response.Should().BeEquivalentTo(new List<ExampleModel> { });
        }

        [Test]
        public void TestAfterLastRow_SkippingSortingDescending_ShouldReturnEmpty()
        {
            ExampleToken token = new() { StringName = KeySetToken.InitField("sharon1"), NullableName = KeySetToken.InitField(1M) };
            Rows = Rows.AddSorting(token, SortDirectionDTO.desc);

            Rows = Rows.KeySetSkip(token, SortDirectionDTO.desc);

            Rows = Rows.Take(1);

            var response = Rows.ToList();

            response.Should().BeEquivalentTo(new List<ExampleModel> { });
        }

        public class WrongNameToken : KeySetToken
        {
            public WrongNameToken()
            : base(new List<string>() { "NotKeySetTokenValue" })
            { }
            public KeySetTokenValue<int> NotKeySetTokenValue { get; set; }

            public override List<string> DefaultFields { get; set; }
        }

        [Test]
        public void TestBadDefinedToken_NotExistingField_ShouldThrow()
        {
            WrongNameToken token = new();
            Assert.Throws<ArgumentException>(() => Rows.AddSorting(token, SortDirectionDTO.desc), $"There is no such field NotKeySetTokenValue");

            // Sorting covers the validation here
           //Assert.Throws<ArgumentException>(() => Rows.KeySetSkip(token, SortDirectionDTO.desc), $"There is no such field NotKeySetTokenValue");

        }

        [Test]
        public void TestBadDefinedToken_NotExistingFieldInit_ShouldThrow()
        {
            WrongNameToken token = new() { NotKeySetTokenValue = new(1) };
            Assert.Throws<ArgumentException>(() => Rows.AddSorting(token, SortDirectionDTO.desc), $"There is no such field NotKeySetTokenValue");

            Assert.Throws<ArgumentException>(() => Rows.KeySetSkip(token, SortDirectionDTO.desc), $"There is no such field NotKeySetTokenValue");
        }

        public class NoTokenValueFieldToken : KeySetToken
        {
            public NoTokenValueFieldToken()
            : base(new List<string>() { "NotKeySetTokenValue" })
            { }
            public int NotKeySetTokenValue { get; set; }

            public override List<string> DefaultFields { get; set; }
        }

        [Test]
        public void TestBadDefinedToken_NoTokenValueField_ShouldThrow()
        {
            Assert.Throws<ArgumentException>(() => new NoTokenValueFieldToken(), "$Property of type: NotKeySetTokenValue must be of type KeySetTokenValue");
        }

        [Test]
        public void TestBadDefinedToken_NoTokenValueFieldInit_ShouldThrow()
        {
            Assert.Throws<ArgumentException>(() => new NoTokenValueFieldToken() { NotKeySetTokenValue = 1 }, "$Property of type: NotKeySetTokenValue must be of type KeySetTokenValue");
        }

        public class NoPropertiesToken : KeySetToken
        {
            public NoPropertiesToken()
            : base(new List<string>() { "NotKeySetTokenValue" })
            { }

            public override List<string> DefaultFields { get; set; }
        }

        [Test]
        public void TestBadDefinedToken_NoPropertiesToken_ShouldThrow()
        {
            Assert.Throws<ArgumentException>(() => new NoPropertiesToken(), "$Property of type: NotKeySetTokenValue must be of type KeySetTokenValue");
        }
    }
}